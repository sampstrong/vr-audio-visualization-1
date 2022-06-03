using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    private AudioSource _audioPlayer;

    

    private void Start()
    {
        _audioPlayer = GameObject.Find("AudioPlayer").GetComponent<AudioSource>();
    }

    public void PlayAudio()
    {
        _audioPlayer.Play();
    }


    public void PauseAudio()
    {
        _audioPlayer.Pause();
    }

}
