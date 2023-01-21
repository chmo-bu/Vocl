using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicListeningObject : MonoBehaviour
{
    public float moveSpeed = 0.0f;
    private Rigidbody2D rigBody2D;
    float currentLoudness;
    bool jump = true;
    float loudness = 0.3f;
    float jumpForce = 10;
    public StreamingMic streamingMic;

    private void FixedUpdate()
    {
        //rigBody2D.velocity = new Vector2(moveSpeed, 0);
    }

    // Use this for initialization
    void Start()    
    {
        rigBody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        currentLoudness = streamingMic.m_level;
        if (currentLoudness > loudness)
        {
            currentLoudness = streamingMic.m_level;
            // Debug.Log("Jump currentLoudness =" + currentLoudness);
            rigBody2D.AddForce(new Vector2(0, jumpForce));
            jump = false;
        }

    }
}
    