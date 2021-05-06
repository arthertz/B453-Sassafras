using System;
using System.Collections;
using System.Collections.Generic;
using MarchingCubesProject;
using UnityEngine;



public class Chunk : MonoBehaviour
{

    private int CHUNK_SIZE;

    public Material m_material;

    float[] voxelData;

    Bounds chunkBounds;

    static internal string DefaultLayerName = "Procedural";

    Mesh mesh;

    Plane[] planes;

    GameObject meshObj;

    bool initialized = false;

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

    public void InitializeChunk () {
        meshObj = new GameObject("Mesh");

        meshObj.AddComponent<MeshFilter>();

        meshObj.AddComponent<MeshRenderer>();

        meshObj.AddComponent<MeshCollider>();

        meshObj.transform.parent = transform;

        meshObj.layer = LayerMask.NameToLayer(DefaultLayerName);    

        initialized = true;
    }


    public void ReloadChunk (int chunksize, Material chunkMat, Vector3 position, Cave.PointSampler sampler, Marching marcher) {

        if (!initialized || !meshObj) {
            InitializeChunk();
        }

        CHUNK_SIZE = chunksize+1;
        voxelData = new float[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
        
        List<Vector3> Verts = new List<Vector3>();
        List<int> Indices = new List<int>();

        transform.position = Vector3.zero;

        m_material = chunkMat;

        sample(sampler);

        march(marcher, Verts, Indices);

        transform.position = position;

        mesh = new Mesh();

        mesh.SetVertices(Verts);
        mesh.SetTriangles(Indices, 0);
        mesh.RecalculateBounds();

        chunkBounds = mesh.bounds;

        mesh.RecalculateNormals();

        meshObj.GetComponent<MeshFilter>().mesh = mesh;

        meshObj.GetComponent<MeshRenderer>().material = m_material;

        meshObj.GetComponent<MeshCollider>().sharedMesh = mesh;

    }

    internal void march(Marching marcher, List<Vector3> vertices, List<int> indices)
    {
        //note to self: weld vertices for better mesh data
        // this is a problem from Scrawk's implementation that I can fix for fun and profit -Arthur
        marcher.Generate(transform.position, voxelData, CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE, vertices, indices);
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