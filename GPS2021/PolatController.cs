using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using DonaDona.Device.GPS;
using static ScottPlot.Colors;

namespace GPS2021
{
	/// <summary>
	/// Interaction logic for PolarControl.xaml
	/// </summary>
	public partial class PolarControl : UserControl
	{
		Pen BlackPen;
		Pen GreyPen;
		Pen RedPen;
		Pen LimeGreenPen;
		double radius;
		Point center;
		Brush orangeBrush;

		List<SatelliteInView> satellitePositions = new List<SatelliteInView>();

		public PolarControl()
		{
			//InitializeComponent();

			Setup();
			
		}

		private void Setup()
		{
			LimeGreenPen = new Pen(new SolidColorBrush(Colors.LimeGreen), 1.0);
			LimeGreenPen.Freeze();
			GreyPen = new Pen(new SolidColorBrush(Colors.Black), 1.0);
			GreyPen.Freeze();
			BlackPen = new Pen(new SolidColorBrush(Colors.Black), 1.0);
			BlackPen.Freeze();
			RedPen = new Pen(new SolidColorBrush(Colors.Red), 1.0);
			RedPen.Freeze();
			orangeBrush = new SolidColorBrush(Colors.Orange);
			orangeBrush.Freeze();

		}

		private void DrawGrid(DrawingContext dc)
		{
			dc.DrawEllipse(null, BlackPen, center, radius, radius);

			// lets draw some lines at 10 degree intervals
			for (int x = 0; x < 360; x += 10)
			{
				int newx = x - 90;
				Point outer = new Point();
				outer.X = center.X + radius * Math.Cos((x * 2.0 * Math.PI) / 360.0);
				outer.Y = center.Y + radius * Math.Sin((x * 2.0 * Math.PI) / 360.0);
				dc.DrawLine(BlackPen, center, outer);

				// Create the initial formatted text string.
				FormattedText formattedText = new FormattedText(
					x.ToString(),
					CultureInfo.GetCultureInfo("en-us"),
					FlowDirection.LeftToRight,
					new Typeface("Verdana"),
					12,
					Brushes.Black);

				outer.X = center.X + 1.1 * radius * Math.Cos((newx * 2.0 * Math.PI) / 360.0) - formattedText.Width / 2;
				outer.Y = center.Y + 1.1 * radius * Math.Sin((newx * 2.0 * Math.PI) / 360.0) - formattedText.Height / 2;


				dc.DrawText(formattedText, outer);
			}

			// draw the range rings
			for (int x = 0; x < 90; x += 10)
			{
				Point outer = new Point();
				double newRadius;
				newRadius = radius * Math.Cos((2.0 * x * Math.PI) / 360.0);
				dc.DrawEllipse(null, LimeGreenPen, center, newRadius, newRadius);

				// Create the initial formatted text string.
				FormattedText formattedText = new FormattedText(
					(90 - x).ToString(),
					CultureInfo.GetCultureInfo("en-us"),
					FlowDirection.LeftToRight,
					new Typeface("Verdana"),
					8,
					Brushes.Black);

				outer.X = center.X - formattedText.Width / 2;
				outer.Y = center.Y - radius * Math.Cos((x * 2.0 * Math.PI) / 360.0) - formattedText.Height / 2;


				dc.DrawText(formattedText, outer);

			}
		}

		/// <summary>
		/// actually draw the circle
		/// </summary>
		/// <param name="dc"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		private void DrawCircle(DrawingContext dc, double x, double y)
		{
			Point p = new Point(center.X + x, center.Y + y);
			dc.DrawEllipse(orangeBrush, BlackPen, p, 10, 10);
		}

		/// <summary>
		/// givenb our point calculate the x and y coords for it
		/// </summary>
		/// <param name="dc"></param>
		/// <param name="azimuth"></param>
		/// <param name="elevation"></param>
		private void CircleScaling(DrawingContext dc, double azimuth, double elevation)
		{
			double RadiusSat = 100.0;
			elevation = 90 - elevation;

			// toDOI still need a proper radius calculation
			RadiusSat = radius * Math.Cos((2.0 * elevation * Math.PI) / 360.0);


			Point loc = new Point();

			double tmpazimuth;
			tmpazimuth = azimuth - 90;
			azimuth -= 90;

			loc.X = RadiusSat * Math.Cos((azimuth * 2.0 * Math.PI) / 360.0);
			loc.Y = RadiusSat * Math.Sin((azimuth * 2.0 * Math.PI) / 360.0);

			DrawCircle(dc, loc.X, loc.Y);

			var s = satellitePositions;
		}

		/// <summary>
		/// givenb our point calculate the x and y coords for it
		/// </summary>
		/// <param name="dc"></param>
		/// <param name="azimuth"></param>
		/// <param name="elevation"></param>
		private void CircleScaling(DrawingContext dc, SatelliteInView sat)
		{
			// ADD the position of the new sat so we can redraw it on a refresh
			// satellitePositions.Add( sat );

			double RadiusSat = 100.0;
			double elevation = 90 - sat.Elevation;

			// toDOI still need a proper radius calculation
			RadiusSat = radius * Math.Cos((2.0 * elevation * Math.PI) / 360.0);


			Point loc = new Point();

			double tmpazimuth = sat.Azimuth - 90;
			//sat.azimuth -= 90;

			loc.X = RadiusSat * Math.Cos((sat.Azimuth * 2.0 * Math.PI) / 360.0);
			loc.Y = RadiusSat * Math.Sin((sat.Azimuth * 2.0 * Math.PI) / 360.0);

			DrawCircle(dc, loc.X, loc.Y);

			var s = satellitePositions;
		}

		/// <summary>
		/// delete the history 
		/// </summary>
		public void DeleteHistory()
		{
			// delete the memory
			satellitePositions.Clear();

			// redraw - calls on Render effectively
			this.InvalidateVisual();
		}

		public void AddSat(SatelliteInView pos)
		{
			satellitePositions.Add(pos);
		}

		/// <summary>
		/// This is where the draw command lands in the control.
		/// </summary>
		/// <param name="dc"></param>
		protected override void OnRender(DrawingContext dc)
		{
			

			// Draw All
			lock (satellitePositions)
			{
				foreach (SatelliteInView sat in satellitePositions)
				{
					CircleScaling(dc, sat);
				}
			}

			// in the test program design these values are fixed.
			
			
				Console.WriteLine("this.ActualWidth / 2 = " + this.ActualWidth / 2 + ", this.ActualHeight / 2 = " + this.ActualHeight / 2);
				center = new Point(this.ActualWidth / 2, this.ActualHeight / 2);
				radius = 2 * this.ActualWidth / 10;
			

			// draw the grid
			DrawGrid(dc);

			//// create a test satelite position
			//SatellitePosition test = new SatellitePosition();
			//test.azimuth = 135;
			//test.elevation = 50;

			//// and dispay it
			//CircleScaling(dc, test);

			// call the base rendering, needed by default
			base.OnRender(dc);
			Console.WriteLine("this.ActualWidth / 2 = " + this.ActualWidth / 2 + ", this.ActualHeight / 2 = " + this.ActualHeight / 2);
		}
	}
}
