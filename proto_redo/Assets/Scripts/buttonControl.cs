using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttonControl : MonoBehaviour
{
    public GameObject submitButton;
    public GameObject rabbit;
    public GameObject startLocation;
    public GameObject destination1;
    public GameObject destination2;
    public Vector3 rabbitLocation;

    public RuntimeAnimatorController CelebrateRun;

    public Vector3 target1;
    public Vector3 target2;
    public Vector3 start;


    void Start()
    {
        rabbitLocation = rabbit.transform.position;
        target1 = destination1.transform.position;
        target2 = destination2.transform.position;
        start = startLocation.transform.position;
        submitButton.SetActive(false);
    }

    void Update()
    {
        rabbitLocation = rabbit.transform.position;
        Animator currentAnimator = rabbit.GetComponent<Animator>();
        

        float distFromStart = Vector3.Distance(rabbitLocation, start);
        float distFromTarget1 = Vector3.Distance(rabbitLocation, target1);
        float distFromTarget2 = Vector3.Distance(rabbitLocation, target2);

        //Debug.Log(distFromTarget1);

        if (distFromStart <= 3 || distFromTarget2 <= 3 || currentAnimator.runtimeAnimatorController == CelebrateRun)
        {
            submitButton.SetActive(false);
        }
        else if(distFromTarget1 <= 3)
        {
            submitButton.SetActive(true);
        }
    }
}