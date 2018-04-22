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
using Android.Views.InputMethods;
using System.Threading.Tasks;

namespace XAM_Trial_1 {
	[Activity(Label = "XAM_Trial_1", /*Icon = "@mipmap/icon",*/ Theme = "@style/MainTheme")]
	public class MainActivity : Activity
	{
		static EditText searchInput;
		static TickModel searchTickData = new TickModel();
		static SfChart searchChart;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Utils.changeToLowProfile(this);
			Window.DecorView.SetBackgroundColor(Color.White);

			SetContentView(Resource.Layout.Main);

			SetUpPositionsChart();

			SetUpSearchChart();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			getTickerChartData(Resources.GetString(Resource.String.default_search_ticker), "1d", searchTickData);
#pragma warning restore CS4014

			// make sure search ticker text is all caps
			searchInput = FindViewById<EditText>(Resource.Id.searchTicker);
			IInputFilter[] editFilters = searchInput.GetFilters();
			IInputFilter[] newFilters = new IInputFilter[editFilters.Length + 1];
			Array.Copy(editFilters, 0, newFilters, 0, editFilters.Length);
			newFilters[editFilters.Length] = new InputFilterAllCaps();
			searchInput.SetFilters(newFilters);
			searchInput.InputType |= InputTypes.TextFlagNoSuggestions | InputTypes.TextVariationVisiblePassword;

			// change function of enter key in search ticker txtbox
			searchInput.KeyPress += OnSearchTickerBoxKeyPressAsync;

			// hide keyboard when touching outside of search ticker box
		}


		private void SetUpPositionsChart()
		{
			// set up doughnut chart
			SfChart posChart = FindViewById<SfChart>(Resource.Id.posChart);
			PositionModel positionData = new PositionModel();
			DoughnutSeries doughnut = new DoughnutSeries()
			{
				ItemsSource = positionData.CurrentPositions,
				DoughnutCoefficient = .75,
				CircularCoefficient = .8,
				XBindingPath = "Name",
				YBindingPath = "NetChange",
				EnableAnimation = true,
				AnimationDuration = 1,
				ExplodeAll = true,
				ExplodeRadius = .8,
			};
			doughnut.ColorModel.ColorPalette = ChartColorPalette.Custom;
			doughnut.ColorModel.CustomColors = new List<Color>() { Color.DarkRed, Color.DarkGreen};
			posChart.Series.Add(doughnut);
		}

		private void SetUpSearchChart()
		{
			// set up search ticker chart
			searchChart = FindViewById<SfChart>(Resource.Id.searchChart);
			ChartZoomPanBehavior zoomPanBehavior = new ChartZoomPanBehavior() {
				ZoomingEnabled = true,
				DoubleTapEnabled = true,
				MaximumZoomLevel = 3,
			};
			searchChart.Behaviors.Add(zoomPanBehavior);

			ChartTrackballBehavior trackballBehavior = new ChartTrackballBehavior
			{
				ShowLabel = true,
				ShowLine = true,
				LabelDisplayMode = TrackballLabelDisplayMode.GroupAllPoints,
			};
			trackballBehavior.LabelStyle.TextColor = Color.WhiteSmoke;
			trackballBehavior.LabelStyle.TextSize = 10;
			trackballBehavior.LabelStyle.BackgroundColor = Color.Black;
			searchChart.Behaviors.Add(trackballBehavior);

			searchChart.PrimaryAxis = new DateTimeAxis();
			searchChart.PrimaryAxis.LabelStyle.LabelFormat = "hh:mm";
			searchChart.PrimaryAxis.ShowTrackballInfo = true;
			searchChart.PrimaryAxis.TrackballLabelStyle.TextColor = Color.WhiteSmoke;
			searchChart.PrimaryAxis.TrackballLabelStyle.TextSize = 10;
			searchChart.PrimaryAxis.TrackballLabelStyle.BackgroundColor = Color.Black;

			searchChart.SecondaryAxis = new NumericalAxis();
			//searchChart.SecondaryAxis.ShowTrackballInfo = true;
			//searchChart.SecondaryAxis.TrackballLabelStyle.BackgroundColor = Color.LightGray;

			HiLoOpenCloseSeries ohlc = new HiLoOpenCloseSeries()
			{
				ItemsSource = { },
				XBindingPath = "Date",
				Open = "Open",
				High = "High",
				Low = "Low",
				Close = "Close",
			};

			//searchChart.Series.Add(ohlc);
			AreaSeries volume = new AreaSeries()
			{
				ItemsSource = searchTickData.TickData,
				XBindingPath = "Date",
				YBindingPath = "Volume",
				EnableAnimation = true,
				AnimationDuration = 1,
				ShowTrackballInfo = true,
				Color = Color.Black
			};
			searchChart.Series.Add(volume);
		}

		private async void OnSearchTickerBoxKeyPressAsync(object sender, View.KeyEventArgs e)
		{
			e.Handled = false;
			if(e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
			{
				try
				{
					await getTickerChartData(searchInput.Text, "1d", searchTickData);
				}
				catch (FormatException formatE)
				{
					var toast = Toast.MakeText(this, formatE.Message, ToastLength.Short);
					toast.Show();
				}
				catch (WebException)
				{
					var toast = Toast.MakeText(this, "Invalid Symbol", ToastLength.Short);
					toast.Show();
				}

				e.Handled = true;
			}
		}
		
		private static async Task getTickerChartData(string ticker, string timeFrame, TickModel tickModel)
		{
			//tickModel.TickData.Clear();
			tickModel = null;
			//foreach (XyDataSeries series in searchChart.Series)
			//{
			//	series.SuspendNotification();
			//}
			// setting up webclient to request
			WebClient client = new WebClient();
			var result = await client.DownloadStringTaskAsync("https://api.iextrading.com/1.0/stock/" + ticker + "/chart/" + timeFrame + "?format=csv");
			TickModel newTickModel = new TickModel();
			//var settings = new JsonSerializerSettings()
			//{
			//	DateFormatString = "yyyyMMdd",
			//};
			//ObservableCollection<Tick> tickData = JsonConvert.DeserializeObject<ObservableCollection<Tick>>(result, settings);
			var listResult = result.Trim().Split(new char[] { '\r', '\n' });
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
						newTickModel.TickData.Add(new Tick(tickDetails[0] + tickDetails[1], Double.Parse(tickDetails[3]), Double.Parse(tickDetails[3]), Double.Parse(tickDetails[4]), Double.Parse(tickDetails[4]), int.Parse(tickDetails[6])));
					}
				}
			}
			foreach (XyDataSeries series in searchChart.Series)
			{
				//series.ResumeNotification();
				series.ItemsSource = newTickModel.TickData;
			}
			client.Dispose();
		}

		public override void OnWindowFocusChanged(bool hasFocus) {
			base.OnWindowFocusChanged(hasFocus);

			if (hasFocus == true) Utils.changeToLowProfile(this);
		}
	}
}

