using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    [Header("move setting")]
    public float panSpeed = 20f;         
    private Vector3 lastMousePosition;   

    [Header("zoom setting")]
    public float zoomSpeed = 5f;        
    public float minSize = 2f;            
    public float maxSize = 15f;         

    private UnityEngine.Camera cam;    

    void Start()
    {
   
        cam = GetComponent<UnityEngine.Camera>();
    }

    void Update()
    {
        HandlePan();   
        HandleZoom();  
    }

   
    void HandlePan()
    {
     
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }


        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            
            Vector3 delta = Input.mousePosition - lastMousePosition;

            float moveStep = panSpeed * Time.deltaTime;
            transform.Translate(-delta.x * moveStep * 0.01f, -delta.y * moveStep * 0.01f, 0);


            lastMousePosition = Input.mousePosition;
        }
    }

 
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (cam.orthographic)
            {

                cam.orthographicSize -= scroll * zoomSpeed;
              
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minSize, maxSize);
            }
            else
            {

                transform.Translate(0, 0, scroll * zoomSpeed, Space.Self);
            }
        }
    }
}