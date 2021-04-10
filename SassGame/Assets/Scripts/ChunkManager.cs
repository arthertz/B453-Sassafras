using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager
{
    static bool debug = true;

    public List<Chunk> LoadedChunkPool = new List<Chunk>();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        



    }


    public void Register (Chunk chunk) {
        LoadedChunkPool.Add(chunk);    
    }

    public void OnDrawGizmos() {
        if (!debug) return;

        foreach (Chunk chunk in LoadedChunkPool) {

            Color chunkColor =  Color.Lerp(Color.red, Color.white, chunk.TimeSinceView());

            chunk.DrawBounds(chunkColor);
        }
    }
}
