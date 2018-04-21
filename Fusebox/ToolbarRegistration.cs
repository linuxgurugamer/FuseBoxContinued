
using UnityEngine;
using ToolbarControl_NS;

namespace Ratzap
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(FuseBox_Core.MODID, FuseBox_Core.MODNAME);
        }
    }
}