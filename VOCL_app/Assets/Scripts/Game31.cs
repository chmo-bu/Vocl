using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game31 : MonoBehaviour
{
    public GameObject rabbit;
    public GameObject destination1;
    public GameObject correct_prompt;
    public GameObject incorrect_prompt;

    public GameObject pink_candy;
    public GameObject green_candy;
    public GameObject blue_candy;
    public RuntimeAnimatorController celebrate;
    public RuntimeAnimatorController idle;
    public bool complete;
    public Vector3 rabbitLocation;
    public bool moving;
    public GameObject tap_prompt;
    public float timer;
    public bool stopFlag;

    void Start()
    {
        rabbitLocation = rabbit.transform.position;
        complete = false;
        moving = false;
        stopFlag = false;
        correct_prompt.SetActive(false);
        incorrect_prompt.SetActive(false);
        tap_prompt.SetActive(false);
        timer = 3.0f;
    }

    void Update()
    {
        rabbitLocation = rabbit.transform.position;
        Animator currentAnimator = rabbit.GetComponent<Animator>();

        if (timer >= 0f && incorrect_prompt.activeSelf)
        {
            timer = timer - Time.deltaTime;
            Debug.Log(timer);
        }

        if(timer < 0f && incorrect_prompt.activeSelf)
        {
            incorrect_prompt.SetActive(false);
            timer = 3.0f;
        }

        if (timer >= 0f && correct_prompt.activeSelf)
        {
            timer = timer - Time.deltaTime;
            Debug.Log(timer);

        }

        if(timer < 0f && correct_prompt.activeSelf && !stopFlag)
        {
            stopFlag = true;
            moving = true;
            rabbit.transform.Rotate(0,90,0);
            timer = 0.5f;
        }
        

        //moving to next destination, Game2
        if (moving == true)
        {
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
                    //timer = 17f; // reset timer
                    // rabbit.transform.Rotate(0,-3,0);
                    currentAnimator.runtimeAnimatorController = idle;
                    //basket.SetActive(true);
                }
            
        }

        if (!complete)
        {
            // at initial, start 
            //Debug.Log("here");
            tap_prompt.SetActive(true);
            if (!complete && !moving)
            {
                //Debug.Log("gamestart");
                // begin game
                if (Input.touchCount == 1)
                {
                    var ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo))
                    {
                    //Debug.Log(ray);

                        if (hitInfo.transform.gameObject.CompareTag("Correct"))
                        {
                            //Debug.Log("pink");
                            completeTask();
                            currentAnimator.runtimeAnimatorController = celebrate;

                        }

                        if (hitInfo.transform.gameObject.CompareTag("Incorrect"))
                        {
                            incorrect_prompt.SetActive(true);

                        }
                    }
                                             
                }
            }
        }
        
    }

    public void completeTask()
    {
        complete = true;
        timer = 1.0f;

        tap_prompt.SetActive(false);
        correct_prompt.SetActive(true);
        incorrect_prompt.SetActive(false);

    }
}
