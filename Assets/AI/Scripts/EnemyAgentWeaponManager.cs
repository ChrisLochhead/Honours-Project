using UnityEngine;
public class EnemyAgentWeaponManager : MonoBehaviour
{

    //Weapons
    public int currentWeapon = 0;

    public GameObject[] guns;
    public GameObject bullet;

    //weapon clips
    public int[] clipSize = { 16, 10, 30, 50, 1 };
    public int[] currentAmmo = { 16, 10, 30, 50, 1 };

    //weapon damage and fire rates
    public int[] damageAmounts = { 30, 60, 16, 24, 85 };

    public float[] fireRates = { 0.75f, 1.8f, 0.25f, 0.35f, 2.0f };
    public float[] currentFireRates = { 0.75f, 1.8f, 0.25f, 0.35f, 2.0f };

    public int currentFired = 0;
    public bool hasFired = false;

    //reload timers (in seconds)
    public int[] reloadTimer = { 2, 5, 4, 3, 3 };


    //Reloading and timer
    public bool isReloading;
    public bool initialReload;
    public float reloadStartTime = 0.0f;
    public float reloadTargetTime = 0.0f;

    public EnemyAgentController controller;
    public GameObject crosshairMarker;

    public void InitialiseWeapons()
    {
        //Initialise current weapon
        currentWeapon = 0;

        //Set guns position
        guns[currentWeapon].transform.position = gameObject.transform.position;
        guns[currentWeapon].transform.rotation = gameObject.transform.rotation;


    }

    public void SetWeapon(int type)
    {
        //Activate the correct weapon, and deactivate all the others
        currentWeapon = type;
        for (int i = 0; i < guns.Length; i++)
        {
            if (i == type)
                guns[i].SetActive(true);
            else
                guns[i].SetActive(false);
        }
    }

    public void Shoot(float action)
    {       
        //If the player can and is shooting, create a bullet, decrement ammo and show a muzzle flash
        if (currentAmmo[currentWeapon] > 0 && fireRates[currentWeapon] == currentFireRates[currentWeapon] && action > 0 && isReloading == false)
        {
            SpawnBullet();
            currentAmmo[currentWeapon]--;
            currentFired = currentWeapon;
            hasFired = true;
        }
    }

    public void Reload(float action)
    {
        //Reloading
        if (action > 0 && (float)currentAmmo[currentWeapon] / (float)clipSize[currentWeapon] < 0.6f)
        {
            if (initialReload == true || isReloading == false)
            {
                reloadStartTime = Time.time;
                reloadTargetTime = reloadStartTime + reloadTimer[currentWeapon];
                isReloading = true;
                initialReload = false;
            }
        }
    }

    public void SwitchWeapon(float weaponIndex)
    {
        if (weaponIndex > 0)
        {
            //If actually changing to a different weapon
            if(currentWeapon == 0 && weaponIndex <= 0.2f || currentWeapon == 1 && weaponIndex <= 0.4f && weaponIndex > 0.20f || currentWeapon == 2 && weaponIndex <= 0.6f && weaponIndex > 0.4f ||
                currentWeapon == 3 && weaponIndex <= 0.8f && weaponIndex >= 0.6f || currentWeapon == 4 && weaponIndex > 0.8f)
            {
                //Cancel attempt at reloading 
                //isReloading = false;
                //initialReload = true;
            }
            //Cycle through weapons
            if (currentWeapon == 0 && weaponIndex <= 0.2f) SetWeapon(0);
            if (currentWeapon == 1 && weaponIndex > 0.2f && weaponIndex <= 0.4f && controller.score > 100) SetWeapon(1);
            if (currentWeapon == 2 && weaponIndex > 0.4f && weaponIndex <= 0.6f && controller.score > 200) SetWeapon(2);
            if (currentWeapon == 3 && weaponIndex > 0.6f && weaponIndex <= 0.8f && controller.score > 400) SetWeapon(3);
            if (currentWeapon == 4 && weaponIndex > 0.8f && controller.score > 750) SetWeapon(4);

        }
    }

    void CycleWeapon(int w, int s)
    {
        if (currentWeapon == w-1 && controller.score > 100) SetWeapon(1);
    }

    void Update()
    {
            //Simulates fire rate by preventing player shooting repeatedly
            if (hasFired)
            {
                if (currentFireRates[currentFired] > 0)
                {
                    currentFireRates[currentFired] -= Time.deltaTime;
                }
                else
                {
                    currentFireRates[currentFired] = fireRates[currentFired];
                    hasFired = false;
                }

            }

        if (currentAmmo[currentWeapon] == 0 && initialReload == true && isReloading == false)
            Reload(1);

        //Reload sequence
        if (isReloading)
        {
            if (reloadStartTime >= reloadTargetTime)
            {
                currentAmmo[currentWeapon] = clipSize[currentWeapon];
                reloadStartTime = 0.0f;
                isReloading = false;
                initialReload = true;
            }
            else
            {
                reloadStartTime = Time.time;
            }
        }      
    }

    public void SpawnBullet()
    {
        GameObject b = (GameObject)Instantiate(bullet, new Vector3(crosshairMarker.transform.position.x, crosshairMarker.transform.position.y, -4.5f), Quaternion.identity);

        //calculate rotation
        Quaternion rot = b.transform.rotation;
        rot = gameObject.transform.rotation;
        rot *= Quaternion.Euler(-90, 0, 0);
        b.transform.rotation = rot;

        //calculate trajectory
        b.GetComponent<Rigidbody>().velocity = b.transform.forward * 100.0f;

        //add tag indicating whose bullet it is
        b.GetComponent<TrainingBullet>().shooter = gameObject;
        //damageAmounts[currentWeapon] = 100;
        b.GetComponent<TrainingBullet>().damageAmount = 0;
        // damageAmounts[currentWeapon];

    }
}
