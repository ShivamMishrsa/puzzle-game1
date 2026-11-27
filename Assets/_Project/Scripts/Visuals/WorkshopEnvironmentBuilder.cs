using System.Collections.Generic;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>
    /// Builds the persistent workshop around the puzzle: a scratched oak plank bench, a ceramic mug (top-left, edge only),
    /// an open black precision-bit kit and a black-and-yellow flathead screwdriver (top-right), plus 15-20 tiny metal
    /// wood screws and washers scattered around the outer corners of the conveyor loop. Props sit in the screen corners,
    /// which the post-processing depth of field softens.
    /// </summary>
    public sealed class WorkshopEnvironmentBuilder : MonoBehaviour
    {
        private Transform root;

        public void Clear()
        {
            if (root != null) Destroy(root.gameObject);
            root = null;
        }

        public void Build(WorkshopCameraRig rig, Vector3 loopCenter, float loopHalfX, float loopHalfZ, int seed)
        {
            Clear();
            root = new GameObject("WorkshopEnvironment").transform;
            root.SetParent(transform, false);

            BuildBench();
            BuildMug(rig.GroundPoint(0.12f, 0.955f));
            BuildBitKit(rig.GroundPoint(0.90f, 0.935f));
            BuildScrewdriver(rig.GroundPoint(0.90f, 0.80f));
            ScatterHardware(rig, loopCenter, loopHalfX, loopHalfZ, seed);
        }

        private void BuildBench()
        {
            MeshBuilder mb = new MeshBuilder();
            mb.AddSlab(0f, 0f, 90f, 90f, 0.5f, 0.6f, 0.12f, -0.5f, 3);
            GameObject top = new GameObject("OakBench");
            top.transform.SetParent(root, false);
            top.AddComponent<MeshFilter>().sharedMesh = mb.ToMesh("OakBench", 0.1f);
            top.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.WorkshopWood;
            // plank seams: thin dark lines running across the bench
            MeshBuilder seams = new MeshBuilder();
            for (int i = -6; i <= 6; i++)
                seams.AddQuad(new Vector3(-45f, 0.003f, i * 3.1f - 0.03f), new Vector3(45f, 0.003f, i * 3.1f - 0.03f), new Vector3(45f, 0.003f, i * 3.1f + 0.03f), new Vector3(-45f, 0.003f, i * 3.1f + 0.03f), Vector3.up);
            GameObject s = new GameObject("PlankSeams");
            s.transform.SetParent(root, false);
            s.AddComponent<MeshFilter>().sharedMesh = seams.ToMesh("PlankSeams", 0.1f);
            s.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.Rubber;
        }

        private void BuildMug(Vector3 at)
        {
            GameObject mug = new GameObject("CoffeeMug");
            mug.transform.SetParent(root, false);
            mug.transform.position = at + new Vector3(-0.3f, 0f, 0.2f);
            Prims.Make("Body", mug.transform, Prims.Cylinder, GameMaterials.Cream, new Vector3(0f, 1.0f, 0f), new Vector3(2.7f, 1.0f, 2.7f));
            Prims.Make("Coffee", mug.transform, Prims.Cylinder, GameMaterials.TapeBrown, new Vector3(0f, 1.95f, 0f), new Vector3(2.4f, 0.02f, 2.4f));
            Prims.Make("Handle", mug.transform, Prims.Cube, GameMaterials.Cream, new Vector3(1.55f, 1.0f, 0f), new Vector3(0.5f, 1.1f, 0.35f));
            Prims.Make("HandleTop", mug.transform, Prims.Cube, GameMaterials.Cream, new Vector3(1.3f, 1.55f, 0f), new Vector3(0.9f, 0.3f, 0.35f));
            Prims.Make("HandleBottom", mug.transform, Prims.Cube, GameMaterials.Cream, new Vector3(1.3f, 0.45f, 0f), new Vector3(0.9f, 0.3f, 0.35f));
        }

        private void BuildBitKit(Vector3 at)
        {
            GameObject kit = new GameObject("BitKit");
            kit.transform.SetParent(root, false);
            kit.transform.position = at;
            kit.transform.rotation = Quaternion.Euler(0f, -8f, 0f);
            Prims.Make("Case", kit.transform, Prims.Cube, GameMaterials.Charcoal, new Vector3(0f, 0.22f, 0f), new Vector3(4.2f, 0.45f, 2.8f));
            Prims.Make("Foam", kit.transform, Prims.Cube, GameMaterials.Rubber, new Vector3(0f, 0.47f, 0f), new Vector3(3.9f, 0.05f, 2.5f));
            for (int r = 0; r < 5; r++)
                for (int c = 0; c < 10; c++)
                    Prims.Make("Bit", kit.transform, Prims.Cylinder, GameMaterials.Steel, new Vector3(-1.7f + c * 0.38f, 0.54f, -0.9f + r * 0.45f), new Vector3(0.2f, 0.06f, 0.2f));
            Prims.Make("Lid", kit.transform, Prims.Cube, GameMaterials.Charcoal, new Vector3(0.2f, 1.3f, 1.7f), new Vector3(4.2f, 0.12f, 2.6f), new Vector3(-62f, 0f, 0f));
        }

        private void BuildScrewdriver(Vector3 at)
        {
            GameObject sd = new GameObject("Screwdriver");
            sd.transform.SetParent(root, false);
            sd.transform.position = at;
            sd.transform.rotation = Quaternion.Euler(0f, 62f, 0f);
            // axis along +x
            Prims.Make("Handle", sd.transform, Prims.Cylinder, GameMaterials.Charcoal, new Vector3(0f, 0.22f, 0f), new Vector3(0.5f, 1.0f, 0.5f), new Vector3(0f, 0f, 90f));
            Prims.Make("GripYellow", sd.transform, Prims.Cylinder, GameMaterials.PlasticYellow, new Vector3(1.0f, 0.22f, 0f), new Vector3(0.52f, 0.55f, 0.52f), new Vector3(0f, 0f, 90f));
            Prims.Make("Shaft", sd.transform, Prims.Cylinder, GameMaterials.Steel, new Vector3(2.9f, 0.2f, 0f), new Vector3(0.1f, 1.4f, 0.1f), new Vector3(0f, 0f, 90f));
            Prims.Make("Tip", sd.transform, Prims.Cube, GameMaterials.Steel, new Vector3(4.35f, 0.2f, 0f), new Vector3(0.2f, 0.16f, 0.03f));
        }

        private void ScatterHardware(WorkshopCameraRig rig, Vector3 center, float hx, float hz, int seed)
        {
            System.Random rng = new System.Random(seed * 131 + 7);
            int placed = 0, tries = 0;
            while (placed < 18 && tries++ < 400)
            {
                float vx = 0.02f + (float)rng.NextDouble() * 0.96f;
                float vy = 0.04f + (float)rng.NextDouble() * 0.9f;
                Vector3 p = rig.GroundPoint(vx, vy);
                // keep clear of the conveyor loop itself and of the corner props
                if (Mathf.Abs(p.x - center.x) < hx + 0.9f && Mathf.Abs(p.z - center.z) < hz + 0.9f) continue;
                if (vy > 0.8f && vx > 0.78f) continue;
                if (vy > 0.88f && vx < 0.25f) continue;
                if (vy < 0.17f) continue; // the bottom strip is reserved for the LED pips / Restart / boosters
                bool washer = placed % 3 == 2;
                if (washer) MakeWasher(p, (float)rng.NextDouble() * 360f);
                else MakeScrew(p, (float)rng.NextDouble() * 360f, (float)(rng.NextDouble() - 0.5) * 12f);
                placed++;
            }
        }

        private void MakeScrew(Vector3 p, float yaw, float tilt)
        {
            GameObject s = new GameObject("WoodScrew");
            s.transform.SetParent(root, false);
            s.transform.position = new Vector3(p.x, 0.07f, p.z);
            s.transform.rotation = Quaternion.Euler(0f, yaw, tilt);
            Prims.Make("Shaft", s.transform, Prims.Cylinder, GameMaterials.Steel, new Vector3(0.28f, 0f, 0f), new Vector3(0.1f, 0.3f, 0.1f), new Vector3(0f, 0f, 90f));
            Prims.Make("Head", s.transform, Prims.Cylinder, GameMaterials.Brass, new Vector3(-0.06f, 0f, 0f), new Vector3(0.2f, 0.035f, 0.2f), new Vector3(0f, 0f, 90f));
            Prims.Make("Slot", s.transform, Prims.Cube, GameMaterials.Charcoal, new Vector3(-0.1f, 0f, 0f), new Vector3(0.012f, 0.14f, 0.03f));
        }

        private void MakeWasher(Vector3 p, float yaw)
        {
            GameObject w = new GameObject("Washer");
            w.transform.SetParent(root, false);
            w.transform.position = new Vector3(p.x, 0.03f, p.z);
            w.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Prims.Make("Ring", w.transform, Prims.Cylinder, GameMaterials.Steel, Vector3.zero, new Vector3(0.34f, 0.015f, 0.34f));
            Prims.Make("Hole", w.transform, Prims.Cylinder, GameMaterials.Rubber, new Vector3(0f, 0.018f, 0f), new Vector3(0.16f, 0.004f, 0.16f));
        }
    }
}
