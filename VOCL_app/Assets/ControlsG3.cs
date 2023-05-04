using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsG3 : MonoBehaviour
{
    public GameObject yesButton;
    public GameObject noButton;
    public GameObject endofGame;

    void Start()
    {
        yesButton.SetActive(false);
        noButton.SetActive(false);
        endofGame.SetActive(false);
    }

    void Update()
    {
        
    }
}
