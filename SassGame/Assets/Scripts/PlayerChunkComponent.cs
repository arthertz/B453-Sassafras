using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class MyVectorEvent : UnityEvent<List<Vector3>, Vector3>
{

}

public class PlayerChunkComponent : MonoBehaviour
{
    public
    int viewDistance = 8;

    public
    int chunkSize = 32;

    public
    float chunkReloadDelay = 1f;

    [SerializeField]
    MyVectorEvent SendVisibleChunks = new MyVectorEvent();


    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("TriggerChunkReload", chunkReloadDelay, chunkReloadDelay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    [ContextMenu("Reload chunks")]
    void TriggerChunkReload () {
        SendVisibleChunks.Invoke(GetVisibleChunks(), transform.position);
    }


    [ContextMenu("Count visible chunks")]
    public List<Vector3> GetVisibleChunks () {
        List<Vector3> visibleChunks = new List<Vector3>();
        
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        Bounds chunkBound;

        for (int x = -viewDistance; x < viewDistance; x++)
        {
            for (int y = -viewDistance; y <  viewDistance; y++)
            {
                for (int z = -viewDistance; z < viewDistance; z++)
                {
                    chunkBound = new Bounds (transform.position + (chunkSize-1) * new Vector3 (x +.5f ,y+.5f , z+.5f), Vector3.one * chunkSize/2);

                    if (GeometryUtility.TestPlanesAABB(planes, chunkBound)) {
                        visibleChunks.Add(transform.position + (chunkSize-1) * new Vector3(x, y, z));
                    }
                }
            }
        }

        //Debug.Log(visibleChunks.Count + " chunks visible");

        return visibleChunks;
    }


    private void OnDrawGizmos() {
    }
}
