using UnityEngine;
using TransformationFPS.Abilities;
using TransformationFPS.Enemies;
using TransformationFPS.Player;
using TransformationFPS.UI;
using TransformationFPS.Weapons;
using TransformationFPS.World;

namespace TransformationFPS.Bootstrap
{
    /// <summary>
    /// Procedurally builds a playable open-world test scene (streaming terrain, light, player,
    /// enemy dummies) the moment any scene loads with no existing Player object. This means the
    /// project is playable from a completely blank Unity scene - open the project, create/open
    /// any scene, press Play.
    ///
    /// This exists because hand-authoring a .unity scene file as text (outside the Editor) is
    /// fragile: GameObject/component references are wired by internal file IDs and GUIDs that are
    /// easy to get subtly wrong and hard to verify without the Editor itself. Building the scene at
    /// runtime via AddComponent/Instantiate sidesteps that risk entirely.
    /// </summary>
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (GameObject.FindGameObjectWithTag("Player") != null)
            {
                return; // A hand-built scene already has a player - don't double up.
            }

            BuildLighting();

            var terrain = BuildTerrainStreaming();
            var player = BuildPlayer(terrain);
            terrain.target = player.transform;

            BuildEnemyDummies(terrain);
            BuildTraversalTestCourse(terrain);
        }

        private static void BuildLighting()
        {
            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.6f, 0.68f);
            RenderSettings.ambientGroundColor = new Color(0.25f, 0.22f, 0.2f);
        }

        private static TerrainStreamingManager BuildTerrainStreaming()
        {
            var go = new GameObject("Terrain Streaming");
            return go.AddComponent<TerrainStreamingManager>();
        }

        private static GameObject BuildPlayer(TerrainStreamingManager terrain)
        {
            var player = new GameObject("Player");
            player.tag = "Player";

            float spawnHeight = terrain.SampleHeight(0f, 0f);
            player.transform.position = new Vector3(0f, spawnHeight + 1.1f, 0f);

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Body";
            visual.transform.SetParent(player.transform, false);
            Object.Destroy(visual.GetComponent<Collider>());
            TintRenderer(visual.GetComponent<Renderer>(), new Color(0.2f, 0.5f, 0.8f));

            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.9f;
            controller.radius = 0.4f;
            controller.center = new Vector3(0f, 0.95f, 0f);

            var cameraGO = new GameObject("Camera Pivot");
            cameraGO.tag = "MainCamera"; // TransformationManager uses Camera.main for ability raycasts
            cameraGO.transform.SetParent(player.transform, false);
            cameraGO.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            var cam = cameraGO.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 2000f;
            cameraGO.AddComponent<AudioListener>();

            var movement = player.AddComponent<FirstPersonController>();
            movement.cameraPivot = cameraGO.transform;

            player.AddComponent<PlayerStats>();

            var weapon = player.AddComponent<WeaponController>();
            weapon.weaponCamera = cam;
            weapon.loadout[0] = SampleWeaponLibrary.CreateAutoRifle();
            weapon.loadout[1] = SampleWeaponLibrary.CreateShotgun();
            weapon.loadout[2] = SampleWeaponLibrary.CreateRocketLauncher();

            var transformationManager = player.AddComponent<TransformationManager>();
            transformationManager.equippedForm = SampleFormLibrary.CreateBeastForm();

            player.AddComponent<VehicleMountController>();
            player.AddComponent<AerialMobilityController>();
            player.AddComponent<LedgeMantleController>();

            player.AddComponent<SimpleHud>();

            return player;
        }

        private static void BuildEnemyDummies(TerrainStreamingManager terrain)
        {
            var rng = new System.Random(5678);
            for (int i = 0; i < 5; i++)
            {
                float x = (float)(rng.NextDouble() * 2 - 1) * 30f;
                float z = (float)(rng.NextDouble() * 2 - 1) * 30f;
                float y = terrain.SampleHeight(x, z);

                var dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                dummy.name = $"EnemyDummy_{i}";
                dummy.transform.position = new Vector3(x, y + 1f, z);
                TintRenderer(dummy.GetComponent<Renderer>(), new Color(0.75f, 0.2f, 0.2f));
                dummy.AddComponent<EnemyDummy>();
            }
        }

        /// <summary>
        /// A few simple structures near spawn purely to give the new traversal systems something
        /// to interact with: ascending steps for ledge mantling, and a gap that requires an aerial
        /// move (multi-jump / glide / blink, depending on the equipped Form) to cross on foot.
        /// </summary>
        private static void BuildTraversalTestCourse(TerrainStreamingManager terrain)
        {
            Vector3 origin = new Vector3(15f, 0f, 15f);

            float[] stepHeights = { 0.6f, 1.2f, 1.8f };
            for (int i = 0; i < stepHeights.Length; i++)
            {
                Vector3 pos = origin + new Vector3(0f, 0f, i * 2.5f);
                float baseY = terrain.SampleHeight(pos.x, pos.z);

                var step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name = $"MantleStep_{i}";
                step.transform.position = new Vector3(pos.x, baseY + stepHeights[i] / 2f, pos.z);
                step.transform.localScale = new Vector3(3f, stepHeights[i], 2f);
                TintRenderer(step.GetComponent<Renderer>(), new Color(0.5f, 0.45f, 0.3f));
            }

            Vector3 gapPos = origin + new Vector3(0f, 0f, stepHeights.Length * 2.5f + 9f);
            float gapBaseY = terrain.SampleHeight(gapPos.x, gapPos.z);

            var farPlatform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            farPlatform.name = "GapPlatform";
            farPlatform.transform.position = new Vector3(gapPos.x, gapBaseY + 1f, gapPos.z);
            farPlatform.transform.localScale = new Vector3(4f, 2f, 4f);
            TintRenderer(farPlatform.GetComponent<Renderer>(), new Color(0.3f, 0.5f, 0.35f));
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
