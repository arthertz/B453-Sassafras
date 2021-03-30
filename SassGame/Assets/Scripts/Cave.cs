using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Generator;
using System;
using Random = UnityEngine.Random;

using MarchingCubesProject;


//Based off of Scrawk's Marching Cubes and Tetrahedra implementation https://github.com/Scrawk/Marching-Cubes
//His interface structure i
//also Sebsatian Lague's Marching Cubes video and procedural generation series

public class Cave : MonoBehaviour
{
    public int CHUNK_SIZE = 8;

    List<GameObject> chunkObjects = new List<GameObject>();

    Marching marcher;

    public Material chunkMat;


    public int chunkDistance = 4;

    public int wormCount = 5;

    public bool wireframe = false;

    public bool invert = false;

    public float cubeSize = 1;
    public float threshold = .5f;
    public double frequency;
    public double lacunarity;
    public double persistence;
    public string seed;
    public bool useComplexCave = true;

    //Noise texture octaves
    public int octaves = 3;
    
    List<Worm> worms = new List<Worm> ();

    LibNoise.ModuleBase noiseGenerator;
    

   public delegate float PointSampler (float x, float y, float z);

   public delegate PointSampler PointSamplerGenerator (Vector3 v);

   PointSamplerGenerator samplerFactory;

    // Start is called before the first frame update
    [ContextMenu("Initialize in editor")]
    void Awake ()
    {

        marcher = new MarchingCubes();

        samplerFactory = v =>
                        (x, y, z) =>
                            invert ?
                            -(float) noiseGenerator.GetValue(v.x + x, v.y + y, v.z + z)
                            :
                            (float) noiseGenerator.GetValue(v.x + x, v.y + y, v.z + z);

        marcher.Surface = threshold;

        PerlinCaveCmd();
        
        for (int i = 0; i < wormCount; i++) {
            worms.Add(RandomWorm());
        }

        for (int x_chunk = 0; x_chunk < chunkDistance; x_chunk++) {
            for (int y_chunk = 0; y_chunk < chunkDistance; y_chunk++) {
                for (int z_chunk = 0; z_chunk < chunkDistance; z_chunk++) {

                    GameObject chunkObj = new GameObject();

                    Chunk chunkComponent = chunkObj.AddComponent<Chunk>();

                    chunkComponent.InitializeChunk(CHUNK_SIZE);

                    chunkComponent.m_material = chunkMat;

                    Vector3 chunkLocation = new Vector3(x_chunk*(CHUNK_SIZE - 1) , y_chunk*(CHUNK_SIZE - 1), z_chunk*(CHUNK_SIZE - 1)) + transform.position;

                    chunkComponent.sample(samplerFactory(chunkLocation));

                    chunkComponent.march(marcher);

                    chunkObj.transform.SetParent(transform);

                    chunkObj.name = "Chunk " + (x_chunk + y_chunk + z_chunk);

                    chunkObj.transform.position = chunkLocation;

                    chunkObjects.Add(chunkObj);
                }
            }
        }
    }


    [ContextMenu("Get rid of chunks (Don't do this too often)")]
    void DeleteChunks () {
        foreach (GameObject obj in chunkObjects) {
            GameObject.Destroy(obj);
        }
    }


    [ContextMenu("Make Perlin Cave")]
    void PerlinCaveCmd () {

        if (useComplexCave) {
            ComplexCave();
        } else {
            PerlinCave();
        }
    }

    void PerlinCave () {
        noiseGenerator = new Perlin (frequency, lacunarity, persistence, octaves, seed.GetHashCode(), QualityMode.Medium);
    }

    void ComplexCave () {
        noiseGenerator = new LibNoise.Operator.Add(new Perlin (frequency, lacunarity, persistence, octaves, seed.GetHashCode(), QualityMode.Medium),
        new Perlin (frequency * 2, lacunarity, persistence, octaves, seed.GetHashCode(), QualityMode.Medium));
    }

    Worm RandomWorm () {
        return new Worm(
            Random.Range(-25, 25),
            Random.Range(0, 360),
            new Vector3 (
                Random.Range(0, CHUNK_SIZE),
                CHUNK_SIZE,
                Random.Range(0, CHUNK_SIZE)));
    }
}
