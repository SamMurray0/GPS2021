using Demo.WindowsPresentation.CustomMarkers;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
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

        public MainWindow()
        {
            InitializeComponent();

            GetSerials();

            nmea.PositionReceived += Nmea_PositionReceived;
            this.Closing += MainWindow_Closing;
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

                if (nmea.gsvParser != null)
                {
                   // Console.WriteLine("SATs 2 =" + nmea.gsvParser.output.Count);
                    DrawBarGraph();
                    smallveh.Text = nmea.gsvParser.output.Count.ToString();
                }

                // do the bar braph

                AddMarker( nmea.latitude, nmea.longitude, nmea.GPSTime.ToLongTimeString() );
            });
        }

        public void DrawBarGraph()
        {
            WpfPlot1.Plot.Clear();

            double[] value = new double[nmea.gsvParser.output.Count];

            int count = 0;
            foreach (GsvSatellite s in nmea.gsvParser.output)
            {
                value[count] = s.Prn;
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
	}
}
