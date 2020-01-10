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


        public static int dayLength;
        public static int yearLength;

        const float WIDTH = 300;
        const float MIN_HEIGHT = 110;
        float MAX_HEIGHT = 800;
        private Rect posBigBenWindow = new Rect(50, 50, WIDTH, MIN_HEIGHT);

        internal const string MODID = "BigBen_ns";
        internal const string MODNAME = "Big Ben";

        static internal ToolbarControl toolbarControl = null;
        bool visible = false;
        bool initted = false;
        GUIStyle labelTextColor;
        Color origTextColor;
        Color origBackgroundColor;

        static public List<Timer> timers = new List<Timer>();
        Timer timer;
        Vector2 listPos;

        internal AlertSoundPlayer soundplayer;
        internal const string SOUND_DIR = "BigBen/Sounds/";
        internal static string defaultAlert = "bell-1";
        internal static string normalAlert = "bell";
        //internal static float normalAlertLength;
        public bool Paused = false;
        void Awake()
        {
            DontDestroyOnLoad(this);
            //soundplayer = new AlertSoundPlayer();
            soundplayer = gameObject.AddComponent<AlertSoundPlayer>();
            if (GameSettings.KERBIN_TIME)
            {
                dayLength = 6;
                yearLength = 426;
            }
            else
            {
                dayLength = 24;
                yearLength = 365;
            }
        }

        void Start()
        {
            AddToolbarButton();

            GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);
            GameEvents.onGamePause.Add(OnPause);
            GameEvents.onGameUnpause.Add(OnUnpause);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);

            AddNewTimer();
            MAX_HEIGHT = Screen.height - 40;
            soundplayer.Initialize(SOUND_DIR + "bell"); // Initializes the player, does some housekeeping
            //normalAlertLength = soundplayer.SoundLength("bell");
            soundplayer.SetVolume = HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().volume;
        }

        void OnDestroy()
        {
            GameEvents.onGameSceneSwitchRequested.Remove(onGameSceneSwitchRequested);
            GameEvents.onGamePause.Remove(OnPause);
            GameEvents.onGameUnpause.Remove(OnUnpause);
            GameEvents.OnGameSettingsApplied.Remove(OnGameSettingsApplied);
        }
        void OnGameSettingsApplied()
        {
            soundplayer.SetVolume = HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().volume;
        }
        void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> fta)
        {
            if (fta.to == GameScenes.MAINMENU)
                timers = null;
        }

        bool wasPlaying = false;
        void OnPause()
        {
            Paused = true;
            wasPlaying = false;
            if (soundplayer == null)
                return;
            if (soundplayer.SoundPlaying())
            {
                wasPlaying = true;
                soundplayer.PauseSound();
            }
        }

        void OnUnpause()
        {
            Paused = false;
            if (wasPlaying)
                soundplayer.unPauseSound();
        }

        void AddToolbarButton()
        {
            if (toolbarControl == null)
            {
                toolbarControl = gameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(GUIButtonToggle, GUIButtonToggle,
                    ApplicationLauncher.AppScenes.SPACECENTER |
                    ApplicationLauncher.AppScenes.VAB |
                    ApplicationLauncher.AppScenes.SPH |
                    ApplicationLauncher.AppScenes.FLIGHT,
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

        void SoundBell(string n, int cnt = 1)
        {
            soundplayer.PlaySound(cnt - 1); //Plays sound
        }

        void OnGUI()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().useAlternateSkin)
                GUI.skin = HighLogic.Skin;
            if (!initted)
            {
                labelTextColor = new GUIStyle("Label");
                labelTextColor.fontStyle = FontStyle.Bold;
                //labelTextColor.fontSize++;
                origTextColor = labelTextColor.normal.textColor;
                origBackgroundColor = GUI.backgroundColor;


                initted = true;
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
            if (timers.Count > 4)
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
            if (timers.Count > 4)
                GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        void DrawEntryWindow(int cnt)
        {
            if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().multiple)
            {
                GUILayout.Label("Name: ");
                timer.timerName = GUILayout.TextField(timer.timerName, GUILayout.MinWidth(90));
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
            if (!Timer.IsNumeric(timer.hrs))
                GUI.backgroundColor = Color.red;
            timer.hrs = GUILayout.TextField(timer.hrs, GUILayout.Width(60));
            GUILayout.Label(":");
            if (!Timer.IsNumeric(timer.min))
                GUI.backgroundColor = Color.red;
            else 
                GUI.backgroundColor = origBackgroundColor;
            timer.min = GUILayout.TextField(timer.min, GUILayout.Width(30));
            GUILayout.Label(":");
            if (!Timer.IsNumeric(timer.sec))
                GUI.backgroundColor = Color.red;
            else 
                GUI.backgroundColor = origBackgroundColor;
            timer.sec = GUILayout.TextField(timer.sec, GUILayout.Width(30));
            GUI.backgroundColor = origBackgroundColor;
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

            if (timer.countDown)
            {
                timer.countDownRepeating = GUILayout.Toggle(timer.countDownRepeating, "Repeat");
            }
            if (GUILayout.Button("Reset"))
            {
                if (timer.countUp)
                {
                    timer.Reset();
                }
                else
                {
                    timer.hours = timer.startHrs;
                    timer.minutes = timer.startMin;
                    timer.seconds = timer.startSec;
                    timer.startTenths = 0;
                }
                timer.elapsedTimeAtPause = 0;
                timer.bellSounded = false;
                timer.bellCnt = 0;
                timer.nextBellAt = 0;
                timer.InitEntryFields();
            }

            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            var bstyle = new GUIStyle(GUI.skin.button);
            bstyle.normal.textColor = Color.yellow;

            GUI.backgroundColor = Color.green;
            if (!timer.isValid)
                GUI.enabled = false;
            if (GUILayout.Button("Start", bstyle))
            {
                timer.startUniversalTime = Planetarium.GetUniversalTime();
                Debug.Log("[BigBen] startUniversalTime: " + timer.startUniversalTime);
                if (timer.elapsedTimeAtPause == 0)
                {
                    timer.ParseEntryFields();

                    timer.hours = timer.startHrs;
                    timer.minutes = timer.startMin;
                    timer.seconds = timer.startSec;
                    timer.startTenths = 0;

                    timer.startTime =
                        timer.repeatingTimeLength = timer.hours * 3600 + timer.minutes * 60 + timer.seconds;

                    timer.bellSounded = false;
                    timer.bellCnt = 0;
                    timer.nextBellAt = 0;

                    // the following line is needed to deal with a countUp timer starting before T-0
                    if (timer.countUp && timer.repeatingTimeLength < 0)
                    {
                        timer.startUniversalTime -= timer.repeatingTimeLength;
                        timer.startTime = timer.startUniversalTime;
                    }
                }
                timer.timerActive = true;
            }

            GUI.backgroundColor = oldColor;
            GUI.enabled = true;
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
                GUILayout.Label("Count Up  ");
            else
                GUILayout.Label("Count Down  ");
            if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().localTenthsSetting)
                timer.showTenths= GUILayout.Toggle(timer.showTenths, "Tenths");

            timer.elapsedTime = Planetarium.GetUniversalTime() + timer.elapsedTimeAtPause - timer.startUniversalTime;
            if (timer.countUp)
            {
                timer.hours = (int)(timer.elapsedTime / 3600);
                timer.minutes = (int)((timer.elapsedTime - timer.hours * 3600) / 60);
                timer.seconds = (int)(timer.elapsedTime - timer.hours * 3600 - timer.minutes * 60);
                timer.tenths = (int)((timer.elapsedTime - timer.hours * 3600 - timer.minutes * 60 - timer.seconds) * 10);
                timer.negative = (timer.elapsedTime < 0);

            }
            else
            { // timer.countDown
                double timeLeft = timer.startTime - timer.elapsedTime - timer.elapsedTimeAtPause;
                double absTimeLeft = Math.Abs(timeLeft);

                timer.hours = (int)(absTimeLeft / 3600f);
                timer.minutes = (int)((absTimeLeft - timer.hours * 3600f) / 60f);
                timer.seconds = (int)(absTimeLeft - timer.hours * 3600f - timer.minutes * 60f);
                timer.tenths = (int)((absTimeLeft - timer.hours * 3600f - timer.minutes * 60f - timer.seconds) * 10);
                if (timeLeft <= 0)
                {
                    if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().bellAtZero)
                    {
                        if (timer.countDownRepeating)
                        {
                            if (timeLeft <= 0)
                            {
                                timer.hours = timer.startHrs;
                                timer.minutes = timer.startMin;
                                timer.seconds = timer.startSec;
                                timer.startTime = timer.repeatingTimeLength;
                                timer.elapsedTimeAtPause = 0;

                                timer.startUniversalTime = Planetarium.GetUniversalTime();

                                timer.bellCnt = (timer.bellCnt % HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().maxBellPings) + 1;
                                timer.nextBellAt = Math.Floor(timeLeft);
                                if (soundplayer.source.audio.isPlaying)
                                    soundplayer.source.audio.Stop();

                                if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().incrementingBells)
                                    SoundBell("bell", timer.bellCnt);
                                else
                                    SoundBell("bell");
                            }
                        }
                        else
                        {
                            if (!timer.bellSounded)
                            {
                                timer.bellSounded = true;
                                SoundBell("bell");
                            }
                        }
                    }
                }
                timer.negative = (timeLeft < 0);
            }

            if (timer.negative)
            {
                labelTextColor.normal.textColor = Color.red;
                labelTextColor.active.textColor = Color.red;
            }
            else
            {
                labelTextColor.normal.textColor = origTextColor;
                labelTextColor.active.textColor = origTextColor;
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label(timer.FormatTime(false, timer.showTenths), labelTextColor);
            GUILayout.FlexibleSpace();

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
            if (timer.countDownRepeating)
                GUILayout.Label("Repeating every " + timer.FormatTime(true));

            var bstyle = new GUIStyle(GUI.skin.button);
            bstyle.normal.textColor = Color.yellow;

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Stop", bstyle))
            {
                timer.elapsedTimeAtPause += Planetarium.GetUniversalTime() - timer.startUniversalTime;
                timer.timerActive = false;
                timer.InitEntryFields();
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
            if (timers.Count <= 4)
                posBigBenWindow.height = MIN_HEIGHT;
        }

        void FixedUpdate()
        {

        }
    }
}
