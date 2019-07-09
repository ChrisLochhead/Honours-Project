using TMPro;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BuildMenu : MonoBehaviour {

    //For the current list of mapitems
    public List<GameObject> mapItems = new List<GameObject>();

    //References to the build menu
    public GameObject openState;
    public GameObject closedState;

    //Variables for the drag and drop functionality
    private bool isDragging = false;
    private Transform objectToDrag;
    private Image dragObjectImage;
    private GameObject dragObjectModel;
    List<RaycastResult> hitObjects = new List<RaycastResult>();
    public GameObject selectedObject;

    //Reference to the save field text object
    public InputField saveField;

    //Reference to the save menu error message text
    public TextMeshProUGUI errorMessage;

    Camera camera;

    //References to the load and save menus
    public GameObject loadMenu;
    public GameObject saveMenu;

    //Reference to map loader and camera
    public BuildMenuMapLoader buildMenuMapLoader;
    public BuildMenuCamera buildMenuCamera;

    // Use this for initialization
    void Start () {
        //Initalise the camera and map loader
        camera = Camera.main;
        buildMenuMapLoader.InitialiseMapFinder();
    }

	void Update () {

        //If clicking down
        if (Input.GetMouseButtonDown(0))
        {
            //Get the corresponding object of the object being clicked on
            objectToDrag = getTransformFromMouse();

            //If an object was found
            if(objectToDrag)
            {
                //Let the system know something is being moved
                isDragging = true;
                objectToDrag.SetAsLastSibling();

                //If the object being clicked is a build button, get its corresponding object
                //and record its place in mapitems before it is added
                if (objectToDrag.GetComponent<Buildbutton>())
                {
                    dragObjectModel = Instantiate(objectToDrag.GetComponent<Buildbutton>().correspondingObject);
                    dragObjectModel.GetComponent<MapItem>().listPlace = mapItems.Count;
                    mapItems.Add(dragObjectModel);
                }
                else
                    dragObjectModel = objectToDrag.gameObject;
            }
        }

        //If an object is currently being dragged
        if (isDragging)
        {
            //Keep the object at a static height while having it follow the cursor
            dragObjectModel.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y, 145));
            Vector3 p = dragObjectModel.transform.position;
            p.z = -5;
            dragObjectModel.transform.position = p;

            //Rotate the object
            if(Input.GetKeyDown("r"))
            {
                dragObjectModel.transform.rotation *= Quaternion.Euler(0, 0, 90);
            }
            //Delete the object
            if (Input.GetKeyDown("t"))
            {
                mapItems.RemoveAt(dragObjectModel.GetComponent<MapItem>().listPlace);
                Destroy(dragObjectModel);
                dragObjectModel = null;
                objectToDrag = null;
                isDragging = false;
            }
        }
        //Reset and let go of object if left mouse is released
        if(Input.GetMouseButtonUp(0))
        {
            if(objectToDrag)
            {
                objectToDrag = null;
                dragObjectModel = null;
            }

            isDragging = false;
        }
    }

    private GameObject SetSelectedObject()
    {
        //Find the object being hovered over
        PointerEventData eventPointer = new PointerEventData(EventSystem.current);
        eventPointer.position = Input.mousePosition;
        EventSystem.current.RaycastAll(eventPointer, hitObjects);

        //If something has been hit
        if (hitObjects.Count <= 0)
        {
            //check if a 3D object being clicked instead
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            //If found a hit return the hit object
            if (Physics.Raycast(ray, out hit))
            {
                return hit.transform.gameObject;
            }
            else
            {
                return null;
            }
        }
        //Otherwise return the first object hit
        return hitObjects[0].gameObject;
    }

    private Transform getTransformFromMouse()
    {
        GameObject clicked = SetSelectedObject();

        //If the hit object is a draggable one, return its transform component.
        if(clicked != null && clicked.tag == "draggable")
        {
            return clicked.transform;
        }
        //Otherwise return nothing
        return null;
    }

    public void ExitButtonPressed()
    {
        //Clear the scene
        Destroy(GameObject.Find("CurrentMapState"));
        buildMenuMapLoader.DeleteMapFinder();

        for (int i = 0; i < mapItems.Count; i++)
        {
            Destroy(mapItems[i]);
        }

        //Return to the main menu
        SceneManager.LoadScene(0);
    }

    public int FindType(GameObject gameObj)
    {
        //Return the object type 
        if (gameObj.name == "RedWall(Clone)")
            return 0;
        else if (gameObj.name == "OrangeWall(Clone)")
            return 1;
        else if (gameObj.name == "GreenWall(Clone)")
            return 2;
        else if (gameObj.name == "GreyWall(Clone)")
            return 3;
        else if (gameObj.name == "GoldCoin(Clone)")
            return 4;
        else if (gameObj.name == "SilverCoin(Clone)")
            return 5;
        else if (gameObj.name == "BronzeCoin(Clone)")
            return 6;
        else if (gameObj.name == "SpawnPoint(Clone)")
            return 7;
        else if (gameObj.name == "SpawnPoint2(Clone)")
            return 8;

        return 0;
    }

    public int FindRot(GameObject gameObj)
    {
        //Finds the objects rotation from file
        //If 90 or 270, rotation is 90, if 180 or 360 then it's 0
        if (gameObj.transform.localEulerAngles.z == 90 || gameObj.transform.localEulerAngles.z == 270)
            return 90;
        else
            return 0;
    }

    private void TriggerSaveError(string t)
    {
        //Throw error message
        errorMessage.text = t;
        errorMessage.gameObject.SetActive(true);
    }

    private bool CheckSpawnPoints()
    {
        //Cycle through objects and make sure at least one of each is a team spawn object.
        int blues = 0;
        int reds = 0;
        for (int i = 0; i < mapItems.Count; i++)
        {
            if (FindType(mapItems[i]) == 7)
            {
                reds++;
            }
            else if (FindType(mapItems[i]) == 8)
            {
                blues++;
            }
        }

        //If there isn't atleast one of each return false
        if (blues == 0 || reds == 0)
            return false;
        else
            return true;
    }

    public void SaveButton()
    {
        //Check file has a name
        if(saveField.text == "")
        {
            TriggerSaveError("Please enter a name for your map");
            return;
        }
        //Check there is actually a map to save
        if(mapItems.Count == 0)
        {
            TriggerSaveError("Cannot save an empty map.");
            return;
        }
        //Check the map has a screenshot
        if(buildMenuCamera.imageTaken == false)
        {
            TriggerSaveError("You must take a screenshot for this map.");
            return;
        }
        //Check the map has enough spawn points
        if (!CheckSpawnPoints())
        {
            TriggerSaveError("You need at least one spawn point for each team.");
            return;
        }

        //Load the screenshot and save it's filepath to the save file
        string filename = saveField.text;
        StreamWriter sr = File.CreateText(Application.dataPath + "/Maps/" + filename + ".txt");
        sr.WriteLine(filename);
        byte[] byteArray = buildMenuCamera.currentImage.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/MapImages/" + filename + ".png", byteArray);
        sr.WriteLine(Application.dataPath + "/MapImages/" + filename + ".png");

        //Save in format all the other map items
        for (int i = 0; i < mapItems.Count; i++)
        {
            if (mapItems[i])
            {
                int t = FindType(mapItems[i]);
                int rot = FindRot(mapItems[i]);
                if (t <= 3)
                {
                    //add in wall format
                    sr.WriteLine(t + "," + mapItems[i].transform.position.x + "," + mapItems[i].transform.position.y + "," + rot);
                }
                else
                {
                    //add in coin format
                    sr.WriteLine(t + "," + mapItems[i].transform.position.x + "," + mapItems[i].transform.position.y);
                }
            }
        }
        //Close the reader and the save menu
        sr.Close();
        CancelSave();

        //Update the map files, remove the error message if it was previously triggered and re-enable camera movement
        buildMenuMapLoader.mapFinderPrefab.GetComponent<MapFinder>().FindFiles();
        errorMessage.gameObject.SetActive(false);
        camera.GetComponent<CameraMovement>().SetMovement(true);
    }

    public void CancelSave()
    {
        //Reset the save menu and disable it
       saveField.text = "";
       errorMessage.gameObject.SetActive(false);
       saveMenu.SetActive(false);
    }

}
