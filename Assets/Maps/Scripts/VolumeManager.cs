using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour {

    public Slider volumeSlider;
    public AudioSource menuMusic;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        menuMusic.volume = volumeSlider.value;
	}
}
