using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class PauseMenuBehavior : MonoBehaviour {
    public bool paused = false;
    private string PauseButton = "Cancel";
    GameObject[] PauseMenu;
    public bool startPaused;
    // Use this for initialization
    void Start() {
        PauseMenu = GameObject.FindGameObjectsWithTag("PauseMenu");

        if (!startPaused)
        {
            foreach (GameObject g in PauseMenu)
            {

                g.SetActive(false);
            }
        }
        else
        {
            //togglePause();
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown(PauseButton))
        {
            paused = togglePause();
        }
    }
    public void showPauseMenu()
    {
        foreach (GameObject g in PauseMenu)
        {

            g.SetActive(true);
        }
    }

    //hides objects with ShowOnFinish tag
    public void hidePauseMenu()
    {
        foreach (GameObject g in PauseMenu)
        {

            g.SetActive(false);
        }
    }
    public bool togglePause()
    {
        if (Time.timeScale == 0f)
        {
            hidePauseMenu();
            Time.timeScale = 1f;
            return (false);
        }
        else
        {
            showPauseMenu();
            Time.timeScale = 0f;
            
            return (true);
        }
    }

    public void onResume()
    {
        paused = togglePause();
        
    }
    public void onMainMenu()
    {
        paused = togglePause();
        SceneManager.LoadScene(0);
    }
    public void onExit()
    {
        Application.Quit();
    }
    public void Reload()
    {
        paused = togglePause();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
