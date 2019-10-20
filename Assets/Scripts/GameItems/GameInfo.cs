using UnityEngine;

public class GameInfo : MonoBehaviour {

    //Temporary container to pass through the main
    //menu to the second scene, to have it's info taken
    //and then to be destroyed.
    public string infoName;
    public float timeLimit = 0;
    public int killLimit = 0;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

}
