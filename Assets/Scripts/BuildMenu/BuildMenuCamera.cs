﻿using UnityEngine;

public class BuildMenuCamera : MonoBehaviour {

    //For snapshot sound feedback
    public AudioClip snapShotSound;

    //For the shutter effect
    private bool shutterEffectActive = false;
    public GameObject topShutter;
    public GameObject bottomShutter;

    //For taking screenshots
    public Texture2D currentImage;
    private bool takingImage = false;
    public bool imageTaken = false;

    //For preventing its use when menus are open
    private bool isDisabled = false;


    public void ToggleCamera(bool t)
    {
        isDisabled = t;
    }

    private void Update()
    {
        //If in the middle of a shutter
        if (shutterEffectActive)
        {
            ShutterEffect();
        }

        //Check if taking a screenshot
        if (Input.GetKeyDown("c") && !shutterEffectActive && !isDisabled)
        {
            TakeScreenShot();
        }
    }
    private void TakeScreenShot()
    {
        //Create a texture from the main cameras viewport
        Camera.main.targetTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 16);
        //Activate the shutter effect
        takingImage = true;
        shutterEffectActive = true;
        //Play the feedback noise
        bottomShutter.GetComponent<AudioSource>().Play();
    }

    private void ShutterEffect()
    {
        //Just track the bottom shutter, there is no point in tracking both if they move at the same speed
        bool Closed = false;

        if (bottomShutter.GetComponent<RectTransform>().sizeDelta.y <= 475)
        {
            topShutter.GetComponent<RectTransform>().sizeDelta = new Vector2(800, topShutter.GetComponent<RectTransform>().sizeDelta.y + 25);
            bottomShutter.GetComponent<RectTransform>().sizeDelta = new Vector2(800, bottomShutter.GetComponent<RectTransform>().sizeDelta.y + 25);
        }
        else
        {
            Closed = true;
        }

        if(Closed)
        {
            shutterEffectActive = false;
            topShutter.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 0);
            bottomShutter.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 0);
        }
  
    }

    private void OnPostRender()
    {
        //If the screen is being photographed this frame
        if (takingImage)
        {
            //Get the viewport and translate it into a texture
            RenderTexture renderTex = Camera.main.targetTexture;
            currentImage = new Texture2D(renderTex.width, renderTex.height, TextureFormat.ARGB32, false);
            Rect tmpRect = new Rect(0, 0, renderTex.width, renderTex.height);
            currentImage.ReadPixels(tmpRect, 0, 0);

            //Reset everything
            Camera.main.targetTexture = null;
            takingImage = false;
            imageTaken = true;
        }
    }
}
