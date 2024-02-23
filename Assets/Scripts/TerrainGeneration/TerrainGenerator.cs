using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

    const float viewerMoveTresholdForChunkUpdate = 25f;
    const float sqrViewerMoveTresholdForChunkUpdate = viewerMoveTresholdForChunkUpdate * viewerMoveTresholdForChunkUpdate;

    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;

    public Transform viewer;
    public Material mapMaterial;
    
    Vector2 viewPosition;
    Vector2 viewerPositionOld;
    float meshWorldSize;
    int chunksVisibleInViewDist;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    void Start() {

        textureSettings.ApplyMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        float maxViewDist = detailLevels[detailLevels.Length - 1].visibleDstTreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / meshWorldSize);

        UpdateVisibleChunks();

    }

    void Update() {

        viewPosition = new Vector2(viewer.position.x, viewer.position.z);

        if(viewPosition != viewerPositionOld)
            foreach (TerrainChunk chunk in visibleTerrainChunks)
                chunk.UpdateCollisionMesh();

        if((viewerPositionOld - viewPosition).sqrMagnitude > sqrViewerMoveTresholdForChunkUpdate) {

            viewerPositionOld = viewPosition;
            UpdateVisibleChunks();

        }

    }

    void UpdateVisibleChunks() {

        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();

        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--) {

            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();

        }

        int currentChunkCoordX = Mathf.RoundToInt(viewer.position.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewer.position.z / meshWorldSize);

        for (int yOffset = -chunksVisibleInViewDist; yOffset <= chunksVisibleInViewDist; yOffset++) {

            for (int xOffset = -chunksVisibleInViewDist; xOffset <= chunksVisibleInViewDist; xOffset++) {

                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord)) {

                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)) terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    else {

                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterial);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load();

                    }

                }

            }

        }

    }

    void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible) {

        if (isVisible) visibleTerrainChunks.Add(chunk);
        else visibleTerrainChunks.Remove(chunk);

    }

}

[System.Serializable]
public struct LODInfo {

    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int lod;
    public float visibleDstTreshold;

    public float sqrVisibleDstTreshold {

        get { return visibleDstTreshold * visibleDstTreshold; }

    }

}