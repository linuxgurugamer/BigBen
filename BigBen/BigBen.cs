using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ToolbarControl_NS;
using ClickThroughFix;
using KSP.UI.Screens;

namespace BigBen
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class BigBen : MonoBehaviour
    {
        const float WIDTH = 350;
        const float KSP_WIDTH = 450;
        const float MIN_HEIGHT = 110;
        internal static float MAX_HEIGHT = 800;
        private Rect posBigBenWindow = new Rect(50, 50, WIDTH, MIN_HEIGHT);

        internal const string MODID = "BigBen_ns";
        internal const string MODNAME = "Big Ben";

        static private ToolbarControl toolbarControl = null;
        bool visible = false;

        static public List<Timer> timers = new List<Timer>();
        Timer timer;
        Vector2 listPos;
        int winId;

        internal AlertSoundPlayer soundplayer;
        internal const string SOUND_DIR = "BigBen/Sounds/";
        internal static string defaultAlert = "bell-1";
        internal static string normalAlert = "bell";
        //internal static float normalAlertLength;
        public bool Paused = false;

        void Awake()
        {
            DontDestroyOnLoad(this);
            soundplayer = gameObject.AddComponent<AlertSoundPlayer>();
#if false
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
#endif
        }


        void Start()
        {
            AddToolbarButton();

            GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);
            GameEvents.onGamePause.Add(OnPause);
            GameEvents.onGameUnpause.Add(OnUnpause);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);

            MAX_HEIGHT = Screen.height - 40;
            soundplayer.Initialize(SOUND_DIR + "bell"); // Initializes the player, does some housekeeping
            winId = SpaceTuxUtility.WindowHelper.NextWindowId("BigBen");
            //StopAllCoroutines();
            StartCoroutine(SlowUpdate());
        }

        IEnumerator SlowUpdate()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.25f);
                if (HighLogic.CurrentGame == null)
                    continue;
                soundplayer.Volume = HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().volume;
                for (int i = timers.Count - 1; i >= 0; i--)
                {
                    var localTimer = timers[i];
                    if (localTimer.timerActive)
                    {
                        if (localTimer.realTime)
                        {
                            localTimer.elapsedTime = Time.realtimeSinceStartup - localTimer.startRealTimeSinceStartup;
                        }
                        else
                        {
                            localTimer.elapsedTime = Planetarium.GetUniversalTime() + localTimer.elapsedTimeAtPause - localTimer.startUniversalTime;
                        }
                        if (localTimer.countUp)
                        {
                            localTimer.hours = (int)(localTimer.elapsedTime / 3600);
                            localTimer.minutes = (int)((localTimer.elapsedTime - localTimer.hours * 3600) / 60);
                            localTimer.seconds = (int)(localTimer.elapsedTime - localTimer.hours * 3600 - localTimer.minutes * 60);
                            localTimer.tenths = (int)((localTimer.elapsedTime - localTimer.hours * 3600 - localTimer.minutes * 60 - localTimer.seconds) * 10);
                            localTimer.negative = (localTimer.elapsedTime < 0);
                        }
                        else
                        { // timer.countDown
                            double timeLeft = localTimer.startTime - localTimer.elapsedTime - localTimer.elapsedTimeAtPause;
                            double absTimeLeft = Math.Abs(timeLeft);

                            localTimer.hours = (int)(absTimeLeft / 3600f);
                            localTimer.minutes = (int)((absTimeLeft - localTimer.hours * 3600f) / 60f);
                            localTimer.seconds = (int)(absTimeLeft - localTimer.hours * 3600f - localTimer.minutes * 60f);
                            localTimer.tenths = (int)((absTimeLeft - localTimer.hours * 3600f - localTimer.minutes * 60f - localTimer.seconds) * 10);

                            if (timeLeft <= 0)
                            {
                                if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().bellAtZero)
                                {
                                    if (timeLeft <= 0)
                                    {
                                        Debug.Log("localTimer.timerActive: " + localTimer.timerActive);
                                        Debug.Log("localTimer.timerName: " + localTimer.timerName);
                                        Debug.Log("localTimer.bellCnt: " + localTimer.bellCnt);

                                        localTimer.hours = localTimer.startHrs;
                                        localTimer.minutes = localTimer.startMin;
                                        localTimer.seconds = localTimer.startSec;
                                        localTimer.startTime = localTimer.repeatingTimeLength;
                                        localTimer.elapsedTimeAtPause = 0;

                                        localTimer.startUniversalTime = Planetarium.GetUniversalTime();

                                        localTimer.bellCnt = (localTimer.bellCnt % HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().maxBellPings) + 1;
                                        localTimer.nextBellAt = Math.Floor(timeLeft);
                                        if (soundplayer.SoundPlaying())
                                            soundplayer.StopSound();

                                        if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().incrementingBells && !localTimer.pauseAtZero)
                                            SoundBell("bell", localTimer.bellCnt);
                                        else
                                            SoundBell("bell");
                                        if (localTimer.countDownRepeating)
                                        {

                                            if (localTimer.pauseAtZero)
                                            {
                                                localTimer.bellSounded = true;
                                                localTimer.timerActive = false;

                                                string dialogMsg = localTimer.timerName + " finished";
                                                string windowTitle = "Saved Timer";
                                                EnablePopup(dialogMsg, windowTitle);
                                            }
                                            else
                                            {
                                                if (localTimer.realTime)
                                                {
                                                    localTimer.startRealTimeSinceStartup = Time.realtimeSinceStartup;
                                                    localTimer.elapsedTime = 0;
                                                }
                                            }
                                        }
                                        else
                                            localTimer.timerActive = false;

                                    }
                                }
                            }
                            localTimer.negative = (timeLeft < 0);
                        }
                    }
                }
            }
            yield return null;
        }

        void OnDestroy()
        {
            GameEvents.onGameSceneSwitchRequested.Remove(onGameSceneSwitchRequested);
            GameEvents.onGamePause.Remove(OnPause);
            GameEvents.onGameUnpause.Remove(OnUnpause);
            GameEvents.OnGameSettingsApplied.Remove(OnGameSettingsApplied);
        }

        bool initTextures = false;
        void OnGameSettingsApplied()
        {
            if (soundplayer != null &&
                HighLogic.CurrentGame != null && HighLogic.CurrentGame.Parameters != null)
            {
                soundplayer.Volume = HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().volume;
                initTextures = true;
            }
        }

        // Get rid of timers when going to the mainmenu

        void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> fta)
        {
            if (fta.to == GameScenes.MAINMENU)
            {
                timers = null;
                visible = false;
                StopAllCoroutines();
                Debug.Log("BigBen, going to MainMenu");
            }
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
            if (soundplayer == null)
            {
                Debug.Log("soundBell, soundplayer is null");
                return;
            }
            soundplayer.Initialize(SOUND_DIR + n); // Initializes the player, does some housekeeping

            soundplayer.PlaySound(cnt - 1); //Plays sound
        }

        void OnGUI()
        {
            if (HighLogic.CurrentGame == null || !visible)
                return;
            if (!HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().useAlternateSkin)
            {
                GUI.skin = HighLogic.Skin;
                if (posBigBenWindow.width < KSP_WIDTH)
                    posBigBenWindow = new Rect(50, 50, KSP_WIDTH, MIN_HEIGHT);

            }
            else
            {
                if (posBigBenWindow.width != WIDTH)
                    posBigBenWindow = new Rect(50, 50, WIDTH, MIN_HEIGHT);
            }
            if (initTextures)
            {
                RegisterToolbar.InitTextures();
                initTextures = false;
            }

            posBigBenWindow = ClickThruBlocker.GUILayoutWindow(winId, posBigBenWindow, DrawList, "Big Ben",
               GUILayout.Width(posBigBenWindow.width), GUILayout.MinHeight(MIN_HEIGHT), GUILayout.MaxHeight(MAX_HEIGHT));
            Debug.Log("BigBen, width: " + posBigBenWindow.width);
            if (popupEnabled)
                ShowPopup();
        }

        void ShowPopup()
        {
            DialogGUIBase[] options = { new DialogGUIButton("OK", ConfirmOK),
                                        new DialogGUIButton("Restart Countdown Timer", Continue)};

            MultiOptionDialog confirmationBox = new MultiOptionDialog("", dialogMsg, windowTitle, HighLogic.UISkin, options);

            popup = PopupDialog.SpawnPopupDialog(confirmationBox, false, HighLogic.UISkin);
            popupEnabled = false;
        }

        float heightOfOne = 1;
        void DrawList(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().multiple = GUILayout.Toggle(HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().multiple, "");
            GUILayout.Label("Multiple");
            GUILayout.EndHorizontal();
            if (timers.Count > 4)
                listPos = GUILayout.BeginScrollView(listPos, GUILayout.Width(WIDTH - 10), GUILayout.MinHeight(4 * heightOfOne), GUILayout.ExpandHeight(true));

            int cnt = 0;
            for (int i = 0; i < timers.Count; i++)
            {
                timer = timers[i];

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
                if (i == 0 && heightOfOne == 1)
                    heightOfOne = posBigBenWindow.height;
            }
            if (timers.Count > 4)
                GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        void DrawEntryWindow(int cnt)
        {
            if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().multiple || timer.countDownRepeating)
            {
                GUILayout.Label("Name: ");
                timer.timerName = GUILayout.TextField(timer.timerName, GUILayout.MinWidth(90));
                GUILayout.FlexibleSpace();
            
                timer.realTime = GUILayout.Toggle(timer.realTime, "");
                GUILayout.Label("RealTime");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
            }
            bool b = false;
            if (timer.countUp)
            {
                timer.countUp = !GUILayout.Toggle(b, "Up", GUI.skin.button);
                if (!timer.countUp)
                {
                    timer.startUniversalTime = 0;
                    timer.countDown = true;
                }
            }
            else
            {
                timer.countDown = !GUILayout.Toggle(b, "Down", GUI.skin.button);
                if (!timer.countDown)
                {
                    timer.startUniversalTime = 0;
                    timer.countUp = true;
                }
            }
            if (!Timer.IsNumeric(timer.hrs))
                GUI.backgroundColor = Color.red;
            GUILayout.FlexibleSpace();
            timer.hrs = GUILayout.TextField(timer.hrs, GUILayout.Width(45));
            GUILayout.Label(":");
            if (!Timer.IsNumeric(timer.min))
                GUI.backgroundColor = Color.red;
            else
                GUI.backgroundColor = RegisterToolbar.origBackgroundColor;
            timer.min = GUILayout.TextField(timer.min, GUILayout.Width(30));
            GUILayout.Label(":");
            if (!Timer.IsNumeric(timer.sec))
                GUI.backgroundColor = Color.red;
            else
                GUI.backgroundColor = RegisterToolbar.origBackgroundColor;
            timer.sec = GUILayout.TextField(timer.sec, GUILayout.Width(30));
            GUI.backgroundColor = RegisterToolbar.origBackgroundColor;
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

            if (timer.countDown)
            {
                timer.countDownRepeating = GUILayout.Toggle(timer.countDownRepeating, "");
                GUILayout.Label("Repeat");
                GUI.enabled = timer.countDownRepeating;
                GUILayout.Label(" ");
                GUILayout.FlexibleSpace();
                timer.pauseAtZero = GUILayout.Toggle(timer.pauseAtZero, "");
                GUILayout.Label("Pause");
                GUI.enabled = true;
            }
            GUILayout.Label(" ");
            GUILayout.FlexibleSpace();
            if (timer.countDown)
            {
                if (GUILayout.Button("Reset to 0"))
                {
                    timer.startHrs = timer.startMin = timer.startSec = timer.hours = timer.minutes = timer.seconds = 0;
                    // timer.startTenths = 0;

                    timer.InitEntryFields(true);

                }
            }
            if (GUILayout.Button("Reset", GUILayout.Width(60)))
            {
                if (timer.countUp)
                {
                    timer.Reset();
                }
                else
                {
#if false
                    if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().resetToZero)
                    {
                        timer.startHrs =
                        timer.startMin =
                        timer.startSec = 0;
                    }
#endif
                    timer.hours = timer.startHrs;
                    timer.minutes = timer.startMin;
                    timer.seconds = timer.startSec;
                    // timer.startTenths = 0;
                }
                timer.InitEntryFields(true);
            }

            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;

            GUI.backgroundColor = Color.green;
            if (!timer.isValid)
                GUI.enabled = false;
            if (GUILayout.Button("Start", RegisterToolbar.buttonYellow, GUILayout.Width(70)))
            {
                DoStart();
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

        void DoStart()
        {
            timer.startUniversalTime = Planetarium.GetUniversalTime();
            timer.startRealTimeSinceStartup = Time.realtimeSinceStartup;
            Debug.Log("[BigBen] startUniversalTime: " + timer.startUniversalTime);
            if (timer.elapsedTimeAtPause == 0)
            {
                timer.ParseEntryFields();

                timer.hours = timer.startHrs;
                timer.minutes = timer.startMin;
                timer.seconds = timer.startSec;
                //timer.startTenths = 0;

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

        void DrawActiveTimingWindow(int cnt)
        {
            if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().multiple || timer.countDownRepeating)
            {
                GUILayout.Label(timer.timerName, GUILayout.MinWidth(60));
                if (timer.realTime)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("RealTime");
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
            }

            if (timer.countUp)
                GUILayout.Label("Count Up  ");
            else
                GUILayout.Label("Count Down  ");
            if (HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().localTenthsSetting)
                timer.showTenths = GUILayout.Toggle(timer.showTenths, "Tenths");

            timer.elapsedTime = Planetarium.GetUniversalTime() + timer.elapsedTimeAtPause - timer.startUniversalTime;
            if (timer.negative)
            {
                RegisterToolbar.labelTextColor.normal.textColor = Color.red;
                RegisterToolbar.labelTextColor.active.textColor = Color.red;
            }
            else
            {
                RegisterToolbar.labelTextColor.normal.textColor = RegisterToolbar.origTextColor;
                RegisterToolbar.labelTextColor.active.textColor = RegisterToolbar.origTextColor;
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label(timer.FormatTime(false, timer.showTenths), RegisterToolbar.labelTextColor);
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
            {
                string str = "Repeating every " + timer.FormatTime(true);
                if (timer.pauseAtZero)
                    str += ", pausing at 0";
                GUILayout.Label(str);
            }
            //var bstyle = new GUIStyle(GUI.skin.button);
            //bstyle.normal.textColor = Color.yellow;

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Stop", RegisterToolbar.buttonYellow))
            {
                timer.elapsedTimeAtPause += Planetarium.GetUniversalTime() - timer.startUniversalTime;
                timer.timerActive = false;
                timer.Reset();
                timer.InitEntryFields();
            }
            GUI.backgroundColor = RegisterToolbar.origBackgroundColor;
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

        private PopupDialog popup;
        string dialogMsg, windowTitle;
        bool popupEnabled = false;

        void EnablePopup(string dialogMsg, string windowTitle)
        {
            this.dialogMsg = dialogMsg;
            this.windowTitle = windowTitle;
            popupEnabled = true;
        }

        public void ConfirmOK()
        {
            Debug.Log("ConfirmOK");
            timer.Reset();
        }

        public void Continue()
        {
            Debug.Log("Continue");
            timer.Reset();
            DoStart();
        }

        internal static void SortList()
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
        internal static void AddNewTimer()
        {
            Timer lTimer = new Timer();
            lTimer.countUp = HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().defaultCountUp;
            lTimer.countDown = !lTimer.countUp;
            timers.Add(lTimer);
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

#if false
        void FixedUpdate()
        {

        }
#endif
    }
}
