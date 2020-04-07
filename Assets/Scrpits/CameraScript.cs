using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform objectFollowedByCam;

    public Vector3 offset;

    Quaternion mouseRotYAxis;
    Quaternion mouseRotXAxis;

    void Start()
    {
        InputEventManager.inputEvent.onMouseMoved += OnMouseMoved;

        offset = transform.position - objectFollowedByCam.transform.position;// ChangeObjAxis(objectFollowedByCam.transform.position);
    }

    void Update()
    {
        offset = mouseRotXAxis * mouseRotYAxis * offset;

        transform.position = objectFollowedByCam.position + offset;
        transform.LookAt(objectFollowedByCam, Vector3.up);

    }

    private void OnMouseMoved(Vector2 mouseAxis,float rotSpeed)
    {
        mouseRotYAxis = Quaternion.AngleAxis(mouseAxis.x * rotSpeed * Time.deltaTime, Vector3.up);

        mouseRotYAxis = Quaternion.Euler(0f, mouseRotYAxis.eulerAngles.y, 0f);

        //limit x axis rotation. 15-45
        if ((transform.rotation.eulerAngles.x <= 15f && mouseAxis.y < 0f) == false && (transform.rotation.eulerAngles.x >= 45f && mouseAxis.y > 0f) == false)
        {
            mouseRotXAxis = Quaternion.AngleAxis(mouseAxis.y * rotSpeed * Time.deltaTime, Vector3.right);

            mouseRotXAxis = Quaternion.Euler(mouseRotXAxis.eulerAngles.x, 0f , 0f);

        }
        else
            if(transform.rotation.eulerAngles.x <= 15f)
                mouseRotXAxis = Quaternion.Euler(15f - transform.rotation.eulerAngles.x, 0f, 0f);
            else if (transform.rotation.eulerAngles.x >= 45f)
                mouseRotXAxis = Quaternion.Euler(45f - transform.rotation.eulerAngles.x, 0f, 0f);


    }
}
