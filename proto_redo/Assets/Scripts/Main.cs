using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//using System.Windows.Forms.Timer;

public class Main : MonoBehaviour
{
    public GameObject loadingObject;
    public GameObject menuObject;
    public GameObject gameLoad;
    //public GameObject game;
   // public GameObject button;
    //public GameObject play_button
   
    private float timeLeft = 5.0f;

    private bool switchDone = false;

    //List<AsyncOperation> scenesToLoad = new List<AsyncOperation>();
    
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Hello world!");
        HideMenu();
        gameLoad.SetActive(false);
       // HideGame();
        //scenesToLoad.Add(SceneManager.LoadSceneAsync)
        ShowLoadingScreen();
    }

/*
    public void HideGame()
    {
        game.SetActive(false);
    } */


    public void HideMenu()
    {
        menuObject.SetActive(false);
    }

    public void ShowLoadingScreen()
    {
        loadingObject.SetActive(true);
    }

    public void switchToMenu()
    {
        loadingObject.SetActive(false);
        menuObject.SetActive(true);
       //button.SetActive(true);
    }
    
    void Update()
    {
        timeLeft -= Time.deltaTime;
        if(timeLeft < 0 && switchDone == false)
        {
            switchToMenu();
            switchDone = true;
        }
    }
}