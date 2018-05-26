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
using System.Data.SqlClient;

namespace XAM_Trial_1 {
	[Activity(Label = "Main Activity", /*Icon = "@mipmap/icon",*/ Theme = "@style/MainTheme", NoHistory = true)]
	public class MainActivity : Activity
	{
		static EditText searchInput;
		static TickModel searchTickData = new TickModel();
		static SfChart searchChart;
		//static string userID = LoginActivity.userID;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Utils.ChangeToLowProfile(this);
			Window.DecorView.SetBackgroundColor(Color.White);

			SetContentView(Resource.Layout.Main);

			// SetUpBig3Async();

			SetUpPositionsChart();

			SetUpSearchChart();
			
			SetUpNews();

			// make sure search ticker text is all caps
			searchInput = FindViewById<EditText>(Resource.Id.searchTicker);
			IInputFilter[] editFilters = searchInput.GetFilters();
			IInputFilter[] newFilters = new IInputFilter[editFilters.Length + 1];
			Array.Copy(editFilters, 0, newFilters, 0, editFilters.Length);
			newFilters[editFilters.Length] = new InputFilterAllCaps();
			searchInput.SetFilters(newFilters);
			searchInput.InputType |= InputTypes.TextFlagNoSuggestions | InputTypes.TextVariationVisiblePassword | InputTypes.TextFlagCapCharacters;
			searchInput.SetSelectAllOnFocus(true);

			// change function of enter key in search ticker txtbox
			searchInput.KeyPress += OnSearchTickerBoxKeyPressAsync;
			searchInput.Click += OnSearchTickerBoxClick;

			// TODO - hide keyboard when touching outside of search ticker box

		}

		

		private void SetUpNews()
		{
			FeedManager feedManager = new FeedManager();
			//feedManager.GetFeedItems("https://us.spindices.com/rss/rss-details/?rssFeedName=all-indices");

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
				AnimationDuration = .7,
				ExplodeAll = true,
				ExplodeRadius = .8,
				SmartLabelsEnabled = true,
				ConnectorType = ConnectorType.Bezier,
				DataMarkerPosition = CircularSeriesDataMarkerPosition.OutsideExtended,
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
			searchChart.PrimaryAxis.TickPosition = AxisElementPosition.Inside;

			searchChart.SecondaryAxis = new NumericalAxis();
			//searchChart.SecondaryAxis.ShowTrackballInfo = true;
			//searchChart.SecondaryAxis.TrackballLabelStyle.BackgroundColor = Color.LightGray;
			//searchChart.SuspendSeriesNotification();
			searchChart.HorizontalScrollBarEnabled = true;
			searchChart.SecondaryAxis.TickPosition = AxisElementPosition.Inside;

			HiLoOpenCloseSeries ohlc = new HiLoOpenCloseSeries()
			{
				ItemsSource = searchTickData.TickData,
				XBindingPath = "Date",
				Open = "Open",
				High = "High",
				Low = "Low",
				Close = "Close",
				EnableAnimation = true,
				AnimationDuration = 3,
				ShowTrackballInfo = true,
				StrokeWidth = 1,
			};
			searchChart.Series.Add(ohlc);

			AreaSeries volume = new AreaSeries()
			{
				ItemsSource = searchTickData.TickData,
				XBindingPath = "Date",
				YBindingPath = "Volume",
				//EnableAnimation = true,
				//AnimationDuration = 1,
				ShowTrackballInfo = true,
				//StrokeWidth = 3,
			};
			//searchChart.Series.Add(volume);
			GetTickerChartData(Resources.GetString(Resource.String.default_search_ticker), "1d", searchTickData);
		}

		private async void OnSearchTickerBoxKeyPressAsync(object sender, View.KeyEventArgs e)
		{
			e.Handled = false;
			if(e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
			{
				try
				{
					await GetTickerChartData(searchInput.Text, "1d", searchTickData);
					var senderTextBox = sender as EditText;
					senderTextBox.ClearFocus();
					InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
					imm.HideSoftInputFromWindow(senderTextBox.WindowToken, 0);
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

		private void OnSearchTickerBoxClick(object sender, EventArgs e)
		{
			var senderTextBox = sender as EditText;
			senderTextBox.SelectAll();
		}

		private static async Task GetTickerChartData(string ticker, string timeFrame, TickModel tickModel)
		{
			tickModel.TickData.Clear();
			//tickModel = null;
			//foreach (XyDataSeries series in searchChart.Series)
			//{
			//	series.SuspendNotification();
			//}
			// setting up webclient to request
			WebClient client = new WebClient();
			var result = await client.DownloadStringTaskAsync("https://api.iextrading.com/1.0/stock/" + ticker + "/chart/" + timeFrame + "?format=csv");
			//var settings = new JsonSerializerSettings()
			//{
			//	DateFormatString = "yyyyMMdd",
			//};
			//ObservableCollection<Tick> tickData = JsonConvert.DeserializeObject<ObservableCollection<Tick>>(result, settings);
			var listResult = result.Trim().Split(new char[] { '\r', '\n' });
			int j = 0;
			for (int i = 1; i < listResult.Length; i++)
			{
				if (listResult[i] != "")
				{
					if (/*j++ % 5 == 0*/true)
					{
						string[] tickDetails = listResult[i].Split(',');
						//if (i == 2)
						//{

						//}
						//else
						if (tickDetails[3] != "-1")
						{
							tickModel.TickData.Add(new Tick(tickDetails[0] + tickDetails[1], tickDetails[15] != "-1" ? Double.Parse(tickDetails[15]) : 40, tickDetails[3] != "-1" ? Double.Parse(tickDetails[3]) : 40 , tickDetails[4] != "-1" ? Double.Parse(tickDetails[4]) : 40, tickDetails[16] != "-1" ? Double.Parse(tickDetails[16]) : 40, int.Parse(tickDetails[6])));
						}
					}
				}
			}
			//foreach (XyDataSeries series in searchChart.Series)
			//{
			//	series.ItemsSource = newTickModel.TickData;
			//	series.ResumeNotification();
			//}
			client.Dispose();
		}

		public override void OnWindowFocusChanged(bool hasFocus) {
			base.OnWindowFocusChanged(hasFocus);

			if (hasFocus == true) Utils.ChangeToLowProfile(this);
		}
	}
}

