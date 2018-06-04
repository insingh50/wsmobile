using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace XAM_Trial_1 {
	[Activity(Label = "Workstation Mobile", Theme = "@style/SplashTheme", MainLauncher = true, NoHistory = true, LaunchMode = LaunchMode.SingleInstance)]
	public class SplashScreen : Activity {
		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);

			// Create your application here
			StartLoginActivity();
		}
		protected override void OnResume()
		{
			base.OnResume();
			StartLoginActivity();
		}

		async void StartLoginActivity() {
			await Task.Run(() => {
				Task.Delay(3000);
				StartActivity(new Intent(this, typeof(MainActivity/*LoginActivity*/)));
			});
		}
	}
}