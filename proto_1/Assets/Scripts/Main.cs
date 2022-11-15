using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//using System.Windows.Forms.Timer;

public class Main : MonoBehaviour
{
    // game objects
    public GameObject loadingObject;
    public GameObject menuObject;
    public GameObject inGameObject;

    // start button flag
    private bool start = false;

    // loading time
    private float timeLeft = 7.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        StopGame();
        HideMenu();
        ShowLoadingScreen();
    }

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
    }
    
    // Update is called once per frame
    void Update()
    {
        timeLeft -= Time.deltaTime;
        if(timeLeft < 0)
        {
            switchToMenu();
        }

        if (start == true) {
            // do start button task
            StartButtonTask();
        }
    }

    private void StartButtonTask() {
        // reveal game object and hide menu object
        StartGame();
        HideMenu();
    }

    private void StartGame() {
        // activate inGameObject
        inGameObject.SetActive(true);
    }

    private void StopGame() {
        // deactivate inGameObject
        inGameObject.SetActive(false);
    }

    public void onStartClick() {
        // change start flag
        start = true;
    }
}
