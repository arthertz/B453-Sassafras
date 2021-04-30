using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLight : MonoBehaviour
{
     private Light myLight;
     
    void Start ()
    {
        myLight = GetComponent<Light>();
    }
     
    void Update ()
    {
        if(Input.GetKeyUp(KeyCode.F))
        {       
            if(myLight.intensity == 0){
                myLight.intensity = 2;
                Debug.Log("intensity = 2");
                return;
            }
            if (myLight.intensity == 1){
                myLight.intensity = 0;
                Debug.Log("intensity = 0");
                return;
            }
            if (myLight.intensity == 2){
                myLight.intensity = 1;
                Debug.Log("intensity = 1");
                return;
            }
            

        }
    }
}

//https://forum.unity.com/threads/flashlight-script.395596/
