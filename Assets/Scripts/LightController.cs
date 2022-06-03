using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    public int _band;
    public float _minIntensity, _maxIntensity;

    private Light _light;


    // Start is called before the first frame update
    void Start()
    {
        _light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        _light.intensity = (AudioSpectrumReader._audioBandIntensityBuffer[_band] * (_minIntensity - _maxIntensity)) + _minIntensity;
    }
}
