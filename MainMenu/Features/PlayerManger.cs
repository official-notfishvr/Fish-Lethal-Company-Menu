using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using LethalCompanyMenu.MainMenu.Patch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;

namespace LethalCompanyMenu.MainMenu.Stuff
{
    public class PlayerManager : MonoBehaviour
    {
        private List<PlayerControllerB> _players = new List<PlayerControllerB>();
        private void Start() { UpdatePlayers(); }
        public void UpdatePlayers()
        {
            _players.Clear();

            if (!LocalPlayer.IsValid())
            {
                FindLocalPlayer();
            }

            _players.AddRange(GameObject.FindObjectsOfType<PlayerControllerB>());
            _players.RemoveAll(player => !IsValidPlayer(player));
        }
        private void FindLocalPlayer()
        {
            var playerControllers = GameObject.FindObjectsOfType<PlayerControllerB>();
            foreach (var playerController in playerControllers)
            {
                if (playerController != null && playerController.isPlayerControlled && playerController.IsOwner)
                {
                    LocalPlayer.localPlayer = new LocalPlayer(playerController);
                    break;
                }
            }
        }
        private bool IsValidPlayer(PlayerControllerB player)
        {
            return player != null &&
                   player != LocalPlayer.localPlayer?.PlayerController &&
                   player.gameObject != null &&
                   player.playerSteamId != 0;
        }
        public IEnumerable<PlayerControllerB> GetPlayers()
        {
            return _players;
        }
    }
    public class LocalPlayer : MonoBehaviour
    {
        public static LocalPlayer localPlayer;
        public PlayerControllerB PlayerController { get; set; }
        public LocalPlayer(PlayerControllerB playerController) { PlayerController = playerController; }
        public static bool IsValid()
        {
            return localPlayer != null &&
                   localPlayer.PlayerController != null &&
                   localPlayer.PlayerController.gameObject != null &&
                   localPlayer.PlayerController.isPlayerControlled;
        }
    }
    public class PlayerControllerHandler 
    {
        public static GrabbableObject[] grabbableObjectsUsed = new GrabbableObject[0];
        internal class Weight
        {
            [HarmonyPostfix][HarmonyPatch(typeof(PlayerControllerB), "Update")]
            public static void WeightUpdate(PlayerControllerB __instance)
            {
                if (LocalPlayer.localPlayer.PlayerController == null || __instance.playerClientId != LocalPlayer.localPlayer.PlayerController.playerClientId) return;
                __instance.carryWeight = MainGUI.Instance.ToggleSelf[6] ? 1f : GetHeldWeight(__instance);
            }
            private static float GetHeldWeight(PlayerControllerB player)
            {
                float weight = 1f;

                if (player.ItemSlots == null) return weight;

                foreach (var item in player.ItemSlots)
                {
                    if (item == null || item.itemProperties == null) continue;
                    weight += Mathf.Clamp(item.itemProperties.weight - 1f, 0.0f, 10f);
                }

                return weight;
            }
        }
        internal class NoFallDamage 
        {
            [HarmonyPatch(typeof(PlayerControllerB), "PlayerHitGroundEffects")]
            public static bool NoFallDamagePatch(PlayerControllerB __instance)
            {
                if (MainGUI.Instance.ToggleSelf[7])
                {
                    __instance.takingFallDamage = false;
                    LocalPlayer.localPlayer.PlayerController.twoHanded = false;
                }

                return true;
            }
        }
        internal class EnemyCannotBeSpawned
        {
            [HarmonyPostfix][HarmonyPatch(typeof(RoundManager), "EnemyCannotBeSpawned")]
            public static bool EnemyCannotBeSpawnedPatch()
            {
                if (MainGUI.Instance.ToggleServer[1])
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        internal class InteractThroughWalls
        {
            [HarmonyPostfix][HarmonyPatch(typeof(PlayerControllerB), "LateUpdate")]
            public static void InteractThroughWallsPatch(PlayerControllerB __instance)
            {
                if (MainGUI.Instance.ToggleSelf[8])
                {
                    __instance.grabDistance = 10000f;
                    LayerMask mask = (LayerMask)LayerMask.GetMask("Props");
                    mask = (LayerMask)LayerMask.GetMask("Props", "InteractableObject");
                    typeof(PlayerControllerB).GetField("interactableObjectsMask", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).SetValue(__instance, mask.value);
                }
                else
                {
                    typeof(PlayerControllerB).GetField("interactableObjectsMask", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).SetValue(__instance, 832);
                }
            }
        }

        public void NeverLoseScrap()
        {
            if (MainGUI.Instance.ToggleServer[3] && StartOfRound.Instance.allPlayersDead)
            {
                StartOfRound.Instance.allPlayersDead = false;
            }
        }
        public void Invisibility()
        {
            if (LocalPlayer.localPlayer == null || !MainGUI.Instance.ToggleSelf[9]) return;

            Vector3 pos = StartOfRound.Instance.shipHasLanded ? StartOfRound.Instance.notSpawnedPosition.position : Vector3.zero;

            LocalPlayer.localPlayer.Reflect().Invoke("UpdatePlayerPositionServerRpc", pos, true, false, false, true);
        }
        public void SetLocalPlayerHealth(int health = int.MaxValue)
        {
            if (LocalPlayer.localPlayer.PlayerController != null && LocalPlayer.localPlayer.PlayerController.isPlayerControlled && LocalPlayer.localPlayer.PlayerController.IsOwner)
            {
                LocalPlayer.localPlayer.PlayerController.health = health;
            }
        }
        public void Fly()
        {
            float Speed = MainGUI.Instance.Speed * Time.deltaTime;
            GameNetcodeStuff.PlayerControllerB ss = LocalPlayer.localPlayer.PlayerController;
            if (UnityInput.Current.GetKey(KeyCode.LeftShift))
            {
                Speed *= 2.2f;
            }
            if (UnityInput.Current.GetKey(KeyCode.W))
            {
                ss.playerRigidbody.transform.position += ss.playerGlobalHead.transform.forward * Speed;
                ss.playerRigidbody.velocity = new UnityEngine.Vector3(0f, 0f, 0f);
            }
            if (UnityInput.Current.GetKey(KeyCode.A))
            {
                ss.playerRigidbody.transform.position += ss.playerGlobalHead.transform.right * -Speed;
                ss.playerRigidbody.velocity = new UnityEngine.Vector3(0f, 0f, 0f);
            }
            if (UnityInput.Current.GetKey(KeyCode.S))
            {
                ss.playerRigidbody.transform.position += ss.playerGlobalHead.transform.forward * -Speed;
                ss.playerRigidbody.velocity = new UnityEngine.Vector3(0f, 0f, 0f);
            }
            if (UnityInput.Current.GetKey(KeyCode.D))
            {
                ss.playerRigidbody.transform.position += ss.playerGlobalHead.transform.right * Speed;
                ss.playerRigidbody.velocity = new UnityEngine.Vector3(0f, 0f, 0f);
            }
            if (UnityInput.Current.GetKey(KeyCode.Space))
            {
                ss.playerRigidbody.transform.position += ss.playerGlobalHead.transform.up * Speed;
                ss.playerRigidbody.velocity = new UnityEngine.Vector3(0f, 0f, 0f);
            }
            if (UnityInput.Current.GetKey(KeyCode.LeftControl))
            {
                ss.playerRigidbody.transform.position += ss.playerGlobalHead.transform.up * -Speed;
                ss.playerRigidbody.velocity = new UnityEngine.Vector3(0f, 0f, 0f);
            }
        }
        public void Speed()
        {
            LocalPlayer.localPlayer.PlayerController.isSpeedCheating = false;
            LocalPlayer.localPlayer.PlayerController.sprintTime = int.MaxValue;
            LocalPlayer.localPlayer.PlayerController.sprintMeter = int.MaxValue;
            LocalPlayer.localPlayer.PlayerController.movementSpeed = 10f;
            LocalPlayer.localPlayer.PlayerController.isExhausted = false;
        }
        public void Update()
        {
            // Self
            if (MainGUI.Instance.ToggleSelf[1])
            {
                MainGUI.nightVision = true;
                GameObject gameObject = GameObject.Find("Systems");

                if (gameObject == null) return;

                gameObject.transform.Find("Rendering").Find("VolumeMain").gameObject.SetActive(!MainGUI.nightVision);
            }
            else { MainGUI.nightVision = false; }
            if (MainGUI.Instance.ToggleSelf[2])
            {
                GameObject environment = GameObject.Find("Environment");
                if (environment != null)
                {
                    Transform testRoom = environment.transform.Find("TestRoom");
                    if (testRoom != null)
                    {
                        Transform directionalLightTest = testRoom.transform.Find("Directional Light Test");
                        if (directionalLightTest != null)
                        {
                            if (MainGUI.Instance.directionalLightClone == null)
                            {
                                MainGUI.Instance.directionalLightClone = GameObject.Instantiate(directionalLightTest.gameObject);
                                MainGUI.Instance.directionalLightClone.transform.parent = environment.transform;
                                MainGUI.Instance.directionalLightClone.SetActive(true);
                            }
                            else
                            {
                                MainGUI.Instance.directionalLightClone.SetActive(!MainGUI.Instance.directionalLightClone.activeSelf);
                            }
                        }
                    }
                }
            }
            if (MainGUI.Instance.ToggleSelf[3]) { SetLocalPlayerHealth(); }
            if (MainGUI.Instance.ToggleSelf[4]) { Speed(); }
            if (MainGUI.Instance.ToggleSelf[5]) { Fly(); }
            if (MainGUI.Instance.ToggleSelf[6]) { Weight.WeightUpdate(LocalPlayer.localPlayer.PlayerController); }
            if (MainGUI.Instance.ToggleSelf[7]) { NoFallDamage.NoFallDamagePatch(LocalPlayer.localPlayer.PlayerController); }
            if (MainGUI.Instance.ToggleSelf[8]) { InteractThroughWalls.InteractThroughWallsPatch(LocalPlayer.localPlayer.PlayerController); }
            if (MainGUI.Instance.ToggleSelf[9]) { Invisibility(); }

            // Server
            if (MainGUI.Instance.ToggleServer[1]) { EnemyCannotBeSpawned.EnemyCannotBeSpawnedPatch(); }
            if (MainGUI.Instance.ToggleServer[2])
            {
                GrabbableObject[] grabbableObjects = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
                MainGUI.Instance.random.Shuffle(grabbableObjects);

                foreach (GrabbableObject grabbableObject in grabbableObjects)
                {
                    if (!grabbableObject.grabbable || grabbableObject.playerHeldBy != null || grabbableObject == null ||
                        grabbableObjectsUsed.Contains(grabbableObject) || !grabbableObject.itemProperties.isScrap ||
                        grabbableObject.isInElevator) { continue; }

                    LocalPlayer.localPlayer.PlayerController.TeleportPlayer(grabbableObject.transform.position, false, 0f, false, true);

                    InteractTrigger interactTrigger = grabbableObject.GetComponent<InteractTrigger>();
                    if (interactTrigger != null) { Debug.Log("Found interact trigger"); }

                    grabbableObjectsUsed = grabbableObjectsUsed.Append(grabbableObject).ToArray();
                    break;
                }
            }
            if (MainGUI.Instance.ToggleServer[3]) { NeverLoseScrap(); }

            // Visuals
            if (MainGUI.Instance.ToggleVisuals[1])
            {
                foreach (GrabbableObject grabbableObject in UnityEngine.Object.FindObjectsOfType(typeof(GrabbableObject)))
                {
                    string text = "Object";
                    if (grabbableObject.itemProperties != null)
                    {
                        if (grabbableObject.itemProperties.itemName != null)
                        {
                            text = grabbableObject.itemProperties.itemName;
                        }
                        int creditsWorth = grabbableObject.itemProperties.creditsWorth;
                        text = text + " (" + grabbableObject.itemProperties.creditsWorth.ToString() + ")";
                    }
                    Vector3 vector;
                    if (MainGUI.WorldToScreen(GameNetworkManager.Instance.localPlayerController.gameplayCamera, grabbableObject.transform.position, out vector))
                    {
                        GUI.Label(new Rect(vector.x, vector.y, 100f, 25f), text);
                    }
                }
            }
            if (MainGUI.Instance.ToggleVisuals[2])
            {
                foreach (PlayerControllerB playerControllerB in UnityEngine.Object.FindObjectsOfType<PlayerControllerB>())
                {
                    string playerUsername = playerControllerB.playerUsername;
                    Vector3 vector;
                    bool flag = MainGUI.WorldToScreen(GameNetworkManager.Instance.localPlayerController.gameplayCamera, playerControllerB.playerGlobalHead.transform.position, out vector);

                    if (flag)
                    {
                        GUI.Label(new Rect(vector.x, vector.y, 100f, 25f), playerUsername);
                    }
                }
            }
            if (MainGUI.Instance.ToggleVisuals[3])
            {
                foreach (EnemyAI enemyAI in UnityEngine.Object.FindObjectsOfType(typeof(EnemyAI)))
                {
                    string enemyName = enemyAI.enemyType.enemyName;
                    Vector3 vector;
                    bool flag = MainGUI.WorldToScreen(GameNetworkManager.Instance.localPlayerController.gameplayCamera, enemyAI.transform.position, out vector);

                    if (flag)
                    {
                        GUI.Label(new Rect(vector.x, vector.y, 100f, 100f), enemyName);
                    }
                }
            }

            // Host
            if (MainGUI.Instance.ToggleHost[1])
            {
                StartOfRound StartOfRound = GameObject.FindObjectOfType<StartOfRound>();
                if (StartOfRound != null)
                {
                    StartOfRound.ReviveDeadPlayers();
                    StartOfRound.PlayerHasRevivedServerRpc();
                    StartOfRound.AllPlayersHaveRevivedClientRpc();
                    MainGUI.Instance.ToggleHost[1] = false;
                }
            }
            if (MainGUI.Instance.ToggleHost[2])
            {
                RoundManager roundManager = GameObject.FindObjectOfType<RoundManager>();
                SpawnEnemyClass.SpawnEnemyWithConfigManager("ForestGiant");

                SpawnableEnemyWithRarity enemy = roundManager.currentLevel.OutsideEnemies[UnityEngine.Random.Range(0, roundManager.currentLevel.OutsideEnemies.Count)];
                SpawnEnemyClass.SpawnEnemyAtLocalPlayer(enemy, 3);
                MainGUI.Instance.ToggleHost[2] = false;
            }
        }
    }
}
