using Demo.WindowsPresentation.CustomMarkers;
using DonaDona.Device.GPS;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// refer https://github.com/amezcua/GPS-NMEA-Parser/tree/master

namespace GPS2021
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort _serialPort=null;

        NmeaInterpreter nmea = new NmeaInterpreter();

        DateTime start;

		Pen BlackPen;
		Pen GreyPen;
		Pen RedPen;
		Pen LimeGreenPen;
		double radius;
		Point center;
		Brush orangeBrush;

		List<SatelliteInView> satellitePositions = new List<SatelliteInView>();

		public MainWindow()
        {
            InitializeComponent();

            GetSerials();



            nmea.PositionReceived += Nmea_PositionReceived;
            this.Closing += MainWindow_Closing;
            //Setup();
         }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (logFile != null)
            {
                logFile.Flush();
                logFile.Close();
            }
        }

        private void Nmea_PositionReceived(string latitude, string longitude)
        {
            this.Dispatcher.Invoke(() =>
            {
                var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);

                nmea.GPSTime += offset;

                lati.Text = latitude;
                lati_v.Text = nmea.latitude.ToString();

                longi.Text = longitude;
                longi_v.Text = nmea.longitude.ToString();

                gpsTime.Text = nmea.GPSTime.ToLongTimeString();
                gpsDate.Text = nmea.GPSTime.ToLongDateString();

               

                if (nmea.Gpgga != null)
                {
                    spaceVehicles.Text = nmea.Gpgga.NumberOfSatellites.ToString();

                    Altitude.Text = nmea.Gpgga.Altitude.ToString();
                    GeoidalSeparation.Text = nmea.Gpgga.GeoidalSeparation.ToString();
                    double total = nmea.Gpgga.Altitude + nmea.Gpgga.GeoidalSeparation;
                    TotalHeight.Text = total.ToString();
                }

                if (nmea.initialised == true)
                {
                   // Console.WriteLine("SATs 2 =" + nmea.gsvParser.output.Count);
                    DrawBarGraph();
                    smallveh.Text = nmea.getNumSattelites().ToString();
                }

                // do the bar braph

                AddMarker( nmea.latitude, nmea.longitude, nmea.GPSTime.ToLongTimeString() );
            });
        }

        public void DrawBarGraph()
        {
            WpfPlot1.Plot.Clear();

            double[] value = new double[nmea.getNumSattelites()];

            int count = 0;
            foreach (SatelliteInView s in nmea.getSattelites())
            {
                value[count] = s.SatelliteID;
            }


            //double[] values = { 5, 10, 7, 13 };
            var barPlot = WpfPlot1.Plot.Add.Bars(value);

            // define the content of labels
            foreach (var bar in barPlot.Bars)
            {
                bar.Label = bar.Value.ToString();
            }

            // customize label style
            barPlot.ValueLabelStyle.Bold = true;
            barPlot.ValueLabelStyle.FontSize = 18;

            WpfPlot1.Plot.Axes.Margins(bottom: 0, top: .2);

            WpfPlot1.Refresh();

        }

        private void onStart(object sender, RoutedEventArgs e)
        {
            String myport = (string)Serials.SelectedItem;

            if (_serialPort == null)
            { 
            doButton.Content = "Stop";

            // serttings that work for my GPS
            _serialPort = new SerialPort( myport, 9600, Parity.None, 8, StopBits.One);
            _serialPort.Handshake = Handshake.None;
       

            _serialPort.Open();
        string io = _serialPort.ReadExisting();
				AppendText(RTB, io, "Red", FontWeights.Bold);
				_serialPort.DiscardInBuffer();
        _serialPort.DiscardOutBuffer();


				_serialPort.DataReceived += _serialPort_DataReceived;

            string str = _serialPort.NewLine;
        }
        else
        {
                _serialPort.Close();
                    Thread.Sleep(1000);
                _serialPort = null;
                doButton.Content = "Start";
            }
        }
     

    private void GetSerials()
        {
            // Get a list of serial port names.
            string[] ports = SerialPort.GetPortNames();

            Console.WriteLine("The following serial ports were found:");

            // Display each port name to the console.
            foreach (string port in ports)
            {
                Console.WriteLine(port);
            }

            Serials.ItemsSource = ports;
            Serials.SelectedIndex = 0;
            
        }

        string DataToPorcess;

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DataToPorcess = _serialPort.ReadLine();
            // Console.WriteLine(data);

            Thread myThread = new Thread(new ThreadStart( DoProcessing ));
            myThread.Start();

            //DoProcessing(data);

        }

        private void DoProcessing()
        {
            string data = DataToPorcess;
            nmea.Parse(data);

            this.Dispatcher.Invoke(() =>
            {

                // Console.WriteLine("Logged: " + data);
                outToLog(data);
                //AppendText(RTB, data, "Purple", FontWeights.Bold);
            });
        }

        private void onTestBtn(object sender, RoutedEventArgs e)
        {
            outToLog("Test" + test.ToString() + "\r" );
            test++;

            AppendText(RTB, "My text", "CornflowerBlue", FontWeights.Bold);
        }

        int test = 0;

        void outToLog(string output)
        {
            AppendText(RTB, output, "Purple", FontWeights.Bold);
            //RTB.AppendText( output, "Purple", FontWeights.Bold);
            RTB.ScrollToEnd();
        }

        void AppendText( RichTextBox RTB, string text, string col , FontWeight fw)
        {
            BrushConverter bc = new BrushConverter();
            TextRange tr = new TextRange(RTB.Document.ContentEnd, RTB.Document.ContentEnd);
            tr.Text = text;
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                    bc.ConvertFromString(col));

                tr.ApplyPropertyValue(TextElement.FontWeightProperty, fw);
            }
            catch (FormatException) { }
        }

        private void mapView_Loaded(object sender, RoutedEventArgs e)
        {
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
            MainMap.CacheLocation = "C:\\mapcache";
            // choose your provider here
            MainMap.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
            MainMap.MinZoom = 2;
            MainMap.MaxZoom = 17;
            // whole world zoom
            MainMap.Zoom = 2;
            // lets the map use the mousewheel to zoom
            MainMap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            // lets the user drag the map
            MainMap.CanDragMap = true;
            // lets the user drag the map with the left mouse button
            MainMap.DragButton = MouseButton.Left;
        }

        private void btnAddMarker(object sender, RoutedEventArgs e)
        {
            AddMarker(nmea.latitude, nmea.longitude, "Test" );
        }

        private void AddMarker( double lat, double lng , string str)
        {
            GMapMarker currentMarker;
            PointLatLng point = new PointLatLng(lat, lng);

            // set current marker
            currentMarker = new GMapMarker(point);
            {
                currentMarker.Shape = new CustomMarkerRed(this, currentMarker, str);
                currentMarker.Offset = new Point(-15, -15);
                currentMarker.ZIndex = int.MaxValue;
                MainMap.Markers.Add(currentMarker);
            }
        }

        private void mapMouseMoved(object sender, MouseEventArgs e)
        {
            Point mousePosition = this.PointToScreen(Mouse.GetPosition(MainMap));

            PointLatLng pt = MainMap.FromLocalToLatLng((int)Mouse.GetPosition(MainMap).X, (int)Mouse.GetPosition(MainMap).Y);

           // Console.WriteLine( pt.Lat + " " + pt.Lng );

            StatusText.Content = pt.Lat.ToString("F4") + " " + pt.Lng.ToString("F4");

        }

        StreamWriter logFile=null;

        private void btnStartLogging(object sender, RoutedEventArgs e)
        {
            logFile = new StreamWriter("logfile.txt");
        }

        private void btnProcessSavedFile(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Document"; // Default file name
            dialog.DefaultExt = ".txt"; // Default file extension
            dialog.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                const Int32 BufferSize = 128;
                using (var fileStream = File.OpenRead(dialog.FileName))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        // Process line
                        AppendText(RTB, line, "Red", FontWeights.Bold);
                        if (line != string.Empty)
                        {
                            nmea.Parse(line);
                        }
                    }
                }
            }


        }

		private void MainMap_MouseDown(object sender, MouseButtonEventArgs e)
		{
            start = DateTime.Now;
		}

		private void MainMap_MouseUp(object sender, MouseButtonEventArgs e)
		{
			DateTime end = DateTime.Now;
			if ((end - start).TotalSeconds < 0.1)
            {
				PointLatLng pt = MainMap.FromLocalToLatLng((int)Mouse.GetPosition(MainMap).X, (int)Mouse.GetPosition(MainMap).Y);
				AddMarker(pt.Lat, pt.Lng, String.Format("{0:0.0000}", pt.Lat) + ", " + String.Format("{0:0.0000}", pt.Lng));
			}
			
		}

		



		//private void Setup()
		//{
		//	LimeGreenPen = new Pen(new SolidColorBrush(Colors.LimeGreen), 1.0);
		//	LimeGreenPen.Freeze();
		//	GreyPen = new Pen(new SolidColorBrush(Colors.Black), 1.0);
		//	GreyPen.Freeze();
		//	BlackPen = new Pen(new SolidColorBrush(Colors.Black), 1.0);
		//	BlackPen.Freeze();
		//	RedPen = new Pen(new SolidColorBrush(Colors.Red), 1.0);
		//	RedPen.Freeze();
		//	orangeBrush = new SolidColorBrush(Colors.Orange);
		//	orangeBrush.Freeze();

		//}

		//private void DrawGrid(DrawingContext dc)
		//{
		//	dc.DrawEllipse(null, BlackPen, center, radius, radius);

		//	// lets draw some lines at 10 degree intervals
		//	for (int x = 0; x < 360; x += 10)
		//	{
		//		int newx = x - 90;
		//		Point outer = new Point();
		//		outer.X = center.X + radius * Math.Cos((x * 2.0 * Math.PI) / 360.0);
		//		outer.Y = center.Y + radius * Math.Sin((x * 2.0 * Math.PI) / 360.0);
		//		dc.DrawLine(BlackPen, center, outer);

		//		// Create the initial formatted text string.
		//		FormattedText formattedText = new FormattedText(
		//			x.ToString(),
		//			CultureInfo.GetCultureInfo("en-us"),
		//			FlowDirection.LeftToRight,
		//			new Typeface("Verdana"),
		//			12,
		//			Brushes.Black);

		//		outer.X = center.X + 1.1 * radius * Math.Cos((newx * 2.0 * Math.PI) / 360.0) - formattedText.Width / 2;
		//		outer.Y = center.Y + 1.1 * radius * Math.Sin((newx * 2.0 * Math.PI) / 360.0) - formattedText.Height / 2;


		//		dc.DrawText(formattedText, outer);
		//	}

		//	// draw the range rings
		//	for (int x = 0; x < 90; x += 10)
		//	{
		//		Point outer = new Point();
		//		double newRadius;
		//		newRadius = radius * Math.Cos((2.0 * x * Math.PI) / 360.0);
		//		dc.DrawEllipse(null, LimeGreenPen, center, newRadius, newRadius);

		//		// Create the initial formatted text string.
		//		FormattedText formattedText = new FormattedText(
		//			(90 - x).ToString(),
		//			CultureInfo.GetCultureInfo("en-us"),
		//			FlowDirection.LeftToRight,
		//			new Typeface("Verdana"),
		//			8,
		//			Brushes.Black);

		//		outer.X = center.X - formattedText.Width / 2;
		//		outer.Y = center.Y - radius * Math.Cos((x * 2.0 * Math.PI) / 360.0) - formattedText.Height / 2;


		//		dc.DrawText(formattedText, outer);

		//	}
		//}

		///// <summary>
		///// actually draw the circle
		///// </summary>
		///// <param name="dc"></param>
		///// <param name="x"></param>
		///// <param name="y"></param>
		//private void DrawCircle(DrawingContext dc, double x, double y)
		//{
		//	Point p = new Point(center.X + x, center.Y + y);
		//	dc.DrawEllipse(orangeBrush, BlackPen, p, 10, 10);
		//}

		///// <summary>
		///// givenb our point calculate the x and y coords for it
		///// </summary>
		///// <param name="dc"></param>
		///// <param name="azimuth"></param>
		///// <param name="elevation"></param>
		//private void CircleScaling(DrawingContext dc, double azimuth, double elevation)
		//{
		//	double RadiusSat = 100.0;
		//	elevation = 90 - elevation;

		//	// toDOI still need a proper radius calculation
		//	RadiusSat = radius * Math.Cos((2.0 * elevation * Math.PI) / 360.0);


		//	Point loc = new Point();

		//	double tmpazimuth;
		//	tmpazimuth = azimuth - 90;
		//	azimuth -= 90;

		//	loc.X = RadiusSat * Math.Cos((azimuth * 2.0 * Math.PI) / 360.0);
		//	loc.Y = RadiusSat * Math.Sin((azimuth * 2.0 * Math.PI) / 360.0);

		//	DrawCircle(dc, loc.X, loc.Y);

		//	var s = satellitePositions;
		//}

		///// <summary>
		///// givenb our point calculate the x and y coords for it
		///// </summary>
		///// <param name="dc"></param>
		///// <param name="azimuth"></param>
		///// <param name="elevation"></param>
		//private void CircleScaling(DrawingContext dc, SatelliteInView sat)
		//{
		//	// ADD the position of the new sat so we can redraw it on a refresh
		//	// satellitePositions.Add( sat );

		//	double RadiusSat = 100.0;
		//	double elevation = 90 - sat.Elevation;

		//	// toDOI still need a proper radius calculation
		//	RadiusSat = radius * Math.Cos((2.0 * elevation * Math.PI) / 360.0);


		//	Point loc = new Point();

		//	double tmpazimuth = sat.Azimuth - 90;
		//	//sat.azimuth -= 90;

		//	loc.X = RadiusSat * Math.Cos((sat.Azimuth * 2.0 * Math.PI) / 360.0);
		//	loc.Y = RadiusSat * Math.Sin((sat.Azimuth * 2.0 * Math.PI) / 360.0);

		//	DrawCircle(dc, loc.X, loc.Y);

		//	var s = satellitePositions;
		//}

		///// <summary>
		///// delete the history 
		///// </summary>
		//public void DeleteHistory()
		//{
		//	// delete the memory
		//	satellitePositions.Clear();

		//	// redraw - calls on Render effectively
		//	this.InvalidateVisual();
		//}

		//public void AddSat(SatelliteInView pos)
		//{
		//	satellitePositions.Add(pos);
		//}

		///// <summary>
		///// This is where the draw command lands in the control.
		///// </summary>
		///// <param name="dc"></param>
		//protected override void OnRender(DrawingContext dc)
		//{
		//	DrawGrid(dc);

		//	// Draw All
		//	lock (satellitePositions)
		//	{
		//		foreach (SatelliteInView sat in satellitePositions)
		//		{
		//			CircleScaling(dc, sat);
		//		}
		//	}

		//	// in the test program design these values are fixed.
		//	center = new Point(this.ActualWidth / 2, this.ActualHeight / 2);
		//	radius = 2 * this.ActualWidth / 5;

		//	// draw the grid
		//	DrawGrid(dc);

		//	//// create a test satelite position
		//	//SatellitePosition test = new SatellitePosition();
		//	//test.azimuth = 135;
		//	//test.elevation = 50;

		//	//// and dispay it
		//	//CircleScaling(dc, test);

		//	// call the base rendering, needed by default
		//	base.OnRender(dc);
		//}
	}
}
