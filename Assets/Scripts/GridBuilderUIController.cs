using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Debug = System.Diagnostics.Debug;

public class GridBuilderUIController : MonoBehaviour
{
    public GridBuilderManager gridBuilderManager;
    public LevelEditor        levelEditor;

    public ModalWindowManager newGridWindow;
    public TMP_InputField     xInputField;
    public TMP_InputField     yInputField;
    public TextMeshProUGUI    xValueUI;
    public TextMeshProUGUI    yValueUI;

    public Button             newGridButton;

    public ModalWindowManager sendGridToSolver;


    public TMP_InputField     rewardValueInputField;

    public TextMeshProUGUI    rewardValueDisplay;

    public TextMeshProUGUI    tileDescriptionText;
    
    // ┌──────────────────────────────┐
    // │ Environment Dynamics Related │
    // └──────────────────────────────┘
    public int                currentlyDisplayedEnvironmentDynamicsImageIndex = 0;

    public Image              currentlyDisplayedEnvironmentDynamicsImage;

    public string             environmentDynamicsPrefix = "SW";

    public HorizontalSelector environmentDynamicsSelector;

    private const string      SlipperyWalk     = "SW";
    private const string      RussellAndNorvig = "RN";
    private const string      FrozenLake       = "FL";
    private const string      RandomWalk       = "RW";
    private const string      Deterministic    = "D";
    private const string      NorthernWind     = "NW";
    
    // 

    public List<Image>        paintTileImages;
    private string            _currentlyDisplayedTile  = "s";
    private const int         ObstacleOrNoStateTile    = 4;
    private const int         StandardStateTile        = 5;
    private const int         TerminalStateGreenOrGoal = 6;
    private const int         TerminalStateRed         = 7;

    private void Awake()
    {
        GameManager.instance.currentScene = (int) BellmanScenes.MdpBuilder;
    }


    // ┌────────────────┐ 
    // │ Build New Grid │ 
    // └────────────────┘ 
    
    public void OpenGridBuildDimensionsWindow()
    {
        newGridWindow.OpenWindow();
        DisableTilePlacement();
    }

    public void BuildGrid()
    {
        gridBuilderManager.BuildGrid();
        levelEditor = gridBuilderManager.levelEditor;
        levelEditor.mdpDynamicsType = (MdpRules) environmentDynamicsSelector.index;
        SetTileToPaint(_currentlyDisplayedTile);
    }

    public void SetXDimensionValue()
    {
        int xVal = int.Parse(xInputField.text);
        if (xVal > 50) return;
        xValueUI.text = xInputField.text;
        gridBuilderManager.dimensions.x = xVal;
        SetGridBuildDimensionsWindowText();
    } 
    
    public void SetYDimensionValue()
    {
        int yVal = int.Parse(yInputField.text);
        if (yVal > 50) return;
        yValueUI.text = yInputField.text;
        gridBuilderManager.dimensions.y = yVal;
        SetGridBuildDimensionsWindowText();
    }

    void SetGridBuildDimensionsWindowText()
    {
        // <color=#FFFFFF><b></b></color>
        string text = $"Creates a new grid world of dimensions (  <color=#FFFFFF><b>{xValueUI.text}</b></color> ×  " +
                      $"<color=#FFFFFF><b>{yValueUI.text}</b></color> ), filled with standard, traversable tiles. \n \n"+
                      "While the Maximum allowable is 99 × 99, anything beyond 50 × 50, gets very slow.";
        newGridWindow.descriptionText = text;
    }
    
    // ┌───────┐ 
    // │ Solve │ <- IOW send to the solver 
    // └───────┘ 
    public void OpenSendToSolverWindowForConfirmation()
    {
        DisableTilePlacement();
        sendGridToSolver.OpenWindow();
        switch (gridBuilderManager.gridLoaded)
        {
            case false:
                sendGridToSolver.titleText = "WARNING: No grid loaded";
                sendGridToSolver.descriptionText = "First generate a grid, then send it to the solver.";
                break;
            default:
                sendGridToSolver.titleText = "Send to Solver";
                sendGridToSolver.descriptionText = "Confirm that you have finished editing the grid and send it to the solver as a Markov Decision Process.";
                break;
        }
    }
    
    public void SendGridToSolver() => gridBuilderManager.TransitionToMarkovDecisionProcessScene();
    
    // ┌──────────────────────┐
    // │ Environment Dynamics │
    // └──────────────────────┘

    public void CycleEnvironmentDynamicsImage()
    {
        if (currentlyDisplayedEnvironmentDynamicsImageIndex < 3) currentlyDisplayedEnvironmentDynamicsImageIndex++;
        else currentlyDisplayedEnvironmentDynamicsImageIndex = 0;

        LoadDynamicsSpriteFromResources();
    }

    private void LoadDynamicsSpriteFromResources()
    {
        string filePath = "Images/EnvironmentDynamics/" +
                          $"{environmentDynamicsPrefix}/" +
                          $"{environmentDynamicsPrefix}" +
                          $"{currentlyDisplayedEnvironmentDynamicsImageIndex}";

        currentlyDisplayedEnvironmentDynamicsImage.sprite = Resources.Load<Sprite>(filePath);
    }

    public void SelectEnvironmentDynamics()
    {
        switch ((MdpRules) environmentDynamicsSelector.index)
        {
            case MdpRules.SlipperyWalk:
                SetDynamics(SlipperyWalk);
                break;
            case MdpRules.RussellAndNorvig:
                SetDynamics(RussellAndNorvig);
                break;
            case MdpRules.RandomWalk:
                SetDynamics(RandomWalk);
                break;
            case MdpRules.FrozenLake:
                SetDynamics(FrozenLake);
                break;
            case MdpRules.GrastiensWindFromTheNorth:
                SetDynamics(NorthernWind);
                break;
            case MdpRules.DrunkBonanza:
                // For now I have Drunk Bonanza disabled
            case MdpRules.Deterministic:
                SetDynamics(Deterministic);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetDynamics(string dynamics)
    {
        environmentDynamicsPrefix = dynamics;
        LoadDynamicsSpriteFromResources();
        if (levelEditor == null) return;
        Debug.Assert(levelEditor != null, nameof(levelEditor) + " != null");
        levelEditor.mdpDynamicsType = (MdpRules) environmentDynamicsSelector.index;
    }

    public void SetTileToPaint(string tile)
    {
        _currentlyDisplayedTile = tile;
        void SetTile(int tileIndex)
        {
            foreach (var tileImage in paintTileImages)
            {
                tileImage.gameObject.SetActive(false);
            }
            paintTileImages[tileIndex - 4].gameObject.SetActive(true);
            if (levelEditor == null) return;
            levelEditor.AssignCurrentTile(tileIndex);
        }

        switch (tile)
        {
            case "o":
                _currentlyDisplayedTile = "o";
                SetTile(ObstacleOrNoStateTile);    
                break;
            case "s": 
                _currentlyDisplayedTile = "s";
                SetTile(StandardStateTile);        
                break;
            case "g": 
                _currentlyDisplayedTile = "g";
                SetTile(TerminalStateGreenOrGoal); 
                break;
            case "t": 
                _currentlyDisplayedTile = "t";
                SetTile(TerminalStateRed);        
                break;
        }

        // void HandleTileImage(int tileIndex)
        // {
        //     foreach (var tileImage in paintTileImages)
        //     {
        //         tileImage.gameObject.SetActive(false);
        //     }
        //     paintTileImages[tileIndex - 4].gameObject.SetActive(true);
        //     if (levelEditor == null) return;
        //     levelEditor.AssignCurrentTile(tileIndex);
        // }
        
        
    }

    public void SetTileDescriptionOnTileSelection(string tileIdentifier)
    {
        switch (tileIdentifier)
        {
            case "s":
                tileDescriptionText.text = "This is a standard tile representing a location in the state space.";
                break;
            case "o":
                tileDescriptionText.text = "This is an obstacle or unreachable state tile. The solver ignores it.";
                break;
            case "tg":
                tileDescriptionText.text =
                    "This is a terminal state. The green indicates a non-negative reward, though this is not enforced.";
                break;
            case "tr":
                tileDescriptionText.text =
                    "This is a terminal state. The red indicates a negative reward, though this is not enforced.";
                break;
        }
    }

    public void EnableTilePlacement()     => gridBuilderManager.EnableTilePlacement();
    public void DisableTilePlacement()    => gridBuilderManager.DisableTilePlacement();

    public void SwitchToRewardLayer()     => levelEditor.SwitchToRewardEditorLayer();

    public void SwitchToGridEditorLayer() => levelEditor.SwitchToGridEditorLayer();

    public void SetRewardValueForPainting()
    {
        if (!float.TryParse(rewardValueInputField.text, out float rewardValue)) return;
        levelEditor.SetRewardValue(rewardValue);
        rewardValueDisplay.text = $"{rewardValue}";
        rewardValueInputField.text = "";
    }
    
    // ┌────────────┐
    // │ Navigation │
    // └────────────┘
    public void NavigateToMainMenuScreen() => GameManager.instance.SwitchScene(BellmanScenes.Title);

}
