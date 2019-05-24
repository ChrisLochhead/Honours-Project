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

    public void SaveButton()
    {
        string filename = saveField.text;
        StreamWriter sr = File.CreateText(Application.dataPath + "/Maps/" + filename + ".txt");
        sr.WriteLine(filename);
        sr.WriteLine("1,1");
        sr.Close();
        
    }

    public void CancelSave()
    {
       saveField.text = "";
       GameObject.Find("Save Menu").SetActive(false);

    }
}
