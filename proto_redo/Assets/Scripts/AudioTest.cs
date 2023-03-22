using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTest : MonoBehaviour
{
    public ClapDetector clapDetector;

    // Start is called before the first frame update
    void Start()
    {
        clapDetector.done = false;
        clapDetector.Listen();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
