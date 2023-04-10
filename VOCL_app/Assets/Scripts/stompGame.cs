using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class stompGame : MonoBehaviour
{
    public PeakDetector detector;
    public GameObject rabbit;
    public GameObject destination1;
    public GameObject lollipop;
    public GameObject basket;

    public Vector3 rabbitLocation;

    public string microphone;
    AudioSource audioSource; 
    public bool complete;

    public GameObject mainPrompt;
    public GameObject stomp_prompt;
    public GameObject correct_prompt;
    public RuntimeAnimatorController celebrate;
    public RuntimeAnimatorController idle;

    public RuntimeAnimatorController candyMove;
    
    public float timer;
    private bool moving;

    bool isListening;

    void Start()
    {
        rabbitLocation = rabbit.transform.position;
        audioSource = GetComponent<AudioSource>();
        complete = false;
        timer = 20f;
        moving = false;
        isListening = false;
        correct_prompt.SetActive(false);

        // get all available microphones
		foreach (string device in Microphone.devices) {
			if (microphone == null) {
				//set default mic to first mic found.
				microphone = device;
                Debug.Log(microphone);
			}
		}
    }

    void Update()
    {
        rabbitLocation = rabbit.transform.position;
        Animator currentAnimator = rabbit.GetComponent<Animator>();
        Animator candyAnimator = lollipop.GetComponent<Animator>();


        if (timer != 0f && complete == true && moving)
        {
            timer = timer - .5f;
        }

        /* Moves rabbit to next destination if running, if not then idle */

        if (moving == true && timer == 0)
        {
            //basket.SetActive(false);
            Vector3 rabbitLocation = rabbit.transform.position;
            Vector3 target = destination1.transform.position;
            var offset = target - rabbitLocation;
            //Debug.Log(offset.magnitude);
            if (offset.magnitude > 90f && moving == true) {
                // If we're further away than 3 units, move towards the target.
                // The minimum allowable tolerance varies with the speed of the object and the framerate.
                // 2 * tolerance must be >= moveSpeed / framerate or the object will jump right over the stop.
                offset = offset.normalized * 400.0f;
                //normalize it and account for movement speed.
                rabbit.transform.position = rabbitLocation + (offset * Time.deltaTime);
                
            } 
            else 
            {
                moving = false;
                timer = 17f; // reset timer
                //rabbit.transform.Rotate(0,350,0);
                currentAnimator.runtimeAnimatorController = idle;
                basket.SetActive(true);
            }
        }

        if (!complete && !moving && !mainPrompt.activeSelf)
        {
            // begin game
            if (!isListening) {
                stomp_prompt.SetActive(true);
                detector.Listen(2);
                isListening = true;
            }
            
            // check if task is complete
            if (detector.done == true)
            {
                completeTask();
               // rabbit.transform.Rotate(0,30,0);
                currentAnimator.runtimeAnimatorController = celebrate;
                candyAnimator.runtimeAnimatorController = candyMove;
                detector.Stop();
            }
        }
        
    }

    public void completeTask()
    {
        complete = true;
        moving = true;
        stomp_prompt.SetActive(false);
        correct_prompt.SetActive(true);
    }
}