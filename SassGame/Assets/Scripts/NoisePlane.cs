using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Generator;

public class NoisePlane : MonoBehaviour
{

    private Texture2D noiseTex;
    private Renderer rend;
    // Width and height of the texture in pixels.
    public int pixWidth;
    public int pixHeight;
    public string seed;

    public bool randomSeed = true;


    /// <summary>
    /// //LibNoise Variables
    /// </summary>
    public double frequency;
    public double lacunarity;
    public double persistence;
    //Noise texture octaves
    public int octaves = 3;
    Perlin perlinGenerator;
    Noise2D noiseForTexture;
    Vector2 origin;


/*
    /// <summary>
    /// Homemade approach variables
    /// </summary>
    // The origin of the sampled area in the plane.
    public float xOrg;
    public float yOrg;

    // The number of cycles of the basic noise pattern that are repeated
    // over the width and height of the texture.
    public float scale = 1.0F;
    
    Color[] pix;
*/

    void Start()
    {
        RegenerateTexture();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            RegenerateTexture();
        }
    }

    [ContextMenu ("RegenerateTexture")]
    void RegenerateTexture () {

        int randSeed = randomSeed ? Random.Range(int.MinValue, int.MaxValue) : seed.GetHashCode();

        Random.InitState(randSeed);
        
        perlinGenerator = new Perlin (frequency, lacunarity, persistence, octaves, randSeed, QualityMode.Medium);

        noiseForTexture = new Noise2D(pixWidth, pixHeight, perlinGenerator);

        rend = GetComponent<Renderer>();


        origin = Random.insideUnitCircle * Random.Range(int.MinValue, int.MaxValue);

        noiseForTexture.GeneratePlanar(origin.x, origin.x + pixWidth, origin.y, origin.y + pixHeight);

        // Set up the texture and a Color array to hold pixels during processing.
        noiseTex = noiseForTexture.GetTexture();  //CalcNoise(textureOctaves, pixWidth, pixHeight, randomSeed ? Random.Range(int.MinValue, int.MaxValue) : seed.GetHashCode());

        rend.sharedMaterial.mainTexture = noiseTex;
    }


    /*
    Texture2D CalcNoise(int octaves, int width, int height, int seed)
    {
        noiseTex = new Texture2D(width, height);

        Random.InitState(seed);

        Vector2 origin = new Vector2(Random.Range(0, int.MaxValue), Random.Range(0, int.MaxValue));
        Vector2 xDir = Random.onUnitSphere;
        Vector2 yDir = Random.onUnitSphere;

        // For each pixel in the texture...
        float y = 0.0F;

        while (y < height)
        {
            float x = 0.0F;
            while (x < width)
            {
                Vector2 xCoord = (xOrg + x / width * scale) * xDir;
                Vector2 yCoord = (yOrg + y / height * scale) * yDir;
                Vector2 offset = xCoord + yCoord;
                float sample = 0;

                for (int i = 1; i <= octaves; i++) {
                    sample += Mathf.PerlinNoise(offset.x*i ,offset.y*i);
                }

                sample /= octaves;
                
                pix[(int)y * width + (int)x] = new Color(sample, sample, sample);
                x++;
            }
            y++;
        }

        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.SetPixels(pix);
        noiseTex.Apply();
        return noiseTex;
    }
    */
}
