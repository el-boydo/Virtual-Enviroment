using UnityEngine;
using System.Collections;

public class MotionLight : MonoBehaviour {
    public GameObject correct;
    public GameObject incorrect;
    public GameObject neutral;
    public int CurrentState = 0;
	// Use this for initialization
	void Start () {
        correct.SetActive(false);
        incorrect.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	    if(CurrentState == -1)
        {
            correct.SetActive(false);
            incorrect.SetActive(true);
            neutral.SetActive(false);
        }
        else if (CurrentState == 1)
        {
            correct.SetActive(true);
            incorrect.SetActive(false);
            neutral.SetActive(false);
        }
        else 
        {
            correct.SetActive(false);
            incorrect.SetActive(false);
            neutral.SetActive(true);
        }
    }
}
