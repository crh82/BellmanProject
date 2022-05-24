using UnityEngine;
using Cinemachine;

public class CornerCameraController : MonoBehaviour
{
    // public static CornerCameraController instance;

    public GameObject cameraRig;

    public CinemachineVirtualCamera virtualCamera;
    
    // Start is called before the first frame update
    void Start()
    {
        // instance = this;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.End))
        {
            cameraRig.SetActive(false);
        }
    }

    public void FocusOn(GameObject quad)
    {
        virtualCamera.Follow = quad.transform;
        virtualCamera.LookAt = quad.transform;
    }
}
