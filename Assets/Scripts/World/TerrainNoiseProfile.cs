using UnityEngine;

namespace TransformationFPS.World
{
    /// <summary>
    /// Fractal (multi-octave) Perlin noise sampled in world-space coordinates, not chunk-local
    /// ones. Because every chunk queries the same continuous function, adjacent chunks' heights
    /// match exactly at their shared edge with no explicit stitching required.
    /// </summary>
    [System.Serializable]
    public class TerrainNoiseProfile
    {
        public int seed = 1337;
        public float scale = 180f;
        [Range(1, 8)] public int octaves = 4;
        [Range(0f, 1f)] public float persistence = 0.5f;
        public float lacunarity = 2f;
        public float heightMultiplier = 45f;

        private Vector2 _seedOffset;
        private bool _initialized;

        private void EnsureInitialized()
        {
            if (_initialized) return;
            var rng = new System.Random(seed);
            _seedOffset = new Vector2((float)rng.NextDouble() * 10000f, (float)rng.NextDouble() * 10000f);
            _initialized = true;
        }

        /// <summary>Normalized height in [0,1] at the given world XZ position.</summary>
        public float GetHeight01(float worldX, float worldZ)
        {
            EnsureInitialized();

            float amplitude = 1f;
            float frequency = 1f;
            float sum = 0f;
            float maxAmplitude = 0f;

            for (int i = 0; i < octaves; i++)
            {
                float sampleX = (worldX + _seedOffset.x) / scale * frequency;
                float sampleZ = (worldZ + _seedOffset.y) / scale * frequency;
                sum += Mathf.PerlinNoise(sampleX, sampleZ) * amplitude;
                maxAmplitude += amplitude;

                amplitude *= persistence;
                frequency *= lacunarity;
            }

            return maxAmplitude > 0f ? sum / maxAmplitude : 0f;
        }

        /// <summary>World-space height (already scaled by heightMultiplier) at the given world XZ position.</summary>
        public float GetWorldHeight(float worldX, float worldZ)
        {
            return GetHeight01(worldX, worldZ) * heightMultiplier;
        }
    }
}
