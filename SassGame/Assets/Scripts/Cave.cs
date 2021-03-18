using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Generator;
using System;
using Random = UnityEngine.Random;

public class Cave : MonoBehaviour
{

    public int wormCount = 5;

    public bool wireframe = false;

    public bool invert = false;

    public float cubeSize = 1;
    public float threshold = .5f;

    public int[] caveDimensions = new int[3];

    public double frequency;
    public double lacunarity;
    public double persistence;

    public string seed;

    public bool usePositionNoise = false;

    //Noise texture octaves
    public int octaves = 3;
    
    List<Worm> worms = new List<Worm> ();

    Perlin perlinGenerator;

   public delegate bool VoxelPresent (int x, int y, int z);

   VoxelPresent checkVoxelPresent;

    // Start is called before the first frame update
    [ContextMenu("Initialize in editor")]
    void Start()
    {
        PerlinCaveCmd();
        
        for (int i = 0; i < wormCount; i++) {
            worms.Add(RandomWorm());
        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, new Vector3(caveDimensions[0], caveDimensions[1], caveDimensions[2]));

        //I plan to optimize this quite a bit later. For now it is only a proof of concept (especially since it won't stay forever)
        //-Arthur

        for (int x = Mathf.RoundToInt(transform.position.x) - caveDimensions[0]/2; x < Mathf.RoundToInt(transform.position.x) + caveDimensions[0]/2; x++) {
            for (int y = Mathf.RoundToInt(transform.position.y) - caveDimensions[0]/2; y < Mathf.RoundToInt(transform.position.y) + caveDimensions[1]/2; y++) {
                for (int z = Mathf.RoundToInt(transform.position.z) - caveDimensions[0]/2; z < Mathf.RoundToInt(transform.position.z) + caveDimensions[2]/2; z++) {
                    if (checkVoxelPresent (x, y, z)) {
                        if (wireframe) {
                            Gizmos.DrawWireCube(new Vector3(x, y, z), Vector3.one * cubeSize);
                        }
                        else
                        {
                            Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one * cubeSize);
                        } 
                    }
                }
            }
        }
    }

    bool PerlinLEQ (int x, int y, int z) {
         if (usePositionNoise && perlinGenerator != null)
         {
            return perlinGenerator.GetValue(x, y, z) <= threshold;
         } else {
             return false;
         }
    }

    bool PerlinGEQ (int x, int y, int z) {
        if (usePositionNoise && perlinGenerator != null)
        {
        return perlinGenerator.GetValue(x, y, z) >= threshold;
        } else {
            return false;
        }
    }

    [ContextMenu("Make Perlin Cave")]
    void PerlinCaveCmd () {
        if (invert) {
            checkVoxelPresent = PerlinLEQ;
        } else {
            checkVoxelPresent = PerlinGEQ;
        }

        PerlinCave();
    }

    void PerlinCave () {
        perlinGenerator = new Perlin (frequency, lacunarity, persistence, octaves, seed.GetHashCode(), QualityMode.Medium);
    }

    Worm RandomWorm () {
        return new Worm(
            Random.Range(-25, 25),
            Random.Range(0, 360),
            new Vector3 (
                Random.Range(0, caveDimensions[0]),
                caveDimensions[1],
                Random.Range(0, caveDimensions[2])));
    }
}
