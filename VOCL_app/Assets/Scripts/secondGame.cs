using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class secondGame : MonoBehaviour
{
    public GameObject rabbit;
    public GameObject start;
    public GameObject destination;
    public GameObject candy1;
    public GameObject candy2;
    //public GameObject basket;

    public Vector3 rabbitLocation;
    public bool complete;

    public GameObject tap_prompt;
    public GameObject correct_prompt;
    public RuntimeAnimatorController celebrate;
    public RuntimeAnimatorController idle;

    public RuntimeAnimatorController candyMove;
    
    public float timer;
    private bool moving;

    private int tapCount = 0;
    
    void Start()
    {
        rabbitLocation = rabbit.transform.position;
        complete = false;
        timer = 20f;
        moving = false;
        correct_prompt.SetActive(false);
        tap_prompt.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        rabbitLocation = rabbit.transform.position;
        Vector3 target2 = start.transform.position;
        Animator currentAnimator = rabbit.GetComponent<Animator>();
        Animator candyAnimator = candy1.GetComponent<Animator>();
        Animator candy2Ani = candy2.GetComponent<Animator>();
        //float distFromTarget2 = Vector3.Distance(rabbitLocation, target2);
        var distFromTarget2 = rabbitLocation - target2;

        if (timer != 0f && complete == true && moving)
        {
            timer = timer - .5f;
        }

        /* Move to next dest. */
        if (moving == true && timer == 0)
        {
            //basket.SetActive(false);
            Vector3 rabbitLocation = rabbit.transform.position;
            Vector3 target = destination.transform.position;
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
                timer = 17f; // reset timer
               // rabbit.transform.Rotate(0,-3,0);
                currentAnimator.runtimeAnimatorController = idle;
                //basket.SetActive(true);
            }
        }
        
        if (distFromTarget2.magnitude < 100f && !complete)
        {
            // at initial, start 
            //Debug.Log("here");
            tap_prompt.SetActive(true);
            if (!complete && !moving)
            {
                   // Debug.Log(Input.touchCount);
                    // begin game
                    if (Input.touchCount == 1 && tapCount != 3)
                    {
                        Touch touch = Input.GetTouch(0);
                        switch (touch.phase)
                        {
                            case TouchPhase.Began:
                                var ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                                RaycastHit hitInfo;
                                if (Physics.Raycast(ray, out hitInfo))
                                {
                                    if (hitInfo.transform.gameObject.CompareTag("Correct"))
                                    {
                                        //Debug.Log("small");
                                        tapCount++;
                                    }
                                }
                                if (tapCount > 3)
                                {
                                    // reset
                                    tapCount = 0;
                                }
                                break;
                            
                                
                        }
               // }
                    }
            
                if (tapCount == 3)
                {
                    completeTask();
                    currentAnimator.runtimeAnimatorController = celebrate;
                    candyAnimator.runtimeAnimatorController = candyMove;
                    candy2Ani.runtimeAnimatorController = candyMove;
                }
            }
            
                
        }

        


    }

    public void completeTask()
    {
        complete = true;
        moving = true;
        tap_prompt.SetActive(false);
        correct_prompt.SetActive(true);
    }
}
