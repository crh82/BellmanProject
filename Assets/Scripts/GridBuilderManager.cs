using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridBuilderManager : MonoBehaviour
{
   public static GridBuilderManager instance;

    public Grid gridPrefab;

    public Tilemap tilemap;

    public Grid currentGrid;

    public bool gridLoaded;

    public Camera mainCamera;

    public Transform target;

    public LevelEditor levelEditor;

    public Vector2Int dimensions;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.M) && Input.GetKey(KeyCode.LeftShift))
        {
            SavePolicy();
        }

        if (Input.GetKey(KeyCode.B) && !gridLoaded)
        {
            BuildGrid();
            gridLoaded = true;
        }

    }

    void SavePolicy()
    {
        MDP test = levelEditor.GenerateMdpFromTileMaps();
        
        MdpAdmin.SaveMdpToFile(test, "Assets/Resources/TestMDPs");
    }

    void BuildGrid()
    {
        currentGrid = Instantiate(gridPrefab);
        
        currentGrid.transform.position = Vector3.zero;
        
        levelEditor = currentGrid.GetComponent<LevelEditor>();

        var bgTiles = levelEditor.tileMaps[0];
        
        var mainTiles = levelEditor.tileMaps[1];

        var policyTiles = levelEditor.tileMaps[2];

        var rewardTiles = levelEditor.tileMaps[3];
        
        FillBackground(bgTiles, levelEditor.tiles[4]);

        CreateTilemapLayer(mainTiles, levelEditor.tiles[5], dimensions.x, dimensions.y);
        
        CreateTilemapLayer(policyTiles, levelEditor.tiles[8], dimensions.x, dimensions.y);
        
        CreateTilemapLayer(rewardTiles, levelEditor.tiles[8], dimensions.x, dimensions.y);

        target.position = mainTiles.cellBounds.center;
        
        mainCamera.transform.position = target.position + new Vector3(0,0,-2f);
        
        mainCamera.orthographicSize = Math.Max(target.position.x, target.position.y);
        
        levelEditor.mainCamera = Camera.main;
       
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
}
