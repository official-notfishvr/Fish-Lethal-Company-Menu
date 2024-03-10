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
    public class OtuherPatch : MonoBehaviour
    {
        [HarmonyPatch(typeof(RoundManager))]
        [HarmonyPatch("SyncScrapValuesClientRpc")]
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

        [HarmonyPatch(typeof(HUDManager))]
        public class ScanRangePatch
        {
            [HarmonyPatch("AssignNewNodes")]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                if (MainGUI.UnlimitedScanRange)
                {
                    foreach (CodeInstruction instruction in instructions)
                    {
                        if (instruction.opcode == OpCodes.Ldc_R4 && instruction.operand.Equals(20.0f))
                        {
                            instruction.operand = 500.0f;
                        }

                        yield return instruction;
                    }
                }
            }

            [HarmonyPatch("MeetsScanNodeRequirements")]
            public static bool Prefix(ScanNodeProperties node, ref bool __result)
            {
                if (node == null) return true;

                __result = true;
                return false;
            }
        }
    }
}
