using BepInEx;
using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Profiling;
using Unity.Services.Authentication.Internal;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using static UnityEngine.InputSystem.DefaultInputActions;
using Color = UnityEngine.Color;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace NIKO_Menu_V2.MainMenu
{
    [BepInPlugin("NIKO.GUI", "NIKO", "1.0")]
    internal class MainGUI : BaseUnityPlugin
    {
        public static bool Doonce = false;
        private void DrawComputerTab()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            ToggleComputer[1] = ToggleButton("Night Vision", ToggleComputer[1]);
            ToggleComputer[2] = ToggleButton("God Mode", ToggleComputer[2]);
            ToggleComputer[3] = ToggleButton("Enemy Cant Be Spawned", ToggleComputer[3]);
            ToggleComputer[4] = ToggleButton("Speed", ToggleComputer[4]);
            ToggleComputer[5] = ToggleButton("Fly", ToggleComputer[5]);
            ToggleComputer[6] = ToggleButton("SpawnBody", ToggleComputer[6]);
            ToggleComputer[7] = ToggleButton("Explosion", ToggleComputer[7]);
            ToggleComputer[8] = ToggleButton("Tpall", ToggleComputer[8]);
            ToggleComputer[9] = ToggleButton("Giveownership", ToggleComputer[9]);

            GUILayout.EndScrollView();
        }
        private void DrawMicModsTab()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.EndScrollView();
        }
        private void DrawMenuBtnTab()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.EndScrollView();
        }
        private void DrawPlayerListTab()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

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
            if (ToggleComputer[1]) { nightVision = true; } else { nightVision = false; }
            if (ToggleComputer[2]) { enableGod = true; } else { enableGod = false; }
            if (ToggleComputer[3]) { EnemyCannotBeSpawned = true; } else { EnemyCannotBeSpawned = false; }
            if (ToggleComputer[4])
            {
                playerRef.isSpeedCheating = false;
                playerRef.sprintTime = int.MaxValue;
                playerRef.sprintMeter = int.MaxValue;
                playerRef.movementSpeed = 2f;
                InfiniteSprint = true;
            }
            if (ToggleComputer[5])
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
            if (ToggleComputer[6])
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
            if (ToggleComputer[7])
            {
                foreach (GameNetcodeStuff.PlayerControllerB ss in UnityEngine.Object.FindObjectsOfType<GameNetcodeStuff.PlayerControllerB>())
                {
                    Landmine.SpawnExplosion(ss.serverPlayerPosition, true, 999f, 999f);
                }
            }
            if (ToggleComputer[8])
            {
                foreach (GameNetcodeStuff.PlayerControllerB ss in UnityEngine.Object.FindObjectsOfType<GameNetcodeStuff.PlayerControllerB>())
                {
                    PlayerControllerB component = ss.playersManager.allPlayerObjects[0].GetComponent<PlayerControllerB>();
                    ss.TeleportPlayer(new Vector3(0f, float.NegativeInfinity, 0f), false, 0f, false, true);
                }
            }
            if (ToggleComputer[9])
            {
                foreach (StartOfRound StartOfRound in UnityEngine.Object.FindObjectsOfType<StartOfRound>())
                {
                    StartOfRound.localPlayerController.GetComponent<NetworkObject>().ChangeOwnership(StartOfRound.localPlayerController.actualClientId);
                }
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
                GUIRect = GUI.Window(69, GUIRect, OnGUI, "NIKO GUI | Toggle: " + toggleKey);
            }
        }
        public static void OnGUI(int windowId)
        {
            GUILayout.BeginArea(new Rect(10, 10, GUIRect.width - 20, GUIRect.height - 20));
            GUILayout.Space(10);

            DrawTabButtons();

            if (selectedTab == 0)
            {
                _instance.DrawComputerTab();
            }
            else if (selectedTab == 1)
            {
                _instance.DrawMicModsTab();
            }
            else if (selectedTab == 2)
            {
                _instance.DrawMenuBtnTab();
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
        internal static bool nightVision, enableGod, EnemyCannotBeSpawned, InfiniteSprint;
        #region Field
        // Textures
        private Texture2D button, buttonHovered, buttonActive;
        private Texture2D windowBackground;
        private Texture2D textArea, textAreaHovered, textAreaActive;
        private Texture2D box;
        private Harmony harmony = new Harmony("NIKO.GUI.2");
        private static MainGUI _instance;
        private bool joining = false;
        public bool shouldSpawnEnemy;
        public AudioReverbTrigger currentAudioTrigger;
        public KeyCode toggleKey = KeyCode.Insert;
        private int SpeedSelection = 0;
        public static GameObject pointer;
        private string[] SpeedOptions = new string[] { "Slow", "Default", "Fast", "Super Fast", "FASTTTTTTTS" };
        private float[] SpeedValues = new float[] { 5.0f, 7.5f, 15.0f, 17.5f, 20.0f };
        private float Speed = 1.7f;
        internal static bool playerManagerEnabled = false;
        public string guiSelectedEnemy;
        private static bool hasGUISynced;

        private static RoundManager currentRound;
        public static MainGUI Instance => _instance;

        // GUI Variables
        private static SelectableLevel currentLevel;
        public static Rect GUIRect = new Rect(0, 0, 540, 240);
        private static int selectedTab = 0;
        private static readonly string[] tabNames = { "Computer", "Mic Mods", "Menu Btns", "Player List", "Settings" };
        private bool[] TogglePlayerList = new bool[999];
        private bool[] ToggleComputer = new bool[999];
        private bool[] ToggleMic = new bool[999];
        private bool toggled = true;
        public static Dictionary<SpawnableEnemyWithRarity, AnimationCurve> enemyPropCurves;
        public static Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>> levelEnemySpawns;
        public static Dictionary<SpawnableEnemyWithRarity, int> enemyRaritys;
        private static EnemyVent[] currentLevelVents;
        private static SpawnableEnemyWithRarity jesterRef;
        internal static PlayerControllerB playerRef;
        internal static float nightVisionIntensity, nightVisionRange;
        internal static Color nightVisionColor;
        public float toggleDelay = 0.5f;
        private float lastToggleTime;
        private Vector2 scrollPosition = Vector2.zero;
        #endregion
        #region Mods
        public static void ExplosionPlayer()
        {
            Ray ray = Camera.main.ScreenPointToRay(UnityInput.Current.mousePosition);
            RaycastHit raycastHit;
            Physics.Raycast(ray.origin, ray.direction, out raycastHit);
            if (pointer == null)
            {
                pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.Destroy(pointer.GetComponent<Rigidbody>());
                Object.Destroy(pointer.GetComponent<SphereCollider>());
                pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            }
            pointer.transform.position = raycastHit.point;
            if (UnityInput.Current.GetMouseButton(0))
            {
                enableGod = true;
                foreach (GameNetcodeStuff.PlayerControllerB ss in UnityEngine.Object.FindObjectsOfType<GameNetcodeStuff.PlayerControllerB>())
                {
                    Landmine.SpawnExplosion(ss.serverPlayerPosition, true, 999f, 999f);
                }
            } 
            else
            {
                enableGod = false;
            }
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

        [HarmonyPatch(typeof(RoundManager), "SpawnEnemyFromVent")]
        [HarmonyPrefix]
        private static void logSpawnEnemyFromVent()
        {
            Debug.Log("Attempting to spawn an enemy");
        }

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

        [HarmonyPatch(typeof(RoundManager), "AdvanceHourAndSpawnNewBatchOfEnemies")]
        [HarmonyPrefix]
        private static void updateCurrentLevelInfo(ref EnemyVent[] ___allEnemyVents, ref SelectableLevel ___currentLevel)
        {
            currentLevel = ___currentLevel;
            currentLevelVents = ___allEnemyVents;
        }

        [HarmonyPatch(typeof(HUDManager), "SubmitChat_performed")]
        [HarmonyPrefix]
        private static void Commands(HUDManager __instance)
        {
            string text = __instance.chatTextField.text;
            string text2 = "/";
            Debug.Log(text);
            if (!text.ToLower().StartsWith(text2.ToLower()))
            {
                return;
            }
            string text3 = "Default Title";
            string text4 = "Default Body";
            if (text.ToLower().StartsWith(text2 + "togglelights"))
            {
                BreakerBox breakerBox = Object.FindObjectOfType<BreakerBox>();
                if (breakerBox != null)
                {
                    text3 = "Light Change";
                    if (breakerBox.isPowerOn)
                    {
                        currentRound.TurnBreakerSwitchesOff();
                        currentRound.TurnOnAllLights(false);
                        breakerBox.isPowerOn = false;
                        text4 = "Turned the lights off";
                    }
                    else
                    {
                        currentRound.PowerSwitchOnClientRpc();
                        text4 = "Turned the lights on";
                    }
                }
            }
        }
        #endregion
        #endregion
    }
}