using UnityEngine;
using BepInEx;

namespace Lethal_Company_Mod_Menu.MainMenu
{
    #region Loader
    [BepInPlugin("com.notfishvr.lethalcompany", "notfishvr", "1.0.0")]
    public class Loader : BaseUnityPlugin
    {
        public void FixedUpdate()
        {
            if (!GameObject.Find("Loader"))
            {
                GameObject Loader = new GameObject("Loader");
                Loader.AddComponent<MainMenu.MainGUI>();
                Loader.AddComponent<MainMenu.PlayerControllerBPatch>();
            }
        }
    }
    #endregion
}