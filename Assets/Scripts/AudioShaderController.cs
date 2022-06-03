using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioShaderController : MonoBehaviour
{

    public Material _material;
    public float _amplitudeMultiplier = 1;
    public float _speedMultiplier = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _material.SetFloat("_Amount", (AudioSpectrumReader._audioBandIntensityBuffer[0] * _amplitudeMultiplier));
        _material.SetFloat("_Speed", (AudioSpectrumReader._audioBandIntensityBuffer[0] * _speedMultiplier));
    }
}
