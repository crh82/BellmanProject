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
    
    public bool             isStateInfoActive;
    
    public float            stateValue = 0f;

    public GameObject       stateMesh;

    public int              stateIndex;

    public TextMeshPro      hoveringText;
    
    public GameObject       textContainer;

    public float            hoverInfoOffset = 0.02f;

    public GameObject       stateQuad;

    public bool             selected;

    public List<GameObject> ActionSprites;


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
        // textContainer.SetActive(!isStateInfoActive);
        // stateInformationText.text = $"V( {stateIndex} ) = {Math.Round(stateValue, 3)}";
    }

    public void UpdateHeight(float value)
    {
        stateValue = value;
        var stateMeshTransform = stateMesh.transform;
        var stateMeshTransformLocalScale = stateMesh.transform.localScale;
        var stateMeshTransformPosition = stateMesh.transform.position;

        // TODO: Might be a bad solution.
        float updateValue = -0.01 < value && value < 0.01 ? 0.01f : value;

        stateMeshTransform.localScale =
            new Vector3(stateMeshTransformLocalScale.x, updateValue, stateMeshTransformLocalScale.z);
        stateMeshTransform.position =
            new Vector3(stateMeshTransformPosition.x, updateValue / 2, stateMeshTransformPosition.z);
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
        
        // This isn't an error. Flipping z and y in the vector below is because of the 90 degree rotation from a quad's
        // origin.
        stateQuad.transform.localScale = new Vector3(stateScale.x,stateScale.z,stateScale.y);
    }


    public void UpdateVisibleActionFromPolicy(MarkovAction action) => UpdateVisibleActionFromPolicy((int) action.Action);
    public void UpdateVisibleActionFromPolicy(GridAction action) => UpdateVisibleActionFromPolicy((int) action);
    public void UpdateVisibleActionFromPolicy(int action)
    {
        foreach (var actionSprite in ActionSprites)
        {
            actionSprite.SetActive(false);
        }
        ActionSprites[action].SetActive(true);
    }
}
