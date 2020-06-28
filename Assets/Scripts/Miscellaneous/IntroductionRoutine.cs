using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IntroductionRoutine : MonoBehaviour {
    // Update is called once per frame
    public GameObject mainmenu;
    public TextMeshProUGUI [] titles;
    bool startRoutineInitialised = false;
	void Update () {
		if(Input.GetKeyDown("space"))
        {
            startRoutineInitialised = true;
        }

        if(startRoutineInitialised)
        {
            Debug.Log("calling in here");
            foreach(TextMeshProUGUI t in titles)
            {
                Debug.Log(titles[0].color);
                t.color = new Color( t.color.r, t.color.g, t.color.b, t.color.a - 0.035f);
                if(t.color.a < 0.0f)
                   t.color = new Color(t.color.r, t.color.g, t.color.b, 0.0f);

                Debug.Log(titles[0].color);
            }

            if(titles[0].color.a == 0.0f)
            {
                mainmenu.SetActive(true);
                this.gameObject.SetActive(false);
            }

        }

	}
}
