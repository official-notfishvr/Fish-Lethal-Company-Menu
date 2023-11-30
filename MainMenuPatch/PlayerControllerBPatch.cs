using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using Lethal_Company_Mod_Menu.MainMenu;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.Netcode;
using UnityEngine;

namespace LethalCompanyHacks.MainMenuPatch
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class PlayerControllerBPatch : MonoBehaviour
    {
        [HarmonyPatch("AllowPlayerDeath")]
        [HarmonyPrefix]
        public static bool OverrideDeath() => !MainGUI.enableGod;

        [HarmonyPatch("DamagePlayer")]
        [HarmonyPrefix]
        public static bool Prefix(PlayerControllerB __instance) =>
            __instance.actualClientId == GameNetworkManager.Instance.localPlayerController.actualClientId && MainGUI.enableGod;


        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void ReadInput(PlayerControllerB playerController)
        {
            FlashlightItem pocketedFlashlight = playerController.pocketedFlashlight as FlashlightItem;
            playerController.helmetLight.enabled = true;
            pocketedFlashlight?.PocketFlashlightServerRpc(true);
        }
    }
}
