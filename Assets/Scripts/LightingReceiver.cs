using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingReceiver : MonoBehaviour
{
    public int _lightNumber;

    private LightingController _lightingController;



    // Start is called before the first frame update
    void Start()
    {
        _lightingController = GameObject.Find("LightingController").GetComponent<LightingController>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
