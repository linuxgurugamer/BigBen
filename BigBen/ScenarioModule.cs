using System.Collections.Generic;
using UnityEngine;

namespace BigBen
{

    [KSPScenario(ScenarioCreationOptions.AddToAllGames | ScenarioCreationOptions.AddToExistingCareerGames,
        GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    public class Scenario : ScenarioModule
    {
        const string BIGBEN = "BIGBEN";
        const string TIMER = "TIMER";

        public override void OnSave(ConfigNode parentNode)
        {
            ConfigNode node = new ConfigNode(BIGBEN);
            for (int i = BigBen.timers.Count - 1; i >= 0; i--)
            {
                var t = BigBen.timers[i];
                if (t.hours != 0f || t.minutes != 0f || t.seconds != 0f ||
                    t.startHrs != 0f || t.startMin !=0f || t.startSec !=0f ||
                    t.hrs != "" || t.min != "" || t.sec !="")
                    node.AddNode(BigBen.timers[i].ToNode(TIMER));
            }
            parentNode.AddNode(node);
            base.OnSave(parentNode);
        }

        public override void OnLoad(ConfigNode parentNode)
        {
            base.OnLoad(parentNode);
            if (BigBen.timers == null)
                BigBen.timers = new List<Timer>();
            else
                BigBen.timers.Clear();
            ConfigNode node = parentNode.GetNode(BIGBEN);
            BigBen.MAX_HEIGHT = Screen.height - 40;

            if (node != null)
            {
                ConfigNode[] nodes = node.GetNodes(TIMER);

                foreach (var n in nodes)
                {
                    Timer timer = new Timer(n);
                    BigBen.timers.Add(timer);
                }
            }
            if (BigBen.timers.Count == 0)
                BigBen.AddNewTimer();
            BigBen.SortList();
        }
    }
}
