﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace XAM_Trial_1 {
	class Utils {

		public static void changeToLowProfile(Activity activity) {
			View decorView = activity.Window.DecorView;
			var uiOptions = (int)decorView.SystemUiVisibility;
			var newUiOptions = (int)uiOptions;

			newUiOptions |= (int)SystemUiFlags.LowProfile;

			decorView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
		}

	}
}