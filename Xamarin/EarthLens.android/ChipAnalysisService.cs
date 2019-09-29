using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using EarthLens.Models;
using EarthLens.Services;

namespace EarthLens.android
{
    public static class ChipAnalysisService
    {
        public static CategoryManager AnalyzeChip(Chip chip)
        {
            if (chip == null)
            {
                throw new ArgumentNullException();
            }

            try
            {
                // Change this to use Tensorflow.
                using (var mlMultiArray = chip.ToMLMultiArray())
                using (var model = new CoreMLModel())
                {
                    var categoryManager = model.Predict(mlMultiArray);

                    var observations = categoryManager.Observations;
                    var observationList = observations.ToList();
                    PostProcessingService.ProcessChipResults(chip.Region.Location, observationList);

                    return categoryManager;
                }
            }
            catch (FileNotFoundException)
            {
                return new CategoryManager();
            }
        }
    }
}
