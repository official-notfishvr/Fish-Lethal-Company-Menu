using System;
using System.Collections.Generic;
using System.Text;
using LethalCompanyMenu.MainMenu;
using UnityEngine;

namespace LethalCompanyHacks.MainMenu
{
    public class Render : MonoBehaviour
    {
        private static Render _instance;
        public static Render Instance => _instance;
        private void Awake()
        {
            _instance = this;
        }
        public void UpdateStyles()
        {
            GUI.skin.button = CreateButtonStyle(MainGUI.Instance.button, MainGUI.Instance.buttonHovered, MainGUI.Instance.buttonActive);
            GUI.skin.window = CreateWindowStyle(MainGUI.Instance.windowBackground);
            GUI.skin.textArea = CreateTextFieldStyle(MainGUI.Instance.textArea, MainGUI.Instance.textAreaHovered, MainGUI.Instance.textAreaActive);
            GUI.skin.textField = CreateTextFieldStyle(MainGUI.Instance.textArea, MainGUI.Instance.textAreaHovered, MainGUI.Instance.textAreaActive);
            GUI.skin.box = CreateBoxStyle(MainGUI.Instance.box);
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
        public static void RenderTooltip()
        {
            if (string.IsNullOrEmpty(MainGUI.strTooltip)) { return; }

            GUIStyle label = GUI.skin.label;
            GUIContent guicontent = new GUIContent(MainGUI.strTooltip);
            float num = label.CalcSize(guicontent).x + 10f;
            float num2 = label.CalcHeight(guicontent, num - 10f) + 10f;
            Vector2 mousePosition = Event.current.mousePosition;
            Color c_Theme = new Color(1f, 1f, 1f, 1f);
            int originalDepth = GUI.depth;
            GUI.depth = 100;

            GUI.color = new Color(c_Theme.r, c_Theme.g, c_Theme.b, 0.8f);
            Rect rect = new Rect(mousePosition.x + 20f, mousePosition.y + 20f, num, num2);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(new Rect(rect.x + 5f, rect.y + 5f, num - 10f, num2 - 10f), MainGUI.strTooltip);
            GUI.depth = originalDepth;
        }
        public static void Reset() { MainGUI.strTooltip = null; }
        public static void ColorPicker(string str, ref Color col)
        {
            GUILayout.Label(string.Concat(new string[] { str, " (R: ", Mathf.RoundToInt(col.r * 255f).ToString(), ", G: ", Mathf.RoundToInt(col.g * 255f).ToString(), ", B: ", Mathf.RoundToInt(col.b * 255f).ToString(), ")" }),
            Array.Empty<GUILayoutOption>());
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            col.r = GUILayout.HorizontalSlider(col.r, 0f, 1f, new GUILayoutOption[] { GUILayout.Width(80f) });
            col.g = GUILayout.HorizontalSlider(col.g, 0f, 1f, new GUILayoutOption[] { GUILayout.Width(80f) });
            col.b = GUILayout.HorizontalSlider(col.b, 0f, 1f, new GUILayoutOption[] { GUILayout.Width(80f) });
            GUILayout.EndHorizontal();
        }
    }
}
