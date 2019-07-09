using UnityEngine;

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


    private void Update()
    {
        //If in the middle of a shutter
        if (shutterEffectActive)
        {
            ShutterEffect();
        }

        //Check if taking a screenshot
        if (Input.GetKeyDown("c") && !shutterEffectActive)
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
        bool bsClosed = false;

        //If the bottom shutter hasn't reached its termination point
        if (bottomShutter.GetComponent<RectTransform>().position.y < 104.0f)
        {
            //Update the bottom shutter
            Vector3 tmp = bottomShutter.GetComponent<RectTransform>().position;
            tmp.y += 25;
            bottomShutter.GetComponent<RectTransform>().position = tmp;

            //Then update the top
            tmp = topShutter.GetComponent<RectTransform>().position;
            tmp.y -= 25;
            topShutter.GetComponent<RectTransform>().position = tmp;
        }
        else
        {
            bsClosed = true;
        }

        //If the shutter is fully closed
        if (bsClosed)
        {
            //Restore
            Vector3 topTmp = topShutter.GetComponent<RectTransform>().position;
            topTmp.y = 561;
            topShutter.GetComponent<RectTransform>().position = topTmp;

            //Restore
            Vector3 bottomTmp = bottomShutter.GetComponent<RectTransform>().position;
            bottomTmp.y = -140;
            bottomShutter.GetComponent<RectTransform>().position = bottomTmp;

            //And then end routine
            shutterEffectActive = false;

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
