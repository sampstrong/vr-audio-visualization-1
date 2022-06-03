using System;
using UnityEngine;
using UnityEditor;

namespace OTBT.Utils.Editor
{
    public static class OtbtEditorUtils
    {
        private const int SpaceHeight = 6;
        private const int LineHeight = 2;
        private const string LogoDark = "Misc/OTBT_logo_dark";
        private const string LogoLight = "Misc/OTBT_logo";

        public static void Separator(int height = LineHeight)
        {
            Space();
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            Space();
        }

        public static void Space(int height = SpaceHeight)
        {
            EditorGUILayout.Space(height);
        }

        public static void DrawLogoHeader()
        {
            Texture logo = Resources.Load<Texture>(EditorGUIUtility.isProSkin ? LogoLight : LogoDark);
            if (logo != null)
            {
                GUI.DrawTexture(new Rect(10, 10, 70, 60), logo);
                GUILayout.Space(80);
            }
            else
            {
                GUILayout.Label("Off The Beaten Track", EditorStyles.boldLabel);
            }
        }

        public static Texture2D TextureFromColor(Texture2D texture, Color color)
        {
            Color[] colors = texture.GetPixels();

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = colors[i].a > 0 ? color : colors[i];
            }

            texture.SetPixels(colors);
            texture.Apply();

            return texture;
        }

        public static Texture2D TextureFromColor(Color color)
        {
            Color[] colors = new Color[1];
            colors[0] = color;

            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        public static GameObject FindInChildren(Transform parent, string name, string parentName, bool strict = false)
        {
            foreach (Transform child in parent)
            {
                if (strict
                    ? (child.name == (name) && parent.name == (parentName))
                    : (child.name.ToLower().Contains(name.ToLower()) &&
                       parent.name.ToLower().Contains(parentName.ToLower())))
                {
                    return child.gameObject;
                }

                GameObject childFound = FindInChildren(child, name, parentName, strict);
                if (childFound != null)
                    return childFound;
            }

            return null;
        }

        public static bool ToggleFoldout(this SerializedObject serializedObject, string propertyName, string caption)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property.propertyType != SerializedPropertyType.Boolean)
                throw new ArgumentException("Serialized Property of Type Boolean expected.");

            GUIStyle pushButton = new GUIStyle(EditorStyles.miniButtonRight);
            if (property.boolValue)
            {
                pushButton.fontStyle = FontStyle.Bold;
            }

            DrawPropertyField(serializedObject, propertyName, caption);

            return property.boolValue;
        }

        public static void DrawPropertyField(this SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            EditorGUILayout.PropertyField(property);
        }

        public static void DrawPropertyField(this SerializedObject serializedObject, string propertyName, string label)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            EditorGUILayout.PropertyField(property, new GUIContent(label));
        }
    }
}