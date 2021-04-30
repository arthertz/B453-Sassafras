using System.Collections;
using System.Linq;
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

    [SerializeField] private bool debug = false;
    [SerializeField] CaveConfig config;

    List<Vector3> WormPaths = new List<Vector3>();

    private
    List<GameObject> chunkObjects = new List<GameObject>();

    private Marching marcher;

    public int wormSteps = 0;

    public float wormSpeed = 5f;

    public float wormRadius = 5f;

    //Config parameters
    private Material chunkMat;
    private int chunkSize;
    private int chunkDistance;
    private float reloadRadius;
    private float noiseScale;
    private float surfaceLevel;
    private double frequency;
    private double lacunarity;
    private double persistence;
    private string seed;
    private float reloadThreshold = .1f;
    private int octaves = 0;
    //End config parameters

    private bool useComplexCave = false;

    public int wormCount;

    private List<Worm> worms = new List<Worm> ();


    private LibNoise.ModuleBase noiseGenerator;
    

   public delegate float PointSampler (float x, float y, float z);

   public delegate PointSampler PointSamplerGenerator (Vector3 v);

   PointSamplerGenerator samplerFactory;

    void LoadConfig() {
        chunkMat = config.chunkMat;
        chunkSize = config.chunkSize;
        chunkDistance = config.chunkDistance;
        reloadRadius = config.reloadRadius;
        noiseScale = config.noiseScale;
        surfaceLevel = config.surfaceLevel;
        frequency = config.frequency;
        lacunarity = config.lacunarity;
        persistence = config.persistence;
        seed = config.seed;
        reloadThreshold = config.reloadThreshold;
        octaves = config.octaves;
    }


    // Start is called before the first frame update
    [ContextMenu("Initialize in editor")]
    void Awake ()
    {
        LoadConfig();

        Random.InitState(seed.GetHashCode());

        marcher = new MarchingCubes();
        
        InitCaveGenerator();
        
        //CreateWorms ();

        //DeployWorms();


        samplerFactory = v =>
                        (x, y, z) =>
                        {
                           /* foreach (Vector3 worm in WormPaths) {
                                if (Vector3.SqrMagnitude (v - worm) < wormRadius * wormRadius) return -1f;
                            }*/

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


    /*
    // Checks if one if the visible chunks is already at this position
    */
    private bool ChunkActiveAt (Vector3 chunkPosition) {
        foreach (GameObject g in chunkObjects) {
            if (Mathf.Approximately(Vector3.SqrMagnitude(g.transform.position - chunkPosition), 0)) {
                return true;
            }
        }

        return false;
    }


    public void ReloadVisibleChunks (List<Vector3> visibleChunks, Vector3 origin) {

        //Make a list of chunk positions that are not active but need to be loaded in
        List<Vector3> inactiveVisible = new List<Vector3>();

        foreach (Vector3 v in visibleChunks) {
            if (!ChunkActiveAt(v)) {
                inactiveVisible.Add(v);
            }
        }

        if (inactiveVisible.Count == 0) {
            return;
        }

        //Make a list of chunks that are active but ready to be reloaded
        List<GameObject> activeUnseen= new List<GameObject>();


        Chunk chunkComp;
        foreach (GameObject g in chunkObjects) {
            chunkComp = g.GetComponent<Chunk>();
            if (!visibleChunks.Contains(g.transform.position)
            && chunkComp.TimeSinceView() > reloadThreshold
            && Vector3.SqrMagnitude (chunkComp.transform.position - origin) > reloadRadius * chunkSize) {
                activeUnseen.Add(g);
            }
        }

        //Order activeUnseen by time since last seen. This would be faster with a heap/priority queue but this is good enough
        activeUnseen = activeUnseen.OrderByDescending(chunkObject => chunkObject.GetComponent<Chunk>().TimeSinceView()).ToList();

        //Order inactiveVisible by importance. This would be faster with a heap/priority queue but this is good enough
        inactiveVisible = inactiveVisible.OrderBy(chunkVector => Vector3.SqrMagnitude(chunkVector - origin)).ToList();

        if (debug) print (inactiveVisible.Count + " " + activeUnseen.Count);

        //While there are active chunks out of view && chunks left load...
        while (activeUnseen.Count > 0 && inactiveVisible.Count > 0) {
            ReloadChunkAt(activeUnseen[0],
                         activeUnseen[0].GetComponent<Chunk>(),
                         inactiveVisible[0]);
            inactiveVisible.RemoveAt(0);
            activeUnseen.RemoveAt(0);
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

                    Vector3 chunkPosition = new Vector3(x_chunk*(chunkSize) , y_chunk*(chunkSize), z_chunk*(chunkSize)) + transform.position;

                    Chunk chunkComponent = chunkObj.AddComponent<Chunk>();

                    chunkObjects.Add(chunkObj);

                    chunkComponent.InitializeChunk();

                    ReloadChunkAt(chunkObj, chunkComponent, chunkPosition);
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
