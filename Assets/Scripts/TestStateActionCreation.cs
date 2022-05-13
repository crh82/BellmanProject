using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class TestStateActionCreation : MonoBehaviour
{
    
    public Vector2Int dims;
    
    private Vector2 _offsetToCenterVector;
    
    private readonly Vector2 _offsetValuesFor2DimensionalGrids = new Vector2(0.5f, 0.5f);

    public Transform gridSquarePrefab;

    public Transform stateSpacePrefab;

    public Transform stateActionPrefab;
    
    public float testHeight;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Build()
    {
        BuildStateActions();
        // Task built = BuildStateActions();
        
       // Debug.Log(built.Status);
    }
    
    public void BuildStateActions()
    {
        var id = 0;
        
        _offsetToCenterVector = new Vector2((-dims.x / 2f), (-dims.y / 2f));

        if (dims.y > 1)
        {
            _offsetToCenterVector += _offsetValuesFor2DimensionalGrids;
        }
        
        var stateSpace = Instantiate(
            stateSpacePrefab, 
            transform, 
            true);
        
        for (int y = 0; y < dims.y; y++)
        {
            for (int x = 0; x < dims.x; x++)
            {
                Instantiate(gridSquarePrefab, new Vector3(_offsetToCenterVector.x + x, 0f, _offsetToCenterVector.y + y), Quaternion.identity, stateSpace);
                
                var   scale              = testHeight;
                
                var   statePosition      = new Vector3(_offsetToCenterVector.x + x, (scale / 2), _offsetToCenterVector.y + y);

                var state = Instantiate(stateActionPrefab, statePosition, Quaternion.identity, stateSpace);

                state.name = $"s{id} at ({x},{y})";

                state.parent = stateSpace;

                var stateAction = state.GetComponent<StateAction>();
                
                stateAction.SetInitialHeights(scale);
                
                id++;
            }
        }
        // await Task.Yield();
    }
}
