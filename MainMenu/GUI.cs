using BepInEx;
using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using LethalCompanyHacks.MainMenu.Stuff;
using LethalCompanyHacks.MainMenuPatch;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using Unity.Netcode;
using Unity.Profiling;
using Unity.Services.Authentication.Internal;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UIElements;
using static UnityEngine.InputSystem.DefaultInputActions;
using Color = UnityEngine.Color;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Lethal_Company_Mod_Menu.MainMenu
{
    [BepInPlugin("lethalcompany.GUI", "notfishvr", "1.0.0")]
    internal class MainGUI : BaseUnityPlugin
    {
        public static bool Doonce = false;
        private void DrawMainTab()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            ToggleMain[1] = ToggleButton("No Fog", ToggleMain[1]);
            ToggleMain[2] = ToggleButton("God Mode", ToggleMain[2]);
            ToggleMain[3] = ToggleButton("Enemy Cant Be Spawned", ToggleMain[3]);
            ToggleMain[4] = ToggleButton("Speed", ToggleMain[4]);
            ToggleMain[5] = ToggleButton("Fly", ToggleMain[5]);
            ToggleMain[6] = ToggleButton("Gets All Scrap", ToggleMain[6]);
            ToggleMain[7] = ToggleButton("No Weight", ToggleMain[7]);
            ToggleMain[8] = ToggleButton("No Hands Full", ToggleMain[8]);

            GUILayout.EndScrollView();
        }
        private void DrawMicModsTab()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            ToggleMic[1] = ToggleButton("Night Vision", ToggleMic[1]);
            ToggleMic[2] = ToggleButton("Give Ownership", ToggleMic[2]);
            ToggleMic[3] = ToggleButton("High Scrap Value", ToggleMic[3]);
            ToggleMic[4] = ToggleButton("Unlimited Scan Range", ToggleMic[4]);
            ToggleMic[5] = ToggleButton("Object ESP", ToggleMic[5]);
            ToggleMic[6] = ToggleButton("Player ESP", ToggleMic[6]);
            ToggleMic[7] = ToggleButton("Enemy ESP", ToggleMic[7]);
            ToggleMic[8] = ToggleButton("Add 100k Cash", ToggleMic[8]);
            ToggleMic[9] = ToggleButton("Remove 100k Cash", ToggleMic[9]);
            ToggleMic[10] = ToggleButton("Set Level High", ToggleMic[10]);

            GUILayout.EndScrollView();
        }
        private void DrawHostTab()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            ToggleHost[1] = ToggleButton("Revive all players", ToggleHost[1]);
            ToggleHost[2] = ToggleButton("Spawn enemy on localpos", ToggleHost[2]);

            GUILayout.EndScrollView();
        }
        private void DrawPlayerListTab()
        {
            _instance.scrollPosition = GUILayout.BeginScrollView(_instance.scrollPosition);
            int num = 1;

            foreach (PlayerControllerB playerControllerB in UnityEngine.Object.FindObjectsOfType<PlayerControllerB>())
            {
                string playerUsername = playerControllerB.playerUsername;

                GUILayout.BeginHorizontal();
                GUILayout.Label("Player " + num.ToString() + ": " + playerUsername);

                if (GUILayout.Button("i", GUILayout.Width(20), GUILayout.Height(20))) { selectedPlayer = playerControllerB; playerManagerEnabled = true; }

                GUILayout.EndHorizontal();

                if (playerManagerEnabled && selectedPlayer == playerControllerB)
                {
                    if (GUILayout.Button("Kill")) { selectedPlayer.DamagePlayerFromOtherClientServerRpc(int.MaxValue, Vector3.zero, -1); }

                    if (GUILayout.Button("TP")) { GameNetworkManager.Instance.localPlayerController.transform.position = selectedPlayer.transform.position; }

                    if (GUILayout.Button("Back")) { selectedPlayer = null; playerManagerEnabled = false; }
                }

                num++;
            }

            GUILayout.EndScrollView();
        }
        private void DrawSettingsTab()
        {
            _instance.scrollPosition = GUILayout.BeginScrollView(_instance.scrollPosition);

            GUILayout.Label("Speed: " + SpeedOptions[SpeedSelection]);
            SpeedSelection = (int)GUILayout.HorizontalSlider(SpeedSelection, 0, SpeedOptions.Length - 1);
            Speed = SpeedValues[SpeedSelection];

            GUILayout.EndScrollView();
        }
        public void Update()
        {
            GUIToggleCheck();
            _playerManager.UpdatePlayers();
            playerControllerHandler.Update();

            if (toggled)
            {
                if (Cursorlock)
                {
                    UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                    UnityEngine.Cursor.visible = false;
                }
            }
            else
            {
                if (Cursorlock)
                {
                    UnityEngine.Cursor.lockState = CursorLockMode.None;
                    UnityEngine.Cursor.visible = true;
                }
            }
            foreach (DeadBodyInfo deadBodyInfoInstance in UnityEngine.Object.FindObjectsOfType<DeadBodyInfo>())
            {
                int num = UnityEngine.Object.FindObjectsOfType<DeadBodyInfo>().Length;
                if (num > numberDeadLastRound)
                {
                    logs = logs + deadBodyInfoInstance.playerScript.playerUsername + " has died!!!\n";
                    numberDeadLastRound++;
                }
            }

            // Main
            if (ToggleMain[1]) 
            { 
                nightVision = true;
                GameObject gameObject = GameObject.Find("Systems");

                if (gameObject == null) return;

                gameObject.transform.Find("Rendering").Find("VolumeMain").gameObject.SetActive(!nightVision);
            }
            else  { nightVision = false;  }
            if (ToggleMain[6])
            {
                 GrabbableObject[] grabbableObjects = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
                 random.Shuffle(grabbableObjects);

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
            if (ToggleMain[7]) { LocalPlayer.localPlayer.PlayerController.carryWeight = 1f; }
            if (ToggleMain[8]) { LocalPlayer.localPlayer.PlayerController.twoHanded = false; }

            // Misc
            if (ToggleMic[1])
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
                            if (directionalLightClone == null)
                            {
                                directionalLightClone = GameObject.Instantiate(directionalLightTest.gameObject);
                                directionalLightClone.transform.parent = environment.transform;
                                directionalLightClone.SetActive(true);
                            }
                            else
                            {
                                directionalLightClone.SetActive(!directionalLightClone.activeSelf);
                            }
                        }
                    }
                }
            }
            if (ToggleMic[2])
            {
                foreach (StartOfRound StartOfRound in UnityEngine.Object.FindObjectsOfType<StartOfRound>())
                {
                    StartOfRound.localPlayerController.GetComponent<NetworkObject>().ChangeOwnership(StartOfRound.localPlayerController.actualClientId);
                }
            }
            if (ToggleMic[3]) { HighScrapValue = true; } else { HighScrapValue = false; }
            if (ToggleMic[4]) { UnlimitedScanRange = true; } else { UnlimitedScanRange = false; }
            // ESP
            if (ToggleMic[5])
            {
                foreach (GrabbableObject grabbableObject in Object.FindObjectsOfType(typeof(GrabbableObject)))
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
                    if (WorldToScreen(GameNetworkManager.Instance.localPlayerController.gameplayCamera, grabbableObject.transform.position, out vector))
                    {
                        GUI.Label(new Rect(vector.x, vector.y, 100f, 25f), text);
                    }
                }
            }
            if (ToggleMic[6])
            {
                foreach (PlayerControllerB playerControllerB in UnityEngine.Object.FindObjectsOfType<PlayerControllerB>())
                {
                    string playerUsername = playerControllerB.playerUsername;
                    Vector3 vector;
                    bool flag = WorldToScreen(GameNetworkManager.Instance.localPlayerController.gameplayCamera, playerControllerB.playerGlobalHead.transform.position, out vector);

                    if (flag)
                    {
                        GUI.Label(new Rect(vector.x, vector.y, 100f, 25f), playerUsername);
                    }
                }
            }
            if (ToggleMic[7])
            {
                foreach (EnemyAI enemyAI in Object.FindObjectsOfType(typeof(EnemyAI)))
                {
                    string enemyName = enemyAI.enemyType.enemyName;
                    Vector3 vector;
                    bool flag = WorldToScreen(GameNetworkManager.Instance.localPlayerController.gameplayCamera, enemyAI.transform.position, out vector);

                    if (flag)
                    {
                        GUI.Label(new Rect(vector.x, vector.y, 100f, 100f), enemyName);
                    }
                }
            }
            // END ESP
            if (ToggleMic[8]) { FindObjectOfType<Terminal>().groupCredits += 100000; }
            if (ToggleMic[9]) { FindObjectOfType<Terminal>().groupCredits -= 100000; }
            if (ToggleMic[10]) { FindObjectOfType<HUDManager>().localPlayerXP += 999; }

            // Host
            if (ToggleHost[1])
            {
                StartOfRound StartOfRound = GameObject.FindObjectOfType<StartOfRound>();
                if (StartOfRound != null)
                {
                    StartOfRound.ReviveDeadPlayers();
                    StartOfRound.PlayerHasRevivedServerRpc();
                    StartOfRound.AllPlayersHaveRevivedClientRpc();
                    ToggleHost[1] = false;
                }
            }
            if (ToggleHost[2])
            {
                RoundManager roundManager = GameObject.FindObjectOfType<RoundManager>();
                SpawnEnemyClass.SpawnEnemyWithConfigManager("ForestGiant");

                SpawnableEnemyWithRarity enemy = roundManager.currentLevel.OutsideEnemies[UnityEngine.Random.Range(0, roundManager.currentLevel.OutsideEnemies.Count)];
                SpawnEnemyClass.SpawnEnemyAtLocalPlayer(enemy, 3);
                ToggleHost[2] = false;
            }
        }
        #region Main GUI
        private void OnGUI()
        {
            GUI.skin = GUI.skin ?? new GUISkin();
            UpdateStyles();
            Update();
            if (toggled)
            {
                GUIRect = GUI.Window(69, GUIRect, OnGUI, "Lethal Company GUI | Toggle: " + toggleKey);
            }
        }
        public static void OnGUI(int windowId)
        {
            //Instance.Update();
            GUILayout.BeginArea(new Rect(10, 10, GUIRect.width - 20, GUIRect.height - 20));
            GUILayout.Space(10);

            DrawTabButtons();

            if (selectedTab == 0)
            {
                _instance.DrawMainTab();
            }
            else if (selectedTab == 1)
            {
                _instance.DrawMicModsTab();
            }
            else if (selectedTab == 2)
            {
                _instance.DrawHostTab();
            }
            else if (selectedTab == 3)
            {
                _instance.DrawPlayerListTab();
            }
            else if (selectedTab == 4)
            {
                _instance.DrawSettingsTab();
            }

            GUILayout.EndArea();
            GUI.DragWindow(new Rect(0, 0, GUIRect.width, 20));
        }
        private void GUIToggleCheck()
        {
            if (UnityInput.Current.GetKey(toggleKey))
            {
                if (Time.time - lastToggleTime >= toggleDelay)
                {
                    toggled = !toggled;
                    lastToggleTime = Time.time;
                }
            }
        }
        private static void DrawTabButtons()
        {
            GUILayout.BeginHorizontal();

            for (int i = 0; i < tabNames.Length; i++)
            {
                if (selectedTab == i)
                {
                    GUIStyle selectedStyle = Instance.CreateButtonStyle(Instance.buttonActive, Instance.buttonHovered, Instance.buttonActive);
                    if (GUILayout.Button(tabNames[i], selectedStyle))
                    {
                        selectedTab = i;
                    }
                }
                else
                {
                    GUIStyle unselectedStyle = Instance.CreateButtonStyle(Instance.button, Instance.buttonHovered, Instance.buttonActive);
                    if (GUILayout.Button(tabNames[i], unselectedStyle))
                    {
                        selectedTab = i;
                    }
                }
            }

            GUILayout.EndHorizontal();
        }
        private bool ToggleButton(string text, bool toggle)
        {
            GUIStyle buttonStyle = CreateButtonStyle(toggle ? buttonActive : button, buttonHovered, buttonActive);

            if (GUILayout.Button(text, buttonStyle))
            {
                return !toggle;
            }

            return toggle;
        }
        #region Styles
        private void Awake()
        {
            _playerManager = new PlayerManager();
            playerControllerHandler = new PlayerControllerHandler();
            _instance = this;
            button = CreateTexture(new Color32(64, 64, 64, 255));
            buttonHovered = CreateTexture(new Color32(75, 75, 75, 255));
            buttonActive = CreateTexture(new Color32(100, 100, 100, 255));
            windowBackground = CreateTexture(new Color32(30, 30, 30, 255));
            textArea = CreateTexture(new Color32(64, 64, 64, 255));
            textAreaHovered = CreateTexture(new Color32(75, 75, 75, 255));
            textAreaActive = CreateTexture(new Color32(100, 100, 100, 255));
            box = CreateTexture(new Color32(40, 40, 40, 255));
        }
        private void UpdateStyles()
        {
            GUI.skin.button = CreateButtonStyle(button, buttonHovered, buttonActive);
            GUI.skin.window = CreateWindowStyle(windowBackground);
            GUI.skin.textArea = CreateTextFieldStyle(textArea, textAreaHovered, textAreaActive);
            GUI.skin.textField = CreateTextFieldStyle(textArea, textAreaHovered, textAreaActive);
            GUI.skin.box = CreateBoxStyle(box);
        }
        public Texture2D CreateTexture(Color32 color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
        public GUIStyle CreateButtonStyle(Texture2D normal, Texture2D hover, Texture2D active)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.normal.background = normal;
            style.hover.background = hover;
            style.active.background = active;
            style.normal.textColor = Color.white;
            style.hover.textColor = Color.white;
            style.active.textColor = Color.white;
            return style;
        }
        public GUIStyle CreateWindowStyle(Texture2D background)
        {
            GUIStyle style = new GUIStyle(GUI.skin.window);
            style.normal.background = background;
            style.onNormal.background = background;
            style.normal.textColor = Color.white;
            style.onNormal.textColor = Color.white;
            return style;
        }
        public GUIStyle CreateTextFieldStyle(Texture2D normal, Texture2D hover, Texture2D active)
        {
            GUIStyle style = new GUIStyle(GUI.skin.textField);
            style.normal.background = normal;
            style.hover.background = hover;
            style.active.background = active;
            style.focused.background = active;
            style.normal.textColor = Color.white;
            style.hover.textColor = Color.white;
            style.active.textColor = Color.white;
            style.focused.textColor = Color.white;
            return style;
        }
        public GUIStyle CreateBoxStyle(Texture2D normal)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = normal;
            style.hover.background = normal;
            style.active.background = normal;
            style.normal.textColor = Color.white;
            style.hover.textColor = Color.white;
            style.active.textColor = Color.white;
            return style;
        }
        #endregion
        #endregion
        internal static bool nightVision, enableGod, HighScrapValue, UnlimitedScanRange;
        #region Field
        // Textures
        public Texture2D button, buttonHovered, buttonActive, windowBackground, textArea, textAreaHovered, textAreaActive, box;
        public GameObject directionalLightClone;
        public PlayerControllerHandler playerControllerHandler;
        public static PlayerManager _playerManager;
        public Dictionary<Type, List<Component>> objectCache = new Dictionary<Type, List<Component>>();
        public bool joining = false;
        public bool shouldSpawnEnemy;
        public AudioReverbTrigger currentAudioTrigger;
        public KeyCode toggleKey = KeyCode.Insert;
        public int numberDeadLastRound;
        public static bool Cursorlock = false;
        public int SpeedSelection = 0;
        public static GameObject pointer;
        public string[] SpeedOptions = new string[] { "Slow", "Default", "Fast", "Super Fast", "FASTTTTTTTS" };
        public float[] SpeedValues = new float[] { 5.0f, 7.5f, 15.0f, 17.5f, 20.0f };
        public float Speed = 1.7f;
        internal static bool playerManagerEnabled = false;
        public string guiSelectedEnemy;
        public static bool hasGUISynced;
        public GrabbableObject[] grabbableObjectsUsed = new GrabbableObject[0];
        public System.Random random = new System.Random();
        public float cacheRefreshInterval = 1.5f;
        public int enemyCount = 0;
        public bool addMoney = false;

        public static RoundManager currentRound;

        public string logs = "Logs (Only shows player deaths for now):\n";
        // GUI Variables
        public static SelectableLevel currentLevel;
        public static Rect GUIRect = new Rect(0, 0, 540, 240);
        public static int selectedTab = 0;
        public static readonly string[] tabNames = { "Main", "Misc", "Host", "Player List", "Settings" };
        public bool[] TogglePlayerList = new bool[999];
        public bool[] ToggleMain = new bool[999];
        public bool[] ToggleMic = new bool[999];
        public bool[] ToggleHost = new bool[999];
        public bool toggled = true;
        public PlayerControllerB selectedPlayer;
        internal static float nightVisionIntensity, nightVisionRange;
        internal static Color nightVisionColor;
        public float toggleDelay = 0.5f;
        public float lastToggleTime;
        public Vector2 scrollPosition = Vector2.zero;

        private static MainGUI _instance;
        public static MainGUI Instance => _instance;
        #endregion
        #region Mods
        public static bool WorldToScreen(Camera camera, Vector3 world, out Vector3 screen)
        {
            screen = camera.WorldToViewportPoint(world);
            screen.x *= (float)Screen.width;
            screen.y *= (float)Screen.height;
            screen.y = (float)Screen.height - screen.y;
            return screen.z > 0f;
        }
        #endregion
    }
}