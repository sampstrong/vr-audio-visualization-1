using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(AudioSource))]
public class AudioSpectrumReader64 : MonoBehaviour
{
    private AudioSource _audioSource;

    private float[] _samplesLeft = new float[512];
    private float[] _samplesRight = new float[512];

    private float[] _freqBand = new float[8];
    private float[] _bandBuffer = new float[8];
    private float[] _bufferDecrease = new float[8];
    private float[] _freqBandHighest = new float[8];

    private float[] _freqBand64 = new float[64];
    private float[] _bandBuffer64 = new float[64];
    private float[] _bufferDecrease64 = new float[64];
    private float[] _freqBandHighest64 = new float[64];

    [HideInInspector]
    private float[] _audioBandIntensity, _audioBandIntensityBuffer;

    [HideInInspector]
    private float[] _audioBandIntensity64, _audioBandIntensityBuffer64;

    [HideInInspector]
    public float _Amplitude, _AmplitudeBuffer;
    private float _AmlitudeHighest;

    public float _audioProfile = 5;

    public enum _channel {Stereo, Left, Right};
    public _channel channel = new _channel();

    // Start is called before the first frame update
    void Start()
    {
        _audioBandIntensity = new float[8];
        _audioBandIntensityBuffer = new float[8];

        _audioBandIntensity64 = new float[64];
        _audioBandIntensityBuffer64 = new float[64];

        _audioSource = GetComponent<AudioSource>();

        SetAudioProfile(_audioProfile);
    }

    // Update is called once per frame
    void Update()
    {
        
        GetSpectrumAudioSource();
        MakeFrequencyBands();
        MakeFrequencyBands64();
        MakeBandBuffer();
        MakeBandBuffer64();
        CreateAudioBandIntensity();
        CreateAudioBandIntensity64();
        GetAmplitude();

    }

    //analyzes audio data before it's played so highest values are known
    void SetAudioProfile(float audioProfile)
    {
        for (int i = 0; i < 8; i++)
        {
            _freqBandHighest[i] = audioProfile;
        }
    }

    //gets the overall amplitude of the audio source in a value ranging from 0 - 1
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

    //gets intensity of each band in a 0-1 value (8 bands)
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

    //gets intensity of each band in a 0-1 value (64 bands)
    void CreateAudioBandIntensity64()
    {
        for (int i = 0; i < 64; i++)
        {


            if (_freqBand64[i] > _freqBandHighest64[i])
            {
                _freqBandHighest64[i] = _freqBand64[i];
            }

            _audioBandIntensity64[i] = (_freqBand64[i] / _freqBandHighest64[i]);
            _audioBandIntensityBuffer64[i] = (_bandBuffer64[i] / _freqBandHighest64[i]);
        }
    }

    //smooths out values to create a more natural response and flow with the audio signal (8 bands)
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

    //smooths out values to create a more natural response and flow with the audio signal (64 bands)
    void MakeBandBuffer64()
    {
        for (int g = 0; g < 64; ++g)
        {
            if (_freqBand64[g] > _bandBuffer64[g])
            {
                _bandBuffer64[g] = _freqBand64[g];
                _bufferDecrease64[g] = 0.005f;
            }
            if (_freqBand64[g] < _bandBuffer64[g])
            {
                _bandBuffer64[g] -= _bufferDecrease64[g];
                _bufferDecrease64[g] *= 1.2f;
            }
        }
    }

    //initial point that collects samples from incoming audio - colleced twice for stereo
    void GetSpectrumAudioSource()
    {
        _audioSource.GetSpectrumData(_samplesLeft, 0, FFTWindow.Blackman);
        _audioSource.GetSpectrumData(_samplesRight, 1, FFTWindow.Blackman);
    }

    
    //divides 20k+ samles into 8 bands
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

            
            if (i == 7)
            {
                sampleCount += 2;
            }
            

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

    //divides 20k+ samples into 64 bands
    void MakeFrequencyBands64()
    {
        /* 0-15 = 1 sample    = 16
         * 16-31 = 2 samples  = 32
         * 32-39 = 4 samples  = 32
         * 40-47 = 6 samples  = 48
         * 48-55 = 16 samples = 128
         * 56-63 = 32 samples = 256
         *                      ---
         *                      512
         */             


        int count = 0;
        int sampleCount = 1;
        int power = 0;


        for (int i = 0; i < 64; i++)
        {
            float average = 0;
            


            if (i == 16 || i == 32 || i == 40 || i == 48 || i == 56)
            {
                power++;
                sampleCount = (int)Mathf.Pow(2, power) * 2;

                if(power == 3)
                {
                    sampleCount -= 2;
                }
            }


            for (int j = 0; j < sampleCount; j++)
            {

                if (channel == _channel.Stereo)
                {
                    average += (_samplesLeft[count] + _samplesRight[count]) * (count + 1);
                }
                if (channel == _channel.Left)
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

            _freqBand64[i] = average * 80;

        }
    }
}