using HerosJourney.Core.WorldGeneration.Chunks;
using HerosJourney.Core.WorldGeneration.Voxels;
using System.Collections.Generic;
using UnityEngine;
using System;
using HerosJourney.Utils;

namespace HerosJourney.Core.WorldGeneration
{
    public class World : MonoBehaviour
    {
        [SerializeField] private int _chunkLength = 16;
        [SerializeField] private int _chunkHeight = 128;
        [SerializeField] private int _worldSizeInChunks;
        [SerializeField] private GameObject _chunkPrefab;

        [SerializeField] private TerrainGenerator _terrainGenerator;

        public Action OnWorldGenerated;

        private Dictionary<Vector3Int, ChunkData> _chunks = new Dictionary<Vector3Int, ChunkData>();
        private Dictionary<Vector3Int, ChunkRenderer> _chunkRenderers = new Dictionary<Vector3Int, ChunkRenderer>();

        public void GenerateWorld() => GenerateWorld(Vector3Int.zero);

        public void GenerateWorld(Vector3Int worldPosition)
        {
            ClearAllChunks();
            GenerateChunkData(worldPosition);
            InitializeChunks();

            OnWorldGenerated?.Invoke();
        }

        private void ClearAllChunks()
        {
            _chunks.Clear();

            foreach (var chunk in _chunkRenderers.Values)
                Destroy(chunk.gameObject);

            _chunkRenderers.Clear();
        }

        private void GenerateChunkData(Vector3Int worldPosition)
        {
            Vector3Int startingPoint = new Vector3Int(worldPosition.x - (_worldSizeInChunks * _chunkLength) / 2, 0,
                worldPosition.z - (_worldSizeInChunks * _chunkLength) / 2);

            for (int x = 0; x < _worldSizeInChunks; ++x)
            {
                for (int z = 0; z < _worldSizeInChunks; ++z)
                {
                    Vector3Int position = new Vector3Int(startingPoint.x + x * _chunkLength, 0, startingPoint.z + z * _chunkLength);

                    ChunkData chunkData = new ChunkData(_chunkLength, _chunkHeight, position, this);
                    _terrainGenerator.GenerateChunkData(chunkData);

                    _chunks.Add(position, chunkData);
                }
            }
        }

        private void InitializeChunks()
        {
            foreach (ChunkData chunkData in _chunks.Values)
            {
                MeshData meshData = ChunkVoxelData.GenerateMeshData(chunkData);
                GameObject chunkInstance = Instantiate(_chunkPrefab, chunkData.WorldPosition, Quaternion.identity);

                ChunkRenderer chunkRenderer = chunkInstance.GetComponent<ChunkRenderer>();
                chunkRenderer.InitializeChunk(chunkData);
                chunkRenderer.UpdateChunk(meshData);

                _chunkRenderers.Add(chunkData.WorldPosition, chunkRenderer);
            }
        }

        public Voxel GetVoxelInWorld(Vector3Int worldPosition)
        {
            Vector3Int chunkPosition = GetChunkPosition(worldPosition);
            ChunkData chunk = null;

            if (_chunks.TryGetValue(chunkPosition, out chunk))
                return ChunkVoxelData.GetVoxelAt(chunk, ChunkVoxelData.WorldToLocalPosition(chunk, worldPosition));

            return null;
        }

        private Vector3Int GetChunkPosition(Vector3Int worldPosition)
        {
            return new Vector3Int
            {
                x = Mathf.FloorToInt(worldPosition.x / (float)_chunkLength) * _chunkLength,
                y = Mathf.FloorToInt(worldPosition.y / (float)_chunkHeight) * _chunkHeight,
                z = Mathf.FloorToInt(worldPosition.z / (float)_chunkLength) * _chunkLength
            };
        }
    }
}