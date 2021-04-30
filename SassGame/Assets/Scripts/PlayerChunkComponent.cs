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

    [SerializeField]
    CaveConfig config;

    private
    int viewDistance;

    private
    int chunkSize;

    private
    float chunkReloadDelay;

    private bool ignoreWalls;

    [SerializeField]
    MyVectorEvent SendVisibleChunks = new MyVectorEvent();

    private float forceReloadDistanceSqrd;


    // Start is called before the first frame update
    void Start()
    {
        chunkSize = config.chunkSize;

        viewDistance = config.viewDistance;

        chunkReloadDelay = config.chunkReloadDelay;

        ignoreWalls = config.ignoreWalls;

        forceReloadDistanceSqrd = chunkSize * chunkSize * config.reloadRadius * config.reloadRadius;

        //Set the camera render distance to match the view distance
        //The view distance is internally kept in chunk units, so you also must convert it to meters
        Camera.main.farClipPlane = Mathf.Min(config.maxViewDist, chunkSize * viewDistance);

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

        Vector3 chunkCenter;

        Vector3 chunkOrigin = new Vector3( Mathf.Round(transform.position.x / chunkSize) * chunkSize,
                                           Mathf.Round(transform.position.y / chunkSize) * chunkSize,
                                           Mathf.Round(transform.position.z / chunkSize) * chunkSize);

        Vector3 chunkDelta;

        bool chunkVisible;

        for (int x = -viewDistance; x < viewDistance; x++)
        {
            for (int y = -viewDistance; y <  viewDistance; y++)
            {
                for (int z = -viewDistance; z < viewDistance; z++)
                {
                    chunkDelta = (chunkSize) * new Vector3 (x + .5f , y + .5f, z + .5f);
                    chunkCenter = chunkOrigin + chunkDelta;
                    chunkVisible = !Physics.Linecast(transform.position, chunkCenter);

                    //the linecast only matters if the chunk is far away
                    //if it is close render it no matter what to avoid weird holes
                    if (chunkVisible || chunkDelta.sqrMagnitude < forceReloadDistanceSqrd || ignoreWalls)
                    {
                        chunkBound = new Bounds (chunkCenter, Vector3.one * chunkSize);

                        if (GeometryUtility.TestPlanesAABB(planes, chunkBound)) {
                            visibleChunks.Add(chunkOrigin + (chunkSize) * new Vector3(x, y, z));
                        }
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
