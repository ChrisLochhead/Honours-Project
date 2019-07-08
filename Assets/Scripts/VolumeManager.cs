using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour {

    //Reference to the slider in the settings menu
    public Slider volumeSlider;
    //Audio source for the main menu
    public AudioSource menuMusic;

	void Update () {
        //Update the volume according to the slider.
        menuMusic.volume = volumeSlider.value;
	}
}
