using System;
using OTBT.VolumetricLight.Helpers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace OTBT.VolumetricLight
{
    /*
     * Handles mesh creation and material modification
     */
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class LightVolume : MonoBehaviour
    {
        [SerializeField] public VolumetricLightProfile profile;
        private const string MaterialPathAlpha = "Assets/OTBT/OTBT_VolumetricLights/Shaders/VolumetricLight_Alpha.mat";

        private const string MaterialPathAdditive =
            "Assets/OTBT/OTBT_VolumetricLights/Shaders/VolumetricLight_Additive.mat";

        private static readonly int LightColor = Shader.PropertyToID("_Color");
        private static readonly int FogTexture = Shader.PropertyToID("_FogTexture");
        private static readonly int FadeTexture = Shader.PropertyToID("_FadeTexture");
        private static readonly int DepthFade = Shader.PropertyToID("_DepthFadeDistance");
        private static readonly int CameraFade = Shader.PropertyToID("_CameraFadeDistance");
        private static readonly int FresnelFade = Shader.PropertyToID("_FresnelFadeStrength");
        private static readonly int FogScrollSpeed = Shader.PropertyToID("_FogScrollSpeed");
        private static readonly int FogOpacity = Shader.PropertyToID("_FogOpacity");

        [HideInInspector, SerializeField] public VolumetricLight volumetricLight;
        private MaterialPropertyBlock _mpb;
        private MaterialPropertyBlock propertyBlock => _mpb ?? (_mpb = new MaterialPropertyBlock());
        private MeshRenderer _renderer;
        private static readonly int FogTextureTiling = Shader.PropertyToID("_FogTextureTiling");
        private MeshRenderer meshRenderer => _renderer ? _renderer : (_renderer = GetComponent<MeshRenderer>());
        private LightType _currentMeshType = LightType.Directional;

        private void Awake() {
            if (volumetricLight == null) {
                if(gameObject.transform.parent != null)
                    volumetricLight = gameObject.transform.parent.GetComponent<VolumetricLight>();
                
                if (volumetricLight == null) 
                    volumetricLight = gameObject.GetComponentInChildren<VolumetricLight>();
            }
        }

        private void Start() => SendValuesToMaterial();

        public void Setup(VolumetricLight vol, VolumetricLightProfile prof)
        {
            _renderer = GetComponent<MeshRenderer>();

            volumetricLight = vol;
            profile = prof;

            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.lightProbeUsage = LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

            UpdateMaterialType();
        }

        public void UpdateMaterialType()
        {
#if UNITY_EDITOR
            if (!profile) return;
            meshRenderer.sharedMaterial =
                AssetDatabase.LoadAssetAtPath(
                        profile.blendingMode == VolumetricLight.BlendingMode.Alpha
                            ? MaterialPathAlpha
                            : MaterialPathAdditive,
                        typeof(Material))
                    as Material;
#endif
        }

        public void SendValuesToMaterial()
        {
#if UNITY_EDITOR
            UpdateMaterialType();
#endif
            if (profile == null) return;
            meshRenderer.GetPropertyBlock(propertyBlock);
            if (propertyBlock == null)
            {
                Debug.LogError("No Mesh PropertyBlock could be loaded!");
                return;
            }

            propertyBlock.Clear();

            propertyBlock.SetColor(LightColor, volumetricLight == null ? Color.white : volumetricLight.GetColor());
            if (profile.useFogTexture && profile.fogTexture != null)
            {
                propertyBlock.SetTexture(FogTexture, profile.fogTexture);
                propertyBlock.SetFloat(FogTextureTiling, profile.fogTextureTiling);
                propertyBlock.SetFloat(FogOpacity, profile.fogOpacity);
                propertyBlock.SetVector(FogScrollSpeed, profile.FogScrollingSpeed);
            }

            if (profile.overrideFading)
            {
                propertyBlock.SetFloat(DepthFade, profile.depthFadeDistance);
                propertyBlock.SetFloat(CameraFade, profile.cameraFadeDistance);
                propertyBlock.SetFloat(FresnelFade, profile.fresnelFadeStrength);
            }

            if (profile.useCustomFade && profile.FadeTexture != null)
            {
                propertyBlock.SetTexture(FadeTexture, profile.FadeTexture);
            }

            meshRenderer.SetPropertyBlock(propertyBlock);
        }

        public void RecalculateMesh()
        {
#if UNITY_EDITOR
            // destroy old mesh before creating a new one
            if (GetComponent<MeshFilter>().sharedMesh != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(GetComponent<MeshFilter>().sharedMesh);
                }
                else
                {
                    DestroyImmediate(GetComponent<MeshFilter>().sharedMesh);
                }

                GetComponent<MeshFilter>().sharedMesh = null;
            }

            Mesh mesh = new Mesh();
            Light lightShape = profile.light;
            switch (lightShape.type)
            {
                case LightType.Spot:
                    float angle = volumetricLight.overrideSpotAngle
                        ? volumetricLight.newSpotAngle / 2f
                        : Mathf.Lerp(lightShape.innerSpotAngle, lightShape.spotAngle, volumetricLight.spotAngleWeight) /
                          2f;
                    mesh = MeshBuilder.HollowCone(lightShape.range, angle);
                    break;
                case LightType.Point:
                    mesh = MeshBuilder.Sphere(lightShape.range, 32);
                    break;
                case LightType.Disc:
                    mesh = MeshBuilder.HollowCylinder(lightShape.range, lightShape.areaSize.x, lightShape.areaSize.x,
                        32, 2);
                    break;
                case LightType.Area:
                    Vector2 size = lightShape.areaSize;
                    mesh = MeshBuilder.Box(lightShape.range, size.x, size.y);
                    break;
                case LightType.Directional:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (mesh == null)
            {
                Debug.LogWarning("Light Type can not be used to create Volumetric Lights!");
                return;
            }

            mesh.RecalculateBounds();
            GetComponent<MeshFilter>().sharedMesh = mesh;
            _currentMeshType = lightShape.type;
#endif
        }
    }
}