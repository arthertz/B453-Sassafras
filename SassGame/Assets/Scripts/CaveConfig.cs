using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "New Cave Configuration")]
public class CaveConfig : ScriptableObject
{
    public Material chunkMat;
    public int chunkSize = 8;
    public int chunkDistance = 4;
    
    public int viewDistance = 8;

    public float reloadRadius = 2f;
    public float noiseScale = .5f;
    public float surfaceLevel = .5f;
    public double frequency = .1;
    public double lacunarity = .5;
    public double persistence = .5;
    public string seed = "Osh";
    public float reloadThreshold = .1f;
    public int octaves = 0;
    public float chunkReloadDelay = 1f;
}
