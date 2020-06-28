using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using TMPro;

public class MainMenu : NetworkBehaviour {

    //Multiplayer dropdowns and input fields
    public TMP_InputField nameInputHostMultiplayer;
    public TMP_InputField nameInputJoinMultiplayer;
    public Dropdown killLimitDropdownMultiplayer;
    public Dropdown timeLimitDropdownMultiplayer;

    //LAN dropdowns and input fields
    public TMP_InputField nameInputHostLAN;
    public TMP_InputField nameInputJoinLAN;
    public Dropdown killLimitDropdownLAN;
    public Dropdown timeLimitDropdownLAN;

    public TMP_InputField nameInputHostStudy;
    public TMP_InputField nameInputJoinStudy;
    public Dropdown killLimitDropdownStudy;
    public Dropdown timeLimitDropdownStudy;

    public TMP_InputField demoNameInput;

    //Reference to the network manager
    private NetworkManager networkManager;

    //Reference to the mapfinder
    public GameObject mapFinderPrefab;

    //Name entry error message
    public TextMeshProUGUI errorMessage;

    AudioSource audioSource;
    public AudioClip buttonClick;

    //Introduction menu
    bool startingAnimation = false;
    public GameObject buttonContainer;
    public GameObject leftBar, rightBar;

    //Background reference
    public Image backgroundImage;
    bool backgroundIsChanging = false;
    Color targetColor;
    float lerpTime = 0.0f;

    //Scene changing transition variables
    bool sceneIsChanging = false;
    public Transform canvas;
    float sceneChangeTimer = 5.0f;

    private void Start()
    {
        //Get the attached AudioSource
        audioSource = GetComponent<AudioSource>();

        //Create a map finder and find all the available 
        InitialiseMapFinder();

        //Set up networking
        networkManager = NetworkManager.singleton;

    }

    public void ChangeBackground(string newValue)
    {
        ColorUtility.TryParseHtmlString(newValue, out targetColor);
        backgroundIsChanging = true;
    }

    public void ChangeSceneTransition()
    {
        sceneIsChanging = true;
    }
    public void PlayClickAudio()
    {
        audioSource.PlayOneShot(buttonClick, 0.3f);
    }
    
    public void StudyButton()
    {
        //Enable multiplayer
        if (networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }

        //Disable any error message if applicable
        errorMessage.gameObject.SetActive(false);

        //Store info for the next scene
        GameObject gameInfo = new GameObject();
        gameInfo.AddComponent<GameInfo>();
        gameInfo.name = "gameInfo";
        gameInfo.GetComponent<GameInfo>().infoName = nameInputHostStudy.text;
        gameInfo.GetComponent<GameInfo>().isStudy = true;

        //Assign the kill and time limit based on value
        gameInfo.GetComponent<GameInfo>().killLimit = (int)getDropdownValue(true, killLimitDropdownStudy.value);
        gameInfo.GetComponent<GameInfo>().timeLimit = getDropdownValue(false, timeLimitDropdownStudy.value);

        //Start a local game
        networkManager.matchMaker.CreateMatch(nameInputHostMultiplayer.text + "'s room", 4, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
    }

    public void JoinStudyButton()
    {
        //Enable multiplayer
        if (networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }

        //If the name isn't empty
        if (nameInputJoinStudy.text != "")
        {
            //Disable any applicable error message
            errorMessage.gameObject.SetActive(false);

            //Store info for the next scene
            GameObject gameInfo = new GameObject();
            gameInfo.AddComponent<GameInfo>();
            gameInfo.name = "gameInfo";
            gameInfo.GetComponent<GameInfo>().infoName = nameInputJoinStudy.text;
            gameInfo.GetComponent<GameInfo>().isStudy = true;
            DeleteMapFinder();

            //Join local game as client
            networkManager.matchMaker.ListMatches(0, 20, "", false, 0, 0, OnMatchList);
        }
        else
            errorMessage.gameObject.SetActive(true);
    }

    public void InitialiseDemo()
    {
        //Store info for the next scene
        GameObject gameInfo = new GameObject();
        gameInfo.AddComponent<GameInfo>();
        gameInfo.name = "gameInfo";
        gameInfo.GetComponent<GameInfo>().infoName = nameInputJoinStudy.text;
        gameInfo.GetComponent<GameInfo>().demoName = demoNameInput.text;

        SceneManager.LoadScene(4);
    }
    public void DisableErrorMessage()
    {
        errorMessage.gameObject.SetActive(false);
    }

    public void InitialiseMapFinder()
    {
        //Initialise prefab but dont add to network
        if(!GameObject.Find("MapFinder(Clone)"))
        Instantiate(mapFinderPrefab);
    }

    public void DeleteMapFinder()
    {
        Destroy(GameObject.Find("MapFinder(Clone)"));
    }

    private float getDropdownValue(bool isKillLimit, int value)
    {
        //Return the value based on which type of dropdown, and said dropdowns value
        if (isKillLimit)
        {
            if (value == 0)
                return 5;
            else if (value == 1)
                return 15;
            else
                return 60;
        }
        else
        {
            if (value == 0)
                return 10;
            else if (value == 1)
                return 15;
            else
                return 30;
        }
    }

    public void HostButton()
    {
        if (nameInputHostMultiplayer.text != "")
        {
            //Disable the error message if applicable
            errorMessage.gameObject.SetActive(false);

            //Destroy any previously loaded games
            if (GameObject.Find("game"))
            {
                Destroy(GameObject.Find("game"));
            }

            //Store info for the next scene
            GameObject gameInfo = new GameObject();
            gameInfo.AddComponent<GameInfo>();
            gameInfo.name = "gameInfo";
            gameInfo.GetComponent<GameInfo>().infoName = nameInputHostMultiplayer.text;

            //Assign the kill and time limit based on value
            gameInfo.GetComponent<GameInfo>().killLimit = (int)getDropdownValue(true, killLimitDropdownMultiplayer.value);
            gameInfo.GetComponent<GameInfo>().timeLimit = getDropdownValue(false, timeLimitDropdownMultiplayer.value);

            //Create a match
            networkManager.matchMaker.CreateMatch(nameInputHostMultiplayer.text + "'s room", 4, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
        }
        else
            errorMessage.gameObject.SetActive(true);
    }

    public void JoinButton()
    {
        //Join game if name isn't empty
        if (nameInputJoinMultiplayer.text != "")
        {
            //Remove error message if applicable
            errorMessage.gameObject.SetActive(false);

            //Only store name info when joining a game
            GameObject gameInfo = new GameObject();
            gameInfo.AddComponent<GameInfo>();
            gameInfo.name = "gameInfo";
            gameInfo.GetComponent<GameInfo>().infoName = nameInputJoinMultiplayer.text;

            DeleteMapFinder();

            //Join the match
            networkManager.matchMaker.ListMatches(0, 20, "", false, 0, 0, OnMatchList);
        }
        else
            errorMessage.gameObject.SetActive(true);
    }

    public void HostButtonLAN()
    {
        //If the name input isn't empty
        if (nameInputHostLAN.text != "")
        {
            //Disable any error message if applicable
            errorMessage.gameObject.SetActive(false);

            //Store info for the next scene
            GameObject gameInfo = new GameObject();
            gameInfo.AddComponent<GameInfo>();
            gameInfo.name = "gameInfo";
            gameInfo.GetComponent<GameInfo>().infoName = nameInputHostLAN.text;

            //Assign the kill and time limit based on value
            gameInfo.GetComponent<GameInfo>().killLimit = (int)getDropdownValue(true, killLimitDropdownLAN.value);
            gameInfo.GetComponent<GameInfo>().timeLimit = getDropdownValue(false, timeLimitDropdownLAN.value);

            //Start a local game
            networkManager.StartHost();
        }
        else
            errorMessage.gameObject.SetActive(true);
    }

    public void JoinButtonLAN()
    {
        //If the name isn't empty
        if (nameInputJoinLAN.text != "")
        {
            //Disable any applicable error message
            errorMessage.gameObject.SetActive(false);

            //Store info for the next scene
            GameObject gameInfo = new GameObject();
            gameInfo.AddComponent<GameInfo>();
            gameInfo.name = "gameInfo";
            gameInfo.GetComponent<GameInfo>().infoName = nameInputJoinLAN.text;
            gameInfo.GetComponent<GameInfo>().isStudy = false;

            DeleteMapFinder();

            //Join local game as client
            networkManager.StartClient();
        }
        else
            errorMessage.gameObject.SetActive(true);
    }

    public void SelectLAN()
    {
        //Disable multiplayer
        if (networkManager.matchMaker != null)
        {
            networkManager.matchMaker = null;
        }
    }

    public void SelectMultiPlayer()
    {
        //Enable multiplayer
        if (networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }
    }


    public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        MatchInfoSnapshot priorityMatch = new MatchInfoSnapshot();

        //Find match closest to being full
        foreach (MatchInfoSnapshot match in matches)
        {
            if(match.currentSize < match.maxSize && priorityMatch.currentSize < match.currentSize)
            {
                priorityMatch = match;
            }
        }
        //Join this match
        networkManager.matchMaker.JoinMatch(priorityMatch.networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);
    }

    public void BuildButton()
    {
        //Destroy the main menus copy of mapfinder
        Destroy(GameObject.Find("MapFinder(Clone)"));
        SceneManager.LoadScene(2);
    }

    public void QuitButton()
    {
        //Close the application
        Application.Quit();
    }

    private void Update()
    {
        //For starting animation
        if(!startingAnimation && buttonContainer.transform.parent.gameObject.active == true)
        {
            //Menu side bar transition
            if (leftBar.transform.localPosition.x < -370)
                leftBar.transform.localPosition = new Vector3(leftBar.transform.localPosition.x + 5.5f, 0, 0);
            else
                leftBar.transform.localPosition = new Vector3(-370, 0, 0);

            if (rightBar.transform.localPosition.x > 370)
                rightBar.transform.localPosition = new Vector3(rightBar.transform.localPosition.x - 5.5f, 0, 0);
            else
                rightBar.transform.localPosition = new Vector3(370, 0, 0);

            //Menu buttons transition
            if (buttonContainer.transform.localPosition.y > 0)           
                buttonContainer.transform.localPosition = new Vector3(0, buttonContainer.transform.localPosition.y - 5.5f, 0);   
            else
                buttonContainer.transform.localPosition = Vector3.zero;

            if (leftBar.transform.localPosition.x == -370 && rightBar.transform.localPosition.x == 370 && buttonContainer.transform.localPosition.y == 0)
                startingAnimation = true;
        }

        //For background transitioning
        if(backgroundIsChanging)
        {
            backgroundImage.color = Color.Lerp(backgroundImage.color, targetColor, lerpTime);
            lerpTime += Time.deltaTime;
            if (backgroundImage.color == targetColor)
            {
                backgroundIsChanging = false;
                lerpTime = 0.0f;
            }
        }

        if (sceneChangeTimer <= 0.0f)
            sceneIsChanging = false;
        else
            sceneChangeTimer -= Time.deltaTime;

        //For scene transition
        if (sceneIsChanging)
        {
            int counter = 0;
            for(int i = 0; i < canvas.transform.childCount; i++)
            {
                if(canvas.GetChild(i).transform.childCount >0 && i != 0)
                {
                    for(int j = 0; j < canvas.GetChild(i).transform.childCount; j++)
                    {

                        if (canvas.GetChild(i).GetChild(j).transform.childCount > 0)
                        {
                            for (int k = 0; k < canvas.GetChild(i).GetChild(j).transform.childCount; k++)
                            {
                                Transform gj = canvas.GetChild(i).GetChild(j).GetChild(k);
                                if (counter % 2 == 0)
                                    gj.transform.localPosition = new Vector3(gj.transform.localPosition.x + 10, gj.transform.localPosition.y, gj.transform.localPosition.z);
                                else
                                    gj.transform.localPosition = new Vector3(gj.transform.localPosition.x - 10, gj.transform.localPosition.y, gj.transform.localPosition.z);
                                counter++;
                            }
                        }
                        else
                        {
                            Transform gi = canvas.GetChild(i).GetChild(j);
                            if (counter % 2 == 0)
                                gi.transform.localPosition = new Vector3(gi.transform.localPosition.x + 10, gi.transform.localPosition.y, gi.transform.localPosition.z);
                            else
                                gi.transform.localPosition = new Vector3(gi.transform.localPosition.x - 10, gi.transform.localPosition.y, gi.transform.localPosition.z);
                            counter++;
                        }
                    }
                }else if (counter != 0)
                {
                    Transform g = canvas.GetChild(i);
                    if (counter % 2 == 0)
                        g.transform.localPosition = new Vector3(g.transform.localPosition.x + 10, g.transform.localPosition.y, g.transform.localPosition.z);
                    else
                        g.transform.localPosition = new Vector3(g.transform.localPosition.x - 10, g.transform.localPosition.y, g.transform.localPosition.z);
                    counter++;
                }else
                counter++;
            }
        }

    }

}
