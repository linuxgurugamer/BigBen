using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceTuxUtility;

namespace BigBen
{
    public class Timer
    {

        public int sortId;
        public string timerName = "";
        public bool countUp = true;
        public bool countDown = false;
        public bool countDownRepeating = false;
        public bool pauseAtZero = false;
        public bool timerActive = false;
        public bool realTime = false;

        public float hours;
        public float minutes;
        public float seconds;
        public float tenths;

        public string hrs;
        public string min;
        public string sec;
        //public string ten;

        public float startHrs;
        public float startMin;
        public float startSec;
        //public float startTenths;

        public double startTime = 0;
        public double repeatingTimeLength = 0;
        public double startUniversalTime = 0;
        public double startRealTimeSinceStartup = 0;
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

        public Timer(ConfigNode node)
        {
            sortId = ConfigNodeUtils.SafeLoad(node, "sortId", 0);
            timerName = ConfigNodeUtils.SafeLoad(node,"timerName", "");
            countUp = ConfigNodeUtils.SafeLoad(node,"countUp", true);
            countDown = ConfigNodeUtils.SafeLoad(node,"countDown", false);
            countDownRepeating = ConfigNodeUtils.SafeLoad(node,"countDownRepeating", false);
            pauseAtZero = ConfigNodeUtils.SafeLoad(node, "pauseAtZero", false);
            timerActive = ConfigNodeUtils.SafeLoad(node, "timerActive", false);
            realTime = ConfigNodeUtils.SafeLoad(node, "realTime", false);
            hours = ConfigNodeUtils.SafeLoad(node,"hours", 0f);
            minutes = ConfigNodeUtils.SafeLoad(node,"minutes", 0f);
            seconds = ConfigNodeUtils.SafeLoad(node,"seconds", 0f);
            tenths = ConfigNodeUtils.SafeLoad(node,"tenths", 0f);
            hrs = ConfigNodeUtils.SafeLoad(node,"hrs","0" );
            min = ConfigNodeUtils.SafeLoad(node,"min", "0");
            sec = ConfigNodeUtils.SafeLoad(node,"sec", "0");
            //node,"ten", ten);
            startHrs = ConfigNodeUtils.SafeLoad(node,"startHrs", 0f);
            startMin = ConfigNodeUtils.SafeLoad(node,"startMin", 0f);
            startSec = ConfigNodeUtils.SafeLoad(node,"startSec", 0f);
            //node,"startTenths", startTenths);
            startTime = ConfigNodeUtils.SafeLoad(node,"startTime", 0f);
            repeatingTimeLength = ConfigNodeUtils.SafeLoad(node,"repeatingTimeLength", 0f);
            startUniversalTime = ConfigNodeUtils.SafeLoad(node, "startUniversalTime", 0f);
            startRealTimeSinceStartup = ConfigNodeUtils.SafeLoad(node, "startRealTimeSinceStartup", 0f);
            elapsedTime = ConfigNodeUtils.SafeLoad(node,"elapsedTime", 0f);
            elapsedTimeAtPause = ConfigNodeUtils.SafeLoad(node,"elapsedTimeAtPause", 0f);
            negative = ConfigNodeUtils.SafeLoad(node,"negative", false);
            bellSounded = ConfigNodeUtils.SafeLoad(node,"bellSounded", false );
            bellCnt = ConfigNodeUtils.SafeLoad(node,"bellCnt", 0);
            nextBellAt = ConfigNodeUtils.SafeLoad(node,"nextBellAt", 0f);
            showTenths = ConfigNodeUtils.SafeLoad(node,"showTenths", false);
        }
        public ConfigNode ToNode(string nodeName)
        {
            ConfigNode node = new ConfigNode(nodeName);

            node.AddValue("sortId", sortId);
            node.AddValue("timerName", timerName);
            node.AddValue("countUp", countUp);
            node.AddValue("countDown", countDown);
            node.AddValue("countDownRepeating", countDownRepeating);
            node.AddValue("pauseAtZero", pauseAtZero);
            node.AddValue("timerActive", timerActive);
            node.AddValue("realTime", realTime);
            node.AddValue("hours", hours);
            node.AddValue("minutes", minutes);
            node.AddValue("seconds", seconds);
            node.AddValue("tenths", tenths);
            node.AddValue("hrs", hrs);
            node.AddValue("min", min);
            node.AddValue("sec", sec);
            //node.AddValue("ten", ten);
            node.AddValue("startHrs", startHrs);
            node.AddValue("startMin", startMin);
            node.AddValue("startSec", startSec);
            //node.AddValue("startTenths", startTenths);
            node.AddValue("startTime", startTime);
            node.AddValue("repeatingTimeLength", repeatingTimeLength);
            node.AddValue("startUniversalTime", startUniversalTime);
            node.AddValue("startRealTimeSinceStartup", startRealTimeSinceStartup);
            node.AddValue("elapsedTime", elapsedTime);
            node.AddValue("elapsedTimeAtPause", elapsedTimeAtPause);
            node.AddValue("negative", negative);
            node.AddValue("bellSounded", bellSounded);
            node.AddValue("bellCnt", bellCnt);
            node.AddValue("nextBellAt", nextBellAt);
            node.AddValue("showTenths", showTenths);

            return node;
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
#if false
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
#endif
                if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().showTimeAsColons)
                    return prefix + h + ":" + m.ToString("00") + ":" + s.ToString("00") + Tenths(t, showTenths);
                else
                    return prefix + h + "h " + m.ToString("00") + "m " + s.ToString("00") + Tenths(t, showTenths) + "s";
            }
            if (m > 0)
                return m + ":" + s.ToString("00") + Tenths(t, showTenths);
            return s.ToString() + Tenths(t, showTenths);
        }

        public void Reset(bool resetStart = false)
        {
            hours = minutes = seconds = 0;
            if (resetStart)
                startHrs = startMin = startSec = 0; // startTenths = 0;
            else
                ParseEntryFields();
        }

        public void ParseEntryFields()
        {
            startHrs = hrs == "" ? 0 : int.Parse(hrs);
            startMin = min == "" ? 0 : int.Parse(min);
            startSec = sec == "" ? 0 : int.Parse(sec);
            bool neg = (startHrs < 0 || startMin < 0 || startSec < 0);
            startHrs = Math.Abs(startHrs);
            startMin = Math.Abs(startMin);
            startSec = Math.Abs(startSec);

            if (countUp && neg)
            {
                if (startHrs > 0)
                    startHrs = -startHrs;
                else
                {
                    if (startMin > 0)
                    {
                        startMin = -startMin;
                    }
                    else
                        startSec = -startSec;
                }
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
