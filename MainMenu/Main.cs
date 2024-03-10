using UnityEngine;
using BepInEx;
using System;
using System.Text.RegularExpressions;
using System.IO;
using LethalCompanyHacks.MainMenu.Stuff;
using LethalCompanyHacks.MainMenu.Patch;

namespace Lethal_Company_Mod_Menu.MainMenu
{
    #region Loader
    [BepInPlugin("com.notfishvr.lethalcompany", "notfishvr", "1.0.0")]
    public class Loader : BaseUnityPlugin
    {
        public void FixedUpdate()
        {
            Awake();
            if (!GameObject.Find("Loader"))
            {
                GameObject Loader = new GameObject("Loader");
                Loader.AddComponent<LocalPlayer>();
                Loader.AddComponent<MainGUI>();
                Loader.AddComponent<PlayerControllerBPatch>();
            }
        }
        private void Awake()
        {
            try
            {
                string text = Paths.ConfigPath + "/BepInEx.cfg";
                string text2 = File.ReadAllText(text);
                text2 = Regex.Replace(text2, "HideManagerGameObject = .+", "HideManagerGameObject = true");
                File.WriteAllText(text, text2);
            }
            catch (Exception ex)
            {

            }
        }
    }
    #endregion
}