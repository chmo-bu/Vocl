using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class disableObject : MonoBehaviour
{
    public GameObject candy;
    public void setInactive()
    {
        candy.SetActive(false);
    }
}
