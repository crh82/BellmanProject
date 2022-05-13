using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Michsky.UI.ModernUIPack;
using TestScripts;
using UnityEngine;

public class TestUIControl : MonoBehaviour
{
    public float amount;

    public GameObject cube;

    public CubeScript cubeScript;

    public Vector2Int dims;

    public List<CubeScript> cubes;

    public int numBoxes;
    
    public int indexOfBox;

    public int playSpeed;

    public bool keepGoing = true;

    private float[] _transitions = new[] {0.33f, 0.33f, 0.34f};

    public int cat;

    public ModalWindowManager modal;

    private Vector2 _offsetToCenterVector;
    
    private readonly Vector2 _offsetValuesFor2DimensionalGrids = new Vector2(0.5f, 0.5f);

    public UnityEngine.Transform gridSquarePrefab;

    public GameObject stateSpace;

    private void Start()
    {
        // cubeScript = cube.GetComponent<CubeScript>();
        //
        // numBoxes = dims.x * dims.y;
        //
        // CreateCubes();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            playSpeed += 10;
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            playSpeed += 100;
        }
        
        if (Input.GetKeyDown(KeyCode.DownArrow))
        { 
            if (playSpeed > 11) playSpeed -= 10;
        } 
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (playSpeed > 110) playSpeed -= 100;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (cat > 0) cat--;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (cat < 3) cat++;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetKgFalse();
        }
    }

    public void UpdateHeightOfCube()
    {
        cubes[indexOfBox].UpdateHeight(amount);
    }
    
    public Task UpdateHeightOfCubeAsync(CubeScript cb)
    {
        cb.UpdateHeight(amount);
        return Task.CompletedTask;
    }
    
    public Task UpdateHeightOfCubeAsync(CubeScript cb, float value)
    {
        cb.UpdateHeight(value);
        return Task.CompletedTask;
    }

    public async Task UpdateAllCubesAsync()
    {
        foreach (var cs in cubes)
        {
            cs.UpdateHeight(amount);
        }
        await Task.Yield();
    }

    public async Task UpdateEachCube()
    {
        foreach (var cb in cubes)
        {
            Task updated = UpdateHeightOfCubeAsync(cb);
            
            await updated;
            
            await Task.Delay(playSpeed);
        }
    }

    public async Task UpdateEachTransition()
    {
        foreach (var cb in cubes)
        {
            foreach (var transition in _transitions)
            {
                Task cbUpdate = UpdateHeightOfCubeAsync(cb, transition);

                await cbUpdate;

                await Task.Delay(playSpeed);
            }
        }
    }

    public void CreateCubes()
    {
        var id = 0;
        
        for (int y = 0; y < dims.y; y++)
        {
            for (int x = 0; x < dims.x; x++)
            {
                var box = Instantiate(cube, new Vector3(x, 0, y), Quaternion.identity);
                box.name = $"{id}";
                cubes.Add(box.GetComponent<CubeScript>());
                id++;
            }
        }
    }
    

    public void SetKgTrue()
    {
        keepGoing = true;
    }

    public void SetKgFalse()
    {
        keepGoing = false;
    }

    public async void SimulateUpdates()
    {
        Debug.Log("Started");
        SetKgTrue();

        var k = 1;
        
        while (keepGoing)
        {
            if (k % 10 == 0) Debug.Log(k);
            
            if (k > 200_000)
            {
                Debug.Log("Maxed");
                break;
            }
            
            switch (cat)
            {
                case 0:
                    
                    await UpdateAllCubesAsync();
                    
                    await Task.Delay(playSpeed);

                    break;
                
                case 1:
                    await UpdateEachCube();

                    await Task.Delay(playSpeed);

                    break;
                
                case 2:
                    await UpdateEachTransition();

                    // await Task.Delay(playSpeed);

                    break;
                
                case 3:
                    await Task.Delay(playSpeed);
                    break;
            }
            
            k++;
        }
        
    }
}
