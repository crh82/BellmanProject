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
    
    public bool isStateInfoActive;
    
    public float stateValue = 0f;

    public GameObject stateMesh;

    [FormerlySerializedAs("stateIDNum")] public int stateIndex;

    public TextMeshPro hoveringText;
    
    public GameObject textContainer;

    public float hoverInfoOffset = 0.02f;

    public GameObject stateQuad;


    public GameObject GetStateCanvasHover()
    {
        return textContainer;
    }
    //
    // public Canvas GetStateCanvasFlat()
    // {
    //     return _stateCanvasFlat;
    // }

    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    
    // —————————— ╔═════════════════════╗ —————————————
    // —————————— ║VISUALISATION METHODS║ —————————————
    // —————————— ╚═════════════════════╝ —————————————
    
    public void ToggleStateInfo()
    {
        textContainer.SetActive(!isStateInfoActive);
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
        var stateMeshTransform = stateMesh.transform;
        var localScale = stateMeshTransform.localScale;
        var position = stateMeshTransform.position;

        textContainer.transform.position = stateValue < 0
            ? new Vector3(position.x, hoverInfoOffset, position.z)
            : new Vector3(position.x, localScale.y + hoverInfoOffset, position.z);
    }

    public void SetStateScale(Vector3 stateScale)
    {
        stateMesh.transform.localScale = stateScale;
        stateQuad.transform.localScale = stateScale;
    }
}
