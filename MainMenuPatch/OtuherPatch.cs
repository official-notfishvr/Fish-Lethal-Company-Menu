using GameNetcodeStuff;
using HarmonyLib;
using Lethal_Company_Mod_Menu.MainMenu;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalCompanyHacks.MainMenuPatch
{
    //[HarmonyPatch(typeof(PlayerControllerB))]
    public class OtuherPatch : MonoBehaviour
    {
        [HarmonyPatch(typeof(HUDManager), "AssignNewNodes")]
        [HarmonyPrefix]
        public static IEnumerable<CodeInstruction> UnlimitedScanRange(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].opcode == OpCodes.Ldc_R4 && (float)list[i].operand == 20f)
                {
                    list[i].operand = MainGUI.UnlimitedScanRange ? 500f : 20f;
                }
            }
            return list;
        }

        [HarmonyPatch(typeof(HUDManager), "MeetsScanNodeRequirements")]
        [HarmonyPatch(new System.Type[] { typeof(ScanNodeProperties), typeof(PlayerControllerB) })]
        public static bool UnlimitedScanRange2(ref bool __result)
        {
            __result = MainGUI.UnlimitedScanRange;
            return !__result;
        }

        [HarmonyPatch(typeof(GrabbableObject), "SyncBatteryServerRpc")]
        [HarmonyPrefix]
        public static bool UnlimitedItemPower(GrabbableObject __instance, ref int charge)
        {
            if (MainGUI.UnlimitedItemPower && __instance.itemProperties.requiresBattery)
            {
                __instance.insertedBattery.empty = false;
                __instance.insertedBattery.charge = 1f;
                charge = 100;
            }
            return true;
        }

        [HarmonyPatch(typeof(RoundManager), "SyncScrapValuesClientRpc")]
        [HarmonyPrefix]
        public static bool HighScrapValue(NetworkObjectReference[] spawnedScrap, ref int[] allScrapValue)
        {
            if (MainGUI.HighScrapValue)
            {
                return true;
            }

            if (spawnedScrap != null)
            {
                for (int i = 0; i < spawnedScrap.Length; i++)
                {
                    if (spawnedScrap[i].TryGet(out NetworkObject networkObject, null))
                    {
                        GrabbableObject component = networkObject.GetComponent<GrabbableObject>();
                        if (component != null)
                        {
                            allScrapValue[i] = 420;
                        }
                    }
                }
            }

            return true;
        }
    }
}
