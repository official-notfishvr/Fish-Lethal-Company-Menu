using UnityEngine;
using BepInEx;

namespace NIKO_Menu_V2.MainMenu
{
    #region Loader
    [BepInPlugin("com.notfishvr.nikomenu", "notfishvr", "1.0.0")]
    public class Loader : BaseUnityPlugin
    {
        public void FixedUpdate()
        {
            if (!GameObject.Find("Loader"))
            {
                GameObject Loader = new GameObject("Loader");
                Loader.AddComponent<NIKO_Menu_V2.MainMenu.MainGUI>();
                Loader.AddComponent<NIKO_Menu_V2.MainMenu.PlayerControllerBPatch>();
            }
        }
    }
    #endregion
}