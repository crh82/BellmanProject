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
        // TD Visibility
        if (Input.GetKeyDown(KeyCode.End))          ToggleTopDownVisibility();
        
        // TD Camera Full Screen/MiniMap modes
        if (Input.GetKeyDown(KeyCode.LeftBracket))  SwitchTopDownToCornerMiniMap();
        
        if (Input.GetKeyDown(KeyCode.RightBracket)) SwitchTopDownToFullScreen();
        
        // Top Down Camera Zoom Controls
        if (Input.GetKey(KeyCode.R))                ZoomInTopDownCamera();
        
        if (Input.GetKey(KeyCode.F))                ZoomOutTopDownCamera();
      
    }

    private void ZoomOutTopDownCamera()
    {
        if (topDownView.m_Lens.OrthographicSize <= 10f)
            topDownView.m_Lens.OrthographicSize += orthZoomValue;
    }

    private void ZoomInTopDownCamera()
    {
        if (topDownView.m_Lens.OrthographicSize >= 0.3f)
            topDownView.m_Lens.OrthographicSize -= orthZoomValue;
    }

    private void ToggleTopDownVisibility() => cornerCamera.gameObject.SetActive(!cornerCamera.gameObject.activeSelf);

    private void SwitchTopDownToFullScreen()
    {
        
        cornerCamera.rect = new Rect(0f, 0f, 1f, 1f);
        cornerCamera.clearFlags = CameraClearFlags.Skybox;
        AssignTheCursorTrailToBeRenderedOnThisTopDownCamera();
    }

    private void SwitchTopDownToCornerMiniMap()
    {
        cornerCamera.rect = new Rect(0.75f, 0.65f, 0.4f, 1f);
        cornerCamera.clearFlags = CameraClearFlags.Depth;
        RenderCursorTrailOnMainCamera();
    }

    private void AssignTheCursorTrailToBeRenderedOnThisTopDownCamera()
    {
        var cursorTrail = GameManager.instance.GetUIController().cursorTrail;
        cursorTrail.startWidth = 0.03f;
        GameManager.instance.GetUIController().cursorTrail.cameraRenderingTheCursorTrail = cornerCamera;
    }

    private void RenderCursorTrailOnMainCamera()
    {
        var uiController = GameManager.instance.GetUIController();
        var cursorTrail = GameManager.instance.GetUIController().cursorTrail;
        cursorTrail.startWidth = 0.007f;
        GameManager.instance.GetUIController().cursorTrail.cameraRenderingTheCursorTrail = uiController.solverMainCamera;
    }


    public void FocusOn(GameObject quad)
    {
        virtualCamera.Follow = quad.transform;
        virtualCamera.LookAt = quad.transform;
    }
    
    
}
