using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ToolbarControl_NS;
using ClickThroughFix;
using KSP.UI;
using KSP.UI.Screens;

namespace BigBen
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, true)]
    public class BigBen : MonoBehaviour
    {
        const float WIDTH = 300;
        const float MIN_HEIGHT = 110;
        float MAX_HEIGHT = 800;
        private Rect posBigBenWindow = new Rect(50, 50, WIDTH, MIN_HEIGHT);

        internal const string MODID = "BigBen_ns";
        internal const string MODNAME = "Big Ben";

        static internal ToolbarControl toolbarControl = null;
        bool visible = false;
        bool initted = false;
        GUIStyle textColor;
        Color origTextColor;
        Color origBackgroundColor;

        public class Timer
        {
            public int sortId;
            public string timerName = "";
            public bool countUp = true;
            public bool countDown = false;
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
            public double startUniversalTime = 0;
            public double elapsedTime = 0;
            public double elapsedTimeAtPause = 0;
            public bool negative = false;
            public Timer()
            {
                sortId = BigBen.timers.Count + 1;
            }
        }

        static public List<Timer> timers = new List<Timer>();
        Timer timer;
        Vector2 listPos;

        void Awake()
        {
            DontDestroyOnLoad(this);
        }
        void Start()
        {
            AddToolbarButton();

            GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);
            AddNewTimer();
            MAX_HEIGHT = Screen.height - 40;
        }

        void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> fta)
        {
            if (fta.to == GameScenes.MAINMENU)
                timers = null;
        }
        void AddToolbarButton()
        {
            if (toolbarControl == null)
            {
                toolbarControl = gameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(GUIButtonToggle, GUIButtonToggle,
                    ApplicationLauncher.AppScenes.SPACECENTER |
                    ApplicationLauncher.AppScenes.VAB |
                    ApplicationLauncher.AppScenes.SPH,
                    MODID,
                    "bigBenButton",
                    "BigBen/PluginData/BigBen_38",
                    "BigBen/PluginData/BigBen_24",
                    MODNAME
                );
            }
        }
        void GUIButtonToggle()
        {
            visible = !visible;
        }

        void OnGUI()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().useAlternateSkin)
                GUI.skin = HighLogic.Skin;
            if (!initted)
            {
                textColor = new GUIStyle("TextField");
                origTextColor = textColor.normal.textColor;
                origBackgroundColor = GUI.backgroundColor;
                initted = false;
            }
            if (visible)
            {
                posBigBenWindow = ClickThruBlocker.GUILayoutWindow(56783457, posBigBenWindow, DrawList, "Big Ben",
                   GUILayout.Width(WIDTH), GUILayout.MinHeight(MIN_HEIGHT), GUILayout.MaxHeight(MAX_HEIGHT));
            }

        }

        void DrawList(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().multiple = GUILayout.Toggle(HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().multiple, "Multiple");
            GUILayout.EndHorizontal();

            listPos = GUILayout.BeginScrollView(listPos, GUILayout.Width(WIDTH - 10), GUILayout.ExpandHeight(true));

            int cnt = 0;
            foreach (Timer t in timers)
            {
                timer = t;

                GUILayout.BeginHorizontal();
                if (!timer.timerActive)
                    DrawEntryWindow(cnt++);
                else
                    DrawActiveTimingWindow(cnt++);
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(0.5f) });
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                if (!HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().multiple)
                    break;
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        void DrawEntryWindow(int cnt)
        {
            if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().multiple)
            {
                GUILayout.Label("Name: ");
                timer.timerName = GUILayout.TextField(timer.timerName, GUILayout.MinWidth(60));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
            }
            if (timer.countUp)
            {
                timer.countUp = GUILayout.Toggle(timer.countUp, "Up", GUI.skin.button);
                if (!timer.countUp)
                {
                    timer.startUniversalTime = 0;
                    timer.countDown = true;
                }
            }
            else
            {
                timer.countDown = GUILayout.Toggle(timer.countDown, "Down", GUI.skin.button);
                if (!timer.countDown)
                {
                    timer.startUniversalTime = 0;
                    timer.countUp = true;
                }
            }

            timer.hrs = timer.hours.ToString();
            timer.min = timer.minutes.ToString();
            timer.sec = timer.seconds.ToString();

            timer.hrs = GUILayout.TextField(timer.hrs, GUILayout.Width(30));
            GUILayout.Label(":");
            timer.min = GUILayout.TextField(timer.min, GUILayout.Width(30));
            GUILayout.Label(":");
            timer.sec = GUILayout.TextField(timer.sec, GUILayout.Width(30));



            if (timer.hrs != timer.hours.ToString() ||
                timer.min != timer.minutes.ToString() ||
                timer.sec != timer.seconds.ToString())
            {
                timer.hours = int.Parse(timer.hrs);
                timer.minutes = int.Parse(timer.min);
                timer.seconds = int.Parse(timer.sec);

                timer.startTime = timer.hours * 3600 + timer.minutes * 60 + timer.seconds;
            }

            if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().multiple)
            {
                if (cnt == 0)
                    GUI.enabled = false;
                if (GUILayout.Button("^", GUILayout.Width(20)))
                    MoveUp(timer);
                GUI.enabled = true;
                if (GUILayout.Button("+", GUILayout.Width(20)))
                    AddNewTimer();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Reset"))
            {
                if (timer.countUp)
                {
                    timer.hours = timer.minutes = timer.seconds = 0;
                    timer.startHrs = timer.startMin = timer.startSec = 0;
                }
                else
                {
                    timer.hours = timer.startHrs;
                    timer.minutes = timer.startMin;
                    timer.seconds = timer.startSec;
                }
                timer.elapsedTimeAtPause = 0;
            }

            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            var bstyle = new GUIStyle(GUI.skin.button);
            bstyle.normal.textColor = Color.yellow;


            GUI.backgroundColor = Color.green;

            if (GUILayout.Button("Start", bstyle))
            {
                timer.timerActive = true;
                timer.startUniversalTime = Planetarium.GetUniversalTime();
                if (timer.elapsedTimeAtPause == 0)
                {
                    timer.startHrs = int.Parse(timer.hrs);
                    timer.startMin = int.Parse(timer.min);
                    timer.startSec = int.Parse(timer.sec);
                    timer.startTenths = 0;
                }
                GUI.backgroundColor = oldColor;
            }
            if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().multiple)
            {
                if (cnt == timers.Count - 1)
                    GUI.enabled = false;
                if (GUILayout.Button("v", GUILayout.Width(20)))
                    MoveDown(timer);
                GUI.enabled = true;
                if (GUILayout.Button("-", GUILayout.Width(20)))
                    RemoveTimer(timer);
            }
        }

        void DrawActiveTimingWindow(int cnt)
        {
            if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().multiple)
            {
                GUILayout.Label(timer.timerName, GUILayout.MinWidth(60));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
            }

            if (timer.countUp)
                GUILayout.TextField("Up");
            else
                GUILayout.TextField("Down");

            timer.elapsedTime = Planetarium.GetUniversalTime() + timer.elapsedTimeAtPause - timer.startUniversalTime;
            if (timer.countUp)
            {
                timer.hours = (int)(timer.elapsedTime / 3600);
                timer.minutes = (int)((timer.elapsedTime - timer.hours * 3600) / 60);
                timer.seconds = (int)(timer.elapsedTime - timer.hours * 3600 - timer.minutes * 60);
                timer.tenths = (int)(timer.hours * 3600 - timer.minutes * 60 - timer.seconds);
                timer.negative = false;
            }
            else
            { // countDown
                double timeLeft = Math.Abs(timer.startTime - timer.elapsedTime - timer.elapsedTimeAtPause);
                timer.hours = (int)(timeLeft / 3600);
                timer.minutes = (int)((timeLeft - timer.hours * 3600) / 60);
                timer.seconds = (int)(timeLeft - timer.hours * 3600 - timer.minutes * 60);

                timer.tenths = (int)(timeLeft - timer.hours * 3600 - timer.minutes * 60 - timer.seconds);

                if (timer.startTime - timer.elapsedTime < 0)
                    timer.negative = true;
                else
                    timer.negative = false;
            }

            timer.hrs = timer.hours.ToString();
            timer.min = timer.minutes.ToString("00");
            timer.sec = timer.seconds.ToString("00");
            if (timer.negative)
                textColor.normal.textColor = Color.red;
            else
                textColor.normal.textColor = origTextColor;

            GUILayout.TextField(timer.hrs, textColor, GUILayout.Width(30));
            GUILayout.Label(":");
            GUILayout.TextField(timer.min, textColor, GUILayout.Width(30));
            GUILayout.Label(":");
            GUILayout.TextField(timer.sec, textColor, GUILayout.Width(30));
            if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().showTenths)
            {
                GUILayout.Label(":");
                GUILayout.TextField(timer.ten, textColor, GUILayout.Width(30));
            }
            if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().multiple)
            {
                if (cnt == 0)
                    GUI.enabled = false;
                if (GUILayout.Button("^", GUILayout.Width(20)))
                    MoveUp(timer);
                GUI.enabled = true;
                if (GUILayout.Button("+", GUILayout.Width(20)))
                    AddNewTimer();
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (timer.timerActive)
                GUI.enabled = false;
            if (GUILayout.Button("Reset"))
            {
                if (timer.countUp)
                {
                    timer.hours = timer.minutes = timer.seconds = 0;
                    timer.startHrs = timer.startMin = timer.startSec = 0;
                }
                else
                {
                    timer.hours = timer.startHrs;
                    timer.minutes = timer.startMin;
                    timer.seconds = timer.startSec;
                }
                timer.elapsedTimeAtPause = 0;
            }
            GUI.enabled = true;

            var bstyle = new GUIStyle(GUI.skin.button);
            bstyle.normal.textColor = Color.yellow;

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Stop", bstyle))
            {
                timer.elapsedTimeAtPause += Planetarium.GetUniversalTime() - timer.startUniversalTime;

                timer.timerActive = false;
            }
            GUI.backgroundColor = origBackgroundColor;
            if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().multiple)
            {
                if (cnt == timers.Count - 1)
                    GUI.enabled = false;
                if (GUILayout.Button("v", GUILayout.Width(20)))
                    MoveDown(timer);
                GUI.enabled = true;
                if (GUILayout.Button("-", GUILayout.Width(20)))
                    RemoveTimer(timer);
            }
        }

        void SortList()
        {
            int cnt = 0;
            timers = timers.OrderBy(x => x.sortId).ToList();
            foreach (var t in timers)
                t.sortId = cnt++;
        }
        void MoveUp(Timer timer)
        {
            if (timer.sortId > 0)
            {
                timers[timer.sortId - 1].sortId = timer.sortId;
                timer.sortId--;
            }
            SortList();
        }
        void MoveDown(Timer timer)
        {
            if (timer.sortId + 1 < timers.Count)
            {
                timers[timer.sortId + 1].sortId = timer.sortId;
                timer.sortId++;
            }
            SortList();
        }
        void AddNewTimer()
        {
            Timer timer = new Timer();
            timer.countUp = HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().defaultCountUp;
            timer.countDown = !timer.countUp;
            timers.Add(timer);
        }
        void RemoveTimer(Timer timer)
        {
            timers.Remove(timer);
            if (timers.Count == 0)
            {
                AddNewTimer();
            }
            SortList();
        }

        void FixedUpdate()
        {

        }
    }
}
