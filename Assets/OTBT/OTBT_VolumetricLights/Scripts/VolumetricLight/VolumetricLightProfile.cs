using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OTBT.VolumetricLight
{
    /*
     * data storage of light properties, overrides by category
     */
    [CreateAssetMenu(menuName = "OTBT/Volumetric Lights/Volumetric Light Profile",
        fileName = "Volumetric Light Profile")]
    public class VolumetricLightProfile : ScriptableObject
    {
        public Light light;

        private const int TextureResolution = 256;

        //BLENDING OPTIONS
        public VolumetricLight.BlendingMode blendingMode = VolumetricLight.BlendingMode.Alpha;
        [Range(0f, 1f)] public float alpha = 1f;

        // COLOR OVERRIDES
        public bool overrideLightColor;
        [ColorUsage(false, false)] public Color newColor = Color.white;
        [Min(0f)] public float newIntensity = 1f;

        [Range(0f, 2f)] public float intensityMultiplier = 1f;

        // FADING OPTIONS
        public bool useCustomFade;
        [GradientUsage(false)] public Gradient fadeGradient = new Gradient();
        [SerializeField] private Texture2D fadeTexture;

        // GEOMETRY AND DEPTH FADING OVERRIDES
        public bool overrideFading;
        [Min(0f)] public float depthFadeDistance = 5f;
        [Min(0f)] public float cameraFadeDistance = 5f;
        [Min(0f)] public float fresnelFadeStrength = 1f;

        // FOG SETTINGS
        public bool useFogTexture;

        [Tooltip("The red channel of this texture is used to mask the light.")]
        public Texture2D fogTexture;

        public float fogTextureTiling = 1;

        [Range(0f, 1f)] public float fogOpacity = 1f;
        public float verticalScrollingSpeed;
        public float horizontalScrollingSpeed;

        // PROPERTIES
        public Vector2 FogScrollingSpeed => new Vector2(horizontalScrollingSpeed, verticalScrollingSpeed);

        public Texture2D FadeTexture {
            get {
                if (fadeTexture == null) {
                    GenerateTexture(TextureResolution);
                }
                return fadeTexture;
            }
        }

        private void OnValidate()
        {
            GenerateTexture(TextureResolution);
        }

        public void GenerateTexture(int textureResolution)
        {
            Color[] pixels = new Color[textureResolution];
            float step = 1f / (textureResolution - 1);
            for (int x = 0; x < pixels.Length; x++)
            {
                pixels[x] = fadeGradient.Evaluate(step * x);
            }

            // create a new texture, if we have to.
            if(fadeTexture == null)
                fadeTexture = new Texture2D(textureResolution, 1);
            if (fadeTexture.width != textureResolution) 
                fadeTexture = new Texture2D(textureResolution, 1);
            
            fadeTexture.SetPixels(pixels);
            fadeTexture.Apply();
        }

#if UNITY_EDITOR
        //adapted from VolumeProfileFactory.cs (renderpipeline-core)
        public static VolumetricLightProfile CreateProfile(Scene scene, string targetName)
        {
            string path;

            if (string.IsNullOrEmpty(scene.path))
            {
                path = "Assets/";
            }
            else
            {
                string scenePath = Path.GetDirectoryName(scene.path);
                string extPath = scene.name;
                string profilePath = scenePath + "/" + extPath;

                if (!AssetDatabase.IsValidFolder(profilePath))
                    AssetDatabase.CreateFolder(scenePath, extPath);

                path = profilePath + "/";
            }

            path += targetName + " Profile.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            VolumetricLightProfile profile = CreateInstance<VolumetricLightProfile>();
            AssetDatabase.CreateAsset(profile, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return profile;
        }
#endif
    }
}