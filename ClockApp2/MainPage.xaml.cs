using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ClockApp2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public static async Task<string> DownloadPageAsync(string url)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;
            handler.AllowAutoRedirect = true;
            HttpClient client = new HttpClient(handler);
            HttpResponseMessage response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        int itr = 0;
        async void Refresh()
        {
            try
            {
                Time = DateTime.Now.ToString("h:mm tt");
                Day = DateTime.Now.ToString("dddd, MMMM d");

                if (itr % 100 == 0)
                {

                    TempNow = (await WeatherLib.WeatherAPI.GetCurrentTemperature("98004") + "°");
                    Conditions = await WeatherLib.WeatherAPI.GetCurrentConditions("98004");
                    WindSpeed = (await WeatherLib.WeatherAPI.GetWindSpeed("98004") + "mph");
                    Sunrise = (await WeatherLib.WeatherAPI.GetSunrise("98004")).ToString("h:mm tt");
                    Sunset = (await WeatherLib.WeatherAPI.GetSunset("98004")).ToString("h:mm tt");
                    Humidity = (await WeatherLib.WeatherAPI.GetHumidity("98004") + "%");
                    var f = await WeatherLib.WeatherAPI.GetForecast("98004");
                    Forecast.Clear();
                    foreach (var fx in f) { Forecast.Add(fx); }

                }

                //Webcam1 = "http://davux.gotdns.com:89/snapshot.cgi?user=admin&pwd=davehome&guid=" + Guid.NewGuid().ToString();
                
                var pg = await DownloadPageAsync("http://localhost:9494");

                XmlDocument x = new XmlDocument();
                x.LoadXml(pg);

                //var x = await XmlDocument.LoadFromUriAsync(new Uri("http://localhost:9494"));

                CPU.Clear();
                foreach (var cpu in x.GetElementsByTagName("cpu"))
                {
                    CPU.Add(cpu.InnerText);
                }

                TotalCPU = x.GetElementsByTagName("allcpu")[0].InnerText + "%";

                var m = x.GetElementsByTagName("memory")[0];
                long total = long.Parse(m.Attributes.GetNamedItem("total").InnerText) / 1073741824; // # 1Gb
                long free = long.Parse(m.InnerText) / 1024;

                TotalMemory = total;
                UsedMemory = total - free;
                FreeMemory = free;

                Memory = string.Format("{0}Gb", free, total);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Day = ex.Message;
            }
            itr++;
        }

        public MainPage()
        {
            this.InitializeComponent();

            Forecast = new ObservableCollection<WeatherLib.WeatherAPI.ForecastEntry>();
            CPU = new ObservableCollection<string>();

            Refresh();
            DataContext = this;

            wb.Navigate(new Uri("https://home.nest.com/"));
            
           
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var dt = new DispatcherTimer();
            dt.Interval = new TimeSpan(0, 0, 5);
            dt.Tick += (_,__) =>
            {
                Refresh();
            };
            dt.Start();


            var Presets = new List<string>
            {
               "White","Relax", "Default", "O_Default"
            };

            var Colors = new List<string> {
               "Red","Blue","Purple","Green"
            };

            foreach (var cc in Colors)
            {
                Button c = new Button
                {
                    Content = cc,
                    Tag = cc,
                    Padding = new Thickness(6),
                    //Margin = new Thickness(1)
                };
                c.Click += (s, _) =>
                {
                    var x = (string)((Button)s).Tag;
                    var hc = new System.Net.Http.HttpClient();

                    hc.GetAsync("http://192.168.0.2:8080/color?" + x);
                };
                colors.Children.Add(c);
            }

            foreach (var p in Presets)
            {
                Button c = new Button
                {
                    Content = p,
                    Tag = p,
                    Padding = new Thickness(6),
                    //Margin = new Thickness(1)
                };
                c.Click += (s, _) =>
                {
                    var x = (string)((Button)s).Tag;
                    var hc = new System.Net.Http.HttpClient();

                    hc.GetAsync("http://192.168.0.2:8080/" + x);
                };
                presets.Children.Add(c);
            }

            for (int ri = 2; ri <= 10; ri += 2)
            {
                var i = ri;
                Button c = new Button
                {
                    Content = string.Format("{0}%", i * 10),
                    Tag = i * 10,
                    Padding = new Thickness(6),
                    //Margin = new Thickness(1)
                };
                c.Click += (s, _) =>
                {
                    int x = (int)((Button)s).Tag;
                    var hc = new System.Net.Http.HttpClient();

                    hc.GetAsync("http://192.168.0.2:8080/brightness?" + ((float)x * 2.55));
                };
                brightness.Children.Add(c);
            }


            //wb.Navigate(new Uri("http://192.168.0.1/graph_if.svg?vlan2"));
        }

        private string _Time;
        public string Time
        {
            get { return _Time; }
            set
            {
                _Time = value;
                RaisePropertyChanged("Time");
            }
        }

        private string _Day;
        public string Day
        {
            get { return _Day; }
            set
            {
                _Day = value;
                RaisePropertyChanged("Day");
            }
        }

        private string _TempNow;
        public string TempNow
        {
            get { return _TempNow; }
            set
            {
                _TempNow = value;
                RaisePropertyChanged("TempNow");
            }
        }

        private string _Conditions;
        public string Conditions
        {
            get { return _Conditions; }
            set
            {
                _Conditions = value;
                RaisePropertyChanged("Conditions");
            }
        }

        private string _WindSpeed;
        public string WindSpeed
        {
            get { return _WindSpeed; }
            set
            {
                _WindSpeed = value;
                RaisePropertyChanged("WindSpeed");
            }
        }

        private string _Sunrise;
        public string Sunrise
        {
            get { return _Sunrise; }
            set
            {
                _Sunrise = value;
                RaisePropertyChanged("Sunrise");
            }
        }

        private string _Sunset;
        public string Sunset
        {
            get { return _Sunset; }
            set
            {
                _Sunset = value;
                RaisePropertyChanged("Sunset");
            }
        }

        private string _Memory;
        public string Memory
        {
            get { return _Memory; }
            set
            {
                _Memory = value;
                RaisePropertyChanged("Memory");
            }
        }

        private long _TotalMemory;
        public long TotalMemory
        {
            get { return _TotalMemory; }
            set
            {
                _TotalMemory = value;
                RaisePropertyChanged("TotalMemory");
            }
        }

        private long _FreeMemory;
        public long FreeMemory
        {
            get { return _FreeMemory; }
            set
            {
                _FreeMemory = value;
                RaisePropertyChanged("FreeMemory");
            }
        }

        private long _UsedMemory;
        public long UsedMemory
        {
            get { return _UsedMemory; }
            set
            {
                _UsedMemory = value;
                RaisePropertyChanged("UsedMemory");
            }
        }

        private string _TotalCPU;
        public string TotalCPU
        {
            get { return _TotalCPU; }
            set
            {
                _TotalCPU = value;
                RaisePropertyChanged("TotalCPU");
            }
        }

        private string _Humidity;
        public string Humidity
        {
            get { return _Humidity; }
            set
            {
                _Humidity = value;
                RaisePropertyChanged("Humidity");
            }
        }

        private string _Webcam1;
        public string Webcam1
        {
            get { return _Webcam1; }
            set
            {
                _Webcam1 = value;
                RaisePropertyChanged("Webcam1");
            }
        }

        public ObservableCollection<WeatherLib.WeatherAPI.ForecastEntry> Forecast { get; set; }
        public ObservableCollection<string> CPU { get; set; }
    }
}
