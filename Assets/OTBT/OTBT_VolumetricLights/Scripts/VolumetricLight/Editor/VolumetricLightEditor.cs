using OTBT.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace OTBT.VolumetricLight.Editor
{
    /*
     * Exposes settings of volumetric light and its profile
     */
    [CustomEditor(typeof(VolumetricLight)), CanEditMultipleObjects]
    public class VolumetricLightEditor : UnityEditor.Editor
    {
        private void SpotlightProperties()
        {
            EditorGUILayout.LabelField("Spotlight Settings", EditorStyles.boldLabel);

            bool overrideSpot = serializedObject.ToggleFoldout("overrideSpotAngle", "Override Spot Angle");
            OtbtEditorUtils.Space();
            EditorGUI.indentLevel++;
            if (overrideSpot)
            {
                SerializedProperty newSpotAngle = serializedObject.FindProperty("newSpotAngle");
                EditorGUILayout.PropertyField(newSpotAngle, new GUIContent("Spot Angle"));
            }
            else
            {
                SerializedProperty spotAngleWeight = serializedObject.FindProperty("spotAngleWeight");
                EditorGUILayout.PropertyField(spotAngleWeight, new GUIContent("Angle Inner/Outer"));
            }

            EditorGUI.indentLevel--;
            OtbtEditorUtils.Space();
        }

        public override void OnInspectorGUI()
        {
            VolumetricLight volumetricLight = target as VolumetricLight;
            serializedObject.Update();
            if (volumetricLight == null) return;

            OtbtEditorUtils.DrawLogoHeader();

            switch (volumetricLight.LightSource.type)
            {
                case LightType.Spot:
                    SpotlightProperties();
                    break;
                case LightType.Directional:
                    EditorGUILayout.HelpBox("Volumetric lights can not be used for directional lights.",
                        MessageType.Warning);
                    if (GUILayout.Button("Fix now!")) volumetricLight.LightSource.type = LightType.Spot;
                    OtbtEditorUtils.Space();
                    break;
                default:
                    EditorGUILayout.HelpBox(
                        "Change the Light type to see specific properties. Current Light Type: " +
                        volumetricLight.LightSource.type, MessageType.Info);
                    break;
            }

            OtbtEditorUtils.Separator();

            using (new EditorGUILayout.HorizontalScope())
            {
                SerializedProperty profileProperty = serializedObject.FindProperty("profile");
                EditorGUILayout.PropertyField(profileProperty);

                if (GUILayout.Button("New"))
                {
                    VolumetricLightProfile profile =
                        VolumetricLightProfile.CreateProfile(volumetricLight.gameObject.scene, volumetricLight.name);

                    profile.name = volumetricLight.gameObject.name + "Profile";
                    profile.light = volumetricLight.LightSource;

                    profileProperty.objectReferenceValue = profile;
                }
            }

            serializedObject.ApplyModifiedProperties();

            if (volumetricLight.Profile == null)
            {
                EditorGUILayout.HelpBox("Assign a profile or create a new one to display all options.", MessageType
                    .Info);
                return;
            }

            OtbtEditorUtils.Separator();

            CreateEditor(volumetricLight.Profile).OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}