using HerosJourney.Core.WorldGeneration.Chunks;
using HerosJourney.Core.WorldGeneration.Voxels;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HerosJourney.Core.WorldGeneration
{
    public class World : MonoBehaviour
    {
        [SerializeField] private int _chunkLength = 16;
        [SerializeField] private int _chunkHeight = 128;
        private WorldData _worldData;

        [SerializeField] [Range(0, 16)] 
        private int _renderDistance = 8;

        [SerializeField] private GameObject _chunkPrefab;

        [SerializeField] private TerrainGenerator _terrainGenerator;

        public Action OnWorldGenerated;
        public Action OnNewChunksGenerated;

        public int ChunkLength => _chunkLength;

        private struct WorldRendererData
        {
            public List<ChunkData> chunkDataToCreate;
            public List<ChunkRenderer> chunkRenderersToCreate;

            public List<ChunkData> chunkDataToRemove;
            public List<ChunkRenderer> chunkRenderersToRemove;
        }

        private void Awake() => _worldData = new WorldData(_chunkLength, _chunkHeight);

        public void GenerateChunks() => GenerateChunks(Vector3Int.zero);

        public void GenerateChunks(Vector3Int worldPosition)
        {
            ClearAllChunks();
            GenerateChunkData(worldPosition);
            InitializeChunks();

            OnWorldGenerated?.Invoke();
        }

        private void ClearAllChunks()
        {
            //_chunks.Clear();

            //foreach (var chunk in _chunkRenderers.Values)
            //    Destroy(chunk.gameObject);

            //_chunkRenderers.Clear();
        }

        private void GenerateChunkData(Vector3Int worldPosition)
        {
            Vector3Int startingPoint = new Vector3Int(worldPosition.x - (_renderDistance * _chunkLength) / 2, 0,
                worldPosition.z - (_renderDistance * _chunkLength) / 2);

            for (int x = 0; x < _renderDistance; ++x)
            {
                for (int z = 0; z < _renderDistance; ++z)
                {
                    Vector3Int position = new Vector3Int(startingPoint.x + x * _chunkLength, 0, startingPoint.z + z * _chunkLength);

                    ChunkData chunkData = new ChunkData(_chunkLength, _chunkHeight, position, this);
                    _terrainGenerator.GenerateChunkData(chunkData);

                    _worldData.chunks.Add(position, chunkData);
                }
            }
        }

        private void InitializeChunks()
        {
            foreach (ChunkData chunkData in _worldData.chunks.Values)
            {
                MeshData meshData = ChunkVoxelData.GenerateMeshData(chunkData);
                GameObject chunkInstance = Instantiate(_chunkPrefab, chunkData.WorldPosition, Quaternion.identity);

                ChunkRenderer chunkRenderer = chunkInstance.GetComponent<ChunkRenderer>();
                chunkRenderer.InitializeChunk(chunkData);
                chunkRenderer.UpdateChunk(meshData);

                _worldData.chunkRenderers.Add(chunkData.WorldPosition, chunkRenderer);
            }
        }

        public void GenerateNewChunksRequest(Vector3Int position)
        {
            GenerateChunks(position);
            OnNewChunksGenerated?.Invoke();
        }

        public Voxel GetVoxelInWorld(Vector3Int worldPosition)
        {
            Vector3Int chunkPosition = GetChunkPosition(worldPosition);
            ChunkData chunk = null;

            if (_worldData.chunks.TryGetValue(chunkPosition, out chunk))
                return ChunkVoxelData.GetVoxelAt(chunk, ChunkVoxelData.WorldToLocalPosition(chunk, worldPosition));

            return null;
        }

        public Vector3Int GetChunkPosition(Vector3Int worldPosition)
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