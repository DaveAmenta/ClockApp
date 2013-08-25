using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;

namespace WeatherLib
{
    public class WeatherAPI
    {
        public struct ForecastEntry
        {
            public string Day  { get; set; }
            public string Low  { get; set; }
            public string High { get; set; }
            public string Text { get; set; }
        }

        private const string WEATHERAPI_BASE_URI = "http://weather.yahooapis.com/forecastrss?q=";

        public static async Task<string> DownloadPageAsync(string url)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;
            handler.AllowAutoRedirect = true;
            HttpClient client = new HttpClient(handler);
            HttpResponseMessage response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string responseBody= await  response.Content.ReadAsStringAsync();
            return responseBody;
        }

        private static async Task<XmlDocument> RetrieveRSS(string zip)
        {

            var pg = await DownloadPageAsync(WEATHERAPI_BASE_URI + zip);

            XmlDocument xd = new XmlDocument();
            xd.LoadXml(pg);
            return xd;

            //return await XmlDocument.LoadFromUriAsync(new Uri(WEATHERAPI_BASE_URI + zip));
        }

        public static async Task<bool> isValidZip(string zip)
        {
            XmlDocument rssDoc = await RetrieveRSS(zip);
            XmlNodeList title = rssDoc.GetElementsByTagName("title");
            if (title[0].InnerText.IndexOf("Error") > -1)
            {
                return false;
            }
            return true;
        }

        public static async Task<int> GetCurrentTemperature(string zip, bool celsius = false)
        {
            XmlDocument rss = await RetrieveRSS(zip);
            IXmlNode temp = rss.GetElementsByTagName("yweather:condition")[0].Attributes.GetNamedItem("temp");
            return (celsius) ? ConvertToCelsius(Int32.Parse(temp.InnerText)) : Int32.Parse(temp.InnerText);
        }

        public static async Task<DateTime> GetSunrise(string zip)
        {
            XmlDocument rss = await RetrieveRSS(zip);
            IXmlNode temp = rss.GetElementsByTagName("yweather:astronomy")[0].Attributes.GetNamedItem("sunrise");
            return DateTime.Parse(temp.InnerText);
        }

        public static async Task<DateTime> GetSunset(string zip)
        {
            XmlDocument rss = await RetrieveRSS(zip);
            IXmlNode temp = rss.GetElementsByTagName("yweather:astronomy")[0].Attributes.GetNamedItem("sunset");
            return DateTime.Parse(temp.InnerText);
        }

        public static async Task<string> GetCityStateString(string zip)
        {
            XmlDocument rss = await RetrieveRSS(zip);
            IXmlNode weatherInfo = rss.GetElementsByTagName("yweather:location")[0];

            IXmlNode city = weatherInfo.Attributes.GetNamedItem("city");
            IXmlNode state = weatherInfo.Attributes.GetNamedItem("region");
            IXmlNode country = weatherInfo.Attributes.GetNamedItem("country");
            return city.InnerText + ", " + (!string.IsNullOrWhiteSpace(state.InnerText) ? state.InnerText : country.InnerText);
        }

        public static async Task<string> GetCurrentConditions(string zip)
        {
            XmlDocument rss = await RetrieveRSS(zip);
            IXmlNode condition = rss.GetElementsByTagName("yweather:condition")[0].Attributes.GetNamedItem("text");
            return condition.InnerText;
        }

        public static async Task<string> GetWindSpeed(string zip)
        {
            XmlDocument rss = await RetrieveRSS(zip);
            IXmlNode condition = rss.GetElementsByTagName("yweather:wind")[0].Attributes.GetNamedItem("speed");
            return condition.InnerText;
        }

        public static async Task<string> GetHumidity(string zip)
        {
            XmlDocument rss = await RetrieveRSS(zip);
            IXmlNode condition = rss.GetElementsByTagName("yweather:atmosphere")[0].Attributes.GetNamedItem("humidity");
            return condition.InnerText;
        }

        public static async Task<List<ForecastEntry>> GetForecast(string zip)
        {
            XmlDocument rss = await RetrieveRSS(zip);
            List<ForecastEntry> ret = new List<ForecastEntry>();
            foreach (var x in rss.GetElementsByTagName("yweather:forecast"))
            {
                var fc = new ForecastEntry();
                fc.Day = x.Attributes.GetNamedItem("day").InnerText;
                fc.Low = x.Attributes.GetNamedItem("low").InnerText;
                fc.High = x.Attributes.GetNamedItem("high").InnerText;
                fc.Text = x.Attributes.GetNamedItem("text").InnerText;
                ret.Add(fc);
            }
            return ret;
        }

        private static int ConvertToCelsius(int f)
        {
            return (int)((5.0 / 9.0) * (f - 32));
        }
    }

}
