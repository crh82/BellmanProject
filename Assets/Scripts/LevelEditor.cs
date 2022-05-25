using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class LevelEditor : MonoBehaviour
{
    [SerializeField] public Tilemap        currentTileMap;
    [SerializeField] public TileBase       currentTile;
    [SerializeField] public Camera         mainCamera;
    [SerializeField] public List<TileBase> tiles;
    [SerializeField] public List<Tilemap>  tileMaps;
    [SerializeField] public List<TileBase> nonStandardTiles;
    [SerializeField] public TileBase       rewardTile;
    public bool inPolicyLayer;
    public bool inRewardLayer;
    public float reward;
    public MdpRules mdpDynamicsType;
    
    public Dictionary<int, string> RewardDict = new Dictionary<int, string>();
    
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
        
        if (Input.GetKey(KeyCode.Keypad0)){SetCurrentTile(0);}
        if (Input.GetKey(KeyCode.Keypad1)){SetCurrentTile(1);}
        if (Input.GetKey(KeyCode.Keypad2)){SetCurrentTile(2);}
        if (Input.GetKey(KeyCode.Keypad3)){SetCurrentTile(3);}
        if (Input.GetKey(KeyCode.Keypad4)){SetCurrentTile(4);}
        if (Input.GetKey(KeyCode.Keypad5)){SetCurrentTile(5);}
        if (Input.GetKey(KeyCode.Keypad6)){SetCurrentTile(6);}
        if (Input.GetKey(KeyCode.Keypad7)){SetCurrentTile(7);}
        
        if (Input.GetKey(KeyCode.Alpha0)){mdpDynamicsType = (MdpRules) 0;}
        if (Input.GetKey(KeyCode.Alpha1)){mdpDynamicsType = (MdpRules) 1;}
        if (Input.GetKey(KeyCode.Alpha2)){mdpDynamicsType = (MdpRules) 2;}
        if (Input.GetKey(KeyCode.Alpha3)){mdpDynamicsType = (MdpRules) 3;}
        if (Input.GetKey(KeyCode.Alpha4)){mdpDynamicsType = (MdpRules) 4;}
        if (Input.GetKey(KeyCode.Alpha5)){mdpDynamicsType = (MdpRules) 5;}
        if (Input.GetKey(KeyCode.Alpha6)){mdpDynamicsType = (MdpRules) 6;}
        
        if (Input.GetKey(KeyCode.P))
        {
            
            SetLayer(2);
            inPolicyLayer = true;
            inRewardLayer = false;
        }

        if (Input.GetKey(KeyCode.O))
        {
            SetLayer(1);
            inPolicyLayer = false;
            inRewardLayer = false;
        }

        if (Input.GetKey(KeyCode.T))
        {
            PrintMap();
        }

        if (Input.GetKey(KeyCode.R))
        {
            SetLayer(3);
            inPolicyLayer = false;
            inRewardLayer = true;
        }
    }


    void PlaceTile(Vector3Int position)
    {
        if (!tileMaps[1].cellBounds.Contains(position)) return;

        var correspondingTileInMainLayer = tileMaps[1].GetTile(position);

        bool nope = nonStandardTiles.Contains(correspondingTileInMainLayer);

        if (inPolicyLayer && nope) return;

        switch (inRewardLayer)
        {
            case true when correspondingTileInMainLayer.name == "o": return;
            case true:
            {
                rewardTile.name = $"{reward}";

                int idx = LocationToIndex(position.x, position.y, tileMaps[1].cellBounds.xMax);

                if (RewardDict.ContainsKey(idx))
                    RewardDict[idx] = rewardTile.name;
                else
                    RewardDict.Add(idx, rewardTile.name);

                currentTileMap.SetTile(position, rewardTile);

                return;
            }
            default:
                currentTileMap.SetTile(position, currentTile);
                break;
        }
    }

    void DeleteTile(Vector3Int position)
    {
        currentTileMap.SetTile(position, null);
    }
    void SetCurrentTile(int tileIndex)
    {
        currentTile = tiles[tileIndex];
    }

    void SetLayer(int layer)
    {
        currentTileMap = tileMaps[layer];
    }

    void PrintMap()
    {
        var tm = tileMaps[1];
        var tilesInMap = tm.GetTilesBlock(tm.cellBounds);
        for (var index = 0; index < tilesInMap.Length; index++)
        {
            var tileBase = tilesInMap[index];
            var tileCoords = IndexToLocation(index, tm.cellBounds.xMax);
            Debug.Log($"{tileBase.name} ({tileCoords.x},{tileCoords.y}) Reward {RewardDict[index]}");
            
        }
    }

    public MDP GenerateMdpFromTileMaps()
    {

        var mainTileMap = tileMaps[1];
        
        var mdpBoundaries = mainTileMap.cellBounds;
        
        var tilesFromTileMap = mainTileMap.GetTilesBlock(mdpBoundaries);
        
        var newMdp = new MDP
        {
            Name     = "TestFromGridEditor",
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

            if (!newState.IsObstacle())
            {
                newState.Reward = RewardDict.ContainsKey(index) ? float.Parse(RewardDict[index]) : 0;
            }
            
            newMdp.States.Add(newState);
        }
        
        MdpAdmin.InitializeActionsAndTransitions(newMdp, false);

        return newMdp;
    }

    public void GenerateTilemapFromMdp(MDP buildMdp)
    {
        
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
}
