using HerosJourney.Core.WorldGeneration.Voxels;
using UnityEngine;

namespace HerosJourney.Core.WorldGeneration.Chunks
{
    public static class ChunkDataHandler
    {
        public static MeshData GenerateMeshData(ChunkData chunkData)
        {
            MeshData meshData = new MeshData(true);

            for (int x = 0; x < chunkData.ChunkLength; ++x)
                for (int y = 0; y < chunkData.ChunkHeight; ++y)
                    for (int z = 0; z < chunkData.ChunkLength; ++z)
                        VoxelFaceGeneration.GenerateVoxel(chunkData, meshData, chunkData.voxels[x, y, z].data, new Vector3Int(x, y, z));

            return meshData;
        }

        public static void SetVoxelAt(ChunkData chunkData, Voxel voxel, Vector3Int localPosition)
        {
            if (IsInBounds(chunkData, localPosition))
                chunkData.voxels[localPosition.x, localPosition.y, localPosition.z] = voxel;
        }

        public static Voxel GetVoxelAt(ChunkData chunkData, Vector3Int localPosition)
        {
            if (IsInBounds(chunkData, localPosition))
                return chunkData.voxels[localPosition.x, localPosition.y, localPosition.z];

            return WorldDataHandler.GetVoxelInWorld(chunkData.World.WorldData, chunkData.WorldPosition + localPosition);
        }

        public static bool IsInBounds(ChunkData chunkData, Vector3Int localPosition)
        {
            if (localPosition.x < 0 || localPosition.x >= chunkData.ChunkLength ||
                localPosition.y < 0 || localPosition.y >= chunkData.ChunkHeight ||
                localPosition.z < 0 || localPosition.z >= chunkData.ChunkLength)
                return false;

            return true;
        }

        public static Vector3Int WorldToLocalPosition(ChunkData chunk, Vector3Int worldPosition)
        {
            return new Vector3Int
            {
                x = worldPosition.x - chunk.WorldPosition.x,
                y = worldPosition.y - chunk.WorldPosition.y,
                z = worldPosition.z - chunk.WorldPosition.z
            };
        }
    }
}