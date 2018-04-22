using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace XAM_Trial_1
{
	public class Position
	{
		public string Name { get; set; }
		public double NetChange { get; set; }
		//public double PercentChange { get; set; }
		public Position(string name, double netChange/*, double percentChange*/)
		{
			this.Name = name;
			this.NetChange = netChange;
			//this.PercentChange = percentChange;
		}
	}
	public class PositionModel
	{
		public ObservableCollection<Position> CurrentPositions { get; set; }

		public PositionModel()
		{
			CurrentPositions = new ObservableCollection<Position>
			{
				new Position("AAPL", 50.00/*, 8.76*/),
				new Position("FB", 24.00/*, 3.65*/),
				new Position("TSLA", 15.00/*, 1.15*/),
				new Position("NVDA", 5.00/*, 0.84*/)
			};
		}
	}

	public class Tick
	{
		//[JsonProperty("last_name")]
		[JsonConverter(typeof(CustomDateTimeConverter))]
		public DateTime Date { get; set; }
		public double Open { get; set; }
		public double High { get; set; }
		public double Low { get; set; }
		public double Close { get; set; }
		public int Volume { get; set; }
		public Color Color { get; set; }
		public Tick(string date, double open, double high, double low, double close, int volume)
		{
			this.Date = !date.Contains('-') ? DateTime.ParseExact(date, "yyyyMMddHH:mm", CultureInfo.InvariantCulture) : DateTime.ParseExact(date, "yyyy-MM-ddHH:mm", CultureInfo.InvariantCulture);
			this.Open = open;
			this.High = high;
			this.Low = low;
			this.Close = close;
			this.Volume = volume;
			this.Color = Color.Red;
		}
	}
	public class TickModel
	{
		public ObservableCollection<Tick> TickData { get; set; }

		public TickModel()
		{
			TickData = new ObservableCollection<Tick>();
		}
	}
	
	class CustomDateTimeConverter : IsoDateTimeConverter
	{
		public CustomDateTimeConverter()
		{
			base.DateTimeFormat = "yyyyMMddHH:mm";
		}
	}
}