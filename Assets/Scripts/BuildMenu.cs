using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;



public class BuildMenu : MonoBehaviour {

    public List<GameObject> mapItems = new List<GameObject>();

    public GameObject openState;
    public GameObject closedState;

    public const string draggable = "draggable";

    private bool isDragging = false;

    private Vector2 originalPos;

    private Transform objectToDrag;
    private Image dragObjectImage;
    private GameObject dragObjectModel;

    List<RaycastResult> hitObjects = new List<RaycastResult>();

    public GameObject selectedObject;

    int currentState;

    public Sprite[] buttons;

    public InputField saveField;

    public bool isSaving = false;

    //templates
    public GameObject redWall;
    public GameObject orangeWall;
    public GameObject greenWall;
    public GameObject greyWall;
    public GameObject goldCoin;
    public GameObject silverCoin;
    public GameObject bronzeCoin;

    // Use this for initialization
    void Start () {
        currentState = 0;
	}
	
	// Update is called once per frame
	void Update () {

        Debug.Log(mapItems.Count);

        if(Input.GetMouseButtonDown(0))
        {
            objectToDrag = getTransformFromMouse();

            if(objectToDrag)
            {
                isDragging = true;

                objectToDrag.SetAsLastSibling();

                originalPos = objectToDrag.position;
                if (objectToDrag.GetComponent<Buildbutton>())
                {
                    dragObjectModel = Instantiate(objectToDrag.GetComponent<Buildbutton>().correspondingObject);
                    dragObjectModel.tag = "draggable";
                    dragObjectModel.GetComponent<mapItem>().listPlace = mapItems.Count;
                    dragObjectModel.GetComponent<mapItem>().type = dragObjectModel.name;
                    mapItems.Add(dragObjectModel);
                }
                else
                    dragObjectModel = objectToDrag.gameObject;

            }
        }

        if (isDragging)
        {

            dragObjectModel.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y, 145));
            Vector3 p = dragObjectModel.transform.position;
            p.z = -5;
            dragObjectModel.transform.position = p;

            if(Input.GetKeyDown("r"))
            {
                dragObjectModel.transform.rotation *= Quaternion.Euler(0, 0, 90);
            }
            if (Input.GetKeyDown("d"))
            {
                mapItems.RemoveAt(dragObjectModel.GetComponent<mapItem>().listPlace);
                Destroy(dragObjectModel);
                dragObjectModel = null;
                objectToDrag = null;
                isDragging = false;
            }
        }

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

    public void CloseClicked()
    {
        if (currentState == 0)
        {
            openState.SetActive(true);
            closedState.SetActive(false);
            currentState = 1;
        }
        else
        {
            openState.SetActive(false);
            closedState.SetActive(true);
            currentState = 0;
        }
    }

    private GameObject SetSelectedObject()
    {
        PointerEventData eventPointer = new PointerEventData(EventSystem.current);

        eventPointer.position = Input.mousePosition;

        EventSystem.current.RaycastAll(eventPointer, hitObjects);

        if (hitObjects.Count <= 0)
        {
            //check if a 3D object being clicked instead
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                return hit.transform.gameObject;
            }
            else
            {
                return null;
            }
        }

        return hitObjects[0].gameObject;
    }

    private Transform getTransformFromMouse()
    {
        GameObject clicked = SetSelectedObject();

        if(clicked != null && clicked.tag == draggable)
        {
            return clicked.transform;
        }

        return null;
    }

    public void ExitButtonPressed()
    {
        SceneManager.LoadScene(0);
    }

    public void LoadButtonPressed()
    {

    }

    public int FindType(GameObject gameObj)
    {
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

                return 0;
    }

    public int FindRot(GameObject gameObj)
    {
        //if 90 or 270 its 90, if 180 or 360 its 0
        if (gameObj.transform.localEulerAngles.z == 90 || gameObj.transform.localEulerAngles.z == 270)
            return 90;
        else
            return 0;
    }
    public void SaveButton()
    {
        string filename = saveField.text;
        StreamWriter sr = File.CreateText(Application.dataPath + "/Maps/" + filename + ".txt");
        sr.WriteLine(filename);
        sr.WriteLine("1,1");
        
        for(int i = 0; i < mapItems.Count; i++)
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
        sr.Close();
        CancelSave();
    }

    public void CancelSave()
    {
       saveField.text = "";
       GameObject.Find("Save Menu").SetActive(false);

    }

    public void LoadMap()
    {

        //clear first
        Destroy(GameObject.Find("CurrentMapState"));

        GameObject newState = new GameObject();
        newState.name = "CurrentMapState";



        List<GameObject> mapInfo = GameObject.Find("PersistentObject").GetComponent<MapFinder>().selectedMap.GetComponent<Map>().GetMapItems();


        for (int i = 0; i < mapInfo.Count; i++)
        {
            GameObject tmp = new GameObject();

            if (mapInfo[i].GetComponent<Wall>())
            {
                if (mapInfo[i].GetComponent<Wall>().type == 0)
                {
                    tmp = Instantiate(redWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, mapInfo[i].GetComponent<Wall>().rot));
                }
                else
                if (mapInfo[i].GetComponent<Wall>().type == 1)
                {
                    tmp = Instantiate(orangeWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, mapInfo[i].GetComponent<Wall>().rot));
                }
                else
                if (mapInfo[i].GetComponent<Wall>().type == 2)
                {
                    tmp = Instantiate(greenWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, mapInfo[i].GetComponent<Wall>().rot));
                }
                if (mapInfo[i].GetComponent<Wall>().type == 3)
                {
                    tmp = Instantiate(greyWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, mapInfo[i].GetComponent<Wall>().rot));
                }
            }
            else
                if (mapInfo[i].GetComponent<Coin>().type == 4)
            {
                tmp = Instantiate(goldCoin, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity);
            }
            else
                if (mapInfo[i].GetComponent<Coin>().type == 5)
            {
                tmp = Instantiate(silverCoin, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity);
            }
            else
                if (mapInfo[i].GetComponent<Coin>().type == 6)
            {
                tmp = Instantiate(bronzeCoin, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity);
            }

            tmp.transform.parent = newState.transform;
        }
    }
}
