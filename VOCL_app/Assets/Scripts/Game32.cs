using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Game32 : MonoBehaviour
{
    public GameObject rabbit;
    public GameObject start;
    public GameObject next_destination;
    public GameObject correct_prompt;
    public GameObject clap_prompt;
    
    public string microphone;
    AudioSource audioSource; 
    public float timer;
    public bool complete;
    public bool moving;
    public bool isListening;
    public Vector3 rabbitLocation;
    
    public RuntimeAnimatorController celebrate;
    public RuntimeAnimatorController idle;
    // Start is called before the first frame update

    void Start()
    {
        clap_prompt.SetActive(false);
        correct_prompt.SetActive(false);
        complete = false;
        moving = false;
        isListening = false;

        audioSource = GetComponent<AudioSource>();

        foreach (string device in Microphone.devices) {
			if (microphone == null) {
				//set default mic to first mic found.
				microphone = device;
                Debug.Log(microphone);
			}
		}

       

    }

    // Update is called once per frame
    void Update()
    {
        Animator currentAnimator = rabbit.GetComponent<Animator>();
        rabbitLocation = rabbit.transform.position;
        Vector3 target2 = start.transform.position;
        var distFromTarget2 = rabbitLocation - target2;


        //moving to the next and final game
        if (moving == true)
        {
            Vector3 rabbitLocation = rabbit.transform.position;
            Vector3 target = next_destination.transform.position;
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
                //timer = 17f; // reset timer
               // rabbit.transform.Rotate(0,-3,0);
                currentAnimator.runtimeAnimatorController = idle;
                //basket.SetActive(true);
            }
        }

        if (distFromTarget2.magnitude < 100f && !complete)
        {
            
            clap_prompt.SetActive(true);
            if (!complete && !moving)
            {
                if (!isListening) 
                {
                    clap_prompt.SetActive(true);
                  //  detector.Listen(1, 1);
                    isListening = true;
                }
                /*

                if (detector.done == true)
                {
                    //completeTask();
                    currentAnimator.runtimeAnimatorController = celebrate;
                    // reset detector 
                    detector.done = false;
                    detector.Stop();
                }*/
            
                                                
            }
        }
    }

}

