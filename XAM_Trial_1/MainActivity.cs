﻿using Android.App;
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
using System.Linq;
using System.Threading;
using Intrinio;
using Android.Views.Animations;
using Newtonsoft.Json.Linq;
using Android.Support.V7.Widget;

namespace XAM_Trial_1 {
	[Activity(Label = "Workstation Mobile", /*Icon = "@mipmap/icon",*/ Theme = "@style/MainTheme", NoHistory = true, LaunchMode = LaunchMode.SingleInstance)]
	public class MainActivity : Activity
	{
		static EditText searchInput;
		static TickModel searchTickData = new TickModel();
		static SfChart searchChart;
		//string[] positions;
		static readonly Timer timer;
		int currentEmptyAction = 0;
		//static string userID = LoginActivity.userID;
		static EditText search;
		static RealTimeClient client;
		private static Dictionary<string, string> lastPrices = new Dictionary<string, string>();

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Utils.ChangeToLowProfile(this);
			Window.DecorView.SetBackgroundColor(Color.White);

			SetContentView(Resource.Layout.Main);

			// SetUpBig3Async();

			//positions = Intent.GetStringArrayExtra("positions");
			//positions = positions.Select(s => s.ToUpper()).ToArray();
			//SetUpPositionsGrid();

			SetUpPositionsChart();

			SetUpSearchChartAsync();
			
			SetUpNewsAsync();

			if (Looper.MyLooper() == null)
				Looper.Prepare();

			string username = "941a5c8713d312df516fd773766dcc66";
			string password = "19c1b7cf9c716dc6b95892177577a730";
			QuoteProvider provider = QuoteProvider.IEX;

			client = new RealTimeClient(username, password, provider);
			QuoteHandler handler = new QuoteHandler();
			var djiaCurPrice = FindViewById<TextView>(Resource.Id.djiaCurPrice);
			var nasdaqCurPrice = FindViewById<TextView>(Resource.Id.nasdaqCurPrice);
			var searchPrice = FindViewById<TextView>(Resource.Id.searchPrice);
			search = FindViewById<EditText>(Resource.Id.searchTicker);
			handler.OnQuote += (IQuote quote) =>
			{
				var newquote = (IexQuote)quote;
				if (newquote.Ticker == search.Text) {
					if (decimal.Parse(searchPrice.Text) < newquote.Price)
						RunOnUiThread(() => { searchPrice.Text = newquote.Price.ToString(); searchPrice.SetTextColor(Color.DarkGreen); });
					else if (decimal.Parse(searchPrice.Text) > newquote.Price)
						RunOnUiThread(() => { searchPrice.Text = newquote.Price.ToString(); searchPrice.SetTextColor(Color.DarkRed); });
					lastPrices[search.Text] = searchPrice.Text;
				}
			};
			client.RegisterQuoteHandler(handler);
			client.Join(search.Text);
			client.Connect();

			// make sure search ticker text is all caps, and other formatting
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
			var autoevent = new AutoResetEvent(false);
			//timer = new Timer(MyTimerCallback, autoevent, 0, 500);

			//var myAnimation = AnimationUtils.LoadAnimation(this, Resource.Animation.MyAnimation);
			//searchPrice.StartAnimation(myAnimation);
		}

		//private void MyTimerCallback(object state)
		//{
		//	for (int i = 0; i < currentEmptyAction; i++)
		//	{
		//		actions[i].Invoke();
		//	}
		//}

		/// <summary>
		/// Retrieve and display news from S&P Dow Jones Indices - All Indices RSS Feed
		/// </summary>
		private async void SetUpNewsAsync()
		{
			LinearLayout newsView = FindViewById<LinearLayout>(Resource.Id.news);
			//var mLayoutManager = new LinearLayoutManager(this);
			//newsView.SetLayoutManager(mLayoutManager);

			var client = new WebClient();
			string retreivedNews = await client.DownloadStringTaskAsync(new Uri("https://newsapi.org/v2/top-headlines?country=us&category=business&apiKey=9dc668131a774b19beba0889446aaeeb"));

			JObject news = JObject.Parse(retreivedNews);

			// get JSON result objects in to a list
			List<JToken> results = news["articles"].Children().ToList();

			// serialize JSON results into .NET objects
			List<NewsApiArticle> newsResults = new List<NewsApiArticle>();
			foreach (JToken result in results)
			{
				// JToken.ToObject is a helper method that uses JsonSerializer internally
				NewsApiArticle searchResult = result.ToObject<NewsApiArticle>();
				newsResults.Add(searchResult);
			}
			foreach(NewsApiArticle article in newsResults)
			{
				TextView newsTextView = new TextView(this)
				{
					Text = article.Title,
					Clickable = true,
				};
				newsTextView.Click += (object sender, EventArgs e) => 
				{
					Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(article.Url));
					StartActivity(intent);
				};
				RunOnUiThread(() => newsView.AddView(newsTextView));
			}
		}

		#region SETUP
		/// <summary>
		/// Display user's tradier positions in grid format
		/// </summary>
		private void SetUpPositionsGrid()
		{
			GridLayout positionsGrid = FindViewById<GridLayout>(Resource.Id.positions);
			//positionsGrid.RowCount = positions.Length;
			//foreach(var position in positions)
			{
				var positionName = new TextView(this);
				//positionName.Text = position;
				FrameLayout.LayoutParams positionNameParams = new FrameLayout.LayoutParams(FrameLayout.LayoutParams.WrapContent, FrameLayout.LayoutParams.WrapContent);
				positionNameParams.SetMargins(0, 25, 0, 0);
				positionName.LayoutParameters = positionNameParams;

				var positionNetChange = new TextView(this) { Text = "numero" };

				RunOnUiThread(() => positionsGrid.AddView(positionName));
				RunOnUiThread(() => positionsGrid.AddView(positionNetChange));
			}
		}

		/// <summary>
		/// Display user's tradier position changes in doughnut chart format
		/// </summary>
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
			RunOnUiThread(() => posChart.Series.Add(doughnut));
		}

		/// <summary>
		/// Set up search ticker chart
		/// </summary>
		private async void SetUpSearchChartAsync()
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
			RunOnUiThread(() => searchChart.Series.Add(ohlc));

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
			await GetTickerChartData(Resources.GetString(Resource.String.default_search_ticker), "1d", searchTickData);
		}
		#endregion

		/// <summary>
		/// If 'Enter' is pressed, gets and displays intraday chart for the ticker typed in search box
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void OnSearchTickerBoxKeyPressAsync(object sender, View.KeyEventArgs e)
		{
			e.Handled = false;
			if(e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
			{
				try
				{
					await GetTickerChartData(searchInput.Text, "1d", searchTickData);
					//search = searchInput.Text;
					var searchPrice = FindViewById<TextView>(Resource.Id.searchPrice);
					if (lastPrices.ContainsKey(searchInput.Text))
					{
						RunOnUiThread(() => searchPrice.Text = lastPrices[searchInput.Text]);
					}
					client.LeaveAll();
					client.Join(searchInput.Text);
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

		/// <summary>
		/// Selects all text in search box
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSearchTickerBoxClick(object sender, EventArgs e)
		{
			var senderTextBox = sender as EditText;
			senderTextBox.SelectAll();
		}

		/// <summary>
		/// Retrieve IEX stock data based on timeframe given
		/// </summary>
		/// <param name="ticker">Stock ticker to search for</param>
		/// <param name="timeFrame">1d 5d 1m 3m 6m</param>
		/// <param name="tickModel"><see cref="TickModel"/> object to add data collection to</param>
		/// <returns></returns>

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