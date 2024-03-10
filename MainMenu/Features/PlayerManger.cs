using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using Lethal_Company_Mod_Menu.MainMenu;
using System.Collections.Generic;
using UnityEngine;

namespace LethalCompanyHacks.MainMenu.Stuff
{
    public class PlayerManager : MonoBehaviour
    {
        private List<PlayerControllerB> _players = new List<PlayerControllerB>();

        private void Start()
        {
            UpdatePlayers();
        }

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

        public LocalPlayer(PlayerControllerB playerController)
        {
            PlayerController = playerController;
        }

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
        [HarmonyPatch(typeof(RoundManager), "EnemyCannotBeSpawned")]
        [HarmonyPrefix]
        public bool EnemyCannotBeSpawned()
        {
            if (MainGUI.Instance.ToggleMain[3])
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void SetLocalPlayerHealth(int health = int.MaxValue)
        {
            if (MainGUI.Instance.ToggleMain[2])
            {
                var playerControllers = GameObject.FindObjectsOfType<PlayerControllerB>();

                foreach (var playerController in playerControllers)
                {
                    if (playerController != null && playerController.isPlayerControlled && playerController.IsOwner)
                    {
                        playerController.health = health;
                    }
                }
            }
        }
        public void Fly()
        {
            if (MainGUI.Instance.ToggleMain[5])
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
        }
        public void Speed()
        {
            if (MainGUI.Instance.ToggleMain[4])
            {
                LocalPlayer.localPlayer.PlayerController.isSpeedCheating = false;
                LocalPlayer.localPlayer.PlayerController.sprintTime = int.MaxValue;
                LocalPlayer.localPlayer.PlayerController.sprintMeter = int.MaxValue;
                LocalPlayer.localPlayer.PlayerController.movementSpeed = 10f;
                LocalPlayer.localPlayer.PlayerController.isExhausted = false;
            }
        }
        public void Update()
        {
            SetLocalPlayerHealth();
            Fly();
            Speed();
            EnemyCannotBeSpawned();
        }
    }
}
