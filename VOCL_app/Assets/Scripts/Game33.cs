using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game33 : MonoBehaviour
{

    public GameObject rabbit;
    public GameObject start;   
    public GameObject gameover_prompt;
    public bool moving;
    public Vector3 rabbitLocation;
    public GameObject yes_button, no_button;


    // Start is called before the first frame update
    void Start()
    {
        gameover_prompt.SetActive(false);
        yes_button.SetActive(false);
        no_button.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Animator currentAnimator = rabbit.GetComponent<Animator>();
        rabbitLocation = rabbit.transform.position;
        Vector3 target2 = start.transform.position;
        var distFromTarget2 = rabbitLocation - target2;


        if (distFromTarget2.magnitude < 100f)
        {
            //Debug.Log("active");
            gameover_prompt.SetActive(true);
            yes_button.SetActive(true);
            no_button.SetActive(true);

        }

    }
}

