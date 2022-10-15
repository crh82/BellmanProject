using UnityEngine;


public class CameraController : MonoBehaviour
{
    public float interpolationSpeed;
    public float movementSpeed;
    public float rotationSpeed;
    public Transform localTarget;
    public Transform globalTarget;

    public float zoomlimit;
    public Camera mainCamera;

    public float hInput;
    public float vInput;
    public float zoomInput;

    public Vector3 rigPosition;

    public Bounds movementLimits;

    public MdpManager mdpManager;

    public bool annotateMode;

    public GameObject trail;

    // Start is called before the first frame update
    private void Start()
    {
        mdpManager = FindObjectOfType<MdpManager>();
    }

    // Update is called once per frame
    private void Update()
    {
        HandleMouseInput();
     
        HandleMovement();
        
        transform.position = Vector3.Lerp(transform.position, rigPosition, Time.deltaTime * interpolationSpeed);
    }

    private void HandleMouseInput()
    {
        zoomInput = Input.mouseScrollDelta.y;
        
        // (CZ) Controls camera zoom on target location (at this stage, the origin)
        if (zoomInput != 0)
        {
           
            
            var currentPosition = mainCamera.transform.position;
            var targetPosition = localTarget.position;
            var newPosition = Vector3.MoveTowards(currentPosition, targetPosition, zoomInput);

            // Sets a limit on how close the camera can zoom in
            if (newPosition.y >= targetPosition.y + zoomlimit) mainCamera.transform.position = newPosition;
        }
        // (CZ end)
    }

    /// <summary>
    /// The HandleMovement function controls the camera's orbit and movement around the grid world environment.
    /// </summary>
    private void HandleMovement()
    {
        // (O) Controls camera orbit
        hInput = Input.GetAxis("Horizontal") * movementSpeed;
        // (O end) 
        
        vInput = Input.GetAxis("Vertical") * movementSpeed;

        if (Input.GetKey(KeyCode.Q))
            transform.Rotate(Vector3.down, -rotationSpeed * Time.deltaTime);
        // rigPosition += (transform.right * -movementSpeed);

        if (Input.GetKey(KeyCode.E))
            transform.Rotate(Vector3.down, rotationSpeed * Time.deltaTime);
        // rigPosition += (transform.right * movementSpeed);

        if (hInput != 0) rigPosition += transform.right * hInput;

        if (vInput != 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                rigPosition += transform.up * vInput;
            }
            else
            {
                rigPosition += transform.forward * vInput;
            }
        }
        

        if (Input.GetKey(KeyCode.Space)) rigPosition = globalTarget.position;
    }

    public void IncreaseCameraPanSpeed()
    {
        if (movementSpeed < 0.3) movementSpeed += 0.1f;
        else movementSpeed = 0.3f;
        
        Debug.Log($"Pan + now {movementSpeed}");
    }

    public void DecreaseCameraPanSpeed()
    {
        if (movementSpeed >= 0.2) movementSpeed -= 0.1f;
        else movementSpeed = 0.1f;
        Debug.Log($"Pan - now {movementSpeed}");
    }

    public void IncreaseCameraRotationSpeed()
    {
        if (rotationSpeed < 250) rotationSpeed += 50;
        else rotationSpeed = 250;
        Debug.Log($"Rot + now {rotationSpeed}");
    }

    public void DecreaseCameraRotationSpeed()
    {
        if (rotationSpeed > 50) rotationSpeed -= 50;
        else rotationSpeed = 50;
        Debug.Log($"Rot - now {rotationSpeed}");
    }
    
}
