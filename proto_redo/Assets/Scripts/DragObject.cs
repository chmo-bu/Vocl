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
        transform.position = GetMouseWorldPos() + mouseOffset;
    }
}
