using BepInEx;
using GameNetcodeStuff;
using LethalCompanyHacks.MainMenu;
using LethalCompanyMenu.MainMenu.Stuff;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using static IngamePlayerSettings;
using static LethalCompanyMenu.MainMenu.Stuff.PlayerControllerHandler;
using Color = UnityEngine.Color;
using Object = UnityEngine.Object;

namespace LethalCompanyMenu.MainMenu
{
    public enum ToggleTypeSelf
    {
        NoFog,
        NightVision,
        GodMode,
        Speed,
        Fly,
        NoWeight,
        NoFallDamage,
        LootThroughWalls,
        Invisibility
    }
    public enum ToggleTypeServer
    {
        EnemyCantBeSpawned,
        Gets_All_Scrap,
        Never_Lose_Scrap,
        Disable_All_Turrets,
        Explode_All_Landmines
    }
    public enum ToggleTypeVisuals
    {
        Object_ESP,
        Player_ESP,
        Enemy_ESP,
    }
    public enum ToggleTypeHost
    {
        Revive_All_Players,
        Spawn_Enemy,
        Force_Start,
        Force_EndGame,
        //Kill_All_Enemies,
        //Delete_All_Enemies
    }

    [BepInPlugin("lethalcompany.GUI", "notfishvr", "1.0.0")]
    internal class MainGUI : BaseUnityPlugin
    {
        #region Main GUI
        private void DrawTab<T>(Dictionary<T, bool> toggleDictionary, Func<T, string> tooltipFunction)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (var toggleType in toggleDictionary.Keys.ToList())
            {
                string toggleName = toggleType.ToString().Replace("_", " ");
                toggleDictionary[toggleType] = ToggleButton(toggleName, toggleDictionary[toggleType], tooltipFunction(toggleType));
            }
            GUILayout.EndScrollView();
        }
        private string GetTooltip<T>(T toggleEnum)
        {
            switch (toggleEnum)
            {
                // Self
                case ToggleTypeSelf.NoFog:
                    return "Theres No Fog";
                case ToggleTypeSelf.NightVision:
                    return "You Can See In Night Time";
                case ToggleTypeSelf.Invisibility:
                    return "Players will not be able to see you.";
                case ToggleTypeSelf.NoFallDamage:
                    return "You Take No Fall Damage";
                case ToggleTypeSelf.Fly:
                    return "You Can Fly!";
                case ToggleTypeSelf.GodMode:
                    return "Prevents you from taking any damage.";
                case ToggleTypeSelf.LootThroughWalls:
                    return "Allows you to interact with anything through walls.";
                case ToggleTypeSelf.Speed:
                    return "You Move Fasttt";
                case ToggleTypeSelf.NoWeight:
                    return "Removes speed limitations caused by item weight.";
                // Server
                case ToggleTypeServer.Disable_All_Turrets:
                    return "Turrets Will be Disable";
                case ToggleTypeServer.EnemyCantBeSpawned:
                    return "Enemy Can NOT be Spawned";
                case ToggleTypeServer.Explode_All_Landmines:
                    return "all Landmines will be Explode";
                case ToggleTypeServer.Gets_All_Scrap:
                    return "You Will Get All Scrap";
                case ToggleTypeServer.Never_Lose_Scrap:
                    return "You Will Never Lose Scrap";
                // Visuals
                case ToggleTypeVisuals.Enemy_ESP:
                    return "You Can See All Enemy";
                case ToggleTypeVisuals.Object_ESP:
                    return "You Can See All Object";
                case ToggleTypeVisuals.Player_ESP:
                    return "You Can See All Players";
                // Host
                case ToggleTypeHost.Force_EndGame:
                    return "You Will Force End The Game";
                case ToggleTypeHost.Force_Start:
                    return "You Will Force Start The Game";
                case ToggleTypeHost.Spawn_Enemy:
                    return "You Will Spawn a Enemy";
                case ToggleTypeHost.Revive_All_Players:
                    return "You Will Revive All Players";
                default:
                    return "";
            }
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
            Render.ColorPicker("Theme", ref c_Theme);
            GUILayout.EndScrollView();
        }
        public void Update()
        {
            GUIToggleCheck();
            _playerManager.UpdatePlayers();
            playerControllerHandler.Update();
            Render.Reset();
            buttonActive = Render.Instance.CreateTexture(c_Theme);
            buttonHovered = Render.Instance.CreateTexture(c_Theme);
        }
        private void OnGUI()
        {
            GUI.skin = GUI.skin ?? new GUISkin();
            Render.Instance.UpdateStyles();
            Update();
            if (toggled) { GUIRect = GUI.Window(69, GUIRect, OnGUI, "Lethal Company GUI | Toggle: " + toggleKey); }
        }
        public static void OnGUI(int windowId)
        {
            //Instance.Update();
            GUILayout.BeginArea(new Rect(10, 10, GUIRect.width - 20, GUIRect.height - 20));
            GUILayout.Space(10);

            DrawTabButtons();

            if (selectedTab == 0)
            {
                Instance.DrawTab(Instance.ToggleSelf, Instance.GetTooltip<ToggleTypeSelf>);
            }
            else if (selectedTab == 1)
            {
                Instance.DrawTab(Instance.ToggleServer, Instance.GetTooltip<ToggleTypeServer>);
            }
            else if (selectedTab == 2)
            {
                Instance.DrawTab(Instance.ToggleVisuals, Instance.GetTooltip<ToggleTypeVisuals>);
            }
            else if (selectedTab == 3)
            {
                Instance.DrawTab(Instance.ToggleHost, Instance.GetTooltip<ToggleTypeHost>);
            }
            else if (selectedTab == 4)
            {
                _instance.DrawPlayerListTab();
            }
            else if (selectedTab == 5)
            {
                _instance.DrawSettingsTab();
            }

            GUILayout.EndArea();
            GUI.DragWindow(new Rect(0, 0, GUIRect.width, 20));
            Render.RenderTooltip();
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
                    GUIStyle selectedStyle = Render.Instance.CreateButtonStyle(Instance.buttonActive, Instance.buttonHovered, Instance.buttonActive);
                    if (GUILayout.Button(tabNames[i], selectedStyle))
                    {
                        selectedTab = i;
                    }
                }
                else
                {
                    GUIStyle unselectedStyle = Render.Instance.CreateButtonStyle(Instance.button, Instance.buttonHovered, Instance.buttonActive);
                    if (GUILayout.Button(tabNames[i], unselectedStyle))
                    {
                        selectedTab = i;
                    }
                }
            }

            GUILayout.EndHorizontal();
        }
        private bool ToggleButton(string text, bool toggle, string Tooltip = "")
        {
            GUIStyle buttonStyle = Render.Instance.CreateButtonStyle(toggle ? buttonActive : button, buttonHovered, buttonActive);
            if (GUILayout.Button(text, buttonStyle)) { return !toggle; }
            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                strTooltip = Tooltip;
            }
            return toggle;
        }
        private void Awake()
        {
            ToggleSelf = Enum.GetValues(typeof(ToggleTypeSelf)).Cast<ToggleTypeSelf>().ToDictionary(t => t, t => false);
            ToggleServer = Enum.GetValues(typeof(ToggleTypeServer)).Cast<ToggleTypeServer>().ToDictionary(t => t, t => false);
            ToggleVisuals = Enum.GetValues(typeof(ToggleTypeVisuals)).Cast<ToggleTypeVisuals>().ToDictionary(t => t, t => false);
            ToggleHost = Enum.GetValues(typeof(ToggleTypeHost)).Cast<ToggleTypeHost>().ToDictionary(t => t, t => false);
            _playerManager = new PlayerManager();
            playerControllerHandler = new PlayerControllerHandler();
            _instance = this;
            if (didit)
            {
                c_Theme = new Color32(75, 75, 75, 255);
                buttonActive = Render.Instance.CreateTexture(new Color32(100, 100, 100, 255));
                buttonHovered = Render.Instance.CreateTexture(new Color32(75, 75, 75, 255));
                button = Render.Instance.CreateTexture(new Color32(64, 64, 64, 255));
                windowBackground = Render.Instance.CreateTexture(new Color32(30, 30, 30, 255));
                textArea = Render.Instance.CreateTexture(new Color32(64, 64, 64, 255));
                textAreaHovered = Render.Instance.CreateTexture(new Color32(75, 75, 75, 255));
                textAreaActive = Render.Instance.CreateTexture(new Color32(100, 100, 100, 255));
                box = Render.Instance.CreateTexture(new Color32(40, 40, 40, 255));
                didit = false;
            }
        }
        #endregion
        #region Field
        // Textures
        public Texture2D button, buttonHovered, buttonActive, windowBackground, textArea, textAreaHovered, textAreaActive, box;
        public GameObject directionalLightClone;
        internal static bool nightVision;
        public PlayerControllerHandler playerControllerHandler;
        public static PlayerManager _playerManager;
        public Dictionary<Type, List<Component>> objectCache = new Dictionary<Type, List<Component>>();
        public bool joining = false;
        public bool didit = true;
        public bool shouldSpawnEnemy;
        public static string strTooltip = null;
        public static Color c_Theme;
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
        public static readonly string[] tabNames = { "Self Tab", "Server Tab", "Visuals", "Host & World Tab", "Player List", "Settings" };
        public bool[] TogglePlayerList = new bool[999];
        public Dictionary<ToggleTypeSelf, bool> ToggleSelf = new Dictionary<ToggleTypeSelf, bool>();
        public Dictionary<ToggleTypeServer, bool> ToggleServer = new Dictionary<ToggleTypeServer, bool>();
        public Dictionary<ToggleTypeVisuals, bool> ToggleVisuals = new Dictionary<ToggleTypeVisuals, bool>();
        public Dictionary<ToggleTypeHost, bool> ToggleHost = new Dictionary<ToggleTypeHost, bool>();
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
    #region Loader
    [BepInPlugin("com.notfishvr.lethalcompany", "notfishvr", "1.0.0")]
    public class Loader : BaseUnityPlugin
    {
        public void FixedUpdate()
        {
            Awake();
            if (!GameObject.Find("Loader"))
            {
                GameObject Loader = new GameObject("Loader");
                Loader.AddComponent<LocalPlayer>();
                Loader.AddComponent<Render>();
                Loader.AddComponent<MainGUI>();
            }
        }
        private void Awake()
        {
            try
            {
                string text = Paths.ConfigPath + "/BepInEx.cfg";
                string text2 = File.ReadAllText(text);
                text2 = Regex.Replace(text2, "HideManagerGameObject = .+", "HideManagerGameObject = true");
                File.WriteAllText(text, text2);
            }
            catch (Exception ex)
            {

            }
        }
    }
    #endregion
}