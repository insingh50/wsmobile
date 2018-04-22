using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using System.Collections.Generic;
using Com.Syncfusion.Charts;
using System.Collections.ObjectModel;
using Android.Text;
using System;
using System.Net;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace XAM_Trial_1 {
	[Activity(Label = "XAM_Trial_1", /*Icon = "@mipmap/icon",*/ Theme = "@style/MainTheme")]
	public class MainActivity : Activity
	{
		static EditText searchInput;

		static TickModel searchTickData = new TickModel();
		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			Utils.changeToLowProfile(this);
			Window.DecorView.SetBackgroundColor(Color.White);

			SetContentView(Resource.Layout.Main);

			// set up doughnut chart
			SfChart posChart = FindViewById<SfChart>(Resource.Id.posChart);
			PositionModel positionData = new PositionModel();
			DoughnutSeries doughnut = new DoughnutSeries()
			{
				ItemsSource = positionData.CurrentPositions,
				DoughnutCoefficient = .75,
				CircularCoefficient = .8,
				XBindingPath = "Name",
				YBindingPath = "NetChange"
			};
			posChart.Series.Add(doughnut);


			// set up search ticker chart
			SfChart searchChart = FindViewById<SfChart>(Resource.Id.searchChart);
			searchChart.PrimaryAxis = new DateTimeAxis();
			searchChart.PrimaryAxis.ShowTrackballInfo = true;
			searchChart.PrimaryAxis.LabelStyle.LabelFormat = "hh:mm";
			searchChart.SecondaryAxis = new NumericalAxis()
			{
				Maximum = 30000,
			};
			TickModel searchTicks = new TickModel();
			HiLoOpenCloseSeries ohlc = new HiLoOpenCloseSeries()
			{
				ItemsSource = searchTicks.TickData,
				XBindingPath = "Date",
				Open = "Open",
				High = "High",
				Low = "Low",
				Close = "Close"
			};
			//searchChart.Series.Add(ohlc);
			AreaSeries volume = new AreaSeries()
			{
				ItemsSource = searchTickData.TickData,
				XBindingPath = "Date",
				YBindingPath = "Volume"
			};
			searchChart.Series.Add(volume);

			// make sure search ticker text is all caps
			searchInput = FindViewById<EditText>(Resource.Id.searchTicker);
			searchInput.SetFilters(new IInputFilter[] { new InputFilterAllCaps()});
			// change function of enter key in search ticker txtbox
			searchInput.KeyPress += OnSearchTickerBoxKeyPressAsync;
		}

		private async void OnSearchTickerBoxKeyPressAsync(object sender, View.KeyEventArgs e)
		{
			e.Handled = false;
			if(e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
			{
				try
				{
					// setting up webclient to reques
					WebClient client = new WebClient();
					var result = await client.DownloadStringTaskAsync("https://api.iextrading.com/1.0/stock/" + searchInput.Text + "/chart/1d?format=csv");
					//var settings = new JsonSerializerSettings()
					//{
					//	DateFormatString = "yyyyMMdd",
					//};
					//ObservableCollection<Tick> tickData = JsonConvert.DeserializeObject<ObservableCollection<Tick>>(result, settings);
					var listResult = result.Trim().Split(new char[] { '\r',  '\n' });
					for (int i = 1; i < listResult.Length; i++)
					{
						if (listResult[i] != "")
						{
							string[] tickDetails = listResult[i].Split(',');
							//if (i == 2)
							//{

							//}
							//else
							{
								searchTickData.TickData.Add(new Tick(tickDetails[0] + tickDetails[1], Double.Parse(tickDetails[3]), Double.Parse(tickDetails[3]), Double.Parse(tickDetails[4]), Double.Parse(tickDetails[4]), int.Parse(tickDetails[6])));
							}
							
						}
					}
					client.Dispose();
				}
				catch (FormatException formatE)
				{
					var toast = Toast.MakeText(this, formatE.Message, ToastLength.Short);
					toast.Show();
				}
				catch (WebException webE)
				{
					var toast = Toast.MakeText(this, webE.Message, ToastLength.Short);
					toast.Show();
				}

				e.Handled = true;
			}
		}

		public override void OnWindowFocusChanged(bool hasFocus) {
			base.OnWindowFocusChanged(hasFocus);

			if (hasFocus == true) Utils.changeToLowProfile(this);
		}
	}
}

