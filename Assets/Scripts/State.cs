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
    
    public float stateValue;

    public GameObject stateMesh;

    public Dictionary<int, List<Array>> applicableActions;

    public int stateIDNum;


    public Canvas GetStateCanvas()
    {
        return _stateCanvasHover;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        SetupStateVisualisation();
    }

    // Update is called once per frame
    void Update()
    {
        isStateInfoActive = _stateCanvasHover.gameObject.activeSelf;
        
        if (isStateInfoActive)
        {
            SetHoverCanvasPosition();
        }
        
        // Todo needs refactoring to tidy up
        stateInformationTextFlat.text = $"{stateMesh.transform.localScale.y}";
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
    void SetHoverCanvasPosition()
    {
        var position = transform.position;
        var localScale = stateMesh.transform.localScale;
        
        Vector3 hoverCanvasPos = position;
        hoverCanvasPos.y = (localScale.y  + stateCanvasHoverOffset);
        _stateCanvasHover.transform.position = hoverCanvasPos;
        
        // var transformPosition = _stateCanvasFlat.transform.position;
        // transformPosition.y = localScale.y;
        // new Vector3(position.x, localScale.y, position.z);
    }

    

    public void ToggleStateInfo()
    {
        _stateCanvasHover.gameObject.SetActive(!isStateInfoActive);
        
        // Todo Change to state value. This is a temporary solution. (transform.position.y * 2)
        // stateInformationText.text = $"V( {gameObject.name[0]},{gameObject.name[1]} ) = {stateMesh.transform.localScale.y}";
        stateInformationText.text = $"V( {gameObject.name[0]},{gameObject.name[1]} ) | StateID: {stateIDNum}";
    }
    
    
}
