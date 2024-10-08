using Assets.Scripts.BoatScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;
using static UnityEngine.Rendering.DebugUI.Table;
using Random = UnityEngine.Random;

public class BoatGridManager : MonoBehaviour
{
    [SerializeField] int width, height;
    public int Width
    {
        get { return width; }
    }
    public int Height
    {
        get { return height; }
    }
    [SerializeField] float gridY = 0;
    [SerializeField] BoatWorldTile tilePrefab;
    [SerializeField] int[] difficultyThresholds = new int[4];

    /// <summary>
    /// determines at what y val hazards start spawning
    /// </summary>
    [SerializeField] int startSafeZoneSize = 5;
    /// <summary>
    /// determines at what y val hazards stop spawning
    /// </summary>
    [SerializeField] int endSafeZoneSize = 3;



    [Serializable]
    struct HazardEntry
    {
        public BoatHazard hazardPrefab;
        public float spawnWeight;
        public int minNumber;
        public int maxNumber;
    }
    [SerializeField] public BoatHazard BossHazard;
    [SerializeField] List<HazardEntry> terrainHazards;
    [SerializeField] List<HazardEntry> eventHazards;
    [SerializeField] List<HazardEntry> bonusHazards;

    Dictionary<HazardEntry, int> hazardsSpawned = new Dictionary<HazardEntry, int>();

    private BoatWorldTile[,] tileGrid;
    [HideInInspector] public float[] boatLaneXPositions = new float[3];

    BoatPathFinder pathFinder = new BoatPathFinder();
    public float tileSize { get; private set;}

    public int pathHazardGenerationPasses = 1;
    public bool logPathLogic;
    public bool paintSolution;
    List<BoatWorldTile> path;

    private void Start()
    {

        terrainHazards.Sort((h, h1) => h.spawnWeight < h1.spawnWeight ? 0 : 1);
        for (int i = 0; i < terrainHazards.Count; i++)
        {
            hazardsSpawned.Add(terrainHazards[i], 0);
            Debug.Log(i + ":"  + terrainHazards[i].spawnWeight);
        }
        for (int i = 0; i < eventHazards.Count; i++)
        {
            hazardsSpawned.Add(eventHazards[i], 0);
            Debug.Log(i + ":" + eventHazards[i].spawnWeight);
        }
        for(int i = 0; i  < bonusHazards.Count; i++)
        {
            hazardsSpawned.Add(bonusHazards[i], 0);
            Debug.Log(i + ":" + bonusHazards[i].spawnWeight);

        }

        tileSize = 5;
        if (tilePrefab)
        {
            tileGrid = new BoatWorldTile[width, height];
            boatLaneXPositions = new float[width];
            int threshInc = (int)((float)height * 0.3f);
            for (int i = 0; i < difficultyThresholds.Length; i++)
            {
                difficultyThresholds[i] = threshInc * i;
            }
            
            GenerateGrid();
            CreateValidPath();
            PopulatePathHazards();
            // PopulateEmptySpace();
            SpawnBoss();
        }
    }

    void GenerateGrid()
    {
        for(int x = 0; x < width; x++)
        {
            
            for (int y = 0; y < height; y++)
            {
                tileGrid[x,y] = Instantiate(tilePrefab, new Vector3(x*tileSize, gridY, -y*tileSize), Quaternion.identity,transform);
                tileGrid[x,y].name = $"Tile ({x},{y})";
                tileGrid[x,y].gridPosition = new Vector2Int (x, y);
                boatLaneXPositions[x] = tileGrid[x, y].transform.position.x;
                if (y > startSafeZoneSize && y < height - endSafeZoneSize) InstantiateHazard(ref tileGrid[x,y], terrainHazards);
                //Debug.Log($"{tileGrid[x, y].name} : weight :{tileGrid[x, y].pathWeight} ");
            }
        }
    }

    void InstantiateHazard(ref BoatWorldTile tile, List<HazardEntry> hazardSet)
    {
        //int roll = UnityEngine.Random.Range(0, hazardList.Count + emptyTileWeight);
        //if (roll >= hazardList.Count)
        //{
        //    tile.pathWeight = 1;
        //    return;
        //}

        int roll = RollHazardsWeighted(hazardSet);

        BoatHazard hazard = hazardSet[roll].hazardPrefab;
        hazardsSpawned[hazardSet[roll]]++;
        if(!hazard)
        {
            tile.pathWeight = 1;
            return;
        }
        Vector3 yOffset = 0.8f * Vector3.up;
        tile.hazard = Instantiate(hazard, tile.transform.position + yOffset, Quaternion.identity, transform);
        BattleHazard battle = tile.hazard.GetComponent<BattleHazard>();
        if (battle)
        {
            for (int i = difficultyThresholds.Length-1; i >= 0; i--)
            {
                int threshold = difficultyThresholds[i];
                if (tile.gridPosition.y >= threshold)
                {
                    battle.battleDifficulty = (BattleDifficulty)i;
                    break;
                }
            }
        }

        tile.pathWeight = hazard.pathFindingInfluence;
    }

    void CreateValidPath()
    {
        pathFinder.SetDebug(logPathLogic);
        pathFinder.Initialize(this);
        pathFinder.Enter(1, 0, width/2, height - 1);
        pathFinder.GeneratePath();
        if (pathFinder.IsDone())
        {
            Debug.Log($"[BOAT GRID][PATHFINDER] solution length: {pathFinder.solution.Count}");
            CleanSolution(pathFinder.solution, paintSolution);
            path = pathFinder.solution;
        }
    }


    void PopulatePathHazards()
    {
        for(int pass = 0; pass < pathHazardGenerationPasses; pass++)
        {
            List<BoatWorldTile> pathCopy = path;
            while (pathCopy.Count > 0)
            {
                int tileRoll = Random.Range(0, pathCopy.Count);
                BoatWorldTile tile = pathCopy[tileRoll];
                if (tile.gridPosition.y < startSafeZoneSize || tile.gridPosition.y > height - endSafeZoneSize)
                {
                    pathCopy.Remove(tile);
                    continue;
                }
                InstantiateHazard(ref tile, eventHazards);
                for (int i = 0; i < tile.neighbors.Count; i++)
                {
                    pathCopy.Remove(tile.neighbors[i]);
                }
                pathCopy.Remove(tile);
            }

            //int xRoll = Random.Range(0, width);
            //int yRoll = Random.Range(0, height);
            //
        }
        
    }

    void SpawnBoss()
    {
        BoatWorldTile tile = tileGrid[width / 2, height - 1];
        Vector3 yOffset = 3.2f * Vector3.up;
        tile.hazard = Instantiate(BossHazard, tile.transform.position + yOffset, Quaternion.identity, transform);
    }

    void PopulateEmptySpace()
    {
        List<BoatWorldTile> nonPath = new List<BoatWorldTile>();
        for (int x = 0; x < width; x++)
        {

            for (int y = 0; y < height; y++)
            {
                nonPath.Add(tileGrid[x,y]);
            }
        }
        for (int i = 0; i < path.Count ; i++)
        {
            nonPath.Remove(path[i]);
        }

        while (nonPath.Count > 0)
        {
            int tileRoll = Random.Range(0, nonPath.Count);
            BoatWorldTile tile = nonPath[tileRoll];
            if (tile.gridPosition.y < startSafeZoneSize || tile.gridPosition.y > height - endSafeZoneSize)
            {
                nonPath.Remove(tile);
                continue;
            }
            InstantiateHazard(ref tile, eventHazards);
            for (int i = 0; i < tile.neighbors.Count; i++)
            {
                BoatWorldTile neighborTile = tile.neighbors[i];

                if (tile && !path.Contains(neighborTile))
                {
                    InstantiateHazard(ref neighborTile, bonusHazards);
                }

                nonPath.Remove(tile.neighbors[i]);
            }
            nonPath.Remove(tile);
        }

    }

    public BoatWorldTile GetTile(int x, int y)
    {
        if ((0 <= x) && (0 <= y) && (x < width) && (y < height))
        {
            return tileGrid[x,y];
        }
        else
        {
            // Maybe we should assert...naaah.
            return null;
        }
    }

    

    void CleanSolution(List<BoatWorldTile> solution, bool paintSolution = false)
    {
        foreach(BoatWorldTile tile in solution)
        {
            if(paintSolution) tile.Paint();
            if (tile.pathWeight >= 5) Destroy(tile.hazard.gameObject);
            ////Super lazy way to prevent battles from being next to eachother (just on the main path tho...)
            if (tile.GetComponent<BattleHazard>())
            {
                int x = tile.gridPosition.x;
                int y = tile.gridPosition.y;
                BoatWorldTile neighborTile = GetTile(x + 1, y);
                if (neighborTile && neighborTile.hazard.GetComponent<BattleHazard>()) Destroy(tile.hazard.gameObject);
                neighborTile = GetTile(x - 1, y);
                if (neighborTile && neighborTile.hazard.GetComponent<BattleHazard>()) Destroy(tile.hazard.gameObject);
                neighborTile = GetTile(x, y + 1);
                if (neighborTile && neighborTile.hazard.GetComponent<BattleHazard>()) Destroy(tile.hazard.gameObject);
                neighborTile = GetTile(x, y - 1);
                if (neighborTile && neighborTile.hazard.GetComponent<BattleHazard>()) Destroy(tile.hazard.gameObject);
            }
        }
    }

    int RollHazardsWeighted(List<HazardEntry> hazardSet)
    {
        float weightedTotal = 0;
        List<HazardEntry> hazardListCopy = hazardSet;
        for (int i = hazardListCopy.Count-1; i > 0; i--)
        {
            //if (hazardsSpawned[hazardListCopy[i]] >= hazardListCopy[i].maxNumber || i == lastSpawnedHazard)
            //{
            //    //Debug.Log($"Max number of {hazardListCopy[i].hazardPrefab.name} reached :: {hazardsSpawned[hazardListCopy[i]]}/{hazardListCopy[i].maxNumber}");

            //    hazardListCopy.RemoveAt(i);
            //}
            //else
            //{
            //    weightedTotal += hazardListCopy[i].spawnWeight;
            //}
            weightedTotal += hazardListCopy[i].spawnWeight * (1 - hazardsSpawned[hazardListCopy[i]] / hazardListCopy[i].maxNumber);
        }
        Debug.Log("weighted total " + weightedTotal);

        float weightRoll = Random.Range(0, weightedTotal);
        Debug.Log("weighted roll " + weightRoll);

        for (int i = 0; i < hazardListCopy.Count; i++)
        {
            //Debug.Log($" wRoll: {weightRoll} :: hWeight: {hazardList[i].spawnWeight} ");

            if ( weightRoll <= hazardListCopy[i].spawnWeight)
            {
                return i;
            }
            else
            {
                weightRoll -= hazardListCopy[i].spawnWeight;
            }
        }

        Debug.Log("Weighted roll of hazards failed and exited with zero");
        return 0;
    }
}
