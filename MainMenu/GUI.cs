using BepInEx;
using GameNetcodeStuff;
using LethalCompanyMenu.MainMenu.Stuff;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using static LethalCompanyMenu.MainMenu.Stuff.PlayerControllerHandler;
using Color = UnityEngine.Color;
using Object = UnityEngine.Object;

namespace LethalCompanyMenu.MainMenu
{
    [BepInPlugin("lethalcompany.GUI", "notfishvr", "1.0.0")]
    internal class MainGUI : BaseUnityPlugin
    {
        public static bool Doonce = false;
        private void DrawSelfTab()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            ToggleSelf[1] = ToggleButton("No Fog", ToggleSelf[1]);
            ToggleSelf[2] = ToggleButton("Night Vision", ToggleSelf[2]);
            ToggleSelf[3] = ToggleButton("God Mode", ToggleSelf[3]);
            ToggleSelf[4] = ToggleButton("Speed", ToggleSelf[4]);
            ToggleSelf[5] = ToggleButton("Fly", ToggleSelf[5]);
            ToggleSelf[6] = ToggleButton("No Weight", ToggleSelf[6]);
            ToggleSelf[7] = ToggleButton("No Fall Damage", ToggleSelf[7]);
            ToggleSelf[8] = ToggleButton("Loot Through Walls", ToggleSelf[8]);
            ToggleSelf[9] = ToggleButton("Invisibility", ToggleSelf[9]);

            GUILayout.EndScrollView();
        }
        private void DrawServerTab()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            ToggleServer[1] = ToggleButton("Enemy Cant Be Spawned", ToggleServer[1]);
            ToggleServer[2] = ToggleButton("Gets All Scrap", ToggleServer[2]);
            ToggleServer[3] = ToggleButton("Never Lose Scrap", ToggleServer[3]);

            GUILayout.EndScrollView();
        }
        private void DrawVisualsTab()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            ToggleVisuals[1] = ToggleButton("Object ESP", ToggleVisuals[1]);
            ToggleVisuals[2] = ToggleButton("Player ESP", ToggleVisuals[2]);
            ToggleVisuals[3] = ToggleButton("Enemy ESP", ToggleVisuals[3]);

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
                _instance.DrawSelfTab();
            }
            else if (selectedTab == 1)
            {
                _instance.DrawServerTab();
            }
            else if (selectedTab == 2)
            {
                _instance.DrawVisualsTab();
            }
            else if (selectedTab == 3)
            {
                _instance.DrawHostTab();
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
        internal static bool nightVision, enableGod;
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
        public static readonly string[] tabNames = { "Self Tab", "Server Tab", "Visuals Tab", "Host", "Player List", "Settings" };
        public bool[] TogglePlayerList = new bool[999];
        public bool[] ToggleSelf = new bool[999];
        public bool[] ToggleServer = new bool[999];
        public bool[] ToggleVisuals = new bool[999];
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