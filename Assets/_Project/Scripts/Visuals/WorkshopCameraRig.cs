using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ColorGateBlast
{
    /// <summary>
    /// High-angle 3/4 top-down tabletop camera (FOV ~38, pitch ~58) framed for 9:16 portrait:
    /// the top ~12% stays clear for the HUD plaques, the conveyor loop (monument on top, puzzle tray below) fills the
    /// middle, and the bottom ~14% stays clear for the track LEDs, Restart plaque and booster medallions.
    /// The rig solves distance and look-at point numerically, so it works for every tray size and screen shape.
    /// </summary>
    public sealed class WorkshopCameraRig : MonoBehaviour
    {
        public float Fov = 38f;
        public float Pitch = 58f;
        public float TopClear = 0.135f;
        public float BottomClear = 0.15f;
        public float SideClear = 0.025f;

        private Camera cam;

        public float Distance { get; private set; }
        public Vector3 Target { get; private set; }

        public void Init(Camera c)
        {
            cam = c;
            cam.orthographic = false;
            cam.fieldOfView = Fov;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.045f, 0.04f);
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 120f;
            UniversalAdditionalCameraData data = cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        }

        private void Place(Vector3 target, float dist)
        {
            float p = Pitch * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(0f, Mathf.Sin(p), -Mathf.Cos(p));
            cam.transform.position = target + dir * dist;
            cam.transform.LookAt(target, Vector3.up);
        }

        /// <summary>Frames the conveyor loop (centre, half extents in world x / z) inside the HUD-safe area.</summary>
        public void Frame(Vector3 loopCenter, float halfX, float halfZ) { Frame(loopCenter, halfX, halfZ, new Vector3(loopCenter.x, 0f, loopCenter.z + halfZ)); }

        /// <summary>Same, but <paramref name="extraTop"/> (e.g. the tip of the monument) must also stay under the top HUD clearance.</summary>
        public void Frame(Vector3 loopCenter, float halfX, float halfZ, Vector3 extraTop)
        {
            if (cam == null) return;
            cam.fieldOfView = Fov;
            Vector3 topPt = new Vector3(loopCenter.x, 0f, loopCenter.z + halfZ);
            Vector3 botPt = new Vector3(loopCenter.x, 0f, loopCenter.z - halfZ);
            Vector3 nearL = new Vector3(loopCenter.x - halfX, 0f, loopCenter.z - halfZ);
            Vector3 nearR = new Vector3(loopCenter.x + halfX, 0f, loopCenter.z - halfZ);
            float wantTop = 1f - TopClear;

            float lo = 8f, hi = 140f;
            float bestTz = loopCenter.z;
            for (int outer = 0; outer < 30; outer++)
            {
                float dist = (lo + hi) * 0.5f;
                // inner: choose the look-at z that puts the far edge of the loop exactly at the top clearance line
                float a = loopCenter.z - 40f, b = loopCenter.z + 40f;
                float tz = loopCenter.z;
                for (int inner = 0; inner < 30; inner++)
                {
                    tz = (a + b) * 0.5f;
                    Place(new Vector3(loopCenter.x, 0f, tz), dist);
                    float top = Mathf.Max(cam.WorldToViewportPoint(topPt).y, cam.WorldToViewportPoint(extraTop).y);
                    if (top > wantTop) a = tz; else b = tz; // looking further away pushes the far edge down
                }
                Place(new Vector3(loopCenter.x, 0f, tz), dist);
                float bottom = cam.WorldToViewportPoint(botPt).y;
                float left = cam.WorldToViewportPoint(nearL).x;
                float right = cam.WorldToViewportPoint(nearR).x;
                bool fits = bottom >= BottomClear && left >= SideClear && right <= 1f - SideClear;
                if (fits) { hi = dist; bestTz = tz; } else lo = dist;
            }
            Distance = hi;
            Target = new Vector3(loopCenter.x, 0f, bestTz);
            Place(Target, Distance);
        }

        /// <summary>Plane (y = 0) hit point of a viewport position; used to place props in the screen corners.</summary>
        public Vector3 GroundPoint(float vx, float vy)
        {
            Ray r = cam.ViewportPointToRay(new Vector3(vx, vy, 0f));
            if (Mathf.Abs(r.direction.y) < 0.0001f) return Vector3.zero;
            float t = -r.origin.y / r.direction.y;
            return r.origin + r.direction * t;
        }
    }
}
