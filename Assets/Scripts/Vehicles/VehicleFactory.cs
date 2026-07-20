using UnityEngine;

namespace TransformationFPS.Vehicles
{
    /// <summary>
    /// Builds a hover vehicle entirely from primitives at runtime, matching how
    /// GameBootstrap builds the rest of the test scene - no art assets required.
    /// </summary>
    public static class VehicleFactory
    {
        public static HoverVehicleController CreateHoverVehicle(Vector3 position, Quaternion rotation)
        {
            var go = new GameObject("HoverVehicle");
            go.transform.SetPositionAndRotation(position, rotation);

            var bodyVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bodyVisual.name = "Body";
            bodyVisual.transform.SetParent(go.transform, false);
            bodyVisual.transform.localScale = new Vector3(1.2f, 0.5f, 2.4f);
            Object.Destroy(bodyVisual.GetComponent<Collider>());
            TintRenderer(bodyVisual.GetComponent<Renderer>(), new Color(0.15f, 0.2f, 0.25f));

            var noseVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            noseVisual.name = "Nose";
            noseVisual.transform.SetParent(go.transform, false);
            noseVisual.transform.localPosition = new Vector3(0f, 0f, 1.4f);
            noseVisual.transform.localScale = new Vector3(0.6f, 0.3f, 0.8f);
            Object.Destroy(noseVisual.GetComponent<Collider>());
            TintRenderer(noseVisual.GetComponent<Renderer>(), new Color(0.3f, 0.55f, 0.8f));

            var collider = go.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.2f, 0.5f, 2.8f);

            go.AddComponent<Rigidbody>();
            var controller = go.AddComponent<HoverVehicleController>();
            return controller;
        }

        private static void TintRenderer(Renderer renderer, Color color)
        {
            if (renderer == null) return;
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_Color", color);
            renderer.SetPropertyBlock(block);
        }
    }
}
