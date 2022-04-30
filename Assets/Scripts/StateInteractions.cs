using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class StateInteractions : MonoBehaviour
{
    private State _state;
    private Material _stateMaterial;
    private bool _selected = false;

    public Material highlighted;
    public Material unhighlighted;

    // Start is called before the first frame update
    void Start()
    {
        _state = gameObject.GetComponentInParent<State>();
        _stateMaterial = gameObject.GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnMouseDown()
    {
        Debug.Log("Clicked State Mesh");
        _state.ToggleStateInfo();
        _selected = !_selected;
        gameObject.GetComponent<MeshRenderer>().material = _selected ? highlighted : unhighlighted;

        // GameObject.Find("OriginTarget").transform.position = _state.GetStateCanvas().transform.position;
        CameraController orbiter = GameObject.Find("Orbiter").gameObject.GetComponent<CameraController>();
        orbiter.target = _state.GetStateCanvasHover().transform;
    }
}
