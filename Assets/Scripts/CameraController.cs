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
        // trail = GameObject.FindWithTag("RedLine");
        
        mdpManager = FindObjectOfType<MdpManager>();
        // mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    private void Update()
    {
        
        // Todo Get the screen annotation mode working. 
        // if (annotateMode && Input.GetMouseButton(1))
        // {
        //     Debug.Log(mainCamera.ScreenToViewportPoint(Input.mousePosition));
        //     mainCamera.ViewportToScreenPoint(Input.mousePosition);
        //     // trail.transform.position = Vector3.Lerp(trail.transform.position, mainCamera.ScreenToWorldPoint(Input.mousePosition), 10);
        //     trail.transform.position = mainCamera.ScreenToViewportPoint(Input.mousePosition);
        //     // trail.transform.position = mainCamera.ViewportToScreenPoint(Input.mousePosition);
        //     //     trail.transform.position = mainCamera.ViewportToWorldPoint(Input.mousePosition);
        // }
        
        
        HandleMouseInput();
        HandleMovement();
        
        transform.position = Vector3.Lerp(transform.position, rigPosition, Time.deltaTime * interpolationSpeed);
    }

    private void HandleMouseInput()
    {
        zoomInput = Input.mouseScrollDelta.y;

       
        
        // vInput = zoomInput;
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
    
}
