using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace XAM_Trial_1
{
	[Activity(Label = "Login Activity", /*Icon = "@mipmap/icon",*/ Theme = "@style/MainTheme", NoHistory = true)]
	public class LoginActivity : Activity
	{
		public static string userID;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			StartLoginActivity();
		}

		async void StartLoginActivity()
		{
			await Task.Run(() => {
				Task.Delay(3000);
				StartActivity(new Intent(this, typeof(LoginActivity)));
			});
		}
	}
}