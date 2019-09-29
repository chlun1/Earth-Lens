using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using EarthLens.Models;
using EarthLens.Services;
using SkiaSharp;
using SkiaSharp.Views.Android;

namespace EarthLens.android
{
    public class AnalyzingFragment : Fragment
    {
        private IList<ImageEntry> _imageEntries;
        private CancellationToken _ct;
        private CancellationTokenSource _ts;
        private Task _task;
        private int _totalNumberOfChips;
        private int _numberOfProcessedChips;

        public static AnalyzingFragment NewInstance(IList<ImageEntry> imageEntries)
        {
            return new AnalyzingFragment { _imageEntries = imageEntries };
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_analyzing, container, false);
            var imageView = view.FindViewById<ImageView>(Resource.Id.imageView);
            var entry = _imageEntries[0];
            var bitmap = entry.Image.ToBitmap();
            imageView.SetImageBitmap(bitmap);

            return view;
        }

        // TODO: Move this to ImageAnalysisService.
        private void StartAnalysis()
        {
            /*
            if (_imageEntries == null || _imageEntries.Count == 0)
            {
                LaunchImageUploadPage();
            }
            */

            _totalNumberOfChips = _imageEntries.Sum(imageEntry => GetNumberOfChips(imageEntry.Image));
            _numberOfProcessedChips = 0;

            _task = Task.Factory.StartNew(() =>
            {
                try
                {
                    foreach (var imageEntry in _imageEntries)
                    {
                        var categoryManagers = ImageAnalysisService.AnalyzeImage(imageEntry.Image,
                            ChipAnalysisService.AnalyzeChip,
                            _ct,
                            () => Interlocked.Increment(ref _numberOfProcessedChips))
                        .ToList();

                        var observations = PostProcessingService.SelectObservations(
                                categoryManagers,
                                SharedConstants.DefaultIOUThreshold,
                                NSUserDefaults.StandardUserDefaults.DoubleForKey(
                                    Constants.ConfidenceThresholdUserDefaultName), _ct)
                            .ToList();
                        imageEntry.Observations = observations;
                    }

                    if (_ts.IsCancellationRequested) return;
                    Task.Delay(Constants.DelayBeforeResults / _numberOfProcessedChips, _ct).Wait(_ct);
                    BeginInvokeOnMainThread(LaunchResultsView);
                }
                catch (System.OperationCanceledException)
                {
                    // ignored
                }
            }, _ct);
        }

        private static int GetNumberOfChips(SKImage image)
        {
            var width = image.Width;
            var height = image.Height;
            var numberWidth = Convert.ToInt32(Math.Ceiling((double)width / SharedConstants.DefaultChipWidth));
            var numberHeight = Convert.ToInt32(Math.Ceiling((double)height / SharedConstants.DefaultChipHeight));
            return numberWidth * numberHeight;
        }
    }
}
