using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct TileTypeChances
{
    [SerializeField]
    private List<int> chances; //Not Ideal for huge quantities but it's a way;
    
    [SerializeField]
    private TileType type;

    public int GetRandomChance()
    {
        if (chances.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, chances.Count);
            return chances[index];
        }

        return -1;
    }

    public TileType GetTileType()
    {
        return type;
    }
    
}

[Serializable]
public struct prefabData
{
    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private TileType type;

    public GameObject GetPrefab()
    {
        return prefab;
    }
    public TileType GetTileType()
    {
        return type;
    }
    
}

public class Map : MonoBehaviour
{
    [SerializeField]
    private List<prefabData> tilePrefabs;

    private Dictionary<TileType, GameObject> prefabDict = new Dictionary<TileType, GameObject>();
    public float tilesDistance = 1f;

    [HideInInspector]
    public Tile[,] tileMap;

    public int xLen = 59, yLen = 34;
    
    [SerializeField]
    private List<TileTypeChances> TileChances;

    private List<(TileType type, int count)> SpecialTilesToBeAdded = new List<(TileType type, int count)>();
    
    void Start()
    {
        prefabDict = tilePrefabs.ToDictionary(p=>p.GetTileType(),p=> p.GetPrefab());
    }

    public void ReGenerateMap()
    {
        GetSpecialTiles();
        GenerateMap();
        SetSpecialTiles();
        RenderMap();
    }

    private void GetSpecialTiles()
    {
        if (SpecialTilesToBeAdded.Count != 0)
        {
            SpecialTilesToBeAdded.Clear();
        }
        for (int i = 0; i < TileChances.Count; i++)
        {
            SpecialTilesToBeAdded.Add((TileChances[i].GetTileType(),TileChances[i].GetRandomChance()));
        }
    }
    private void GenerateMap()
    {
        tileMap = new Tile[xLen, yLen];
        for (int i = 0; i < xLen; i++)
        {
            for (int j = 0; j < yLen; j++)
            {
                tileMap[i, j] = new Tile(TileType.Default, i, j);
            }
        }
    }

    private void SetSpecialTiles()
    {
        foreach (var specialTile in SpecialTilesToBeAdded)
        {
            var quant = specialTile.count;
            var type = specialTile.type;
            for (int i = 0; i < quant; i++)
            {
                if (type == TileType.Teleport)
                {
                    int xRef1 = UnityEngine.Random.Range(0, xLen);
                    int yRef1 = UnityEngine.Random.Range(0, yLen);
                    int xRef2 = UnityEngine.Random.Range(0, xLen);
                    int yRef2 = UnityEngine.Random.Range(0, yLen);
                    
                    tileMap[xRef1, yRef1].type = type;
                    tileMap[xRef1, yRef1].SetTpBrother(tileMap[xRef2,yRef2]);
                    tileMap[xRef2, yRef2].type = type;
                    tileMap[xRef2, yRef2].SetTpBrother(tileMap[xRef1,yRef1]);
                }
                else
                {
                    int xRef1 = UnityEngine.Random.Range(0, xLen);
                    int yRef1 = UnityEngine.Random.Range(0, yLen);
                    
                    tileMap[xRef1, yRef1].type = type;
                }
            }
        }
    }

    public void ClearMap()
    {
        foreach (Transform child in transform) 
        {
            Destroy(child.gameObject);
        }
    }
    private void RenderMap()
    {
        for (int i = 0; i < xLen; i++)
        {
            for (int j = 0; j < yLen; j++)
            {
                var type = tileMap[i, j].type;
                Instantiate(prefabDict[type], new Vector3(i, j, 200), transform.rotation, transform);
            }
        }
    }
}
