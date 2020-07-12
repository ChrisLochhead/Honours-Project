using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
public class ClientWeaponManager : NetworkBehaviour {


    //Weapons
    public int currentWeapon = 0;

    public GameObject[] guns;

    public GameObject[] muzzleFlashes;
    public GameObject bullet;
    public GameObject studyBullet;

    //weapon clips
    public int[] maxAmmo = { 54, 40, 120, 300, 15 };
    public int[] currentMaxAmmo = { 54, 40, 120, 300, 15 };
    public int[] clipSize = { 6, 10, 30, 50, 1 };
    public int[] currentAmmo = { 6, 10, 30, 50, 1 };

    //weapon damage and fire rates
    public int[] damageAmounts = { 12, 15, 8, 6, 40 };

    public float[] fireRates = { 1.55f, 1.8f, 0.25f, 0.35f, 2.0f };
    public float[] currentFireRates = { 1.55f, 1.8f, 0.25f, 0.35f, 2.0f };

    public int currentFired = 0;
    public bool hasFired = false;

    //reload timers (in seconds)
    public int[] reloadTimer = { 2, 5, 4, 3, 3 };


    //Reloading and timer
    public bool isReloading = false;
    public bool initialReload = true;
    public float reloadStartTime = 0.0f;
    public float reloadTargetTime = 0.0f;

    //For muzzle flash timing
    float muzzleFlashTimer = 0.15f;
    bool muzzleShot = false;

    Animator weaponAnim;

    public AudioClip[] gunSounds;
    public Client Owner;

    public GameObject grenadePrefab;
    [SerializeField]
    GameObject grenade;
    public int numGrenades = 1;

    public void InitialiseWeapons()
    {
        //check if a study
        if (Owner.isStudy)
            bullet = studyBullet;

        //Initialise current weapon
        currentWeapon = 0;

        //Set guns position
        guns[currentWeapon].transform.position = Owner.player.transform.position;
        guns[currentWeapon].transform.rotation = Owner.player.transform.rotation;

        //Animation code not currently being applied
        weaponAnim = guns[currentWeapon].GetComponent<Animator>();
        weaponAnim.enabled = false;

        //Deactivate all muzzle flashes
        for (int i = 0; i < muzzleFlashes.Length; i++)
            muzzleFlashes[i].SetActive(false);

    }

    [Command]
    void CmdSetWeapon(int type)
    {
        SetWeapon(type);
        RpcSetWeapon(type);
    }

    [ClientRpc]
    public void RpcSetWeapon(int type)
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
    [Command]
    void CmdSpawnGrenade()
    {
        grenade = (GameObject)Instantiate(grenadePrefab, new Vector3(Owner.crosshairMarker.transform.position.x, Owner.crosshairMarker.transform.position.y, -4.5f), Quaternion.identity);
        NetworkServer.Spawn(grenade);

        //calculate rotation
        Quaternion rot = grenade.transform.rotation;
        rot = Owner.player.transform.rotation;
        rot *= Quaternion.Euler(-90, 0, 0);
        grenade.transform.rotation = rot;

        grenade.GetComponent<Grenade>().team = Owner.team;
        grenade.GetComponent<Rigidbody>().velocity = grenade.transform.forward * 1.0f;
    }
    void Update () {

        //Checks for a grenade throw
        if (Input.GetKeyDown("g") && Owner.isLocal && numGrenades > 0 && grenade == null)
        {
            CmdSpawnGrenade();
            numGrenades -= 1;
        }

        //Game is not over, and player has set his name (and therefore has joined the game)
        if (!Owner.hasWon && !Owner.hasLost && !Owner.Paused)
        {

            //If the player can and is shooting, create a bullet, decrement ammo and show a muzzle flash
            if (Input.GetMouseButton(0) && currentAmmo[currentWeapon] > 0 && Owner.isLocal &&
                fireRates[currentWeapon] == currentFireRates[currentWeapon] && !isReloading && !Owner.isDead)
            {
                CmdSpawnBullet();
                currentAmmo[currentWeapon]--;               
                currentFired = currentWeapon;
                muzzleShot = true;
                hasFired = true;
            }

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

            //Timer to remove muzzle flash
            if (muzzleShot)
            {

                muzzleFlashTimer -= Time.deltaTime;

                if (muzzleFlashTimer <= 0.0f)
                {
                    CmdRemoveFlash();
                    muzzleFlashTimer = 0.15f;
                    muzzleShot = false;
                }
            }

            ////Weapon switching
            if (Input.GetKey("1") && Owner.isLocal) CmdSetWeapon(0);
            if (Input.GetKey("2") && Owner.isLocal && Owner.score > 100) CmdSetWeapon(1);
            if (Input.GetKey("3") && Owner.isLocal && Owner.score > 200) CmdSetWeapon(2);
            if (Input.GetKey("4") && Owner.isLocal && Owner.score > 400) CmdSetWeapon(3);
            if (Input.GetKey("5") && Owner.isLocal && Owner.score > 750) CmdSetWeapon(4);

            //Reload sequence
            if (isReloading)
            {
                if (reloadStartTime >= reloadTargetTime)
                {                    
                    //get remaining bullets in the clip
                    int remainingBullets = currentAmmo[currentWeapon];

                    //If the clip isnt already full
                    if (remainingBullets != clipSize[currentWeapon])
                    {
                        UnityEngine.Debug.Log(clipSize[currentWeapon] + remainingBullets);

                        //Fill the clip back up
                        if (currentMaxAmmo[currentWeapon] != 0)
                        {
                            if (currentMaxAmmo[currentWeapon] < clipSize[currentWeapon])
                                currentAmmo[currentWeapon] = currentMaxAmmo[currentWeapon];
                            else
                                currentAmmo[currentWeapon] = clipSize[currentWeapon];

                            //Remove the appropriate number of bullets from the max ammo
                            currentMaxAmmo[currentWeapon] += -clipSize[currentWeapon] + remainingBullets;
                        }
                        else
                            currentAmmo[currentWeapon] = remainingBullets;

                    }

                    if (currentMaxAmmo[currentWeapon] < 0)
                        currentMaxAmmo[currentWeapon] = 0;

                    reloadStartTime = 0.0f;
                    isReloading = false;
                    initialReload = true;
                }
                else
                {
                    reloadStartTime = Time.time;
                }
            }

            //Cancel reload if reloading mid-weapon switch
            if (Input.GetKey("1") || Input.GetKey("2") || Input.GetKey("3") || Input.GetKey("4") || Input.GetKey("5"))
            {
                isReloading = false;
                initialReload = true;
            }

            //Reloading
            if (Input.GetKey("r") && initialReload == true || currentAmmo[currentWeapon] == 0 && initialReload == true  && currentMaxAmmo[currentWeapon] > 0 || Input.GetKey("r") && isReloading == false)
            {
                reloadStartTime = Time.time;
                reloadTargetTime = reloadStartTime + reloadTimer[currentWeapon];
                isReloading = true;
                initialReload = false;
            }
        }
    }
    [Command]
    public void CmdSpawnBullet()
    {
        GameObject b = (GameObject)Instantiate(bullet, new Vector3(Owner.crosshairMarker.transform.position.x, Owner.crosshairMarker.transform.position.y, -4.5f), Quaternion.identity);
        NetworkServer.Spawn(b);
        //calculate rotation
        Quaternion rot = b.transform.rotation;
        rot = Owner.player.transform.rotation;
        rot *= Quaternion.Euler(-90, 0, 0);
        b.transform.rotation = rot;

        //calculate trajectory
        b.GetComponent<Rigidbody>().velocity = b.transform.forward * 1.0f;
        b.GetComponent<Bullet>().shooter = Owner.player;
        b.GetComponent<Bullet>().shooterID = 16;

        b.GetComponent<Bullet>().damageAmount = damageAmounts[currentWeapon];

        MuzzleFlash(true);
        RpcMuzzleFlash(true);
    }

    [ClientRpc]
    public void RpcSpawnBullet(GameObject b)
    {
        b.GetComponent<Bullet>().shooter = Owner.player;
    }
    [Command]
    public void CmdRemoveFlash()
    {
        MuzzleFlash(false);
        RpcMuzzleFlash(false);
    }

    public void MuzzleFlash(bool istrue)
    {
        //Create muzzle flash and play gunshot sound
        muzzleFlashes[currentWeapon].SetActive(istrue);
        if (isLocalPlayer && istrue == true)
        {
            Owner.gameObject.GetComponent<AudioSource>().clip = gunSounds[currentWeapon];
            Owner.gameObject.GetComponent<AudioSource>().Play();
        }
    }

    [ClientRpc]
    public void RpcMuzzleFlash(bool istrue)
    {
        //Create muzzle flash and play gunshot sound
        muzzleFlashes[currentWeapon].SetActive(istrue);
        if (isLocalPlayer && istrue == true)
        {
            Owner.gameObject.GetComponent<AudioSource>().clip = gunSounds[currentWeapon];
            Owner.gameObject.GetComponent<AudioSource>().Play();
        }
    }
}
