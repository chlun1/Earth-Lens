using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using System.Threading.Tasks;

namespace EarthLens.android
{
    [Activity(Label = "@string/app_name", Theme = "@style/NoBarTheme", MainLauncher = true)]
    public class SplashActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_splash);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnResume()
        {
            base.OnResume();
            Task.Run(() => LaunchImageUploadScreenAsync());
        }

        private async Task LaunchImageUploadScreenAsync()
        {
            await Task.Delay(SharedConstants.WelcomeSplashDelay);
            StartActivity(typeof(ImageUploadActivity));
        }
    }
}
