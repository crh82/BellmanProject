using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Windows;
using File = System.IO.File;
using Random = UnityEngine.Random;

/// <summary>
/// This script draws on Sebastian Lague's MapGenerator tutorial.
/// See https://github.com/SebLague/Create-a-Game-Source/blob/master/Episode%2008/MapGenerator.cs
/// </summary>
public class MDPGenerator : MonoBehaviour
{
    public Transform statePrefab;

    public Vector2 mdpGridWorldDimensions = Vector2.one;

    [Range(0, 1)] public float gapBetweenStates = 0.25f;

    private float _offsetToCenterGridX;
    
    private float _offsetToCenterGridY;

    private Vector2 _offsetToCenterVector;

    private Vector2 _2Doffset = new Vector2(0.5f, 0.5f);

    public Transform stateSpace;

    public MDP mdp;

    public TextAsset mdpFileToLoad;

    

    // Start is called before the first frame update
    void Start()
    {
        mdp = JsonUtility.FromJson<MDP>(mdpFileToLoad.text);
        // var jsonFile = Resources.Load<TextAsset>("Assets/Resources/CanonicalMDPs/RussellNorvigGridworld-v0");
        //Todo Fix this shit
        // string fileName = File.ReadAllText("/Users/christopherhowell/UnityCreateWithCode/BellmanV0/Assets/CanonicalMDPs/RussellNorvigGridworld-v0.json");
        // Console.WriteLine();
        // _mdp = JsonUtility.FromJson<MDP>(fileName);
        // _mdp = MDP.CreateFromJSON(jsonFile.ToString());
        // Debug.Log($"State probability is : {_mdp.TransitionFunction[0][0][0].Probability}");
        // _offsetToCenterGridX = -mdpGridWorldDimensions.x / 2 + 0.5f;
        // _offsetToCenterGridY = -mdpGridWorldDimensions.y / 2 + 0.5f;
        
        _offsetToCenterVector = new Vector2((-mdp.dimX / 2), (-mdp.dimY / 2));
        if (mdp.dimY > 1)
        {
            _offsetToCenterVector += _2Doffset;
        }

        Vector2 testLocation = new Vector2(2, 2);
        testLocation += Vector2.down;
        
        // _offsetToCenterGridX = (-mdp.dimX / 2) + 0.5f;
        // _offsetToCenterGridY = (-mdp.dimY / 2) + 0.5f;
        //
        // _offsetToCenterGridX = (-mdp.dimX / 2);
        // _offsetToCenterGridY = (-mdp.dimY / 2);
        // if (mdp.dimY > 1){_offsetToCenterGridY = -mdp.dimY / 2 + 0.5f;}
        // else {_offsetToCenterGridY = 1;}
        //
        
        InstantiateMdpGridWorld();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InstantiateMdpGridWorld()
    {
        int id = 0;
        
        // for (int y = 0; y < mdpGridWorldDimensions.y; y++)
        // for (int x = 0; x < mdpGridWorldDimensions.x; x++)
        for (int y = 0; y < mdp.dimY; y++)
        {
            for (int x = 0; x < mdp.dimX; x++)
            {
                Vector3 scale = new Vector3(
                    (1 - gapBetweenStates),
                    2.0f,
                    (1 - gapBetweenStates));
                
                // // Todo remove this block. This is just for testing
                // float yScale = Random.Range(0.5f, 7.6f);
                float yScale = scale.y;
                float yPositionOffset = yScale / 2;
                //
                // Vector3 statePosition = new Vector3(
                //     _offsetToCenterGridX + x, 
                //     yPositionOffset, 
                //     _offsetToCenterGridY + y);
                
                Vector3 statePosition = new Vector3(
                    _offsetToCenterVector.x + x, 
                    yPositionOffset, 
                    _offsetToCenterVector.y + y);
                
                Transform state = Instantiate(
                    statePrefab, 
                    statePosition, 
                    Quaternion.Euler(Vector3.zero));

                // state.localScale = new Vector3(
                //     (1 - gapBetweenStates), 
                //     0, 
                //     (1 - gapBetweenStates));
                
                // state.Find("StateMesh").localScale = new Vector3(
                //     (1 - gapBetweenStates), 
                //     2.0f, 
                //     (1 - gapBetweenStates));

                state.Find("StateMesh").localScale = scale;
                
                state.parent = stateSpace;
                state.name = $"{x}{y}";
                
                GameObject currentState = GameObject.Find($"{x}{y}");
                State curSt = currentState.GetComponent<State>();
                curSt.stateIDNum = id;
                // if (curSt.stateIDNum == 0 || curSt.stateIDNum == 5)
                // {
                //     currentState.SetActive(false);
                // }
                id++;

            }
        }
    }
}

