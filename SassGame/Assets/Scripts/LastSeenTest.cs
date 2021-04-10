using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastSeenTest : MonoBehaviour
{
     Plane [] planes;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    bool CheckInView () {
        planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        return GeometryUtility.TestPlanesAABB(planes, new Bounds(transform.position, Vector3.one));
    }

    private void OnDrawGizmos() {
        if (!CheckInView()){
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }    
    }
}
