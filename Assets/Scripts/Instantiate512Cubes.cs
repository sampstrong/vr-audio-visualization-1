using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instantiate512Cubes : MonoBehaviour
{
    public GameObject _sampleCubePrefab;
    private GameObject[] _sampleCube = new GameObject[512];

    private AudioSpectrumReader _audioSpectrumReader;

    public float _maxScale;
    public float _circleRadius = 300;
    // Start is called before the first frame update
    void Start()
    {
        _audioSpectrumReader = GameObject.Find("AudioPlayer").GetComponent<AudioSpectrumReader>();

        for (int i = 0; i < 512; i++)
        {
            GameObject _instantiateSampleCube = (GameObject)Instantiate(_sampleCubePrefab);
            _instantiateSampleCube.transform.position = this.transform.position;
            _instantiateSampleCube.transform.parent = this.transform;
            _instantiateSampleCube.name = "Sample Cube" + i;
            this.transform.eulerAngles = new Vector3(0, -0.703125f * i, 0);
            _instantiateSampleCube.transform.position = Vector3.forward * _circleRadius;
            _sampleCube[i] = _instantiateSampleCube;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < 512; i++)
        {
            if (_sampleCube != null)
            {
                _sampleCube[i].transform.localScale = new Vector3(1, (_audioSpectrumReader._samplesLeft[i] + _audioSpectrumReader._samplesRight[i]) * _maxScale + 2, 1);
            }
        }
    }
}
