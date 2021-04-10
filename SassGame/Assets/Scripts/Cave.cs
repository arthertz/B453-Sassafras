using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Generator;
using System;
using Random = UnityEngine.Random;

using MarchingCubesProject;


//Based off of Scrawk's Marching Cubes and Tetrahedra implementation https://github.com/Scrawk/Marching-Cubes
//also Sebsatian Lague's Marching Cubes video and procedural generation series

public class Cave : MonoBehaviour
{
    public int chunkSize = 8;

    List<Vector3> WormPaths = new List<Vector3>();

    private
    List<GameObject> chunkObjects = new List<GameObject>();

    ChunkManager chunkManager;

    Marching marcher;

    public Material chunkMat;
    
    public int wormSteps = 300;

    public float wormSpeed = 5f;

    public float wormRadius = 5f;

    public int chunkDistance = 4;

    public float reloadRadius = 2f;

    public float noiseScale = 1f;

    public int wormCount = 5;
    public float surfaceLevel = .5f;
    public double frequency;
    public double lacunarity;
    public double persistence;
    public string seed;
    public bool useComplexCave = true;

    public float reloadThreshold = 1f;

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

        Random.InitState(seed.GetHashCode());

        chunkManager = new ChunkManager();

        marcher = new MarchingCubes();
        
        InitCaveGenerator();
        
        CreateWorms ();

        DeployWorms();


        samplerFactory = v =>
                        (x, y, z) =>
                        {
                            foreach (Vector3 worm in WormPaths) {
                                if (Vector3.SqrMagnitude (v - worm) < wormRadius * wormRadius) return -1f;
                            }

                            return (float) noiseGenerator.GetValue(v.x + x, v.y + y, v.z + z);
                        };

        marcher.Surface = surfaceLevel;

        InitChunks();

    }

    void CreateWorms () {
        for (int i = 0; i < wormCount; i++) {
            worms.Add(RandomWorm());
        }
    }


    void SampleWormPoint (Worm worm) {

        WormPaths.Add(worm.pos);
    }


    public void AdvanceWorm (Worm worm) {
        worm.pitch = Mathf.Clamp(worm.pitch + wormSpeed * Mathf.PerlinNoise(worm.pos.x, worm.pos.y), -25, 25);
        worm.yaw = (360 + worm.yaw + wormSpeed * Mathf.PerlinNoise(worm.pos.y, worm.pos.x)) % 360;

        worm.Advance(wormSpeed);
    }


    void DeployWorms () {

        for (int t = 0; t < wormSteps; t++) {
            foreach (Worm worm in worms) {
                SampleWormPoint(worm);
                AdvanceWorm(worm);
            }
        }

    }


    public void ReloadVisibleChunks (List<Vector3> visibleChunks, Vector3 origin) {
        Chunk chunkComp;
        foreach (GameObject chunkObj in chunkObjects) {
            chunkComp = chunkObj.GetComponent<Chunk>();
            if (!visibleChunks.Contains(chunkObj.transform.position)) {
                if (chunkComp.TimeSinceView() > reloadThreshold
                && Vector3.SqrMagnitude(chunkObj.transform.position - origin) > reloadRadius * reloadRadius * chunkDistance * chunkDistance
                    && visibleChunks.Count > 0) {
                    ReloadChunkAt(chunkObj, chunkComp, visibleChunks[0]);
                visibleChunks.RemoveAt(0);
                }
            }
        }
    }

    void ReloadChunkAt (GameObject chunkObj, Chunk chunkComponent, Vector3 newPos) {

        chunkComponent.ReloadChunk(chunkSize, chunkMat, newPos, samplerFactory(newPos), marcher);

        chunkObj.transform.SetParent(transform);

    }

    void InitChunks () {
        for (int x_chunk = 0; x_chunk < chunkDistance; x_chunk++) {
            for (int y_chunk = 0; y_chunk < chunkDistance; y_chunk++) {
                for (int z_chunk = 0; z_chunk < chunkDistance; z_chunk++) {

                    GameObject chunkObj = new GameObject();

                    chunkObj.name = "Chunk " + (x_chunk + y_chunk + z_chunk);

                    Vector3 chunkPosition = new Vector3(x_chunk*(chunkSize - 1) , y_chunk*(chunkSize - 1), z_chunk*(chunkSize - 1)) + transform.position;

                    Chunk chunkComponent = chunkObj.AddComponent<Chunk>();

                    chunkObjects.Add(chunkObj);

                    chunkComponent.InitializeChunk();

                    ReloadChunkAt(chunkObj, chunkComponent, chunkPosition);

                    chunkManager.Register(chunkComponent);
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
    void InitCaveGenerator () {

        if (useComplexCave) {
            ComplexCave();
        } else {
            PerlinCave();
        }
    }

    void PerlinCave () {
        noiseGenerator = new LibNoise.Operator.Scale(noiseScale, noiseScale, noiseScale, new Perlin (frequency, lacunarity, persistence, octaves, seed.GetHashCode(), QualityMode.Medium));
    }

    void ComplexCave () {
        noiseGenerator = new LibNoise.Operator.Add(
            new Perlin (frequency*4, lacunarity, persistence, octaves, seed.GetHashCode(), QualityMode.Medium),
            new LibNoise.Operator.Add(
                new Perlin (frequency*2, lacunarity, persistence, octaves, seed.GetHashCode(), QualityMode.Medium),
                new Perlin (frequency, lacunarity, persistence, octaves, seed.GetHashCode(), QualityMode.Medium)));
    }

    Worm RandomWorm () {

        return new Worm(
            Random.Range(-25, 25),
            Random.Range(0, 360),
            new Vector3 (
                Random.Range(0, chunkSize*chunkDistance),
                chunkSize*chunkDistance,
                Random.Range(0, chunkSize*chunkDistance)));
    }


    
    private void OnDrawGizmos() {

        Gizmos.DrawWireCube(transform.position + Vector3.one * chunkDistance/2 *chunkSize, Vector3.one * chunkSize * chunkDistance);

        Gizmos.DrawSphere(transform.position + Vector3.one * chunkDistance/2 *chunkSize, 1f);

        foreach (GameObject chunk in chunkObjects) {

            Color chunkColor =  Color.Lerp(Color.red, Color.white, chunk.GetComponent<Chunk>().TimeSinceView());

            chunk.GetComponent<Chunk>().DrawBounds(chunkColor);
        }

        Gizmos.color = Color.red;

        foreach (Vector3 worm in WormPaths) {
            Gizmos.DrawWireSphere(worm, wormRadius);
        }
    }
}
