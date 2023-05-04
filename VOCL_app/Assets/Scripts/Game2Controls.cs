using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game2Controls : MonoBehaviour
{
    public GameObject mainPrompt;
    public GameObject homeButton;
    public GameObject EndofGame;
    public GameObject yesButton;
    public GameObject noButton;
    public GameObject prompt1;
    public GameObject basket;
   // public GameObject rabbit;
  //  public GameObject startLocation;
   // public GameObject destination1;
   // public GameObject destination2;
   // public Vector3 rabbitLocation;

    //public RuntimeAnimatorController CelebrateRun;

   // public Vector3 target1;
   // public Vector3 target2;
   // public Vector3 start;
    public double timer;
    public bool startUpDone;


    void Start()
    {
        //rabbitLocation = rabbit.transform.position;
        //target1 = destination1.transform.position;
        //target2 = destination2.transform.position;
        //start = startLocation.transform.position;
        //submitButton.SetActive(false);
        mainPrompt.SetActive(true);
        homeButton.SetActive(false);
        EndofGame.SetActive(false);
        yesButton.SetActive(false);
        noButton.SetActive(false);
        prompt1.SetActive(false);
        basket.SetActive(false);
        startUpDone = false;
        timer = 3.0f;
    }

    void Update()
    {
        //rabbitLocation = rabbit.transform.position;
        //Animator currentAnimator = rabbit.GetComponent<Animator>();
        if (timer < 0)
        {
            startUpDone = true;
            mainPrompt.SetActive(false);
            homeButton.SetActive(true);
           // prompt1.SetActive(true);
            basket.SetActive(true);
            timer = 3.0f;
        }
        
        if (!startUpDone)
        {
            timer = timer - Time.deltaTime;
        }
       
        
       
        //float distFromStart = Vector3.Distance(rabbitLocation, start);
        //float distFromTarget1 = Vector3.Distance(rabbitLocation, target1);
        //float distFromTarget2 = Vector3.Distance(rabbitLocation, target2);


        
    }
}
