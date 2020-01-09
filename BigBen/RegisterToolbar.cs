using UnityEngine;
using ToolbarControl_NS;

namespace BigBen
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(BigBen.MODID, BigBen.MODNAME);
        }
    }
}