using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class Main_Menu_Script : MonoBehaviour {

    private AssetBundle myLoadedAssetBundle;
    private string[] scenePaths;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update () {
	
	}

    public void LoadOnIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void exit()
    {
        Application.Quit();
    }
}
