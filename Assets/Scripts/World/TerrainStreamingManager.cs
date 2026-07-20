using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TransformationFPS.World
{
    /// <summary>
    /// Streams Terrain chunks in/out of a grid around a tracked target (the player). Chunks are
    /// generated procedurally at load time, so nothing is shipped or saved - unloading just
    /// destroys the GameObject, and reloading it later regenerates identical terrain from the
    /// same noise profile. Initial chunks around the target load synchronously in Start() so the
    /// ground exists before the first physics step; further loads are throttled per frame as the
    /// player moves to avoid hitches.
    /// </summary>
    public class TerrainStreamingManager : MonoBehaviour
    {
        [Header("Target")]
        public Transform target;

        [Header("Chunking")]
        public int chunkSize = 240;
        public int heightmapResolution = 65; // must be 2^n + 1 (33, 65, 129, ...)
        public int viewDistanceInChunks = 2;
        public int maxChunkLoadsPerFrame = 1;

        [Header("Terrain Shape")]
        public TerrainNoiseProfile noiseProfile = new TerrainNoiseProfile();

        public event Action<Terrain> OnChunkLoaded;

        private readonly Dictionary<Vector2Int, Terrain> _loadedChunks = new Dictionary<Vector2Int, Terrain>();
        private readonly Queue<Vector2Int> _loadQueue = new Queue<Vector2Int>();
        private readonly HashSet<Vector2Int> _queuedForLoad = new HashSet<Vector2Int>();

        private Vector2Int _lastTargetChunk;
        private bool _hasLastTargetChunk;
        private TerrainLayer _sharedLayer;

        private void Awake()
        {
            _sharedLayer = CreateDefaultLayer();
        }

        private void Start()
        {
            Vector2Int startChunk = target != null ? WorldToChunk(target.position) : Vector2Int.zero;
            foreach (var coord in ChunksWithinRadius(startChunk, viewDistanceInChunks))
            {
                LoadChunkImmediate(coord);
            }
            _lastTargetChunk = startChunk;
            _hasLastTargetChunk = true;
        }

        private void Update()
        {
            if (target == null) return;

            Vector2Int currentChunk = WorldToChunk(target.position);
            if (!_hasLastTargetChunk || currentChunk != _lastTargetChunk)
            {
                RefreshDesiredChunks(currentChunk);
                _lastTargetChunk = currentChunk;
                _hasLastTargetChunk = true;
            }

            ProcessLoadQueue();
        }

        /// <summary>World-space terrain height at the given XZ, independent of whether that chunk is currently loaded.</summary>
        public float SampleHeight(float worldX, float worldZ)
        {
            return noiseProfile.GetWorldHeight(worldX, worldZ);
        }

        private void RefreshDesiredChunks(Vector2Int centerChunk)
        {
            var desired = new HashSet<Vector2Int>(ChunksWithinRadius(centerChunk, viewDistanceInChunks));

            // Keep a one-chunk buffer beyond view distance before unloading, so chunks don't
            // load/unload repeatedly while the player straddles a boundary.
            int keepRadius = viewDistanceInChunks + 1;
            var toRemove = _loadedChunks.Keys.Where(c => ChebyshevDistance(c, centerChunk) > keepRadius).ToList();
            foreach (var coord in toRemove)
            {
                UnloadChunk(coord);
            }

            foreach (var coord in desired)
            {
                if (_loadedChunks.ContainsKey(coord) || _queuedForLoad.Contains(coord)) continue;
                _queuedForLoad.Add(coord);
                _loadQueue.Enqueue(coord);
            }
        }

        private void ProcessLoadQueue()
        {
            int loadsThisFrame = 0;
            while (loadsThisFrame < maxChunkLoadsPerFrame && _loadQueue.Count > 0)
            {
                var coord = _loadQueue.Dequeue();
                _queuedForLoad.Remove(coord);
                if (_loadedChunks.ContainsKey(coord)) continue;
                LoadChunkImmediate(coord);
                loadsThisFrame++;
            }
        }

        private void LoadChunkImmediate(Vector2Int coord)
        {
            if (_loadedChunks.ContainsKey(coord)) return;
            var terrain = TerrainChunkBuilder.Build(coord, chunkSize, heightmapResolution, noiseProfile, _sharedLayer, transform);
            _loadedChunks[coord] = terrain;
            RelinkNeighbors(coord);
            OnChunkLoaded?.Invoke(terrain);
        }

        private void UnloadChunk(Vector2Int coord)
        {
            if (!_loadedChunks.TryGetValue(coord, out var terrain)) return;
            _loadedChunks.Remove(coord);
            if (terrain != null)
            {
                Destroy(terrain.terrainData);
                Destroy(terrain.gameObject);
            }
            RelinkNeighbors(coord);
        }

        private void RelinkNeighbors(Vector2Int coord)
        {
            var affected = new[]
            {
                coord,
                coord + Vector2Int.left,
                coord + Vector2Int.right,
                coord + new Vector2Int(0, 1),
                coord + new Vector2Int(0, -1)
            };

            foreach (var c in affected)
            {
                if (!_loadedChunks.TryGetValue(c, out var terrain) || terrain == null) continue;

                _loadedChunks.TryGetValue(c + Vector2Int.left, out var left);
                _loadedChunks.TryGetValue(c + Vector2Int.right, out var right);
                _loadedChunks.TryGetValue(c + new Vector2Int(0, 1), out var top);
                _loadedChunks.TryGetValue(c + new Vector2Int(0, -1), out var bottom);
                terrain.SetNeighbors(left, top, right, bottom);
            }
        }

        private Vector2Int WorldToChunk(Vector3 worldPos)
        {
            return new Vector2Int(Mathf.FloorToInt(worldPos.x / chunkSize), Mathf.FloorToInt(worldPos.z / chunkSize));
        }

        private static IEnumerable<Vector2Int> ChunksWithinRadius(Vector2Int center, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    yield return new Vector2Int(center.x + dx, center.y + dz);
                }
            }
        }

        private static int ChebyshevDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }

        private static TerrainLayer CreateDefaultLayer()
        {
            var layer = ScriptableObject.CreateInstance<TerrainLayer>();
            layer.name = "DefaultGrass";
            layer.diffuseTexture = CreateSolidTexture(new Color(0.32f, 0.42f, 0.22f));
            layer.tileSize = new Vector2(50f, 50f);
            return layer;
        }

        private static Texture2D CreateSolidTexture(Color color)
        {
            var texture = new Texture2D(4, 4);
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}
