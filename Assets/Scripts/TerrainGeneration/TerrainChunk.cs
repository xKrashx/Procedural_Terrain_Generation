using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk {

    const float colliderGenerationDistanceTreshold = 5;
    public event System.Action<TerrainChunk, bool> onVisibilityChanged;
    public Vector2 coord;

    GameObject meshObject;
    Vector2 sampleCentre;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;
    int colliderLODIndex;

    HeightMap heightMap;
    bool heightMapReceived;
    int preiousLODIndex = -1;
    bool hasSetCollider;
    float maxViewDist;

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;
    Transform viewer;

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material) {

        this.coord = coord;
        this.colliderLODIndex = colliderLODIndex;
        this.detailLevels = detailLevels;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;

        sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshRenderer.material = material;

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];

        for (int i = 0; i < detailLevels.Length; i++) {

            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;
            if (i == colliderLODIndex) lodMeshes[i].updateCallback += UpdateCollisionMesh;

        }

        maxViewDist = detailLevels[detailLevels.Length - 1].visibleDstTreshold;

    }

    public void Load() {

        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numberOfVertsPerLine, meshSettings.numberOfVertsPerLine, heightMapSettings, sampleCentre), OnHeightMapReceived);

    }

    void OnHeightMapReceived(object heightMapObject) {

        this.heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;

        UpdateTerrainChunk();

    }

    Vector2 viewerPosition {

        get { return new Vector2(viewer.position.x, viewer.position.z); }

    }

    public void UpdateTerrainChunk() {

        if (heightMapReceived) {

            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

            bool wasVisible = IsVisible();
            bool visible = viewerDistanceFromNearestEdge <= maxViewDist;

            if (visible) {

                int lodIndex = 0;

                for (int i = 0; i < detailLevels.Length; i++) {

                    if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDstTreshold) lodIndex = i + 1;
                    else break;

                }

                if (lodIndex != preiousLODIndex) {

                    LODMesh lodMesh = lodMeshes[lodIndex];

                    if (lodMesh.hasMesh) {

                        preiousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                        meshCollider.sharedMesh = lodMesh.mesh;

                    }
                    else if (!lodMesh.hasRequestedMesh) {

                        lodMesh.RequestMesh(heightMap, meshSettings);

                    }

                }

            }

            if (wasVisible != visible) {

                SetVisible(visible);

                if(onVisibilityChanged != null) onVisibilityChanged(this, visible);


            }

        }

    }

    public void UpdateCollisionMesh() {

        if (!hasSetCollider) {

            float sqrDstFromViewrToEdge = bounds.SqrDistance(viewerPosition);

            if (sqrDstFromViewrToEdge < detailLevels[colliderLODIndex].sqrVisibleDstTreshold)
                if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
                    lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);

            if (sqrDstFromViewrToEdge < colliderGenerationDistanceTreshold * colliderGenerationDistanceTreshold) {

                if (lodMeshes[colliderLODIndex].hasMesh) {

                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                    hasSetCollider = true;

                }

            }

        }

    }

    public void SetVisible(bool visible) {

        meshObject.SetActive(visible);

    }

    public bool IsVisible() {

        return meshObject.activeSelf;

    }

}

class LODMesh {

    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int lod;
    public event System.Action updateCallback;

    public LODMesh(int lod) {

        this.lod = lod;

    }

    void OnMeshDataReceived(object meshDataObject) {

        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasMesh = true;

        updateCallback();

    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings) {

        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);

    }

}
