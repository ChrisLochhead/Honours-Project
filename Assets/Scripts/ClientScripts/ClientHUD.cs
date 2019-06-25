using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ClientHUD : MonoBehaviour {

    //HUD object references
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI healthText;

    public GameObject healthBar;
    public GameObject rankImage;

    public Sprite[] rankIcons;
    public int[] rankHealthValues;

    public Client Owner;
	
	// Update is called once per frame
	void Update () {

        //Update the HUD
        //Health
        healthBar.GetComponent<Slider>().maxValue = rankHealthValues[Owner.rank];
        healthBar.GetComponent<Slider>().value = Owner.health;
        healthText.text = Owner.health.ToString() + "/" + rankHealthValues[Owner.rank];

        //Score
        scoreText.text = Owner.score.ToString();

        //Ammo
        ammoText.text = Owner.clientWeaponManager.currentAmmo[Owner.clientWeaponManager.currentWeapon].ToString() + "/" + Owner.clientWeaponManager.clipSize[Owner.clientWeaponManager.currentWeapon];

        //Rank
        rankImage.GetComponent<Image>().sprite = rankIcons[Owner.rank];

    }
}
