using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine.AI;
using UnityEngine;

namespace LethalCompanyMenu.MainMenu.Stuff
{
    internal class SpawnEnemyClass
    {
        public static void SpawnEnemyWithConfigManager(string enemyName)
        {
            RoundManager roundManager = GameObject.FindObjectOfType<RoundManager>();

            bool flag = false;
            foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in roundManager.currentLevel.Enemies)
            {
                if (spawnableEnemyWithRarity.enemyType.enemyName.ToLower().Contains(enemyName.ToLower()))
                {
                    try
                    {
                        flag = true;
                        string enemyName2 = spawnableEnemyWithRarity.enemyType.enemyName;
                        SpawnEnemy(spawnableEnemyWithRarity, 1, true);
                        break;
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            if (!flag)
            {
                foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity2 in roundManager.currentLevel.OutsideEnemies)
                {
                    if (spawnableEnemyWithRarity2.enemyType.enemyName.ToLower().Contains(enemyName.ToLower()))
                    {
                        try
                        {
                            flag = true;
                            string enemyName3 = spawnableEnemyWithRarity2.enemyType.enemyName;
                            SpawnEnemy(spawnableEnemyWithRarity2, 1, false);
                            break;
                        }
                        catch (Exception ex)
                        {
                            break;
                        }
                    }
                }
            }
        }
        public static void SpawnEnemy(SpawnableEnemyWithRarity enemy, int amount, bool inside)
        {
            RoundManager roundManager = GameObject.FindObjectOfType<RoundManager>();
            if (inside)
            {
                try
                {
                    for (int i = 0; i < amount; i++)
                    {
                        roundManager.SpawnEnemyOnServer(LocalPlayer.localPlayer.PlayerController.transform.position, 0, roundManager.currentLevel.Enemies.IndexOf(enemy));
                        GameObject enemyObject = UnityEngine.Object.Instantiate<GameObject>(enemy.enemyType.enemyPrefab, LocalPlayer.localPlayer.PlayerController.transform.position, Quaternion.Euler(Vector3.zero));
                    }
                    return;
                }
                catch
                {
                    return;
                }
            }
            for (var j = 0; j < amount; j++)
            {
                var enemyPrefab = roundManager.currentLevel.OutsideEnemies[roundManager.currentLevel.OutsideEnemies.IndexOf(enemy)].enemyType.enemyPrefab;
                var outsideAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
                var randomNodePosition = outsideAINodes[UnityEngine.Random.Range(0, outsideAINodes.Length - 1)].transform.position;
                var localPlayerPosition = LocalPlayer.localPlayer.PlayerController.transform.position;

                var instantiatedEnemy1 = UnityEngine.Object.Instantiate<GameObject>(enemyPrefab, randomNodePosition, Quaternion.Euler(Vector3.zero));
                instantiatedEnemy1.GetComponentInChildren<NetworkObject>().Spawn(true);

                var instantiatedEnemy2 = UnityEngine.Object.Instantiate<GameObject>(enemyPrefab, localPlayerPosition, Quaternion.Euler(Vector3.zero));
                instantiatedEnemy2.GetComponentInChildren<NetworkObject>().Spawn(true);
            }
        }
        public static void SpawnEnemyAtLocalPlayer(SpawnableEnemyWithRarity enemy, int amount)
        {
            RoundManager roundManager = GameObject.FindObjectOfType<RoundManager>();
            if (roundManager == null)
            {
                return;
            }

            if (LocalPlayer.localPlayer == null || LocalPlayer.localPlayer.PlayerController == null)
            {
                return;
            }

            Vector3 localPlayerPosition = LocalPlayer.localPlayer.PlayerController.transform.position;

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(localPlayerPosition, out hit, 10.0f, NavMesh.AllAreas))
            {
                return;
            }

            for (int i = 0; i < amount; i++)
            {

                int enemyIndex = roundManager.currentLevel.OutsideEnemies.IndexOf(enemy);
                if (enemyIndex == -1)
                {
                    continue;
                }

                EnemyType enemyType = roundManager.currentLevel.OutsideEnemies[enemyIndex].enemyType;
                if (enemyType == null)
                {
                    continue;
                }

                GameObject enemyPrefab = enemyType.enemyPrefab;
                if (enemyPrefab == null)
                {
                    continue;
                }

                GameObject enemyObject = UnityEngine.Object.Instantiate<GameObject>(enemyPrefab, hit.position, Quaternion.Euler(Vector3.zero));
                if (enemyObject == null)
                {
                    continue;
                }

                NetworkObject networkObject = enemyObject.GetComponentInChildren<NetworkObject>();
                if (networkObject == null)
                {
                    continue;
                }

                networkObject.Spawn(true);

                var beginSendClientRpc = typeof(RoundManager).GetMethod("__beginSendClientRpc", BindingFlags.NonPublic | BindingFlags.Instance);
                var endSendClientRpc = typeof(RoundManager).GetMethod("__endSendClientRpc", BindingFlags.NonPublic | BindingFlags.Instance);

                var clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new List<ulong> { LocalPlayer.localPlayer.PlayerController.playerClientId }
                    }
                };

                var serverRpcParams = default(ServerRpcParams);

                var fastBufferWriter = (FastBufferWriter)beginSendClientRpc.Invoke(roundManager, new object[] { 46494176U, clientRpcParams, 0 });
                fastBufferWriter.WriteValueSafe(localPlayerPosition);
                fastBufferWriter.WriteValueSafe(enemyIndex);
                BytePacker.WriteValueBitPacked(fastBufferWriter, 1);

                endSendClientRpc.Invoke(roundManager, new object[] { fastBufferWriter, 46494176u, serverRpcParams, RpcDelivery.Reliable });
            }
        }
    }
}
