using System;
using System.Collections;
using System.Collections.Generic;
using MarchingCubesProject;
using UnityEngine;


public class ChunkData
{
    public List<Vector3> Verts;
    public List<int> Indices;

    public ChunkData(List<Vector3> Verts, List<int> Indices)
    {
        this.Verts = Verts;
        this.Indices = Indices;
    }
}

public class Chunk : MonoBehaviour
{

    private int CHUNK_SIZE;

    public Material m_material;

    [SerializeField]
    ChunkData data;

    float[] voxelData;

    Bounds chunkBounds;

    Mesh mesh;

    Plane[] planes;


    float lastSeen = 0f;

    void Update () {
        if (CheckInView()) {
            lastSeen = 0f;
        } else {
            lastSeen += Time.deltaTime;
        }
    }

    bool CheckInView () {
        planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        chunkBounds = new Bounds (transform.position + Vector3.one * CHUNK_SIZE/2, Vector3.one * CHUNK_SIZE);
        return GeometryUtility.TestPlanesAABB(planes, chunkBounds);
    }

    public void InitializeChunk (int chunksize) {
        CHUNK_SIZE = chunksize;
        voxelData = new float[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
        data = new ChunkData(new List<Vector3>(), new List<int>());
    }


    [ContextMenu("Reload")]
    public void reloadChunk () {
        mesh = new Mesh();
        mesh.SetVertices(data.Verts);
        mesh.SetTriangles(data.Indices, 0);

        GameObject go = new GameObject("Mesh");
        go.transform.parent = transform;
        mesh.RecalculateBounds();
        chunkBounds = mesh.bounds;
        mesh.RecalculateNormals();
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<Renderer>().material = m_material;
        go.GetComponent<MeshFilter>().mesh = mesh;
    }

    internal void march(Marching marcher)
    {
        //note to self: weld vertices for better mesh data
        // this is a problem from Scrawk's implementation that I can fix for fun and profit -Arthur
        marcher.Generate(transform.position, voxelData, CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE, data.Verts, data.Indices);

        reloadChunk();
    }

    internal void sample(Cave.PointSampler sampler)
    {
        int[] position = new int[3] {
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y),
            Mathf.RoundToInt(transform.position.z)
        };

        for (int x = position[0]; x < CHUNK_SIZE; x++)
        {
            for (int y = position[1]; y <  CHUNK_SIZE; y++)
            {
                for (int z = position[2]; z < CHUNK_SIZE; z++)
                {
                    int id = x + y * CHUNK_SIZE + z * CHUNK_SIZE * CHUNK_SIZE;

                    voxelData[id] = sampler(position[0] + x, position[1] + y, position[2] + z);
                }
            }
        }
    }

    internal void load (ChunkData savedData) {
        data = savedData;

        reloadChunk();
    }

    public ChunkData GetData () {
        return data;
    }

    public void DrawBounds (Color chunkColor) {
        Bounds b = chunkBounds;

        // bottom
        var p1 = new Vector3(b.min.x, b.min.y, b.min.z);
        var p2 = new Vector3(b.max.x, b.min.y, b.min.z);
        var p3 = new Vector3(b.max.x, b.min.y, b.max.z);
        var p4 = new Vector3(b.min.x, b.min.y, b.max.z);

        Debug.DrawLine(p1, p2, chunkColor);
        Debug.DrawLine(p2, p3, chunkColor);
        Debug.DrawLine(p3, p4, chunkColor);
        Debug.DrawLine(p4, p1, chunkColor);

        // top
        var p5 = new Vector3(b.min.x, b.max.y, b.min.z);
        var p6 = new Vector3(b.max.x, b.max.y, b.min.z);
        var p7 = new Vector3(b.max.x, b.max.y, b.max.z);
        var p8 = new Vector3(b.min.x, b.max.y, b.max.z);

        Debug.DrawLine(p5, p6, chunkColor);
        Debug.DrawLine(p6, p7, chunkColor);
        Debug.DrawLine(p7, p8, chunkColor);
        Debug.DrawLine(p8, p5, chunkColor);

        // sides
        Debug.DrawLine(p1, p5, chunkColor);
        Debug.DrawLine(p2, p6, chunkColor);
        Debug.DrawLine(p3, p7, chunkColor);
        Debug.DrawLine(p4, p8, chunkColor);
    }

    public Vector3 GetCenter () {
        return transform.position + .5f * Vector3.one * CHUNK_SIZE;
    }


    public float GetChunkSize () {
        return CHUNK_SIZE;
    }

    public float TimeSinceView () {
        return lastSeen;
    }

}