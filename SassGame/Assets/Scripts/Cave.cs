using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Generator;

public class Cave : MonoBehaviour
{

    public int wormCount = 5;

    public bool wireframe = false;

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

   int [,,] caveData;

    // Start is called before the first frame update
    [ContextMenu("Initialize in editor")]
    void Start()
    {
        perlinGenerator = new Perlin (frequency, lacunarity, persistence, octaves, seed.GetHashCode(), QualityMode.Medium);
        
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
                    if (VoxelPresent (x, y, z)) {
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

    // Update is called once per frame
    void Update()
    {
        
    }

    bool VoxelPresent (int x, int y, int z) {
         if (usePositionNoise && perlinGenerator != null)
         {
            return perlinGenerator.GetValue(x, y, z) >= threshold;
         } else {
            return caveData[x,y,z] >= threshold;
        }
    }

    [ContextMenu("Decimate cave")]
    void DecimateCaveCmd () {
        caveData = DecimateCave(caveData);
    }

    [ContextMenu("Make Perlin Cave")]
    void PerlinCaveCmd () {
        caveData = PerlinCave();
    }

    int[,,] PerlinCave () {
        int[,,] newCave = new int[caveDimensions[0], caveDimensions[1], caveDimensions[2]];

        perlinGenerator = new Perlin (frequency, lacunarity, persistence, octaves, seed.GetHashCode(), QualityMode.Medium);

        Vector3 origin = Random.onUnitSphere * Random.Range(int.MinValue, int.MaxValue);

        for (int x = 0; x < caveDimensions[0]; x++) {
            for (int y = 0; y < caveDimensions[1]; y++) {
                for (int z = 0; z < caveDimensions[2]; z++) {
                    newCave[x,y,z] = perlinGenerator.GetValue(origin.x + x, origin.y + y, origin.z + z) >= threshold ? 1 : 0;
                }
            }
        }


        return newCave;
    }


    int[,,] DecimateCave (int[,,] oldCave) {
        int[,,] newCave = new int[caveDimensions[0], caveDimensions[1], caveDimensions[2]];

        int cubesLeft = 0;

        for (int x = 0; x < caveDimensions[0]; x++) {
            for (int y = 0; y < caveDimensions[1]; y++) {
                for (int z = 0; z < caveDimensions[2]; z++) {
                    newCave[x,y,z] = Random.Range(0f, 1f) < .1f ? 0 : oldCave[x,y,z];
                    cubesLeft += newCave[x,y,z];
                }
            }
        }

        print (cubesLeft + " cubes left");
        return newCave;
    }

    int[,,] AllOnes () {
        int[,,] newCave = new int[caveDimensions[0], caveDimensions[1], caveDimensions[2]];

        for (int x = 0; x < caveDimensions[0]; x++) {
            for (int y = 0; y < caveDimensions[1]; y++) {
                for (int z = 0; z < caveDimensions[2]; z++) {
                    newCave[x,y,z] = 1;
                }
            }
        }

        return newCave;
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
