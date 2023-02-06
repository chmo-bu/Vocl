using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragObject : MonoBehaviour
{
    private Vector3 mouseOffset;
    private float mzcoord;

    public void OnMouseDown() 
    {
        mzcoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        mouseOffset = gameObject.transform.position - GetMouseWorldPos();
    }

    public Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mzcoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    } 

    public void OnMouseDrag() 
    {
        Vector3 temp = GetMouseWorldPos() + mouseOffset;
        if (temp.x > 591)
        {
            temp.x = 591;
        }
        else if(temp.x < 583)
        {
            temp.x = 583;
        }

        if (temp.y > 168)
        {
            temp.y = 168;
        }
        else if(temp.y < 163)
        {
            temp.y = 163;
        }

        temp.z = (float)688.5;
        transform.position = temp;
       // Debug.Log(transform.position);
       // Debug.Log("\n");
    }
}
