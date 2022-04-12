using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed;
    public Transform target;
    public float zoomlimit;
    private Camera _mainCamera;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // (O) Controls camera orbit
        float horizontalInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.down, horizontalInput * rotationSpeed * Time.deltaTime);
        // (O end) 
        
        // (CZ) Controls camera zoom on target location (at this stage, the origin)
        float zoomInput = Input.GetAxis("Vertical");
        
        if (zoomInput != 0)
        {
            Vector3 currentPosition = _mainCamera.transform.position;
            Vector3 targetPosition = target.position;
            Vector3 newPosition = Vector3.MoveTowards(currentPosition, targetPosition, zoomInput);
            
            // Sets a limit on how close the camera can zoom in
            if (newPosition.y >= (targetPosition.y + zoomlimit))
            {
                _mainCamera.transform.position = newPosition;
            }
        }
        // (CZ end)


    }
}
