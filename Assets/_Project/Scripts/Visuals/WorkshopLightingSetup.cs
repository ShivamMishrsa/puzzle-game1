using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ColorGateBlast
{
    /// <summary>
    /// Lighting and post-processing for the workshop look, applied identically to every level:
    ///  * warm angled key light with soft, grounded shadows falling to the bottom-left of everything,
    ///  * cool low rim / fill light to separate metallic edges and screws from the dark wood,
    ///  * URP volume: bloom (threshold 0.95) so neon gate chevrons, lasers and sparks glow, gentle depth of field that
    ///    softens the far corners, ACES tonemapping and a soft warm vignette (0.22).
    /// </summary>
    public sealed class WorkshopLightingSetup : MonoBehaviour
    {
        private Light key, rim;
        private Volume volume;

        public void Apply(Camera cam, float focusDistance)
        {
            // the Setup tool's generic light / volume would double up with the workshop rig: switch them off
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
                if (lights[i].type == LightType.Directional && !lights[i].gameObject.name.StartsWith("Workshop") && lights[i].gameObject != gameObject) lights[i].enabled = false;
            Volume[] vols = FindObjectsByType<Volume>(FindObjectsSortMode.None);
            for (int i = 0; i < vols.Length; i++)
                if (vols[i].gameObject.name != "WorkshopVolume") vols[i].enabled = false;

            if (key == null) key = MakeLight("WorkshopKeyLight", new Color(1f, 0.93f, 0.8f), 1.5f, LightShadows.Soft);
            key.shadowStrength = 0.75f;
            key.shadowBias = 0.03f;
            key.shadowNormalBias = 0.2f;
            // light travels down and slightly to the left/towards the camera => shadows fall to the bottom-left
            key.transform.rotation = Quaternion.LookRotation(new Vector3(-0.42f, -0.82f, -0.38f), Vector3.up);

            if (rim == null) rim = MakeLight("WorkshopRimLight", new Color(0.55f, 0.72f, 1f), 0.45f, LightShadows.None);
            rim.transform.rotation = Quaternion.LookRotation(new Vector3(0.5f, -0.45f, 0.75f), Vector3.up);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.34f, 0.31f, 0.3f);

            BuildVolume(focusDistance);
            UniversalAdditionalCameraData data = cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
        }

        private Light MakeLight(string name, Color color, float intensity, LightShadows shadows)
        {
            // reuse a light that already exists in the scene (the Setup tool creates one)
            Light l = null;
            Light[] all = FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++) if (all[i].type == LightType.Directional && all[i].gameObject.name == name) l = all[i];
            if (l == null)
            {
                GameObject go = new GameObject(name);
                go.transform.SetParent(transform, false);
                l = go.AddComponent<Light>();
                l.type = LightType.Directional;
            }
            l.color = color;
            l.intensity = intensity;
            l.shadows = shadows;
            return l;
        }

        private void BuildVolume(float focusDistance)
        {
            if (volume == null)
            {
                GameObject go = new GameObject("WorkshopVolume");
                go.transform.SetParent(transform, false);
                volume = go.AddComponent<Volume>();
                volume.isGlobal = true;
                volume.priority = 10f;
                VolumeProfile p = ScriptableObject.CreateInstance<VolumeProfile>();
                volume.sharedProfile = p;

                Bloom bloom = p.Add<Bloom>(true);
                bloom.threshold.Override(0.95f);
                bloom.intensity.Override(0.75f);
                bloom.scatter.Override(0.6f);

                Vignette vig = p.Add<Vignette>(true);
                vig.intensity.Override(0.22f);
                vig.smoothness.Override(0.55f);
                vig.color.Override(new Color(0.12f, 0.06f, 0.02f));

                Tonemapping tm = p.Add<Tonemapping>(true);
                tm.mode.Override(TonemappingMode.ACES);

                ColorAdjustments ca = p.Add<ColorAdjustments>(true);
                ca.postExposure.Override(0.15f);
                ca.contrast.Override(8f);
                ca.saturation.Override(6f);
            }

            // depth of field: sharp on the tray + monument, soft on the far screen corners
            DepthOfField dof;
            if (!volume.sharedProfile.TryGet(out dof)) dof = volume.sharedProfile.Add<DepthOfField>(true);
            dof.mode.Override(DepthOfFieldMode.Gaussian);
            dof.gaussianStart.Override(focusDistance * 1.05f);
            dof.gaussianEnd.Override(focusDistance * 1.45f);
            dof.gaussianMaxRadius.Override(1.2f);
            dof.highQualitySampling.Override(false);
        }
    }
}
