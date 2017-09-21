using UnityEngine;
using System.Collections;
using UnityEngine.UI;

using UnityEngine.SceneManagement;
public class UI_Manager : MonoBehaviour {
    public int countDownTimeLeft;
    public float score=0;
    public Text FinalScoreHolder;
    public Text ScoreHolder;
    public Text countdownTimer;
    public Text TransitionTimer;

    private bool hasStarted = false;

    HandPhysicsController promptHand ;
    GameObject[] GameOverObjects;
    GameObject[] GameplayObjects;
    GameObject[] GameStartObjects;
    

    // Use this for initialization
    void Start () {
        GameOverObjects = GameObject.FindGameObjectsWithTag("ShowOnGameOver");
        GameplayObjects = GameObject.FindGameObjectsWithTag("ShowDuringGameplay");
        GameStartObjects = GameObject.FindGameObjectsWithTag("ShowAtStart");
        
        hideFinished();
        hideGameplay();
    }
	
	// Update is called once per frame
	void Update () {
        if (!hasStarted)
        {
            countdownTimer.text = countDownTimeLeft.ToString();
        }
        ScoreHolder.text = "Score: " + score.ToString();
    }

    //updates ingame transition timer appearence and color
    public void updateTransitionTimer(int time)
    {
        TransitionTimer.text = time.ToString();
        
    }



    //shows objects with ShowOnFinish tag
    public void showGameOver()
    {
        foreach (GameObject g in GameplayObjects)
        {

            g.SetActive(false);
        }
        foreach (GameObject g in GameOverObjects)
        { 


            FinalScoreHolder.text = "Total Score: " +    score.ToString();
            g.SetActive(true);
         
        }
    }

    //hides objects with ShowOnFinish tag
    public void hideFinished()
    {
        foreach (GameObject g in GameOverObjects)
        {
            g.SetActive(false);
        }
    }

    public void showGamePlay()
    {
        hasStarted = true;
        foreach (GameObject g in GameStartObjects)
        {

            g.SetActive(false);
        }
        foreach (GameObject g in GameplayObjects)
        {


            FinalScoreHolder.text = "Total Score: " + score.ToString()+"/100";
            g.SetActive(true);
        }
    }

    //hides objects with ShowOnFinish tag
    public void hideGameplay()
    {
        foreach (GameObject g in GameplayObjects)
        {
            g.SetActive(false);
        }
    }
    //Reloads the Level
    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
