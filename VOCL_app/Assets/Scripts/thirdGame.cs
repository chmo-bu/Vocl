using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class thirdGame : MonoBehaviour
{
    public PeakDetector detector;
    public GameObject rabbit;
    public GameObject start;
    public GameObject destination;
    public GameObject candy1, candy2, candy3;

    public GameObject basket;
    public GameObject log;

    public Vector3 rabbitLocation;

    public string microphone;
    AudioSource audioSource; 
    public bool complete;

    public GameObject shout_prompt;
    public GameObject correct_prompt;
    public GameObject endGame;
    public RuntimeAnimatorController celebrate;
    public RuntimeAnimatorController arrived;

    public RuntimeAnimatorController candyMove;
    
    public float timer;
    private bool moving;

    bool isListening;

    void Start()
    {
        shout_prompt.SetActive(false);
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
        Vector3 target2 = start.transform.position;
        Animator currentAnimator = rabbit.GetComponent<Animator>();
        Animator candyAnimator = candy1.GetComponent<Animator>();
        Animator candyAnimator2 = candy2.GetComponent<Animator>();
        Animator candyAnimator3 = candy3.GetComponent<Animator>();
        var distFromTarget2 = rabbitLocation - target2;
        if (timer != 0f && complete == true && moving)
        {
            timer = timer - .5f;
        }

        /* Moves rabbit to next destination if running, if not then idle */

        if (moving == true && timer == 0)
        {
            Vector3 rabbitLocation = rabbit.transform.position;
            Vector3 target = destination.transform.position;
            var offset = target - rabbitLocation;
            //Debug.Log(offset.magnitude);
            if (offset.magnitude > 100f && moving == true) {
                // If we're further away than 3 units, move towards the target.
                // The minimum allowable tolerance varies with the speed of the object and the framerate.
                // 2 * tolerance must be >= moveSpeed / framerate or the object will jump right over the stop.
                offset = offset.normalized * 400.0f;
                rabbit.transform.position = rabbitLocation + (offset * Time.deltaTime);
                
            } 
            else 
            {
                moving = false;
                timer = 17f; // reset timer
              
                currentAnimator.runtimeAnimatorController = arrived;
                endGame.SetActive(true);
                basket.SetActive(false);
            }
        }

        if (distFromTarget2.magnitude < 100f && !complete)
        {
            // at initial, start 
            // begin game
            shout_prompt.SetActive(true);
            if (!isListening) {
                shout_prompt.SetActive(true);
                detector.Listen(1, 2);
                isListening = true;
            }
            
            // check if task is complete
            if (detector.done == true)
            {
                completeTask();
                currentAnimator.runtimeAnimatorController = celebrate;
                candyAnimator.runtimeAnimatorController = candyMove;
                candyAnimator2.runtimeAnimatorController = candyMove;
                candyAnimator3.runtimeAnimatorController = candyMove;
                detector.Stop();
            }
        }
        
    }

    public void completeTask()
    {
       // tree.SetActive(false);
        log.SetActive(false);
        complete = true;
        moving = true;
        shout_prompt.SetActive(false);
        correct_prompt.SetActive(true);
    }
}
