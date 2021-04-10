using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChunkComponent : MonoBehaviour
{
    public
    int viewDistance = 8;

    public
    int chunkSize = 16;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }




    public List<Vector3> GetVisibleChunks (int ChunkSize) {
        List<Vector3> visibleChunks = new List<Vector3>();
        
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        for (int x = -viewDistance; x < viewDistance; x++)
        {
            for (int y = -viewDistance; y <  viewDistance; y++)
            {
                for (int z = -viewDistance; z < viewDistance; z++)
                {
                    Bounds chunkBounds = new Bounds (transform.position + new Vector3 (x,y,z) + Vector3.one * chunkSize/2, Vector3.one * chunkSize/2);

                    if (GeometryUtility.TestPlanesAABB(planes, chunkBounds)) {
                        visibleChunks.Add(new Vector3(x, y, z));
                    }
                }
            }
        }

        return visibleChunks;
    }


    private void OnDrawGizmos() {
    }
}
