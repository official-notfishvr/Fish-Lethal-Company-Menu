using UnityEngine;
using BepInEx;
using LethalCompanyHacks.MainMenuPatch;

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
                Loader.AddComponent<MainGUI>();
                Loader.AddComponent<PlayerControllerBPatch>();
                Loader.AddComponent<OtuherPatch>();
            }
        }
    }
    #endregion
}