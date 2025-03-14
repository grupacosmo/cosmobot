using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MouseLook : MonoBehaviour
{
    public float speed = 1;
    public float zoomSpeed = 1;
    public bool invertY = false;
    Camera cam;
    Vector2 mouseStart;
    Vector3 lookStart;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            mouseStart = Input.mousePosition;
            lookStart = transform.rotation.eulerAngles;
        }

        if(Input.GetMouseButton(0)){
            transform.rotation = Quaternion.Euler(
                (Input.mousePosition.y - mouseStart.y) * (invertY?-1:1) * speed * cam.fieldOfView + lookStart.x,
                (mouseStart.x - Input.mousePosition.x) * speed * cam.fieldOfView + lookStart.y,
                0
            );
        }
        if(Input.GetKey(KeyCode.W)){
            cam.fieldOfView *= 1 - (zoomSpeed * Time.deltaTime);
        }
        if(Input.GetKey(KeyCode.S)){
            cam.fieldOfView *= 1 + (zoomSpeed * Time.deltaTime);
        }
    }
}
