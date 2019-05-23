using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class BuildMenu : MonoBehaviour {

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

	// Use this for initialization
	void Start () {
        currentState = 0;
	}
	
	// Update is called once per frame
	void Update () {
		
        if(Input.GetMouseButtonDown(0))
        {
            objectToDrag = getTransformFromMouse();

            if(objectToDrag)
            {
                isDragging = true;

                objectToDrag.SetAsLastSibling();

                originalPos = objectToDrag.position;
                dragObjectImage = objectToDrag.GetComponent<Image>();
                dragObjectModel = Instantiate(objectToDrag.GetComponent<Buildbutton>().correspondingObject);
                dragObjectImage.raycastTarget = false;
            }
        }

        if (isDragging)
        {
            objectToDrag.position = Input.mousePosition;

            dragObjectModel.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 p = dragObjectModel.transform.position;
            p.z = -5;
            dragObjectModel.transform.position = p;
        }

        if(Input.GetMouseButtonUp(0))
        {
            if(objectToDrag)
            {
                Transform replaceableObject = getTransformFromMouse();

                if(replaceableObject)
                {
                    objectToDrag.position = replaceableObject.position;
                    replaceableObject.position = originalPos;
                }
                else
                {
                    objectToDrag.position = originalPos;
                }

                dragObjectImage.raycastTarget = true;
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

        if (hitObjects.Count <= 0) return null;

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
}
