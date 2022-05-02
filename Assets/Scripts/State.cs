using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

public class State : MonoBehaviour
{

    [FormerlySerializedAs("_stateInformationText")] public TextMeshProUGUI stateInformationText;

    public TextMeshProUGUI stateInformationTextFlat;

    private Canvas[] _stateCanvasArray;

    private Canvas _stateCanvasHover;

    private Canvas _stateCanvasFlat;

    public float reward;
    
    public bool isStateInfoActive;

    public float stateCanvasHoverOffset = 1.5f;

    public float stateCanvasFlatOffset = 0.01f;
    
    public float stateValue = 0f;

    public GameObject stateMesh;

    public Dictionary<int, List<Array>> applicableActions;

    [FormerlySerializedAs("stateIDNum")] public int stateIndex;

    public TextMeshPro hoveringText;
    
    public GameObject textContainer;


    public Canvas GetStateCanvasHover()
    {
        return _stateCanvasHover;
    }

    public Canvas GetStateCanvasFlat()
    {
        return _stateCanvasFlat;
    }

    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        SetupStateVisualisation();
    }

    // Update is called once per frame
    void Update()
    {
        // isStateInfoActive = _stateCanvasHover.gameObject.activeSelf;
        //
        // if (isStateInfoActive)
        // {
        //     SetHoverCanvasPosition();
        // }
        //
        // // Todo needs refactoring to tidy up
        // stateInformationTextFlat.text = $"{Math.Round(stateMesh.transform.localScale.y, 4)}";
        
    }
    
    
    // —————————— ╔═════════════════════╗ —————————————
    // —————————— ║VISUALISATION METHODS║ —————————————
    // —————————— ╚═════════════════════╝ —————————————
    
    /// <summary>
    /// Method initialises the canvases which hold the UI visualisations of the state information.
    /// These canvases contain all the UI elements that display the information about the state.
    /// </summary>
    private void SetupStateVisualisation()
    {
        // (Canvases) 
        _stateCanvasArray = gameObject.GetComponentsInChildren<Canvas>();
        
        foreach (Canvas canvas in _stateCanvasArray)
        {
            canvas.worldCamera = Camera.main;
        }
        
        _stateCanvasHover = _stateCanvasArray[0];
        _stateCanvasFlat  = _stateCanvasArray[1];

        _stateCanvasHover.gameObject.SetActive(false);
        _stateCanvasFlat.gameObject.SetActive(false);
        // (Canvases end)
    }
    
    /// <summary>
    /// Sets the position of the state information that hovers above each state.
    /// </summary>
    private void SetHoverCanvasPosition()
    {
        var position = transform.position;
        
        var localScale = stateMesh.transform.localScale;
        
        Vector3 hoverCanvasPos = position;
        
        float newStateTextHoverHeight = (localScale.y  + stateCanvasHoverOffset);
        
        hoverCanvasPos.y = (newStateTextHoverHeight < 0) ? 0.02f : newStateTextHoverHeight;
        
        _stateCanvasHover.transform.position = hoverCanvasPos;
        
        var canvasFlatPos = position;
        
        float newStateCanvasFlatHeight = localScale.y + stateCanvasFlatOffset;

        canvasFlatPos.y = (newStateCanvasFlatHeight < 0) ? 0.02f : newStateCanvasFlatHeight;
        
        _stateCanvasFlat.transform.position = canvasFlatPos;
        
        textContainer.transform.position = canvasFlatPos + new Vector3(0f, 0.03f, 0f);
        // hoveringText.transform.position = canvasFlatPos + new Vector3(0f, 0.1f, 0f);
    }

    public void SetCamera(Camera cameraForCanvas)
    {
        _stateCanvasHover.worldCamera = cameraForCanvas;
        _stateCanvasFlat.worldCamera = cameraForCanvas;
    }
    

    public void ToggleStateInfo()
    {
        _stateCanvasHover.gameObject.SetActive(!isStateInfoActive);
        _stateCanvasFlat.gameObject.SetActive(!isStateInfoActive);
        
        // Todo Change to state value. This is a temporary solution. (transform.position.y * 2)
        // stateInformationText.text = $"V( {gameObject.name[0]},{gameObject.name[1]} ) = {stateMesh.transform.localScale.y}";
        stateInformationText.text = $"V( {stateIndex} ) = {Math.Round(stateValue, 3)}";
    }

    public void UpdateHeight(float value)
    {
        stateValue = value;
        var stateMeshTransform = stateMesh.transform;
        var stateMeshTransformLocalScale = stateMesh.transform.localScale;
        var stateMeshTransformPosition = stateMesh.transform.position;

        stateMeshTransform.localScale =
            new Vector3(stateMeshTransformLocalScale.x, value, stateMeshTransformLocalScale.z);
        stateMeshTransform.position =
            new Vector3(stateMeshTransformPosition.x, value / 2, stateMeshTransformPosition.z);
        hoveringText.text = $"{Math.Round(stateValue, 4)}";

        
        
        UpdateStateValueVisual();
    }

    private void UpdateStateValueVisual()
    {
        var localScale = stateMesh.transform.localScale;

        textContainer.transform.position += stateValue < 0
            ? new Vector3(0f, stateCanvasFlatOffset, 0f)
            : new Vector3(0f, localScale.y + stateCanvasFlatOffset, 0f);
    }
}
