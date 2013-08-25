using System;
using System.Threading.Tasks;
using System.Xml;

namespace WeatherLib
{
    public class WeatherAPI
    {
        private const string WEATHERAPI_BASE_URI = "http://weather.yahooapis.com/forecastrss?q=";

        private static async Task<XmlDocument> RetrieveRSS(string zip)
        {
            XmlDocument rssDoc = new XmlDocument();
            await Task.Run(() => rssDoc.Load(new XmlTextReader(WEATHERAPI_BASE_URI + zip)));
            return rssDoc;
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
            XmlNode temp = rss.GetElementsByTagName("yweather:condition")[0].Attributes.GetNamedItem("temp");
            return (celsius) ? ConvertToCelsius(Int32.Parse(temp.InnerText)) : Int32.Parse(temp.InnerText);
        }

        public static async Task<DateTime> GetSunrise(string zip)
        {
            XmlDocument rss = await RetrieveRSS(zip);
            XmlNode temp = rss.GetElementsByTagName("yweather:astronomy")[0].Attributes.GetNamedItem("sunrise");
            return DateTime.Parse(temp.InnerText);
        }

        public static async Task<DateTime> GetSunset(string zip)
        {
            XmlDocument rss = await RetrieveRSS(zip);
            XmlNode temp = rss.GetElementsByTagName("yweather:astronomy")[0].Attributes.GetNamedItem("sunset");
            return DateTime.Parse(temp.InnerText);
        }

        public static async Task<string> GetCityStateString(string zip)
        {
            XmlDocument rss = await RetrieveRSS(zip);
            XmlNode weatherInfo = rss.GetElementsByTagName("yweather:location")[0];

            XmlNode city = weatherInfo.Attributes.GetNamedItem("city");
            XmlNode state = weatherInfo.Attributes.GetNamedItem("region");
            XmlNode country = weatherInfo.Attributes.GetNamedItem("country");
            return city.InnerText + ", " + (!string.IsNullOrWhiteSpace(state.InnerText) ? state.InnerText : country.InnerText);
        }

        public static async Task<int> GetCurrentConditions(string zip)
        {
            XmlDocument rss = await RetrieveRSS(zip);
            XmlNode condition = rss.GetElementsByTagName("yweather:condition")[0].Attributes.GetNamedItem("code");
            return Int32.Parse(condition.InnerText);
        }

        private static int ConvertToCelsius(int f)
        {
            return (int)((5.0 / 9.0) * (f - 32));
        }
    }

}
