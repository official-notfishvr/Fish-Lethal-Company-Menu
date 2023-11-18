using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NIKO_Menu_V2.MainMenu
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class PlayerControllerBPatch : MonoBehaviour
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        private static void getNightVision(ref PlayerControllerB __instance)
        {
            MainGUI.playerRef = __instance;
            MainGUI.nightVision = MainGUI.playerRef.nightVision.enabled;
            MainGUI.nightVisionIntensity = MainGUI.playerRef.nightVision.intensity;
            MainGUI.nightVisionColor = MainGUI.playerRef.nightVision.color;
            MainGUI.nightVisionRange = MainGUI.playerRef.nightVision.range;
            MainGUI.playerRef.nightVision.color = Color.green;
            MainGUI.playerRef.nightVision.intensity = 1000f;
            MainGUI.playerRef.nightVision.range = 10000f;
        }

        [HarmonyPatch("SetNightVisionEnabled")]
        [HarmonyPostfix]
        private static void updateNightVision()
        {
            if (MainGUI.nightVision)
            {
                MainGUI.playerRef.nightVision.color = Color.green;
                MainGUI.playerRef.nightVision.intensity = 1000f;
                MainGUI.playerRef.nightVision.range = 10000f;
            }
            else
            {
                MainGUI.playerRef.nightVision.color = MainGUI.nightVisionColor;
                MainGUI.playerRef.nightVision.intensity = MainGUI.nightVisionIntensity;
                MainGUI.playerRef.nightVision.range = MainGUI.nightVisionRange;
            }
        }
        [HarmonyPatch("AllowPlayerDeath")]
        [HarmonyPrefix]
        public static bool OverrideDeath()
        {
            return !MainGUI.enableGod;
        }
    

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void InfiniteSprint(ref float ___sprintMeter)
        {
            if (MainGUI.InfiniteSprint)
            {
                Mathf.Clamp(___sprintMeter += 0.02f, 0f, 1f);
            }
        }
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        public static void ReadInput(PlayerControllerB playerController)
        {
            FlashlightItem pocketedFlashlight = playerController.pocketedFlashlight as FlashlightItem;
            playerController.helmetLight.enabled = true;
            pocketedFlashlight.usingPlayerHelmetLight = true;
            pocketedFlashlight.PocketFlashlightServerRpc(true);
        }
    }
}