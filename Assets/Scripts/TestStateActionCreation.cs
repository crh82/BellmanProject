using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TestStateActionCreation : MonoBehaviour
{
    
    public Vector2Int dims;
    
    private Vector2 _offsetToCenterVector;
    
    private readonly Vector2 _offsetValuesFor2DimensionalGrids = new Vector2(0.5f, 0.5f);

    public Transform gridSquarePrefab;

    public Transform stateSpacePrefab;

    public Transform stateActionPrefab;
    
    public float testHeight;

    public List<GameObject> things;

    public GameObject mum;

    public TEXDraw stuff;
    
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
                
                things.Add(state.gameObject);
                
                id++;
            }
        }
        // await Task.Yield();
        
        // var try = " + $"{ }" + @";
        StringBuilder inj = new StringBuilder();
        inj.Append(@"\[\sum_{");
        inj.Append(VariableInjector(id)).Append(Ender());
        
        var amount = id * 2;
        stuff.text = @"\[\sum_{" + $"{id}" + @"}^{" + $"{amount}" + @"}\]";
        stuff.text = inj.ToString();
    }

    public string VariableInjector<T>(T injection)
    {
        return "{" + $"{injection}";
    }

    public string Ender()
    {
        return @"}\]";
    }
}
// Injector
// " + $"{ }" + @"