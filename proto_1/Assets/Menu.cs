using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class mainScene : MonoBehaviour
{
    public GameObject loadingObject;
    public GameObject menuObject;
    List<AsyncOperation> scenesToLoad = new List<AsyncOperation>();

    public void HideMenu()
    {
        loadingObject.SetActive(false);
    }

    void Start()
    {
        HideMenu();
    }
    /*
    public GameObject MenuScreen;
    public GameObject LoadingScreen;
    public 

    List<AsyncOperation> scenesToLoad = new List<AsyncOperation>();

    // Start is called before the first frame update
    void StartGame()
    {
        //HideMenu();
        ShowLoadingScreen();
        scenesToLoad.Add(SceneManager.LoadSceneAsync("Menu"));
        scenesToLoad.Add(SceneManager.LoadSceneAsync("Gameplay", LoadSceneMode.Additive));
        StartCoroutine(LoadingScreen());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    */
}
