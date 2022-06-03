using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightingController : MonoBehaviour
{
    public Light[] _upperLights = new Light[22];
    private MeshRenderer[] _lightVolumes = new MeshRenderer[22];
    public GameObject[] _lightPrefabs = new GameObject[22];
    public GameObject[] _lightHeads = new GameObject[22];
    public GameObject[] _lightSources = new GameObject[22];

    private Quaternion[] _baseLightRotations = new Quaternion[22];
    private Quaternion[] _randomLightHeadRotations = new Quaternion[22];

    //public Light _upperlight;
    //private float _minIntensity;
    private float _maxIntensity = 4;
    public Color _whiteColor;
    public Color _redColor;

    public Material _whiteLightSource;
    public Material _redLightSource;
    
    private Slider _lightIntensitySlider;
    private Slider _strobeSpeedSlider;
    private Slider _rotationSpeedSlider;

    private Toggle _strobeToggle1;
    private Toggle _strobeToggle2;
    private Toggle _lightHeadRotationToggle;
    private Toggle _randomizeRotationToggle;

    private float _lightIntensity = 1;
    private bool _strobeLights1On = false;
    private bool _strobeLights2On = false;
    private bool _lightHeadsRotating = false;
    private bool _randomizeRotation;

    private float lightHeadRotationValue;
    private float _rotationOffset;

    private float _whiteLightIntensity = 0.7f;
    private float _redLightIntensity = 1.0f;

    private float _strobeSpeed;

    private float _averageAmplitude = AudioSpectrumReader._AmplitudeBuffer;

    // Start is called before the first frame update
    void Start()
    {
        //get all needed components for interactions
        _lightIntensitySlider = GameObject.Find("Light Intensity Slider").GetComponent<Slider>();
        _strobeSpeedSlider = GameObject.Find("Strobe Speed Slider").GetComponent<Slider>();
        _rotationSpeedSlider = GameObject.Find("Rotation Speed Slider").GetComponent<Slider>();

        _strobeToggle1 = GameObject.Find("Strobe Toggle 1").GetComponent<Toggle>();
        _strobeToggle2 = GameObject.Find("Strobe Toggle 2").GetComponent<Toggle>();
        _lightHeadRotationToggle = GameObject.Find("Light Head Rotation Toggle").GetComponent<Toggle>();
        _randomizeRotationToggle = GameObject.Find("Randomize Rotation Toggle").GetComponent<Toggle>();

        //set light volume array at start
        GetLightVolumes();

        //capture initial light rotationa s a reference to revert back to
        CaptureBaseRotation();
    }

    // Update is called once per frame
    void Update()
    {
        //continually update light intensity, strobe light speed and light head rotation based on in game user input
        SetLightIntensity();
        ControlStrobeLightsType1();
        ControlStrobeLightsType2();
        RotateLightHeads();
    }

    public void SwitchToRed()
    {
        //initiate change to red light color for light beams
        StartCoroutine(SwitchToRedBeamLerp());

        //set light source material to match red light beam color
        foreach (GameObject _lightSource in _lightSources)
        {
            _lightSource.GetComponent<MeshRenderer>().material = _redLightSource;
        }

        //set light intensity to desired value for red lights
        _lightIntensitySlider.value = _redLightIntensity;
        Debug.Log(_lightIntensitySlider.value);
    }

    //smooth transition to red light beam
    IEnumerator SwitchToRedBeamLerp()
    {
        float lerpDuration = 1.0f;
        float timeElapsed = 0;

        while(timeElapsed < lerpDuration)
        {
            foreach (Light _light in _upperLights)
            {
                Color _currentColor = _light.color;

                _light.color = Color.Lerp(_currentColor, _redColor, (timeElapsed / lerpDuration));
            }

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        //set final value (otherwise would not go 100% of the way to red)
        foreach (Light _light in _upperLights)
        {
            _light.color = _redColor;
        }
    }

    public void SwitchToWhite()
    {
        //initiate light beam transition to white
        StartCoroutine(SwitchToWhiteLerp());
        

        foreach (GameObject _lightSource in _lightSources)
        {
            _lightSource.GetComponent<MeshRenderer>().material = _whiteLightSource;
        }

        //set light intensity to desired value for white lights
        _lightIntensitySlider.value = _whiteLightIntensity;
        Debug.Log(_lightIntensitySlider.value);
    }

    //smooth transition to white light beam
    IEnumerator SwitchToWhiteLerp()
    {
        float lerpDuration = 1.0f;
        float timeElapsed = 0;

        while (timeElapsed < lerpDuration)
        {
            foreach (Light _light in _upperLights)
            {
                Color _currentColor = _light.color;

                _light.color = Color.Lerp(_currentColor, _whiteColor, (timeElapsed / lerpDuration));
            }

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        //sets final value (otherwise would not go 100% of the way to white)
        foreach (Light _light in _upperLights)
        {
            _light.color = _whiteColor;
            
        }
    }

    //called in update - continually updates light intensity based on user input and/or presets
    public void SetLightIntensity()
    {
        for (int i = 0; i < 22; i++)
        {
            
            _lightIntensity = _lightIntensitySlider.value;
            _upperLights[i].intensity = _maxIntensity * _lightIntensity;
            _upperLights[i].color = new Color(_upperLights[i].color.r, _upperLights[i].color.g, _upperLights[i].color.b, _lightIntensity);
        }
    }

    //populates _lightvolumes[] array
    void GetLightVolumes()
    {
        for (int i = 0; i < _upperLights.Length; i++)
        {
            _lightVolumes[i] = _upperLights[i].transform.Find("LightVolume").GetComponent<MeshRenderer>();
        }
    }


    void ControlStrobeLightsType1()
    {
        //sets strobe speed from user input
        float _strobeSpeed = _strobeSpeedSlider.value;

        //sets booleans based on toggle input
        if(_strobeToggle1.isOn)
        {
            _strobeLights1On = true;
            _strobeLights2On = false;
            _strobeToggle2.isOn = false;
        }
        if (!_strobeToggle1.isOn)
        {
            _strobeLights1On = false;
            
        }

        //starts strobe if toggle is on
        if (_strobeLights1On)
        {
            StartCoroutine(StrobeLightLoop1(_strobeSpeed));
        }

        //makes sure lights are on when strobe is turned off
        if(!_strobeLights1On)
        {
            foreach (MeshRenderer _lightVolume in _lightVolumes)
            {
                _lightVolume.enabled = true;
            }
        }
    }

    //part 1 of 2 part coroutine on strobe light type 1
    IEnumerator StrobeLightLoop1(float _strobeSpeed)
    {
        yield return new WaitForSeconds(_strobeSpeed);

        if(_strobeLights1On)
        {
            foreach (MeshRenderer _lightVolume in _lightVolumes)
            {
                _lightVolume.enabled = false;
            }

            StartCoroutine(StrobeLightLoop2(_strobeSpeed));
        }
    }

    //part 2 of 2 part coroutine on strobe light type 1
    IEnumerator StrobeLightLoop2(float _strobeSpeed)
    {
        yield return new WaitForSeconds(_strobeSpeed);

        if(_strobeLights1On)
        {
            foreach (MeshRenderer _lightVolume in _lightVolumes)
            {
                _lightVolume.enabled = true;
            }

            StartCoroutine(StrobeLightLoop1(_strobeSpeed));
        }
    }

    void ControlStrobeLightsType2()
    {
        //controls strobe speed based on user input
        _strobeSpeed = _strobeSpeedSlider.value;
    }

    public void ToggleStrobeLights()
    {
        Debug.Log("Strobe Lights Toggled");

        //sets booleans based on user input
        if (_strobeToggle2.isOn)
        {
            _strobeLights2On = true;
            _strobeLights1On = false;
            _strobeToggle1.isOn = false;

        }
        if (!_strobeToggle2.isOn)
        {
            _strobeLights2On = false;

        }

        //starts strobe is toggle is on
        if (_strobeLights2On)
        {
            StartCoroutine(StrobeLightLoopNew());
        }

        //makes sure lights are on when toggle is turned off
        if (!_strobeLights2On)
        {
            foreach (MeshRenderer _lightVolume in _lightVolumes)
            {
                _lightVolume.enabled = true;
            }
        }
    }

    //coroutine for a simple looping strobe (type 2)
    IEnumerator StrobeLightLoopNew()
    {
        Debug.Log("Strobe Lights Coroutine Started");
        while(_strobeLights2On)
        {
            for (int i = 0; i < _lightVolumes.Length; i++)
            {
                _lightVolumes[i].enabled = !_lightVolumes[i].enabled; 
            }

            yield return new WaitForSeconds(_strobeSpeed);
        }
    }

    //sets a value for each lights initial rotation to reference back to after rotations are randomized (called at start)
    void CaptureBaseRotation()
    {
        for (int i = 0; i < _lightHeads.Length; i++)
        {
            _baseLightRotations[i] = _lightHeads[i].transform.rotation;
        }
    }

    //starts rotation of all lights back to base values
    void RotateBackToBase()
    {
        StartCoroutine(RotateBackToBaseLerp());
    }

    //smooth transition back to base values
    IEnumerator RotateBackToBaseLerp()
    {
        float lerpDuration = 3.0f;
        float timeElapsed = 0;

        while (timeElapsed < lerpDuration)
        {
            for (int i = 0; i < _baseLightRotations.Length; i++)
            {
                _lightHeads[i].transform.rotation = Quaternion.Slerp(_lightHeads[i].transform.rotation, _baseLightRotations[i], (timeElapsed / lerpDuration));

            }

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        for (int i = 0; i < _baseLightRotations.Length; i++)
        {
            _lightHeads[i].transform.rotation = _baseLightRotations[i];

        }
    }

    //sets booleans for light head rotation based on user input
    public void ToggleLightHeadRotation()
    {
        if(_lightHeadRotationToggle.isOn)
        {
            _lightHeadsRotating = true;

            
        }
        if (!_lightHeadRotationToggle.isOn)
        {
            _lightHeadsRotating =false;
        }
    }

    
    public void ToggleRandomRotate()
    {
        //sets booleans for light head rotation based on user input
        if (_randomizeRotationToggle.isOn)
        {
            _randomizeRotation = true;
        }
        if (!_randomizeRotationToggle.isOn)
        {
            _randomizeRotation = false;
        }

        //initiates rotation back to base when randomize rotation is not checked
        if (!_randomizeRotation)
        {
            RotateBackToBase();
        }

        //initiates a loop for random light head rotation that sets new rotation values over time
        if (_randomizeRotation)
        {
            StartCoroutine(RandomRotationUpdater());
        }
    }

    //controls light head rotations depending on how booleans are set (called in update)
    void RotateLightHeads()
    {
        float _rotationSpeed = _rotationSpeedSlider.value;
        float _rotationMultiplier = (Mathf.PingPong(Time.time * _rotationSpeed, 20) - 10) * _rotationSpeed;

        if(_lightHeadsRotating)
        {
            if(!_randomizeRotation)
            {
                foreach (GameObject _lighthead in _lightHeads)
                {
                    _lighthead.transform.Rotate(Vector3.forward * Time.deltaTime * _rotationMultiplier);
                }
            }
            
            if (_randomizeRotation)
            {
                for (int i = 0; i < _lightHeads.Length; i++)
                {
                    _lightHeads[i].transform.rotation = _randomLightHeadRotations[i];
                }

                foreach (GameObject _lighthead in _lightHeads)
                {
                    _lighthead.transform.Rotate(Vector3.forward * Time.deltaTime * _rotationMultiplier);
                }
            }
        }

        if(!_lightHeadsRotating)
        {
            if (_randomizeRotation)
            {
                for (int i = 0; i < _lightHeads.Length; i++)
                {
                    _lightHeads[i].transform.rotation = _randomLightHeadRotations[i];
                }

                foreach (GameObject _lighthead in _lightHeads)
                {
                    _lighthead.transform.Rotate(Vector3.forward * Time.deltaTime * _rotationMultiplier);
                }
            }
        }
    }

    //gives a new array of random rotations at a specified interval - values appled to light heads when random rotation is checked by a user
    IEnumerator RandomRotationUpdater()
    {
        while(_randomizeRotation)
        {
            yield return new WaitForSeconds(0.1f);

            for (int i = 0; i < _lightHeads.Length; i++)
            {
                _randomLightHeadRotations[i] = Quaternion.Euler(_lightHeads[i].transform.rotation.x, _lightHeads[i].transform.rotation.y, Random.Range(-120, -10));
            }
        }
        
    }

    //all methods below correspond to presets on the main control panel

    public void BreakNoStrobeWhite()
    {
        _lightHeadRotationToggle.isOn = true;
        _lightHeadsRotating = true;
        ToggleLightHeadRotation();

        _randomizeRotationToggle.isOn = false;
        ToggleRandomRotate();

        _strobeToggle1.isOn = false;
        _strobeToggle2.isOn = false;

        _lightIntensitySlider.value = _whiteLightIntensity;

        SwitchToWhite();

        Debug.Log(_lightIntensitySlider.value);

    }

    public void BreakNoStrobeRed()
    {
        _lightHeadRotationToggle.isOn = true;
        ToggleLightHeadRotation();

        _randomizeRotationToggle.isOn = false;
        ToggleRandomRotate();

        _strobeToggle1.isOn = false;
        _strobeToggle2.isOn = false;

        _lightIntensitySlider.value = _redLightIntensity;

        SwitchToRed();

        Debug.Log(_lightIntensitySlider.value);

    }

    public void BreakWithStrobeWhite()
    {
        _lightHeadRotationToggle.isOn = true;
        ToggleLightHeadRotation();

        _randomizeRotationToggle.isOn = false;
        ToggleRandomRotate();

        _strobeToggle1.isOn = true;
        _strobeToggle2.isOn = false;

        _lightIntensitySlider.value = _whiteLightIntensity;

        SwitchToWhite();

        Debug.Log(_lightIntensitySlider.value);

    }

    public void BreakWithStrobeRed()
    {
        _lightHeadRotationToggle.isOn = true;
        ToggleLightHeadRotation();

        _randomizeRotationToggle.isOn = false;
        ToggleRandomRotate();

        _strobeToggle1.isOn = true;
        _strobeToggle2.isOn = false;

        _lightIntensitySlider.value = _redLightIntensity;

        SwitchToRed();

        Debug.Log(_lightIntensitySlider.value);

    }

    public void DropNoStrobeWhite()
    {
        _lightHeadRotationToggle.isOn = true;
        ToggleLightHeadRotation();

        _randomizeRotationToggle.isOn = true;
        ToggleRandomRotate();

        _strobeToggle1.isOn = false;
        _strobeToggle2.isOn = false;

        _lightIntensitySlider.value = _whiteLightIntensity;

        SwitchToWhite();

        Debug.Log(_lightIntensitySlider.value);

    }

    public void DropNoStrobeRed()
    {
        _lightHeadRotationToggle.isOn = true;
        ToggleLightHeadRotation();

        _randomizeRotationToggle.isOn = true;
        ToggleRandomRotate();

        _strobeToggle1.isOn = false;
        _strobeToggle2.isOn = false;

        _lightIntensitySlider.value = _redLightIntensity;

        SwitchToRed();

        Debug.Log(_lightIntensitySlider.value);

    }

    public void DropWithStrobeWhite()
    {
        _lightHeadRotationToggle.isOn = true;
        ToggleLightHeadRotation();

        _randomizeRotationToggle.isOn = true;
        ToggleRandomRotate();

        _strobeToggle1.isOn = false;
        _strobeToggle2.isOn = true;
        _strobeSpeedSlider.value = 0.15f;

        _lightIntensitySlider.value = _whiteLightIntensity;

        SwitchToWhite();

        Debug.Log(_lightIntensitySlider.value);

    }

    public void DropWithStrobeRed()
    {
        _lightHeadRotationToggle.isOn = true;
        ToggleLightHeadRotation();

        _randomizeRotationToggle.isOn = true;
        ToggleRandomRotate();

        _strobeToggle1.isOn = false;
        _strobeToggle2.isOn = true;
        _strobeSpeedSlider.value = 0.15f;

        _lightIntensitySlider.value = _redLightIntensity;

        SwitchToRed();

        Debug.Log(_lightIntensitySlider.value);

    }
}
     