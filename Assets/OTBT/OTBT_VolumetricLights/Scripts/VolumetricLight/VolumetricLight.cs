using System;
using UnityEngine;

namespace OTBT.VolumetricLight
{
    /*
     * Stores light source and profile, handles creation of supporting objects.
     */
    [RequireComponent(typeof(Light)), ExecuteAlways, SelectionBase, AddComponentMenu("Rendering/Volumetric Light")]
    public class VolumetricLight : MonoBehaviour
    {
        public enum BlendingMode
        {
            Alpha,
            Additive
        }

        // SPOTLIGHT SETTINGS
        [Range(0f, 1f)] public float spotAngleWeight = 1f;
        public bool overrideSpotAngle;
        [Range(1f, 179f)] public float newSpotAngle = 90;

        [SerializeField] private VolumetricLightProfile profile;
        [SerializeField] private LightVolume lightVolume;
        [SerializeField] private Light lightSource;

        private LightVolume Volume
        {
            get
            {
                if (lightVolume != null) return lightVolume;
                lightVolume = AddLightVolume();
                return lightVolume;
            }
        }

        public Light LightSource
        {
            get
            {
                if (lightSource == null)
                {
                    lightSource = GetComponent<Light>();
                }

                return lightSource;
            }
        }

        public VolumetricLightProfile Profile
        {
            get => profile;
            set => profile = value;
        }

        private void Update()
        {
            if (profile == null) return;
#if UNITY_EDITOR
            SetUpVolume(); // calling this to react to changes in the light settings
#endif
            UpdateVolumetricLight();
        }

        public void SetUpVolume()
        {
            if (Profile == null) return;

            Profile.light = LightSource;
            Volume.profile = Profile;
            Volume.RecalculateMesh();
        }
        public Color GetColor()
        {
            if (profile == null) return Color.white;
            Color col = profile.overrideLightColor
                ? profile.newColor * profile.newIntensity
                : lightSource.color * profile.intensityMultiplier * lightSource.intensity;
            col.a = profile.blendingMode == VolumetricLight.BlendingMode.Alpha ? profile.alpha : 1;
            return col;
        }
#if UNITY_EDITOR
        private void OnEnable()
        {
            SetUpVolume();
        }
#endif
        private void OnDestroy()
        {
#if UNITY_EDITOR
            DestroyImmediate(Volume.gameObject);
#else
            Destroy(Volume.gameObject);
#endif
        }

        public void UpdateVolumetricLight()
        {
            Volume.SendValuesToMaterial();
        }

        private LightVolume AddLightVolume()
        {
            GameObject obj = new GameObject("LightVolume");
            obj.transform.parent = transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.localRotation = Quaternion.identity;

            obj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

            LightVolume vol = obj.AddComponent<LightVolume>();

            vol.Setup(this, Profile);

            return vol;
        }
    }
}