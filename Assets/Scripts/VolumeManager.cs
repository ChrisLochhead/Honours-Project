using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour {

    public Slider volumeSlider;
    public AudioSource menuMusic;

	void Update () {
        menuMusic.volume = volumeSlider.value;
	}
}
