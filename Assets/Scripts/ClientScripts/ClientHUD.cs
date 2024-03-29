﻿using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ClientHUD : MonoBehaviour
{

    //HUD object references
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI timerText;

    public GameObject healthBar;
    public GameObject rankImage;

    public Sprite[] rankIcons;
    public int[] rankHealthValues;

    public GameObject floatingCanvas;
    public Client Owner;

    public TextMeshProUGUI timeTillNextAgent;
    public TextMeshProUGUI currentAgent;

    StudyManager studyManager;
    private void Start()
    {
        if (Owner.isStudy)
        {
            currentAgent.gameObject.SetActive(true);
            timeTillNextAgent.gameObject.SetActive(true);
            studyManager = GameObject.Find("StudyManager").GetComponent<StudyManager>();
        }
    }
    // Update is called once per frame
    void Update()
    {

        //Update the HUD
        //Health
        if (Owner.isStudy)
        {
            floatingCanvas.SetActive(false);
            currentAgent.text =  "Agent : " + (studyManager.currentEnemyIndex + 1).ToString();

            if (studyManager.CurrentEnemy == null)
                timeTillNextAgent.text = "Time Till Next Agent : " + Mathf.RoundToInt(studyManager.respawnTimer);
            else
                timeTillNextAgent.text = "";

        }

        //Update Study related HUD

        healthBar.GetComponent<Slider>().maxValue = rankHealthValues[Owner.rank];
        healthBar.GetComponent<Slider>().value = Owner.health;
        healthText.text = Owner.health.ToString() + "/" + rankHealthValues[Owner.rank];
        timerText.text = Mathf.Floor(Owner.timeLimit / 60) + ":" + Mathf.RoundToInt(Owner.timeLimit % 60);
        

        //Score
        scoreText.text = Owner.score.ToString();

        //Ammo
        ammoText.text = Owner.clientWeaponManager.currentAmmo[Owner.clientWeaponManager.currentWeapon].ToString() + "/" + Owner.clientWeaponManager.currentMaxAmmo[Owner.clientWeaponManager.currentWeapon];

        //Rank
        rankImage.GetComponent<Image>().sprite = rankIcons[Owner.rank];

    }

}
