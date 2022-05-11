using System.Collections;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StateAction : MonoBehaviour
{
    public float stateValue;
    
    public int stateIndex;

    public GridAction currentAction;

    public GridAction previousAction;
    
    
    public GameObject stateMesh;

    private Transform _stateMeshTransform;

    public GameObject textContainer;

    public Canvas hoverCanvas;

    public TextMeshProUGUI hoveringText;

    private const float HoverTextOffset = 0.05f;

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

    
    
    
    
    // Start is called before the first frame update
    private void Start()
    {
        hoverCanvas.worldCamera = Camera.main;

        _stateMeshTransform = stateMesh.transform;
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

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

        // hoveringText.text = 2 > 50 
        hoveringText.text = _mdpManager.mdp.StateCount > 50 
            ? $"{Math.Round(stateValue, 2)}" : $"{Math.Round(stateValue, 4)}";
        
        await UpdateTextThatHoversAboveState();
        
    }
    
    private Task UpdateTextThatHoversAboveState()
    {
        var stateMeshTransform = stateMesh.transform;
        var localScale = stateMeshTransform.localScale;
        var position = stateMeshTransform.position;

        hoverCanvas.transform.position = stateValue < 0
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
    public void ShowActionSprites() => actionSpritesContainer.SetActive(true);

    public void ShowPreviousActionSprites() => previousActionSpritesContainer.SetActive(true);

    public void ShowActionObjects() => actionMeshesContainer.SetActive(true);
    
    public void HideActionSprites() => actionSpritesContainer.SetActive(false);

    public void HidePreviousActionSprites() => previousActionSpritesContainer.SetActive(false);

    public void HideActionObjects() => actionMeshesContainer.SetActive(false);

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