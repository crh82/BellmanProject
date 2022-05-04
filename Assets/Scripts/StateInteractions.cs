using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class StateInteractions : MonoBehaviour
{
    private State _state;
    public GameObject stateMesh;
    private Material _stateMaterial;
    private bool _selected = false;

    public Material highlighted;
    [FormerlySerializedAs("unhighlighted")] public Material normalColor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnMouseDown()
    {
        if (_state == null)
        {
            _state = gameObject.GetComponentInParent<State>();
            _stateMaterial = stateMesh.gameObject.GetComponent<MeshRenderer>().material;
        }
        
        Debug.Log("Clicked State Mesh");
        _state.ToggleStateInfo();
        _state.selected = !_state.selected;
        
        _selected = !_selected;
        gameObject.GetComponent<MeshRenderer>().material = _selected ? highlighted : normalColor;
        //
        // // GameObject.Find("OriginTarget").transform.position = _state.GetStateCanvas().transform.position;
        // CameraController orbiter = GameObject.Find("Orbiter").gameObject.GetComponent<CameraController>();
        // orbiter.target = _state.GetStateCanvasHover().transform;
    }
}
