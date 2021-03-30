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

    public void InitializeChunk (int chunksize) {
        CHUNK_SIZE = chunksize;
        voxelData = new float[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
        data = new ChunkData(new List<Vector3>(), new List<int>());
    }


    [ContextMenu("Reload")]
    public void reloadChunk () {
        Mesh mesh = new Mesh();
        mesh.SetVertices(data.Verts);
        mesh.SetTriangles(data.Indices, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        GameObject go = new GameObject("Mesh");
        go.transform.parent = transform;
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
}