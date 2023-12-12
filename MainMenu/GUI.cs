using BepInEx;
using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using LethalCompanyHacks.MainMenuPatch;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.Netcode;
using Unity.Profiling;
using Unity.Services.Authentication.Internal;
using UnityEngine;
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
    [BepInPlugin("lethalcompany.GUI", "notfishvr", "1.0")]
    internal class MainGUI : BaseUnityPlugin
    {
        public static bool Doonce = false;
        private void DrawMainTab()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            ToggleMain[1] = ToggleButton("Night Vision", ToggleMain[1]);
            ToggleMain[2] = ToggleButton("God Mode", ToggleMain[2]);
            ToggleMain[3] = ToggleButton("Enemy Cant Be Spawned", ToggleMain[3]);
            ToggleMain[4] = ToggleButton("Speed", ToggleMain[4]);
            ToggleMain[5] = ToggleButton("Fly", ToggleMain[5]);
            ToggleMain[6] = ToggleButton("SpawnBody", ToggleMain[6]);
            ToggleMain[7] = ToggleButton("Explosion", ToggleMain[7]);
            ToggleMain[8] = ToggleButton("Infinite Sprint", ToggleMain[8]);
            ToggleMain[9] = ToggleButton("Unlimited Item Power", ToggleMain[9]);

            GUILayout.EndScrollView();
        }
        private void DrawMicModsTab()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            ToggleMic[1] = ToggleButton("Tp All", ToggleMic[1]);
            ToggleMic[2] = ToggleButton("Give Ownership", ToggleMic[2]);
            ToggleMic[3] = ToggleButton("Add Money", ToggleMic[3]);
            ToggleMic[4] = ToggleButton("High Scrap Value", ToggleMic[4]);
            ToggleMic[5] = ToggleButton("Unlimited Scan Range", ToggleMic[5]);
            ToggleMic[6] = ToggleButton("Object ESP", ToggleMic[6]);
            ToggleMic[7] = ToggleButton("Player ESP", ToggleMic[7]);
            ToggleMic[8] = ToggleButton("Enemy ESP", ToggleMic[8]);
            ToggleMic[9] = ToggleButton("Add 100k Cash", ToggleMic[9]);
            ToggleMic[10] = ToggleButton("Remove 100k Cash", ToggleMic[10]);
            ToggleMic[11] = ToggleButton("Remove Current Cash", ToggleMic[11]);
            ToggleMic[12] = ToggleButton("Set Level High", ToggleMic[12]);

            GUILayout.EndScrollView();
        }
        private void DrawInfoTab()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label(logs);
            if (HUDManager.Instance != null)
            {
                GUILayout.Label("Current XP: " + HUDManager.Instance.localPlayerXP.ToString());
                GUILayout.Label("Current Level: " + HUDManager.Instance.localPlayerLevel.ToString());
            } else { GUILayout.Label("Current XP: ???"); GUILayout.Label("Current Level: ???"); }

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
                foreach (HDAdditionalLightData lightData in UnityEngine.Object.FindObjectsOfType<HDAdditionalLightData>())
                {
                    foreach (TimeOfDay timeOfDay in UnityEngine.Object.FindObjectsOfType<TimeOfDay>())
                    {
                        foreach (StartOfRound startOfRound in UnityEngine.Object.FindObjectsOfType<StartOfRound>())
                        {
                            lightData.lightDimmer = float.MaxValue;
                            lightData.distance = float.MaxValue;
                            timeOfDay.insideLighting = false;
                            startOfRound.blackSkyVolume.weight = 0;
                        }
                    }
                }
            }
            else  { nightVision = false;  }
            if (ToggleMain[2]) { enableGod = true; /*localPlayerController.health = 100;*/  } else { enableGod = false; }
            if (ToggleMain[3]) { EnemyCannotBeSpawned = true; } else { EnemyCannotBeSpawned = false; }
            if (ToggleMain[4])
            {
                foreach (PlayerControllerB PlayerControllerB in UnityEngine.Object.FindObjectsOfType<PlayerControllerB>())
                {
                    PlayerControllerB.isSpeedCheating = false;
                    PlayerControllerB.sprintTime = int.MaxValue;
                    PlayerControllerB.sprintMeter = int.MaxValue;
                    PlayerControllerB.movementSpeed = 10f;
                    InfiniteSprint = true;
                }
            }
            if (ToggleMain[5])
            {
                float Speed = this.Speed * Time.deltaTime;
                foreach (GameNetcodeStuff.PlayerControllerB ss in UnityEngine.Object.FindObjectsOfType<GameNetcodeStuff.PlayerControllerB>())
                {
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
            if (ToggleMain[6])
            {
                if (Doonce)
                {
                    Doonce = true;
                    foreach (GameNetcodeStuff.PlayerControllerB ss in UnityEngine.Object.FindObjectsOfType<GameNetcodeStuff.PlayerControllerB>())
                    {
                        PlayerControllerB component = ss.playersManager.allPlayerObjects[0].GetComponent<PlayerControllerB>();
                        ss.SpawnDeadBody(ss.playersManager.thisClientPlayerId, new Vector3(0f, 10f, 0f), 0, component);
                    }
                }
            }
            if (ToggleMain[7])
            {
                foreach (GameNetcodeStuff.PlayerControllerB ss in UnityEngine.Object.FindObjectsOfType<GameNetcodeStuff.PlayerControllerB>())
                {
                    Landmine.SpawnExplosion(ss.serverPlayerPosition, true, 999f, 999f);
                }
            }
            if (ToggleMain[8]) 
            { 
                InfiniteSprint = true;
                foreach (PlayerControllerB playerControllerB in UnityEngine.Object.FindObjectsOfType<PlayerControllerB>())
                {
                    playerControllerB.sprintTime = 9999f;
                    playerControllerB.sprintMeter = 1f;
                    playerControllerB.isExhausted = false;
                }
            }
            else { InfiniteSprint = false; }
            if (ToggleMain[9])
            {
                if (GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer != null)
                {
                    if (GameNetworkManager.Instance.localPlayerController.IsServer)
                    {
                        GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer.insertedBattery.charge = 1f;
                    }
                }
            }

            // Misc
            if (ToggleMic[1])
            {
                foreach (GameNetcodeStuff.PlayerControllerB ss in UnityEngine.Object.FindObjectsOfType<GameNetcodeStuff.PlayerControllerB>())
                {
                    PlayerControllerB component = ss.playersManager.allPlayerObjects[0].GetComponent<PlayerControllerB>();
                    ss.TeleportPlayer(new Vector3(0f, float.NegativeInfinity, 0f), false, 0f, false, true);
                }
            }
            if (ToggleMic[2])
            {
                foreach (StartOfRound StartOfRound in UnityEngine.Object.FindObjectsOfType<StartOfRound>())
                {
                    StartOfRound.localPlayerController.GetComponent<NetworkObject>().ChangeOwnership(StartOfRound.localPlayerController.actualClientId);
                }
            }
            if (ToggleMic[3])
            {
                foreach (Terminal Terminal in UnityEngine.Object.FindObjectsOfType<Terminal>())
                {
                    if (Terminal != null && addMoney)
                    {
                        if (GameNetworkManager.Instance.localPlayerController.IsServer)
                        {
                            Terminal.groupCredits += 200;
                            addMoney = false;
                            ToggleMic[3] = false;
                        }
                        else
                        {
                            Terminal.groupCredits += 200;
                            Terminal.SyncGroupCreditsServerRpc(Terminal.groupCredits, Terminal.numberOfItemsInDropship);
                            addMoney = false;
                            ToggleMic[3] = false;
                        }
                    }
                }
            }
            if (ToggleMic[4]) { HighScrapValue = true; } else { HighScrapValue = false; }
            if (ToggleMic[5]) { UnlimitedScanRange = true; } else { UnlimitedScanRange = false; }
            // ESP
            if (ToggleMic[6])
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
            if (ToggleMic[7])
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
            if (ToggleMic[8])
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
            if (ToggleMic[9]) { FindObjectOfType<Terminal>().groupCredits += 100000; }
            if (ToggleMic[10]) { FindObjectOfType<Terminal>().groupCredits -= 100000; }
            if (ToggleMic[11])
            {
                int groupCredits = FindObjectOfType<Terminal>().groupCredits;
                FindObjectOfType<Terminal>().groupCredits -= groupCredits;
            }
            if (ToggleMic[12]) { FindObjectOfType<HUDManager>().localPlayerXP += 999; }
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
                _instance.DrawInfoTab();
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
            enemyRaritys = new Dictionary<SpawnableEnemyWithRarity, int>();
            levelEnemySpawns = new Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>>();
            enemyPropCurves = new Dictionary<SpawnableEnemyWithRarity, AnimationCurve>();
            EnemyCannotBeSpawned = false;
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
        internal static bool nightVision, enableGod, EnemyCannotBeSpawned, InfiniteSprint, UnlimitedItemPower, HighScrapValue, UnlimitedScanRange;
        #region Field
        // Textures
        private Texture2D button, buttonHovered, buttonActive;
        private Texture2D windowBackground;
        private Texture2D textArea, textAreaHovered, textAreaActive;
        private GameObject directionalLightClone;
        private Texture2D box;
        private static MainGUI _instance;
        private Dictionary<Type, List<Component>> objectCache = new Dictionary<Type, List<Component>>();
        private bool joining = false;
        public bool shouldSpawnEnemy;
        public AudioReverbTrigger currentAudioTrigger;
        public KeyCode toggleKey = KeyCode.Insert;
        private int numberDeadLastRound;
        private int SpeedSelection = 0;
        public static GameObject pointer;
        private string[] SpeedOptions = new string[] { "Slow", "Default", "Fast", "Super Fast", "FASTTTTTTTS" };
        private float[] SpeedValues = new float[] { 5.0f, 7.5f, 15.0f, 17.5f, 20.0f };
        private float Speed = 1.7f;
        internal static bool playerManagerEnabled = false;
        public string guiSelectedEnemy;
        private static bool hasGUISynced;
        private float cacheRefreshInterval = 1.5f;
        private int enemyCount = 0;
        private bool addMoney = false;

        private static RoundManager currentRound;
        public static MainGUI Instance => _instance;

        private string logs = "Logs (Only shows player deaths for now):\n";
        // GUI Variables
        private static SelectableLevel currentLevel;
        public static Rect GUIRect = new Rect(0, 0, 540, 240);
        private static int selectedTab = 0;
        private static readonly string[] tabNames = { "Main", "Misc", "Info", "Player List", "Settings" };
        private bool[] TogglePlayerList = new bool[999];
        private bool[] ToggleMain = new bool[999];
        private bool[] ToggleMic = new bool[999];
        private bool toggled = true;
        private PlayerControllerB selectedPlayer;
        public static Dictionary<SpawnableEnemyWithRarity, AnimationCurve> enemyPropCurves;
        public static Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>> levelEnemySpawns;
        public static Dictionary<SpawnableEnemyWithRarity, int> enemyRaritys;
        private static EnemyVent[] currentLevelVents;
        private static SpawnableEnemyWithRarity jesterRef;
        internal static float nightVisionIntensity, nightVisionRange;
        internal static Color nightVisionColor;
        public float toggleDelay = 0.5f;
        private float lastToggleTime;
        private Vector2 scrollPosition = Vector2.zero;
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
        public static void PermaLockDoor()
        {
            foreach (DoorLock s in UnityEngine.Object.FindObjectsOfType<DoorLock>())
            {
                s.LockDoor(float.PositiveInfinity);
            }
        }
        public static void UnLockDoor()
        {
            foreach (DoorLock s in UnityEngine.Object.FindObjectsOfType<DoorLock>())
            {
                s.UnlockDoor();
            }
        }
        #region Patches

        [HarmonyPatch(typeof(RoundManager), "EnemyCannotBeSpawned")]
        [HarmonyPrefix]
        private static bool OverrideCannotSpawn()
        {
            if (EnemyCannotBeSpawned)
            {
                return false;
            } 
            else
            {
                return true;
            }
        }

        [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
        [HarmonyPrefix]
        private static bool ModifyLevel(ref SelectableLevel newLevel)
        {
            currentRound = RoundManager.Instance;
            if (!levelEnemySpawns.ContainsKey(newLevel))
            {
                List<SpawnableEnemyWithRarity> list = new List<SpawnableEnemyWithRarity>();
                foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in newLevel.Enemies)
                {
                    list.Add(spawnableEnemyWithRarity);
                }
                levelEnemySpawns.Add(newLevel, list);
            }
            List<SpawnableEnemyWithRarity> list2;
            levelEnemySpawns.TryGetValue(newLevel, out list2);
            newLevel.Enemies = list2;
            foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity2 in newLevel.Enemies)
            {
                Debug.Log("Inside: " + spawnableEnemyWithRarity2.enemyType.enemyName);
                if (!enemyRaritys.ContainsKey(spawnableEnemyWithRarity2))
                {
                    enemyRaritys.Add(spawnableEnemyWithRarity2, spawnableEnemyWithRarity2.rarity);
                }
                int num = 0;
                enemyRaritys.TryGetValue(spawnableEnemyWithRarity2, out num);
                spawnableEnemyWithRarity2.rarity = num;
            }
            foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity3 in newLevel.OutsideEnemies)
            {
                Debug.Log("Outside: " + spawnableEnemyWithRarity3.enemyType.enemyName);
                if (!enemyRaritys.ContainsKey(spawnableEnemyWithRarity3))
                {
                    enemyRaritys.Add(spawnableEnemyWithRarity3, spawnableEnemyWithRarity3.rarity);
                }
                int num2 = 0;
                enemyRaritys.TryGetValue(spawnableEnemyWithRarity3, out num2);
                spawnableEnemyWithRarity3.rarity = num2;
            }
            foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity4 in newLevel.Enemies)
            {
                if (!enemyPropCurves.ContainsKey(spawnableEnemyWithRarity4))
                {
                    enemyPropCurves.Add(spawnableEnemyWithRarity4, spawnableEnemyWithRarity4.enemyType.probabilityCurve);
                }
                AnimationCurve animationCurve = new AnimationCurve();
                enemyPropCurves.TryGetValue(spawnableEnemyWithRarity4, out animationCurve);
                spawnableEnemyWithRarity4.enemyType.probabilityCurve = animationCurve;
            }
            HUDManager.Instance.AddTextToChatOnServer("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n", -1);
            SelectableLevel selectableLevel = newLevel;
            SpawnableMapObject[] spawnableMapObjects = selectableLevel.spawnableMapObjects;
            for (int i = 0; i < spawnableMapObjects.Length; i++)
            {

            }
            newLevel = selectableLevel;
            return true;
        }

        #endregion
        #endregion
    }
}