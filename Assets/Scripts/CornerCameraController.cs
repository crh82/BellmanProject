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
        if (Input.GetKeyDown(KeyCode.End)) cornerCamera.gameObject.SetActive(!cornerCamera.gameObject.activeSelf);
        
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            Debug.Log("OriginalCorner");
            cornerCamera.rect = new Rect(0.75f, 0.65f, 0.4f, 1f);
            cornerCamera.clearFlags = CameraClearFlags.Depth;
            // Debug.Log("orig");
            // cornerCamera.rect = new Rect(0, 0.75f, 0.2f, 0.25f);
            // cornerCamera.rect.Set(0,0.75f,0.2f,0.25f);
        }

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            Debug.Log("FullScreen");
            cornerCamera.rect = new Rect(0f, 0f, 1f, 1f);
            cornerCamera.clearFlags = CameraClearFlags.Skybox;
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
