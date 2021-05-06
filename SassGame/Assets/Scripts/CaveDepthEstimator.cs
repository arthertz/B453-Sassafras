using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CaveDepthEstimator : MonoBehaviour
{
    /*
    // Keeps a running estimate of how deep/big the cave is for Oli's acoustics simulations
    // -Arthur
    */


    [SerializeField] bool debug = false;
    [SerializeField] float maxDepth = 999f;
    [SerializeField] float minDepth = 0f;
    [SerializeField] private int initialRays = 10;
    [SerializeField] private int burstSize = 5;
    [SerializeField] private int numBursts = 5; 
    [SerializeField] private float RayStagger = .1f;    
    private float currentSummedDistance = 0f;
    private int currentSamples = 0;
    private int levelMask;


    public UnityEvent RefreshedEstimate = new UnityEvent();



    public float UnclampedAverageDepth () {
        return currentSummedDistance/currentSamples;
    }

    public float AverageDepth () {
        return Mathf.Clamp(UnclampedAverageDepth (), minDepth, maxDepth);
    }

    public float CubedAvgDepth () {
        return AverageDepth() * AverageDepth() * AverageDepth();
    }

    private void OnEnable () {
        levelMask = ~LayerMask.NameToLayer("Procedural");
        StartCoroutine( ResampleEstimate() );
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    private void OnDestroy() {
        StopAllCoroutines();
    }

    private void SampleRandomRay () {
        Ray randomRay = new Ray(transform.position, Random.onUnitSphere);

        RaycastHit hit;

        Physics.Raycast(randomRay, out hit, maxDepth, levelMask);

        currentSamples++;
        currentSummedDistance += hit.distance;
    }


    private IEnumerator RaySampleBurst () {
        for (int i = 0; i < burstSize; i++) {
            SampleRandomRay();
            yield return new WaitForSeconds(RayStagger);
        }
        RefreshedEstimate.Invoke();
    }

    private IEnumerator ResampleEstimate () {
        if (debug) print ("Resampling depth estimate");
        currentSamples = 0;
        currentSummedDistance = 0f;
        for (int i = 0; i < initialRays; i++) {
            if (debug) print ("Sampling ray " + i + " out of " + initialRays + " initial rays");
            SampleRandomRay();
        }

        if (debug) print("Initial depth estimate is " + AverageDepth() + ", will continue to refine");

        RefreshedEstimate.Invoke();

        yield return StartCoroutine( RefineEstimate() );
    }


    private IEnumerator RefineEstimate () {
        if (debug) print("Refining ray depth estimate");
        for (int i = 0; i < numBursts; i++) {
            yield return StartCoroutine( RaySampleBurst() );
            if (debug) print("Updated depth estimate is " + AverageDepth() + ", will continue to refine");
        }

        yield return StartCoroutine ( ResampleEstimate() );
    }
}
