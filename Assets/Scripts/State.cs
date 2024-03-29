using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class State : MonoBehaviour
{
    public GameObject stateQuad;

    public float      stateValue;
    
    public int        stateIndex;

    public StateType  stateType;

    public Vector3    statePosition;

    
    
    public  GameObject      stateMesh;

    private Transform       _stateMeshTransform;

    public  GameObject      textContainer;

    public  Canvas          hoverCanvas;

    public  TextMeshProUGUI hoveringText;

    private const float     HoverTextOffset = 0.02f;

    public  bool            selected;
    
    public Material         highlighted;
    
    public Material         normalColor;
    

    public GameObject actionSpritesContainer;
    
    public GameObject actionMeshesContainer;

    public GameObject previousActionSpritesContainer;
    

    public List<GameObject>           actionSprites;

    public List<GameObject>           previousActionSprites;
    
    public List<GameObject>           actionGameObjects;
    
    public List<GameObject>           actionTargetGameObjects;
    
    public readonly Queue<GridAction> currentActionPreviousAction = new Queue<GridAction>();


    public Transform leftMesh;

    public Transform downMesh;

    public Transform rightMesh;

    public Transform upMesh;
    

    private MdpManager        _mdpManager;

    private StateInteractions _stateInteractionsScript;
    

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        hoverCanvas.worldCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// <para>
    /// Sets a reference to the Scene's (usually the solver scene) MdpManager in this state object so the state can
    /// communicate back to the MdpManager when it needs to. 
    /// </para>
    /// <para>
    /// Also assigns the <c>StateInteractions</c> which handles click events on the individual states (for things like
    /// highlighting and editing. 
    /// </para>
    /// </summary>
    /// <param name="mdpManager">Scene's (usually the solver scene) MdpManager</param>
    public void DistributeMdpManagerReferenceToComponents(MdpManager mdpManager)
    {
        if (_stateInteractionsScript == null)
        {
            _stateInteractionsScript = GetComponent<StateInteractions>();
        }
        _mdpManager = mdpManager;
    }
    
    
    // —————————— ╔═════════════════════╗ —————————————
    // —————————— ║VISUALISATION METHODS║ —————————————
    // —————————— ╚═════════════════════╝ —————————————

    public void SetStateScale(Vector3 stateScale)
    {
        stateMesh.transform.localScale = stateScale;
        
        // This isn't an error. Flipping z and y in the vector below is because of the 90 degree rotation from a quad's
        // origin.
        stateQuad.transform.localScale = new Vector3(stateScale.x,stateScale.z,stateScale.y);
    }
    
    public void StateHighlightToggle() => stateMesh.GetComponent<MeshRenderer>().material = selected ? highlighted : normalColor;

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
    
    
    // ┌─────────┐
    // │ Heights │
    // └─────────┘
    public async Task UpdateAllActionHeightsAsync(float value)
    {
        var index = 0;
        
        foreach (var actionGameObject in actionGameObjects)
        {
            SetActionValueTargetPosition(index, value);
            
            await UpdateObjectHeight(actionGameObject, value, true);
            
            index++;
        }
    }

    public async Task UpdateActionHeightAsync(int action, float value)
    {
        SetActionValueTargetPosition(action, value);

        await UpdateObjectHeight(actionGameObjects[action], value, true);
    }

    private void SetActionValueTargetPosition(int action, float value)
    {
        var oldPosition = actionTargetGameObjects[action].transform.position;

        actionTargetGameObjects[action].transform.position = new Vector3(oldPosition.x, value, oldPosition.z);
    }

    public async Task UpdateStateHeightAsync(float value)
    {
        stateValue = value;
        
        await UpdateObjectHeight(stateMesh, value);
        
        // This controls the amount of text visible on the screen. TextMeshPro or really any text heavily slows down the 
        // framerate so with larger state spaces we reduce the number of significant digits displayed.
        if (stateType == StateType.Standard)
        {
            hoveringText.text = _mdpManager.Mdp.StateCount switch
            {
                var i1 when i1 < 50 => $"{Math.Round(stateValue, 4)}",
                var i2 when (i2 > 50 && i2 <= 100) => $"{Math.Round(stateValue, 3)}",
                var i3 when i3 > 100 => $"{Math.Round(stateValue, 2)}",
                _ => $"{Math.Round(stateValue, 4)}"
            };
        }
        else
        {
            hoveringText.text = $"{Math.Round(stateValue, 4)}";
        }

        await UpdateTextThatHoversAboveState();
        
    }
    
    private Task UpdateObjectHeight(GameObject stateOrAction,  float value, bool action=false)
    {
        var stateOrActionTransform = stateOrAction.transform;
        var stateOrActionTransformLocalScale = stateOrAction.transform.localScale;
        var stateOrActionTransformPosition = stateOrAction.transform.position;

        // So the visual representation is never actually zero because it looks wonky and the colliders don't seem to work.
        float updateValue = -0.01 < value && value < 0.01 ? 0.01f : value;

        if (action)
        {
            stateOrActionTransform.localScale = ActionHeightTranslator(stateOrActionTransformLocalScale, updateValue);
        }
        else
            stateOrActionTransform.localScale = new Vector3(stateOrActionTransformLocalScale.x, updateValue,
                stateOrActionTransformLocalScale.z);

        stateOrActionTransform.position =
            new Vector3(stateOrActionTransformPosition.x, updateValue / 2, stateOrActionTransformPosition.z);
        
        return Task.CompletedTask;
    }

    private Vector3 ActionHeightTranslator(Vector3 oldHeight, float value) => new Vector3(oldHeight.x, oldHeight.y, value);

    // ┌──────┐
    // │ Text │
    // └──────┘
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

    // ┌─────────┐
    // │ Toggles │
    // └─────────┘
    public void ToggleActionSprites()         => actionSpritesContainer.SetActive(!actionSpritesContainer.activeSelf);

    public void TogglePreviousActionSprites() => previousActionSpritesContainer.SetActive(!previousActionSpritesContainer.activeSelf);

    public void ToggleActionObjects()         => actionMeshesContainer.SetActive(!actionMeshesContainer.activeSelf);

    public void ToggleStateValue()            => stateMesh.SetActive(!stateMesh.activeSelf);

    public void ToggleStateValueText()        => hoverCanvas.gameObject.SetActive(!hoverCanvas.gameObject.activeSelf);
    
    // Switch ON
    public void ToggleOnActionSprites()          => actionSpritesContainer.SetActive(true);

    public void ToggleOnPreviousActionSprites()  => previousActionSpritesContainer.SetActive(true);

    public void TogglOnActionObjects()           => actionMeshesContainer.SetActive(true);

    public void ToggleOnStateValue()             => stateMesh.SetActive(true);

    public void ToggleOnStateValueText()         => hoverCanvas.gameObject.SetActive(true);
    
    // Switch OFF
    public void ToggleOffActionSprites()         => actionSpritesContainer.SetActive(false);

    public void ToggleOffPreviousActionSprites() => previousActionSpritesContainer.SetActive(false);

    public void TogglOffActionObjects()          => actionMeshesContainer.SetActive(false);

    public void ToggleOffStateValue()            => stateMesh.SetActive(false);

    public void ToggleOffStateValueText()        => hoverCanvas.gameObject.SetActive(false);

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
    
    public void UpdateActionSprite(GridAction action, bool edited = false)
    {
        UpdateActionSprites(action);
        
        currentActionPreviousAction.Enqueue(action);

        if (currentActionPreviousAction.Count != 2) return;
        
        var previous = currentActionPreviousAction.Dequeue();
        
        UpdateActionSprites(previous, true);
    }
    
    public void UpdateActionSprite(MarkovAction action) => UpdateActionSprite(action.Action);
    public void UpdateActionSprite(int actionIndex)     => UpdateActionSprite((GridAction) actionIndex);
}
