using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    [SerializeField]
    protected TilemapVisualizer tileMapVisualizer = null;
    [SerializeField]
    protected Vector2Int startPosition = Vector2Int.zero;
    public GameObject playerInstance;
    
    public void GenerateDungeon()
    {
        tileMapVisualizer.Clear();
        RunProceduralGeneration();
    }

    public void ClearAll()
    {
        tileMapVisualizer.Clear();
        if (playerInstance != null)
        {
            DestroyImmediate(playerInstance);
        }
    }

    protected abstract void RunProceduralGeneration();
}
