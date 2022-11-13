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
   // private Timer timer1; 
    private float timeLeft = 7.0f;

    //List<AsyncOperation> scenesToLoad = new List<AsyncOperation>();
    
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Hello world!");
        HideMenu();
        //scenesToLoad.Add(SceneManager.LoadSceneAsync)
        ShowLoadingScreen();
      //  InitTimer();
    }

    // public void InitTimer()
    // {
    //     timer1 = new Timer();
    //     timer1.Tick += new EventHandler(timer1_Tick);
    //     timer1.Interval = 3000; // in miliseconds
    //     timer1.Start();
    // }

    // private void timer1_Tick(object sender, EventArgs e)
    // {
    //     switchToMenu();
    // }

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
    
    // IEnumerator LoadingScreen()
    // {
    //     float totalProgress = 0;
    //     for (int i = 0; i < scenesToLoad.Count; i++)
    //     {
    //         while (!scenesToLoad[i].isDone)
    //         {
    //             totalProgress += scenesToLoad[i].progress;
    //             yield return null;
    //         }
    //     }
    // }
    /*
    // Update is called once per frame
    */
    void Update()
    {
        timeLeft -= Time.deltaTime;
        if(timeLeft < 0)
        {
            switchToMenu();
        }
    }
}
