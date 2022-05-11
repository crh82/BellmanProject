using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

public class State : MonoBehaviour
{

    // [FormerlySerializedAs("_stateInformationText")] public TextMeshProUGUI stateInformationText;
    //
    // public bool             isStateInfoActive;
    //
    // public float            stateValue = 0f;
    //
    // public GameObject       stateMesh;
    //
    // public int              stateIndex;
    //
    // public TextMeshPro      hoveringText;
    //
    // public GameObject       textContainer;
    //
    // private const float HoverTextOffset = 0.05f;
    //
    public GameObject       stateQuad;
    //
    // public bool             selected;
    //
    // public Canvas hoverCanvas;
    //
    // [FormerlySerializedAs("ActionSprites")] public List<GameObject> actionSprites;
    //
    // private MdpManager      _mdpManager;
    //
    // private StateInteractions _stateInteractionsScript;
    
    public float stateValue;
    
    public int stateIndex;

    public StateType stateType;

    public GridAction currentAction;

    public GridAction previousAction;
    
    
    public GameObject stateMesh;

    private Transform _stateMeshTransform;

    public GameObject textContainer;

    public Canvas hoverCanvas;

    public TextMeshProUGUI hoveringText;

    private const float HoverTextOffset = 0.02f;

    public bool selected;
    

    public GameObject actionSpritesContainer;
    
    public GameObject actionMeshesContainer;

    public GameObject previousActionSpritesContainer;
    

    public List<GameObject> actionSprites;

    public List<GameObject> previousActionSprites;
    
    public List<GameObject> actionGameObjects;

    public readonly Queue<GridAction> CurrentActionPreviousAction = new Queue<GridAction>();


    public Transform leftMesh;

    public Transform downMesh;

    public Transform rightMesh;

    public Transform upMesh;
    

    private MdpManager        _mdpManager;

    private StateInteractions _stateInteractionsScript;


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
        hoverCanvas.worldCamera = Camera.main;
        // SetupStateVisualisation();
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    private void SetupStateVisualisation()
    {
        // (Canvases) 
        // Canvas hoverCanvas = gameObject.GetComponentInChildren<Canvas>();
        hoverCanvas.worldCamera = Camera.main;
        
        // foreach (Canvas canvas in _stateCanvasArray)
        // {
        //     canvas.worldCamera = Camera.main;
        // }
        //
        // _stateCanvasHover = _stateCanvasArray[0];
        // _stateCanvasFlat  = _stateCanvasArray[1];
        //
        // _stateCanvasHover.gameObject.SetActive(false);
        // _stateCanvasFlat.gameObject.SetActive(false);
        // (Canvases end)
    }

    public void DistributeMdpManagerReferenceToComponents(MdpManager mdpManager)
    {
        if (_stateInteractionsScript == null)
        {
            _stateInteractionsScript = GetComponent<StateInteractions>();
        }
        
        _mdpManager = mdpManager;
        // _stateInteractionsScript.AssignMdpManager(mdpManager);
    }
    
    
    // —————————— ╔═════════════════════╗ —————————————
    // —————————— ║VISUALISATION METHODS║ —————————————
    // —————————— ╚═════════════════════╝ —————————————
    
    public void ToggleStateInfo()
    {
        // textContainer.SetActive(!isStateInfoActive);
        // stateInformationText.text = $"V( {stateIndex} ) = {Math.Round(stateValue, 3)}";
    }

    // public void UpdateHeight(float value)
    // {
    //     stateValue = value;
    //     var stateMeshTransform = stateMesh.transform;
    //     var stateMeshTransformLocalScale = stateMesh.transform.localScale;
    //     var stateMeshTransformPosition = stateMesh.transform.position;
    //
    //     // TODO: Might be a bad solution.
    //     float updateValue = -0.01 < value && value < 0.01 ? 0.01f : value;
    //
    //     stateMeshTransform.localScale =
    //         new Vector3(stateMeshTransformLocalScale.x, updateValue, stateMeshTransformLocalScale.z);
    //     stateMeshTransform.position =
    //         new Vector3(stateMeshTransformPosition.x, updateValue / 2, stateMeshTransformPosition.z);
    //     hoveringText.text = $"{Math.Round(stateValue, 4)}";
    //     
    //     UpdateStateValueVisual();
    // }

    // public Task UpdateHeightAsync(float value)
    // {
    //     stateValue = value;
    //     var stateMeshTransform = stateMesh.transform;
    //     var stateMeshTransformLocalScale = stateMesh.transform.localScale;
    //     var stateMeshTransformPosition = stateMesh.transform.position;
    //
    //     // TODO: Might be a bad solution.
    //     float updateValue = -0.01 < value && value < 0.01 ? 0.01f : value;
    //
    //     stateMeshTransform.localScale =
    //         new Vector3(stateMeshTransformLocalScale.x, updateValue, stateMeshTransformLocalScale.z);
    //     stateMeshTransform.position =
    //         new Vector3(stateMeshTransformPosition.x, updateValue / 2, stateMeshTransformPosition.z);
    //     
    //     hoveringText.text = _mdpManager.mdp.StateCount > 50 ? $"{Math.Round(stateValue, 2)}" : $"{Math.Round(stateValue, 4)}";
    //     
    //     UpdateStateValueVisual();
    //     
    //     return Task.CompletedTask;
    // }

    // private void UpdateStateValueVisual()
    // {
    //     var stateMeshTransform = stateMesh.transform;
    //     var localScale = stateMeshTransform.localScale;
    //     var position = stateMeshTransform.position;
    //
    //     textContainer.transform.position = stateValue < 0
    //         ? new Vector3(position.x, HoverTextOffset, position.z)
    //         : new Vector3(position.x, localScale.y + HoverTextOffset, position.z);
    // }

    public void SetStateScale(Vector3 stateScale)
    {
        stateMesh.transform.localScale = stateScale;
        
        // This isn't an error. Flipping z and y in the vector below is because of the 90 degree rotation from a quad's
        // origin.
        stateQuad.transform.localScale = new Vector3(stateScale.x,stateScale.z,stateScale.y);
    }


    // public void UpdateVisibleActionFromPolicy(MarkovAction action) => UpdateVisibleActionFromPolicy((int) action.Action);
    // public void UpdateVisibleActionFromPolicy(GridAction action) => UpdateVisibleActionFromPolicy((int) action);
    // public void UpdateVisibleActionFromPolicy(int action)
    // {
    //     foreach (var actionSprite in actionSprites)
    //     {
    //         actionSprite.SetActive(false);
    //     }
    //     actionSprites[action].SetActive(true);
    // }
    
    
    // ┌────────────────┐
    // │ Initialization │
    // └────────────────┘
    public async void SetInitialHeights(float scale)
    {
        var setState   = UpdateStateHeightAsync(scale);
        var setActions = UpdateAllActionHeightsAsync(scale);

        await setState;
        await setActions;
    }
    
    public async Task UpdateAllActionHeightsAsync(float value)
    {
        foreach (var actionGameObject in actionGameObjects)
        {
            await UpdateObjectHeight(actionGameObject, value, true);
        }
    }
    
    
    
    public async Task UpdateStateHeightAsync(float value)
    {
        stateValue = value;
        
        await UpdateObjectHeight(stateMesh, value);

        hoveringText.text = $"{Math.Round(stateValue, 4)}";
        // hoveringText.text = _mdpManager.mdp.StateCount switch
        // {
        //     var i when i < 50             => $"{Math.Round(stateValue, 4)}",
        //     var i when i > 50 && i <= 100 => $"{Math.Round(stateValue, 3)}",
        //     var i when i > 100            => $"{Math.Round(stateValue, 2)}",
        //     _ => hoveringText.text
        // };

        // hoveringText.text = 2 > 50 
        // hoveringText.text = _mdpManager.mdp.StateCount > 50 
        //     ? $"{Math.Round(stateValue, 2)}" : $"{Math.Round(stateValue, 4)}";
        
        await UpdateTextThatHoversAboveState();
        
    }
    
    private Task UpdateTextThatHoversAboveState()
    {
        var stateMeshTransform = stateMesh.transform;
        var localScale = stateMeshTransform.localScale;
        var position = stateMeshTransform.position;

        textContainer.transform.position = stateValue < 0
            ? new Vector3(position.x, HoverTextOffset, position.z)
            : new Vector3(position.x, localScale.y + HoverTextOffset, position.z);
        
        return Task.CompletedTask;
    }

    private Task UpdateObjectHeight(GameObject stateOrAction,  float value, bool action=false)
    {
        var stateOrActionTransform = stateOrAction.transform;
        var stateOrActionTransformLocalScale = stateOrAction.transform.localScale;
        var stateOrActionTransformPosition = stateOrAction.transform.position;

        // So the visual representation is never actually zero because it looks wonky and the colliders don't seem to work.
        float updateValue = -0.01 < value && value < 0.01 ? 0.01f : value;

        if (action)
            stateOrActionTransform.localScale = ActionHeightTranslator(stateOrActionTransformLocalScale, updateValue);
        else
            stateOrActionTransform.localScale = new Vector3(stateOrActionTransformLocalScale.x, updateValue,
                stateOrActionTransformLocalScale.z);

        stateOrActionTransform.position =
            new Vector3(stateOrActionTransformPosition.x, updateValue / 2, stateOrActionTransformPosition.z);
        
        return Task.CompletedTask;
    }

    
    private Vector3 ActionHeightTranslator(Vector3 oldHeight, float value)
    {
        Debug.Log(oldHeight.ToString());
        Debug.Log(value);
        return new Vector3(oldHeight.x, oldHeight.y, value);
    }
    
    
    // ┌─────────┐
    // │ Toggles │
    // └─────────┘
    public void ToggleActionSprites()         => actionSpritesContainer.SetActive(!actionSpritesContainer.activeSelf);

    public void TogglePreviousActionSprites() => previousActionSpritesContainer.SetActive(!actionSpritesContainer.activeSelf);

    public void ToggleActionObjects()         => actionMeshesContainer.SetActive(!actionSpritesContainer.activeSelf);
    
    public void HideActionSprites()         => actionSpritesContainer.SetActive(false);

    public void HidePreviousActionSprites() => previousActionSpritesContainer.SetActive(false);

    public void HideActionObjects()         => actionMeshesContainer.SetActive(false);

    // ┌────────────────┐
    // │ Action Sprites │
    // └────────────────┘
    private void UpdateActionSprites(GridAction action, bool previous = false)
    {
        var actionSpriteList = previous ? previousActionSprites : actionSprites;
        
        foreach (var actionSprite in actionSpriteList)
        {
            actionSprite.SetActive(false);
        }
        actionSpriteList[(int) action].SetActive(true);
    }
    
    public void UpdateActionSprite(GridAction action)
    {
        UpdateActionSprites(action);
        
        CurrentActionPreviousAction.Enqueue(action);

        if (CurrentActionPreviousAction.Count != 2) return;
        
        var previous = CurrentActionPreviousAction.Dequeue();
        
        UpdateActionSprites(previous, true);
    }
    
    public void UpdateActionSprite(MarkovAction action) => UpdateActionSprite(action.Action);
    public void UpdateActionSprite(int actionIndex)     => UpdateActionSprite((GridAction) actionIndex);
}
