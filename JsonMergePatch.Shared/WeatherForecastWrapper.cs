using System;

namespace LaDeak.JsonMergePatch.Shared
{
    public class WeatherForecastWrapper : Patch<WeatherForecast>
    {
        private DateTime _date;
        private int _temperatureC;
        private string _summary;

        public WeatherForecastWrapper()
        {
            Properties = new bool[3];
        }

        public DateTime Date
        {
            get { return _date; }
            set
            {
                Properties[0] = true;
                _date = value;
            }
        }

        public int TemperatureC
        {
            get { return _temperatureC; }
            set
            {
                Properties[1] = true;
                _temperatureC = value;
            }
        }

        public string Summary
        {
            get { return _summary; }
            set
            {
                Properties[2] = true;
                _summary = value;
            }
        }

        public override WeatherForecast ApplyPatch(WeatherForecast input)
        {
            if (Properties[0])
                input.Date = Date;
            if (Properties[1])
                input.TemperatureC = TemperatureC;
            if (Properties[2])
                input.Summary = Summary;
            return input;
        }
    }
}
