﻿using UnityEngine;
using ToolbarControl_NS;

namespace BigBen
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        bool initted = false;
        public static GUIStyle labelTextColor;
        public static Color origTextColor;
        public static Color origBackgroundColor;

        void Start()
        {
            ToolbarControl.RegisterMod(BigBen.MODID, BigBen.MODNAME);
        }
        void OnGUI()
        {
            if (!initted)
            {
                labelTextColor = new GUIStyle("Label");
                labelTextColor.fontStyle = FontStyle.Bold;
                origTextColor = labelTextColor.normal.textColor;
                origBackgroundColor = GUI.backgroundColor;

                initted = true;
            }
        }
    }
}