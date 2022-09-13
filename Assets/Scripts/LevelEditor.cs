using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class LevelEditor : MonoBehaviour
{
    [SerializeField] public Tilemap        currentTileMap;
    [SerializeField] public TileBase       currentTile;
    [SerializeField] public Camera         mainCamera;
    [SerializeField] public List<TileBase> tiles;
    [SerializeField] public List<Tilemap>  tileMaps;
    [SerializeField] public List<TileBase> nonStandardTiles;
    [SerializeField] public TileBase       rewardTile;

    public int currentLayer = 1;
    public bool inPolicyLayer;
    public bool inRewardLayer;
    public float reward;
    public MdpRules mdpDynamicsType;

    private const int BackgroundLayer =  0;
    private const int MainTileLayer   =  1;
    private const int PolicyLayer     =  2;
    private const int RewardLayer     =  3;
    
    public TextMeshProUGUI testText;
    public Dictionary<int, GridRewardText> rewardText = new Dictionary<int, GridRewardText>();
    public Dictionary<int, Vector3Int> rewardTileLocations = new Dictionary<int, Vector3Int>();
    public GameObject displayRewardPrefab;

    public GridBuilderManager gridBuilderManager;

    public Dictionary<int, string> rewardDict = new Dictionary<int, string>();
    
    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetMouseButton(0))
        {
            var location = currentTileMap.WorldToCell(mainCamera.ScreenToWorldPoint(Input.mousePosition));
            PlaceTile(location);
            
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(0))
        {
            DeleteTile(currentTileMap.WorldToCell(mainCamera.ScreenToWorldPoint(Input.mousePosition)));
        }
        
        if (Input.GetKey(KeyCode.Keypad0)){AssignCurrentTile(0);}
        if (Input.GetKey(KeyCode.Keypad1)){AssignCurrentTile(1);}
        if (Input.GetKey(KeyCode.Keypad2)){AssignCurrentTile(2);}
        if (Input.GetKey(KeyCode.Keypad3)){AssignCurrentTile(3);}
        if (Input.GetKey(KeyCode.Keypad4)){AssignCurrentTile(4);}
        if (Input.GetKey(KeyCode.Keypad5)){AssignCurrentTile(5);}
        if (Input.GetKey(KeyCode.Keypad6)){AssignCurrentTile(6);}
        if (Input.GetKey(KeyCode.Keypad7)){AssignCurrentTile(7);}
        
        if (Input.GetKey(KeyCode.Alpha0)){mdpDynamicsType = (MdpRules) 0;}
        if (Input.GetKey(KeyCode.Alpha1)){mdpDynamicsType = (MdpRules) 1;}
        if (Input.GetKey(KeyCode.Alpha2)){mdpDynamicsType = (MdpRules) 2;}
        if (Input.GetKey(KeyCode.Alpha3)){mdpDynamicsType = (MdpRules) 3;}
        if (Input.GetKey(KeyCode.Alpha4)){mdpDynamicsType = (MdpRules) 4;}
        if (Input.GetKey(KeyCode.Alpha5)){mdpDynamicsType = (MdpRules) 5;}
        if (Input.GetKey(KeyCode.Alpha6)){mdpDynamicsType = (MdpRules) 6;}
        
        if (Input.GetKey(KeyCode.P)) {SwitchToPolicyLayer();}
     
        if (Input.GetKey(KeyCode.O)) {SwitchToGridEditorLayer();}

        if (Input.GetKey(KeyCode.R)) {SwitchToRewardEditorLayer();}
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            PrintMap();
        }

       
    }

    public void SwitchToRewardEditorLayer()
    {
        AssignCurrentLayer(3);
        inPolicyLayer = false;
        inRewardLayer = true;
    }

    public void SwitchToGridEditorLayer()
    {
        AssignCurrentLayer(1);
        inPolicyLayer = false;
        inRewardLayer = false;
    }

    private void SwitchToPolicyLayer()
    {
        AssignCurrentLayer(2);
        inPolicyLayer = true;
        inRewardLayer = false;
    }


    void PlaceTile(Vector3Int position)
    {
        if (!gridBuilderManager.TilesCanBePlaced()) return;
        
        if (PositionIsOutsideGridBoundaries(position)) return;

        var correspondingTileInMainLayer = tileMaps[1].GetTile(position);

        bool nope = nonStandardTiles.Contains(correspondingTileInMainLayer);
        
        int index = LocationToIndex(position.x, position.y, tileMaps[1].cellBounds.xMax);

        switch (currentLayer)
        {
            case BackgroundLayer: 
                return;
            
            case MainTileLayer:
                
                if (correspondingTileInMainLayer.name == "o") 
                    DisableRewardTextDisplayInUI(index);
                else 
                    EnableRewardTextDisplayInUI(index);
                
                currentTileMap.SetTile(position, currentTile);
                
                break;
            
            case PolicyLayer:
                
                if (nope) return; // Stops action tiles being placed in non-policy layers.
                
                currentTileMap.SetTile(position, currentTile);
                
                break;
            
            case RewardLayer:
                
                switch (correspondingTileInMainLayer.name)
                {
                    case "o": 
                        break;
                    case "s":
                    case "g":
                    case "t":
                        SetRewardValue(index);
                        
                        // if (rewardDict.ContainsKey(idx))
                        //     rewardDict[idx] = rewardTile.name;
                        // else
                        //     rewardDict.Add(idx, rewardTile.name);

                        currentTileMap.SetTile(position, rewardTile);
                        
                        break;
                }
                break;
        }
        
        
        // if (inPolicyLayer && nope) return;
        //
        // switch (inRewardLayer)
        // {
        //     case true when correspondingTileInMainLayer.name == "o": return;
        //     case true:
        //     {
        //         rewardTile.name = $"{reward}";
        //
        //         idx = LocationToIndex(position.x, position.y, tileMaps[1].cellBounds.xMax);
        //
        //         if (rewardDict.ContainsKey(idx))
        //             rewardDict[idx] = rewardTile.name;
        //         else
        //             rewardDict.Add(idx, rewardTile.name);
        //
        //         currentTileMap.SetTile(position, rewardTile);
        //
        //         return;
        //     }
        //     default:
        //         currentTileMap.SetTile(position, currentTile);
        //         break;
        // }
    }

    private void SetRewardValue(int idx) => rewardText[idx].SetValue($"{reward}");
    
    private float GetRewardValue(int index) => rewardText.ContainsKey(index) ? rewardText[index].RewardFloat :0;

    private void DisableRewardTextDisplayInUI(int index) => rewardText[index].rewardTextObject.SetActive(false);
    
    private void EnableRewardTextDisplayInUI(int index) => rewardText[index].rewardTextObject.SetActive(true);

    private bool PositionIsOutsideGridBoundaries(Vector3Int position) => !tileMaps[1].cellBounds.Contains(position);

    private void DeleteTile(Vector3Int position) => currentTileMap.SetTile(position, null);

    public void AssignCurrentTile(int tileIndex) => currentTile = tiles[tileIndex];

    private void AssignCurrentLayer(int layer)
    {
        currentTileMap = tileMaps[layer];
        currentLayer   = layer;
    }

    void PrintMap()
    {
        GenerateRewardTextOverTiles();
        // var tm = tileMaps[1];
        // var tilesInMap = tm.GetTilesBlock(tm.cellBounds);
        // for (var index = 0; index < tilesInMap.Length; index++)
        // {
        //     var tileBase = tilesInMap[index];
        //     var tileCoords = IndexToLocation(index, tm.cellBounds.xMax);
        //     Debug.Log($"{tileBase.name} ({tileCoords.x},{tileCoords.y}) Reward {rewardDict[index]}");
        //     
        // }
    }

    public MDP GenerateMdpFromTileMaps(string mdpName)
    {
        
        var mainTileMap = tileMaps[1];
        
        var mdpBoundaries = mainTileMap.cellBounds;
        
        var tilesFromTileMap = mainTileMap.GetTilesBlock(mdpBoundaries);
        
        var newMdp = new MDP
        {
            Name     = $"{mdpName}",
            Width    = mdpBoundaries.xMax,
            Height   = mdpBoundaries.yMax,
            States   = new List<MarkovState>(),
            MdpRules = mdpDynamicsType
        };

        for (var index = 0; index < tilesFromTileMap.Length; index++)
        {
            var tileBase = tilesFromTileMap[index];
            
            var newState = new MarkovState
            {
                ApplicableActions = new List<MarkovAction>(),
                TypeOfState = AssignStateTypeFromTileType(tileBase),
                StateIndex = index
            };

            if (!newState.IsObstacle()) newState.Reward = GetRewardValue(index);

            // if (newState.IsStandard()) newState.Reward = -0.04f; Todo delete this was just for testing something related to the credit assignment problem
            
            newMdp.States.Add(newState);
        }
        
        MdpAdmin.InitializeActionsAndTransitions(newMdp, false);

        return newMdp;
    }

    
    public void GenerateTilemapFromMdp(MDP buildMdp)
    {
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Note this code was written during a state of exhaustion. If it is insane, it's probably because I'm
    /// going insane.</remarks>
    public void GenerateRewardTextOverTiles()
    {
        var rewardTileMap = tileMaps[1];
        
        var offset = new Vector3(1, 1, 0);
        
        var tilesInMap = rewardTileMap.GetTilesBlock(rewardTileMap.cellBounds);
        
        for (var index = 0; index < tilesInMap.Length; index++)
        {
            var tileBase = tilesInMap[index];
            
            var tileCoords = IndexToLocation(index, rewardTileMap.cellBounds.xMax);

            var pos = new Vector3(tileCoords.x, tileCoords.y, 0) + offset;
            
            switch (tileBase.name)
            {
                case "o": break;
                case "s":
                case "g":
                case "t":
                    var rewardTextDisplay = Instantiate(
                        displayRewardPrefab, 
                        pos, 
                        Quaternion.identity, 
                        gridBuilderManager.stateSpace.transform);
                        
                    rewardText.Add(
                        index,
                        rewardTextDisplay.GetComponentInChildren<GridRewardText>());
                    // Debug.Log($"{tileBase.name} ({tileCoords.x},{tileCoords.y}) Reward {rewardText[index].RewardFloat}");
                    break;
            }
        }
    }

    private StateType AssignStateTypeFromTileType(TileBase tile)
    {
        return tile.name switch
        {
            "o" => StateType.Obstacle,
            "g" => StateType.Goal,
            "t" => StateType.Terminal,
              _ => StateType.Standard
        };
    }

    private Vector2Int IndexToLocation(int index, int xDimension)
    {
        return new Vector2Int(index % xDimension, index / xDimension);
    }

    private int LocationToIndex(int x, int y, int xDimension)
    {
        return (y * xDimension) + x;
    }

    public void SetRewardValue(float value) => reward = value;
}
