using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class StateInteractions : MonoBehaviour
{
    private State             _state;
    public GameObject         stateMesh;
    private Material          _stateMaterial;
    public ModalWindowManager stateInformationWindow;
    private MdpManager        _mdpManager;
    private UIController      _uiController;

    public Material highlighted;
    public Material normalColor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        _mdpManager = GameObject.FindGameObjectWithTag("MdpManager").GetComponent<MdpManager>();
        // stateInformationWindow = GameObject.FindGameObjectWithTag("StateInformationWindow")
        //     .GetComponent<ModalWindowManager>();
        _uiController = GameObject.FindGameObjectWithTag("PolicyEvaluationUI").GetComponent<UIController>();
        stateInformationWindow = _uiController.stateInformationWindow;
        _state = gameObject.GetComponentInParent<State>();
        _stateMaterial = stateMesh.gameObject.GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AssignMdpManager(MdpManager mdpManager)
    {
        _mdpManager = mdpManager;
    }
    
    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
            
        _state.selected = !_state.selected;

        _uiController.SetStateToEdit(_state.stateIndex);
        
        _uiController.OpenStateInformationEditorAndDisplay(_state.stateIndex);
    }

    private string GetStateInformationText()
    {
        MarkovState currentState = _mdpManager.mdp.States[_state.stateIndex];
        // GridAction action;
        //
        // if (_mdpManager.CurrentPolicy != null)
        // {
        //     action = _mdpManager.CurrentPolicy.GetAction(_mdpManager.mdp.States[_state.stateIndex]);
        // }
        var stateName   = $"<b>S</b><sub>{_state.stateIndex}</sub>";
        var stateReward = $"R({stateName}) = {currentState.Reward}";
        var stateValue  = $"V({stateName}) = {_state.stateValue}";
        
        var stateInfo = $"{stateName}\n{stateReward}\n{stateValue}";

        return stateInfo;
    }

    public async Task SetDisplayInfo()
    {
        await DisplayInformation();
    }
    
    public Task DisplayInformation()
    {
        stateInformationWindow.OpenWindow();
        
        stateInformationWindow.windowDescription.text = GetStateInformationText();

        return Task.CompletedTask;

    }
    
}
