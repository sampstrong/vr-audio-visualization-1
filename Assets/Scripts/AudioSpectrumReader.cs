using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(AudioSource))]
public class AudioSpectrumReader : MonoBehaviour
{
    private AudioSource _audioSource;

    [HideInInspector] public float[] _samplesLeft = new float[512];
    [HideInInspector] public float[] _samplesRight = new float[512];

    public static float[] _freqBand = new float[8];
    public static float[] _bandBuffer = new float[8];
    private float[] _bufferDecrease = new float[8];

    private float[] _freqBandHighest = new float[8];
    public static float[] _audioBandIntensity = new float[8];
    public static float[] _audioBandIntensityBuffer = new float[8];

    public static float _Amplitude, _AmplitudeBuffer;
    private float _AmlitudeHighest;

    public float _audioProfile = 5;

    public enum _channel {Stereo, Left, Right};
    public _channel channel = new _channel();

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        SetAudioProfile(_audioProfile);
    }

    // Update is called once per frame
    void Update()
    {
        
        GetSpectrumAudioSource();
        MakeFrequencyBands();
        MakeBandBuffer();
        CreateAudioBandIntensity();
        GetAmplitude();

    }

    void SetAudioProfile(float audioProfile)
    {
        for (int i = 0; i < 8; i++)
        {
            _freqBandHighest[i] = audioProfile;
        }
    }

    void GetAmplitude()
    {

        float _CurrentAmplitude = 0;
        float _CurrentAmplitudeBuffer = 0;

        for (int i = 0; i < 8; i++)
        {
            _CurrentAmplitude += _audioBandIntensity[i];
            _CurrentAmplitudeBuffer += _audioBandIntensityBuffer[i];
        }

        if(_CurrentAmplitude > _AmlitudeHighest)
        {
            _AmlitudeHighest = _CurrentAmplitude;
        }

        _Amplitude = _CurrentAmplitude / _AmlitudeHighest;
        _AmplitudeBuffer = _CurrentAmplitudeBuffer / _AmlitudeHighest;
    }


    void CreateAudioBandIntensity()
    {
        for (int i = 0; i < 8; i++)
        {
            

            if (_freqBand[i] > _freqBandHighest[i])
            {
                _freqBandHighest[i] = _freqBand[i];
            }

            _audioBandIntensity[i] = (_freqBand[i] / _freqBandHighest[i]);
            _audioBandIntensityBuffer[i] = (_bandBuffer[i] / _freqBandHighest[i]);
        }
    }

    void MakeBandBuffer()
    {
        for (int g = 0; g < 8; ++g)
        {
            if (_freqBand[g] > _bandBuffer[g])
            {
                _bandBuffer[g] = _freqBand[g];
                _bufferDecrease[g] = 0.005f;
            }
            if (_freqBand[g] < _bandBuffer[g])
            {
                _bandBuffer[g] -= _bufferDecrease[g];
                _bufferDecrease[g] *= 1.2f;
            }
        }
    }

    void GetSpectrumAudioSource()
    {
        _audioSource.GetSpectrumData(_samplesLeft, 0, FFTWindow.Blackman);
        _audioSource.GetSpectrumData(_samplesRight, 1, FFTWindow.Blackman);
    }

    

    void MakeFrequencyBands()
    {
        /* 22050 Hz / 512 samples = 43Hz per sample
         * 
         * band 0: 2 samples = 86Hz: 0 - 86
         * band 1: 4 samples = 172Hz: 87 - 258
         * band 2: 8 samples = 344Hz: 259 - 602
         * band 3: 16 samples = 688Hz: 603 - 1290
         * band 4: 32 samples = 1376Hz: 1291 - 2666
         * band 5: 64 samples = 2752Hz: 2667 - 5418
         * band 6: 128 samples = 5504Hz: 5419 - 10922
         * band 7: 256 samples = 11008Hz: 10923 - 21930
         *
         *Total = 510 -- 2 short of 512 -- can add 2 to band 7 below
         */

        int count = 0;

        for (int i = 0; i < 8; i++)
        {
            float average = 0;
            int sampleCount = (int)Mathf.Pow(2, i) * 2;

            /*
            if (i == 7)
            {
                sampleCount += 2;
            }
            */

            for (int j = 0; j < sampleCount; j++)
            {

                if(channel == _channel.Stereo)
                {
                    average += (_samplesLeft[count] + _samplesRight[count]) * (count + 1);
                }
                if(channel == _channel.Left)
                {
                    average += _samplesLeft[count] * (count + 1);
                }
                if (channel == _channel.Right)
                {
                    average += _samplesRight[count] * (count + 1);
                }

                count++;
            }

            average /= count;

            _freqBand[i] = average * 10;

        }


    }
}