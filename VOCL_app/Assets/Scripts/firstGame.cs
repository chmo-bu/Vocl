using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class firstGame : MonoBehaviour
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
    public GameObject clap_prompt;
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
        timer = 1.4f;
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


        if (timer > 0f && complete && moving)
        {
            timer = timer - Time.deltaTime;

        }

        /* Moves rabbit to next destination if running, if not then idle */

        if (moving == true && timer < 0f)
        {
            //basket.SetActive(false);
            Vector3 rabbitLocation = rabbit.transform.position;
            Vector3 target = destination1.transform.position;
            var offset = target - rabbitLocation;
            //Debug.Log(offset.magnitude);
            if (offset.magnitude > 100f && moving == true) {
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
                timer = 1.4f; // reset timer
              
                currentAnimator.runtimeAnimatorController = idle;
                basket.SetActive(true);
            }
        }

        if (!complete && !moving && !mainPrompt.activeSelf)
        {
            // begin game
            if (!isListening) {
                clap_prompt.SetActive(true);
                detector.Listen(2, 1);
                isListening = true;
            }
            

            /* remove later */

            if (Input.touchCount == 1)
            {
                var ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    if (hitInfo.transform.gameObject.CompareTag("Correct"))
                    {
                        completeTask();
                        currentAnimator.runtimeAnimatorController = celebrate;
                        candyAnimator.runtimeAnimatorController = candyMove;
                        detector.Stop();
                    }
                }
            }


            // check if task is complete
            if (detector.done == true)
            {
                completeTask();
                currentAnimator.runtimeAnimatorController = celebrate;
                candyAnimator.runtimeAnimatorController = candyMove;
                // reset detector 
                detector.done = false;
                detector.Stop();
            }
        }
        
    }

    public void completeTask()
    {
        complete = true;
        moving = true;
        clap_prompt.SetActive(false);
        correct_prompt.SetActive(true);
    }
}