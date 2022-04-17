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

    public MDP testMdp;

    public TextAsset mdpFileToLoad;



    // Start is called before the first frame update
    void Start()
    {
        string filePath = File.ReadAllText("Assets/Resources/CanonicalMDPs/RussellNorvigGridworld.json");
        testMdp = JsonUtility.FromJson<MDP>(filePath);
        // mdp = JsonUtility.FromJson<MDP>(mdpFileToLoad.text);

        _offsetToCenterVector = new Vector2((-mdp.Width / 2f), (-mdp.Height / 2f));
        
        if (mdp.Height > 1)
        {
            _offsetToCenterVector += _2Doffset;
        }
        
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
        for (int y = 0; y < mdp.Height; y++)
        {
            for (int x = 0; x < mdp.Width; x++)
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

