using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boundries : MonoBehaviour
{
    /*
    private Vector2 screenBounds;
    private float objectWidth;
    private float objectHeight;

    public void OnMouseDown() 
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        objectWidth = transform.GetComponent<Collider>().bounds.size.x / 2;
        objectHeight = transform.GetComponent<Collider>().bounds.size.y / 2 ; 
    } */

    public void OnMouseDrag() 
    {
        Vector3 viewPos = transform.position;
        Vector3 minBounds = Camera.main.WorldToScreenPoint(new Vector3(34,(float) -3.5, 14));
        Vector3 maxBounds = Camera.main.WorldToScreenPoint(new Vector3(42, (float)1.7, 14));

        viewPos.x = Mathf.Clamp(viewPos.x, minBounds.x, maxBounds.x);
        viewPos.y = Mathf.Clamp(viewPos.y, minBounds.y, maxBounds.y);
        transform.position = viewPos;
    }
}
