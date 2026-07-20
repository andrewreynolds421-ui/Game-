using UnityEngine;

namespace TransformationFPS.World
{
    /// <summary>
    /// Builds a single Unity Terrain GameObject for a chunk grid coordinate. Chunk coordinates
    /// are integer grid cells: chunk (cx, cz) covers world space
    /// [cx*chunkSize, (cx+1)*chunkSize] x [cz*chunkSize, (cz+1)*chunkSize].
    /// </summary>
    public static class TerrainChunkBuilder
    {
        public static Terrain Build(Vector2Int chunkCoord, int chunkSize, int heightmapResolution, TerrainNoiseProfile noise, TerrainLayer sharedLayer, Transform parent)
        {
            var data = new TerrainData
            {
                heightmapResolution = heightmapResolution
            };
            data.size = new Vector3(chunkSize, Mathf.Max(1f, noise.heightMultiplier), chunkSize);

            float originX = chunkCoord.x * chunkSize;
            float originZ = chunkCoord.y * chunkSize;

            var heights = new float[heightmapResolution, heightmapResolution];
            for (int z = 0; z < heightmapResolution; z++)
            {
                float worldZ = originZ + z / (float)(heightmapResolution - 1) * chunkSize;
                for (int x = 0; x < heightmapResolution; x++)
                {
                    float worldX = originX + x / (float)(heightmapResolution - 1) * chunkSize;
                    // TerrainData.SetHeights expects a [y, x] array (row = Z axis, column = X axis).
                    heights[z, x] = noise.GetHeight01(worldX, worldZ);
                }
            }
            data.SetHeights(0, 0, heights);

            if (sharedLayer != null)
            {
                data.terrainLayers = new[] { sharedLayer };
            }

            var go = Terrain.CreateTerrainGameObject(data);
            go.name = $"Chunk_{chunkCoord.x}_{chunkCoord.y}";
            go.transform.position = new Vector3(originX, 0f, originZ);
            if (parent != null)
            {
                go.transform.SetParent(parent, true);
            }

            var terrain = go.GetComponent<Terrain>();
            terrain.heightmapPixelError = 8;
            terrain.basemapDistance = 400f;
            return terrain;
        }
    }
}
