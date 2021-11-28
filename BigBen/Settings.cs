using System.Collections;
using System.Reflection;

namespace BigBen
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    // HighLogic.CurrentGame.Parameters.CustomParams<Big_Ben>().
    public class Big_Ben : GameParameters.CustomParameterNode
    {
        public override string Title { get { return ""; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Big Ben"; } }
        public override string DisplaySection { get { return "Big Ben"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }


        [GameParameters.CustomParameterUI("Allow Multiple Timers")]
        public bool multiple = false;

        [GameParameters.CustomParameterUI("Use alternate skin")]
        public bool useAlternateSkin = true;

        [GameParameters.CustomParameterUI("Show tenth's of a sec")]
        public bool showTenths = false;

        [GameParameters.CustomParameterUI("Allow each timer to have a tenths setting")]
        public bool localTenthsSetting = false;

        [GameParameters.CustomParameterUI("Default to Count Up")]
        public bool defaultCountUp = true;

        [GameParameters.CustomParameterUI("Bell when reaching zero")]
        public bool bellAtZero = true;

        [GameParameters.CustomParameterUI("Incrementing bell when reaching zero",
            toolTip = "Will play up to 12 bells before starting back at one")]
        public bool incrementingBells = false;

        [GameParameters.CustomFloatParameterUI("Bell Volume", minValue = 0, maxValue = 1f, stepCount = 101, displayFormat = "F3",
            toolTip = "Volume of the bell, goes from 0 to 1")]
        public float volume = 0.5f;

        [GameParameters.CustomIntParameterUI("Max bell pings", minValue = 1, maxValue = 100,
            toolTip ="The maximum number of times the bill will ring for a single countdown reaching zero")]
        public int maxBellPings = 6;


        [GameParameters.CustomParameterUI("Show time as:  00:00:00", 
            toolTip = "If not enabled, then time will be shown as:  00h 00m 00s")]
        public bool showTimeAsColons = true;


        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {

        }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {

            return true;
        }
        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }

}