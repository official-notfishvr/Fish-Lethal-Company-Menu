using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Lethal_Company_Mod_Menu.MainMenu
{
    internal class JoinGameAfteStart
    {
        [HarmonyPatch(typeof(StartOfRound), "OnClientConnect")]
        [HarmonyWrapSafe]
        internal class OnClientConnect_Patch
        {
            [HarmonyPostfix]
            private static void Postfix(ulong clientId)
            {
                StartOfRound instance = StartOfRound.Instance;
                if (instance.IsServer && !instance.inShipPhase)
                {
                    RoundManager instance2 = RoundManager.Instance;
                    MethodInfo method = typeof(RoundManager).GetMethod("__beginSendClientRpc", BindingFlags.Instance | BindingFlags.NonPublic);
                    MethodInfo method2 = typeof(RoundManager).GetMethod("__endSendClientRpc", BindingFlags.Instance | BindingFlags.NonPublic);
                    ClientRpcParams clientRpcParams = new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new List<ulong> { clientId }
                        }
                    };
                    FastBufferWriter fastBufferWriter = (FastBufferWriter)method.Invoke(instance2, new object[] { 1193916134U, clientRpcParams, 0 });
                    BytePacker.WriteValueBitPacked(fastBufferWriter, StartOfRound.Instance.randomMapSeed);
                    BytePacker.WriteValueBitPacked(fastBufferWriter, StartOfRound.Instance.currentLevelID);
                    method2.Invoke(instance2, new object[] { fastBufferWriter, 1193916134U, clientRpcParams, 0 });
                    FastBufferWriter fastBufferWriter2 = (FastBufferWriter)method.Invoke(instance2, new object[] { 2729232387U, clientRpcParams, 0 });
                    method2.Invoke(instance2, new object[] { fastBufferWriter2, 2729232387U, clientRpcParams, 0 });
                }
            }
            [HarmonyPatch(typeof(GameNetworkManager), "LeaveLobbyAtGameStart")]
            [HarmonyWrapSafe]
            internal static class LeaveLobbyAtGameStart_Patch
            {
                [HarmonyPrefix]
                private static bool Prefix()
                {
                    return false;
                }
            }
            [HarmonyPatch(typeof(GameNetworkManager), "ConnectionApproval")]
            [HarmonyWrapSafe]
            internal static class ConnectionApproval_Patch
            {
                [HarmonyPostfix]
                private static void Postfix(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
                {
                    bool flag = request.ClientNetworkId == NetworkManager.Singleton.LocalClientId;
                    if (!flag)
                    {
                        if (response.Reason.Contains("Game has already started") && GameNetworkManager.Instance.gameHasStarted)
                        {
                            Debug.Log("Uhh, no i do actually want this one to connect.");
                            response.Reason = "";
                            response.CreatePlayerObject = false;
                            response.Approved = true;
                            response.Pending = false;
                        }
                    }
                }

            }
        }
    }
}
