using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Transform focus_transform;
    public float movementSpeed = 5f;
    public float fastMovementSpeed = 50f;
    void Update()
    {

        if (!focus_transform)
            return;

        bool fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float movementSpeed = fastMode ? this.fastMovementSpeed : this.movementSpeed;

        //Depth
        float sa = Input.GetAxis("Mouse ScrollWheel");
        if (sa > 0f)
            transform.position = transform.position + (transform.forward * movementSpeed * Time.deltaTime * sa * 200);
        else if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.UpArrow))
            transform.position = transform.position + (transform.forward * movementSpeed * Time.deltaTime);

        if (sa < 0f)
            transform.position = transform.position + (transform.forward * movementSpeed * Time.deltaTime * sa * 200);
        else if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.DownArrow))
            transform.position = transform.position + (-transform.forward * movementSpeed * Time.deltaTime);

        //Left and Right
        if (Input.GetAxis("Horizontal") != 0)
            transform.position = transform.position + (transform.right * movementSpeed * Time.deltaTime * Input.GetAxis("Horizontal"));

        float va = Input.GetAxis("Vertical");
        if (va > 0)
        {
            if (Vector3.Dot(transform.forward, focus_transform.up) > -0.85f)
                transform.position = transform.position + (transform.up * movementSpeed * Time.deltaTime * va);
        }
        else if (va < 0)
        {
            if (Vector3.Dot(transform.forward, focus_transform.up) < 0.85f)
                transform.position = transform.position + (transform.up * movementSpeed * Time.deltaTime * va);
        }
        transform.LookAt(focus_transform.position);
    }
}
