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
                Loader.hideFlags = HideFlags.None;
                Loader.AddComponent<MainGUI>().hideFlags = HideFlags.None;
                Loader.AddComponent<PlayerControllerBPatch>().hideFlags = HideFlags.None;
                Loader.AddComponent<OtuherPatch>().hideFlags = HideFlags.None;
            }
        }
    }
    #endregion
}