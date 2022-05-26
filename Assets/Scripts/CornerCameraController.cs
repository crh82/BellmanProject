using UnityEngine;
using Cinemachine;

public class CornerCameraController : MonoBehaviour
{
    // public static CornerCameraController instance;

    public GameObject cameraRig;

    public Camera cornerCamera;

    public CinemachineBrain cornerCameraBrain;

    public CinemachineVirtualCamera virtualCamera;

    public CinemachineVirtualCamera topDownView;

    public CinemachineCameraOffset topDownOffset;

    public float orthZoomValue;
    
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

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            Debug.Log("orig");
            cornerCamera.rect = new Rect(0, 0.75f, 0.2f, 0.25f);
            // cornerCamera.rect.Set(0,0.75f,0.2f,0.25f);
        }

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            Debug.Log("Big");
            cornerCamera.rect = new Rect(0.75f, 0.7f, 0.4f, 1f);
            // cornerCamera.rect.Set(0,0.5f,0.4f,1f);
        }

        if (Input.GetKey(KeyCode.R))
        {
            if (topDownView.m_Lens.OrthographicSize >= 0.3f)
                topDownView.m_Lens.OrthographicSize -= orthZoomValue;
            
            // topDownOffset.m_Offset += Vector3.forward * -1;
        }

        if (Input.GetKey(KeyCode.F))
        {
            if (topDownView.m_Lens.OrthographicSize <= 10f)
                topDownView.m_Lens.OrthographicSize += orthZoomValue;
           
            // topDownOffset.m_Offset += Vector3.forward * 1;
        }
    }

    public void FocusOn(GameObject quad)
    {
        virtualCamera.Follow = quad.transform;
        virtualCamera.LookAt = quad.transform;
    }
    
    
}
