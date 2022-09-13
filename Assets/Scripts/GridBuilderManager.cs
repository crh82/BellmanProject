using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GridBuilderManager : MonoBehaviour
{
   // public static GridBuilderManager instance;

    public Grid gridPrefab;

    public GameObject stateSpacePrefab;

    public GameObject stateSpace;

    public Tilemap tilemap;

    public Grid currentGrid;

    public bool gridLoaded;

    public Camera mainCamera;

    public Transform target;

    public LevelEditor levelEditor;

    public Vector2Int dimensions;

    public MDP        sceneMdp;

    private bool _canPlaceTiles = true;

    private void Awake()
    {
        if (GameManager.instance.sendMdp)
        {
            sceneMdp = GameManager.instance.currentMdp;
            GameManager.instance.currentMdp = null;
            GameManager.instance.sendMdp = false;
            BuildGrid(sceneMdp);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.M) && Input.GetKey(KeyCode.LeftShift))
        {
            TransitionToMarkovDecisionProcessScene();
        }

        if (Input.GetKey(KeyCode.B) && !gridLoaded)
        {
            BuildGrid();
            
        }

    }

    private void SaveGridWorldAsMdp()
    {
        MDP test = levelEditor.GenerateMdpFromTileMaps("ChrisPathDecision");

        MdpAdmin.SaveMdpToFile(test, "Assets/Resources/TestMDPs");
    }

    public void TransitionToMarkovDecisionProcessScene()
    {
        GameManager.instance.currentMdp = levelEditor.GenerateMdpFromTileMaps("ChrisPathDecision");
        GameManager.instance.sendMdp    = true;
        GameManager.instance.SwitchScene(BellmanScenes.DynamicProgramming);
    }

    public void BuildGrid(MDP buildMdp = null)
    {
        if (stateSpace != null) Destroy(stateSpace.gameObject);
   
        stateSpace = Instantiate(stateSpacePrefab);

        var buildDimensions = buildMdp == null 
            ? dimensions 
            : new Vector2Int(buildMdp.Width, buildMdp.Height);
        
        currentGrid = Instantiate(gridPrefab, stateSpace.transform, true);
        
        currentGrid.transform.position = Vector3.zero;

        levelEditor = currentGrid.GetComponent<LevelEditor>();

        var bgTiles     = levelEditor.tileMaps[0];
        
        var mainTiles   = levelEditor.tileMaps[1];

        var policyTiles = levelEditor.tileMaps[2];

        var rewardTiles = levelEditor.tileMaps[3];
        
        // FillBackground(bgTiles, levelEditor.tiles[4]);
        
        mainTiles   = CreateTilemapLayer(mainTiles,   levelEditor.tiles[5], buildDimensions.x, buildDimensions.y);
        
        policyTiles = CreateTilemapLayer(policyTiles, levelEditor.tiles[8], buildDimensions.x, buildDimensions.y);
        
        rewardTiles = CreateTilemapLayer(rewardTiles, levelEditor.tiles[8], buildDimensions.x, buildDimensions.y);
        
        target.position = mainTiles.cellBounds.center;

        var targetPos = target.position;
        
        mainCamera.transform.position = targetPos + new Vector3(0,0,-2f);
        
        mainCamera.orthographicSize = Math.Max(targetPos.x, targetPos.y);
        
        levelEditor.mainCamera = Camera.main;

        levelEditor.gridBuilderManager = this;
        
        levelEditor.GenerateRewardTextOverTiles();
        
        gridLoaded = true;
       
    }

    void FillBackground(Tilemap editableTileMap, TileBase tile)
    {
        // editableTileMap.FloodFill(Vector3Int.zero, tile);
        editableTileMap.BoxFill(
            new Vector3Int(dimensions.x * 10, dimensions.y * 10, 0), 
            tile, 
            -dimensions.x * 10,
            -dimensions.y * 10,
            dimensions.x * 10,
            dimensions.y * 10);

        editableTileMap.transform.position = new Vector3((-dimensions.x * 10)/2f, (-dimensions.y * 10)/2f, 0.1f);

        // new Vector3Int(-dimensions.x * 10, -dimensions.y * 10, 0)
    }


    private Tilemap CreateTilemapLayer(Tilemap map, TileBase baseTile, int x, int y)
    {
        map.transform.position = Vector3.zero;
        
        map.origin = Vector3Int.zero;

        map.size = new Vector3Int(x, y, 0);
        
        map.BoxFill(Vector3Int.zero, baseTile, 0,0,x, y);
        
        map.CompressBounds();

        return map;
    }

    public void EnableTilePlacement()  => _canPlaceTiles = true;
    public void DisableTilePlacement() => _canPlaceTiles = false;

    public bool TilesCanBePlaced() => _canPlaceTiles;
}
