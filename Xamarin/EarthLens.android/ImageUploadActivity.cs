using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using EarthLens.Models;
using Newtonsoft.Json;

namespace EarthLens.android
{
    [Activity(Label = "ImageUploadActivity", Theme = "@style/AppTheme")]
    public class ImageUploadActivity : Activity
    {
        private static readonly int PickImageId = 1000;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_image_upload);
            SetButtonAttributes();
        }

        private void SetButtonAttributes()
        {
            var imageUploadButton = FindViewById<ImageButton>(Resource.Id.uploadImage);
            imageUploadButton.Click += (sender, e) => ImageUploadButtonOnClick();
        }

        private void ImageUploadButtonOnClick()
        {
            Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), PickImageId);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            if (requestCode == PickImageId)
            {
                if ((resultCode == Result.Ok) && (intent != null))
                {
                    var uri = intent.Data;
                    var stream = ContentResolver.OpenInputStream(uri);
                    var skImage = SkiaSharp.SKImage.FromBitmap(SkiaSharp.SKBitmap.Decode(stream));
                    var name = uri.LastPathSegment;
                    var imageEntry = new ImageEntry(skImage, name, DateTime.UtcNow, null);
                    LaunchAnalysisScreen(new List<ImageEntry> { imageEntry });
                }
                else
                {
                    // TODO: Display error message.
                    Log.Info("ImageUploadActivity", "Image null");
                }
            }
        }

        private void LaunchAnalysisScreen(IList<ImageEntry> imageEntries)
        {
            var analyzingFragment = AnalyzingFragment.NewInstance(imageEntries);
            FragmentManager.BeginTransaction()
                .Add(Resource.Id.fragmentContainer, analyzingFragment)
                .AddToBackStack(null)
                .Commit();
        }
    }
}
