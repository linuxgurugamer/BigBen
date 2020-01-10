using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBen
{
    public class Timer
    {

        public int sortId;
        public string timerName = "";
        public bool countUp = true;
        public bool countDown = false;
        public bool countDownRepeating = false;
        public bool timerActive = false;

        public float hours;
        public float minutes;
        public float seconds;
        public float tenths;

        public string hrs;
        public string min;
        public string sec;
        public string ten;

        public float startHrs;
        public float startMin;
        public float startSec;
        public float startTenths;

        public double startTime = 0;
        public double repeatingTimeLength = 0;
        public double startUniversalTime = 0;
        public double elapsedTime = 0;
        public double elapsedTimeAtPause = 0;
        public bool negative = false;
        public bool bellSounded = false;
        public int bellCnt = 0;
        public double nextBellAt = 0;
        public bool showTenths;
        public Timer()
        {
            sortId = BigBen.timers.Count + 1;
            InitEntryFields();
            showTenths = HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().showTenths;
        }

        string Tenths(float t, bool tenths)
        {
            if (tenths)
                return "." + Math.Abs(t).ToString();
            return "";
        }
        public string FormatTime(bool startTime, bool showTenths = false)
        {
            if (startTime)
                return FormatSpecifiedTime(startHrs, startMin, startSec, tenths, false);
            else
                return FormatSpecifiedTime(hours, minutes, seconds, tenths, showTenths);
        }
        public string FormatSpecifiedTime(float h, float m, float s, float t, bool showTenths)
        {
            if (h > 0)
            {
                string prefix = "";
                if (h >= BigBen.dayLength)
                {
                    int days = (int)(h / BigBen.dayLength);
                    h -= days * BigBen.dayLength;
                    int years = 0;
                    if (days > BigBen.yearLength)
                    {
                        years = (int)(days / BigBen.yearLength);
                        days -= years * BigBen.yearLength;
                        prefix = years.ToString() + "y " + days.ToString() + "d ";
                    }
                    else
                        prefix = days.ToString() + "d ";
                }
                if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().showTimeAsColons)
                    return prefix + h + ":" + m.ToString("00") + ":" + s.ToString("00") + Tenths(t, showTenths);
                else
                    return prefix + h + "h " + m.ToString("00") + "m " + s.ToString("00") + Tenths(t, showTenths) + "s";
            }
            if (m > 0)
                return m + ":" + s.ToString("00") + Tenths(t, showTenths);
            return s.ToString() + Tenths(t, showTenths);
        }

        public void Reset()
        {
            hours = minutes = seconds = 0;
            startHrs = startMin = startSec = startTenths = 0;
        }

        public void ParseEntryFields()
        {
            startHrs = hrs == "" ? 0 : int.Parse(hrs);
            startMin = min == "" ? 0 : int.Parse(min);
            startSec = sec == "" ? 0 : int.Parse(sec);
            bool negative = (startHrs < 0 || startMin < 0 || startSec < 0);
            startHrs = Math.Abs(startHrs);
            startMin = Math.Abs(startMin);
            startSec = Math.Abs(startSec);

            if (countUp && negative)
            {
                if (startHrs > 0)
                    startHrs = -startHrs;
                else
                    if (startMin > 0)
                    startMin = -startMin;
                else
                    startSec = -startSec;
            }
        }
        internal void InitEntryFields()
        {
            hrs = hours.ToString();
            min = minutes.ToString();
            sec = seconds.ToString();
        }
        public static bool IsNumeric(string text)
        {
            int _out;

            return text == "" || int.TryParse(text, out _out);
        }
        public bool isValid
        {
            get
            {
                return IsNumeric(hrs) & IsNumeric(min) & IsNumeric(sec);
            }
        }
    }
}
