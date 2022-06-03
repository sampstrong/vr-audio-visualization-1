using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AudioSpectrumReader64 _audioSpectrumReader64;

    public int _band;
    public float _startScale, _maxScale;
    public Color _baseColor;

    public bool useBuffer;
    public bool useBands;
    //public bool scale1D = true;
    public bool changeColor = true;

    public enum _scaling {Scale1D, Scale3D, NoScale}
    public _scaling scaling = new _scaling();

    public Material _emissionMaterial;

    public float _delayTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float _audioBandIntensityBuffer = AudioSpectrumReader._audioBandIntensityBuffer[_band];
        float _audioBandIntensity = AudioSpectrumReader._audioBandIntensity[_band];
        float _audioAmplitudeBuffer = AudioSpectrumReader._AmplitudeBuffer;
        float _audioAmplitude = AudioSpectrumReader._Amplitude;

        StartCoroutine(AudioReactiveDelay(_audioBandIntensityBuffer, _audioBandIntensity, _audioAmplitudeBuffer, _audioAmplitude));

    }

    IEnumerator AudioReactiveDelay(float _audioBandIntensityBuffer, float _audioBandIntensity, float _audioAmplitudeBuffer, float _audioAmplitude)
    {
        yield return new WaitForSeconds(_delayTime);

        if(useBuffer)
        {
            if(useBands)
            {
                if(scaling == _scaling.Scale1D)
                {
                    transform.localScale = new Vector3(transform.localScale.x, (_audioBandIntensityBuffer * _maxScale) + _startScale, transform.localScale.z);
                }
                if(scaling == _scaling.Scale3D)
                {
                    transform.localScale = new Vector3((_audioBandIntensityBuffer * _maxScale) + _startScale, (_audioBandIntensityBuffer * _maxScale) + _startScale, (_audioBandIntensityBuffer * _maxScale) + _startScale);
                }
            }
            if(!useBands)
            {
                if(scaling == _scaling.Scale1D)
                {
                    transform.localScale = new Vector3(transform.localScale.x, (_audioAmplitudeBuffer * _maxScale) + _startScale, transform.localScale.z);
                }
                if(scaling == _scaling.Scale3D)
                {
                    transform.localScale = new Vector3((_audioAmplitudeBuffer * _maxScale) + _startScale, (_audioAmplitudeBuffer * _maxScale) + _startScale, (_audioAmplitudeBuffer * _maxScale) + _startScale);
                }
            }

            Color _tempDiffuseColor = new Color(_baseColor.r * _audioBandIntensityBuffer, _baseColor.g * _audioBandIntensityBuffer, _baseColor.b * _audioBandIntensityBuffer, 1);
            _emissionMaterial.SetColor("_emission", _tempDiffuseColor);
        }

        if(!useBuffer)
        {
            if (useBands)
            {
                if (scaling == _scaling.Scale1D)
                {
                    transform.localScale = new Vector3(transform.localScale.x, (_audioBandIntensity * _maxScale) + _startScale, transform.localScale.z);
                }
                if (scaling == _scaling.Scale3D)
                {
                    transform.localScale = new Vector3((_audioBandIntensity * _maxScale) + _startScale, (_audioBandIntensity * _maxScale) + _startScale, (_audioBandIntensity * _maxScale) + _startScale);
                }
            }
            if (!useBands)
            {
                if (scaling == _scaling.Scale1D)
                {
                    transform.localScale = new Vector3(transform.localScale.x, (_audioAmplitude * _maxScale) + _startScale, transform.localScale.z);
                }
                if (scaling == _scaling.Scale3D)
                {
                    transform.localScale = new Vector3((_audioAmplitude * _maxScale) + _startScale, (_audioAmplitude * _maxScale) + _startScale, (_audioAmplitude * _maxScale) + _startScale);
                }
            }

            Color _tempDiffuseColor = new Color(_baseColor.r * _audioBandIntensity, _baseColor.g * _audioBandIntensity, _baseColor.b * _audioBandIntensity, 1);
            _emissionMaterial.SetColor("_emission", _tempDiffuseColor);
        }
    }
}

