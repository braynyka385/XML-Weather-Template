using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using Google.Cloud.Speech.V2;
using System.Xml;

namespace XMLWeather
{
    public partial class Form1 : Form
    {
        // list to hold day objects
        public static List<Day> days = new List<Day>();
        public static SpeechClient speechClient;
        string city = "Stratford";
        string country = "CA";

        public Form1()
        {
            InitializeComponent();
            SpeechClientBuilder speechClientBuilder = new SpeechClientBuilder();
            speechClientBuilder.CredentialsPath = "";
            speechClient = speechClientBuilder.Build();
            RecognizeRequest request = new RecognizeRequest();
            request.Recognizer = "projects/a";
            speechClient.Recognize(request);
            ExtractForecast();
            ExtractCurrent();
            
            // open weather screen for todays weather
            CurrentScreen cs = new CurrentScreen();
            this.Controls.Add(cs);
        }

        private void ExtractForecast()
        {
            XmlReader reader = XmlReader.Create("http://api.openweathermap.org/data/2.5/forecast/daily?q=Stratford,CA&mode=xml&units=metric&cnt=7&appid=3f2e224b815c0ed45524322e145149f0");
            reader.ReadToFollowing("timezone");
            string timeshiftS = reader.ReadString();
            int timeshift = 0;
            if (timeshiftS != "")
                timeshift = int.Parse(timeshiftS);

            while (reader.Read())
            {
                //create a day object
                Day d = new Day();
                
                //fill day object with required data
                reader.ReadToFollowing("time");
                d.date = reader.GetAttribute("day");

                reader.ReadToFollowing("sun");
                d.sunrise = reader.GetAttribute("rise");
                d.sunset = reader.GetAttribute("set");
                if(d.sunrise != null)
                {
                    int indexOfRiseT = d.sunrise.IndexOf('T');
                    d.sunrise = d.sunrise.Substring(indexOfRiseT + 1, d.sunrise.Length - indexOfRiseT - 1);

                    int sunriseSeconds = (60 * 60 * int.Parse(d.sunrise.Substring(0, 2))) +
                        (60 * int.Parse(d.sunrise.Substring(3, 2))) +
                        (int.Parse(d.sunrise.Substring(6, 2)));

                    int sunriseActual = sunriseSeconds + timeshift;
                    if(sunriseActual > 86400)
                    {
                        sunriseActual -= 86400;
                    }
                    if(sunriseActual < 0)
                    {
                        sunriseActual += 86400;
                    }
                    TimeSpan timeSpan = TimeSpan.FromSeconds(sunriseActual);
                    d.sunrise = timeSpan.ToString();
                }
                if (d.sunset != null)
                {
                    int indexOfSetT = d.sunset.IndexOf('T');
                    d.sunset = d.sunset.Substring(indexOfSetT + 1, d.sunset.Length - indexOfSetT - 1);

                    int sunsetSeconds = (60 * 60 * int.Parse(d.sunset.Substring(0, 2))) +
                        (60 * int.Parse(d.sunset.Substring(3, 2))) +
                        (int.Parse(d.sunset.Substring(6, 2)));

                    int sunsetActual = sunsetSeconds + timeshift;
                    if (sunsetActual > 86400)
                    {
                        sunsetActual -= 86400;
                    }
                    if (sunsetActual < 0)
                    {
                        sunsetActual += 86400;
                    }
                    TimeSpan timeSpan = TimeSpan.FromSeconds(sunsetActual);
                    d.sunset = timeSpan.ToString();
                }

                reader.ReadToFollowing("temperature");
                d.tempLow = Math.Round(Convert.ToDouble(reader.GetAttribute("min"))).ToString();
                d.tempHigh = Math.Round(Convert.ToDouble(reader.GetAttribute("max"))).ToString();

                if(d.date != null)
                    days.Add(d);
                //if day object not null add to the days list
            }

           
        }

        private void ExtractCurrent()
        {
            // current info is not included in forecast file so we need to use this file to get it
            XmlReader reader = XmlReader.Create("http://api.openweathermap.org/data/2.5/weather?q=Stratford,CA&mode=xml&units=metric&appid=3f2e224b815c0ed45524322e145149f0");

            //find the city and current temperature and add to appropriate item in days list

            reader.ReadToFollowing("city");
            days[0].location = reader.GetAttribute("name");

            reader.ReadToFollowing("temperature");
            days[0].currentTemp = Math.Round(Convert.ToDouble(reader.GetAttribute("value"))).ToString();
        }


    }
}
