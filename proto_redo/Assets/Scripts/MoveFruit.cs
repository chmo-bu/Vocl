using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFruit : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x, Terrain.activeTerrain.SampleHeight(transform.position) + 4f, transform.position.z); 
        //+ 1 * speed * Time.deltaTime);

    }
}
