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
            foreach(TextMeshProUGUI t in titles)
            {
                t.color = new Color( t.color.r, t.color.g, t.color.b, t.color.a - 0.035f);
                if(t.color.a < 0.0f)
                   t.color = new Color(t.color.r, t.color.g, t.color.b, 0.0f);
            }

            if(titles[0].color.a == 0.0f)
            {
                mainmenu.SetActive(true);
                this.gameObject.SetActive(false);
            }
        }

	}
}
