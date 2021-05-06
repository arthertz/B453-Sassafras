using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortSignalScript : MonoBehaviour
{
    [SerializeField] AK.Wwise.Event onClick;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Click to make a short sound, test reverb
        if (Input.GetKeyDown("mouse 0")) {
            onClick.Post(gameObject);
        }
    }
}
