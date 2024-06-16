using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class firstMapGeneration : SimpleRandomWalkGenerator
{

    public GameObject playerPrefab;
    public List<GameObject> objectPrefabs; // List of object prefabs to spawn in order
    public List<int> objectCounts; // Number of each object to spawn (should be same length as objectPrefabs)

    HashSet<Vector2Int> floorPosition;

    public HashSet<Vector2Int> usedPositions;
    public Vector2Int spawnPosition;

    protected override void RunProceduralGeneration()
    {
        floorPosition = RunRandomWalk(randomWalkParameters, startPosition);
        tileMapVisualizer.Clear();
        tileMapVisualizer.PaintFloorTiles(floorPosition);
        WallGenerator.CreateWalls(floorPosition, tileMapVisualizer);

        SpawnObjects();
        SpawnPlayer();
    }

    Vector2Int SelectStartingRoom()
    {
        // Randomly select a room position from the HashSet of generated rooms
        int randomIndex = UnityEngine.Random.Range(0, floorPosition.Count);
        Vector2Int[] roomArray = new Vector2Int[floorPosition.Count];
        floorPosition.CopyTo(roomArray);
        return roomArray[randomIndex];
    }

    void SpawnPlayer()
    {
        Vector2Int startingRoom = SelectStartingRoom();
        // Destroy any existing player prefab
        if (playerInstance != null)
        {
            DestroyImmediate(playerInstance);
        }

        while (true)
        {
            // Get a random position within the starting room
            spawnPosition = new(startingRoom.x, startingRoom.y);
            if (!usedPositions.Contains(spawnPosition))
            {
                break;
            }
        }

        Vector3 sp = new(spawnPosition.x, spawnPosition.y, 0);

        // Instantiate the player prefab at the random position
        playerInstance = Instantiate(playerPrefab, sp, Quaternion.identity);
        playerInstance.tag = "Player";
    }

    private void SpawnObjects()
    {
        if (objectPrefabs.Count != objectCounts.Count)
        {
            Debug.LogError("The objectPrefabs and objectCounts lists must be of the same length.");
            return;
        }

        // Convert HashSet to list for easier random selection
        List<Vector2Int> floorPositionsList = new List<Vector2Int>(floorPosition);
        usedPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < objectPrefabs.Count; i++)
        {
            for (int j = 0; j < objectCounts[i]; j++)
            {
                if (usedPositions.Count >= floorPositionsList.Count)
                {
                    Debug.LogWarning("Not enough unique positions to spawn all objects. Some objects may not be spawned.");
                    return;
                }

                Vector2Int randomPosition;
                do
                {
                    // Choose a random position from the floor positions
                    randomPosition = floorPositionsList[UnityEngine.Random.Range(0, floorPositionsList.Count)];
                } while (usedPositions.Contains(randomPosition));

                // Mark this position as used
                usedPositions.Add(randomPosition);

                // Convert the 2D position to a 3D world position
                Vector3 spawnPosition = new(randomPosition.x, randomPosition.y, 0f);

                // Spawn the object at the random position
                Instantiate(objectPrefabs[i], spawnPosition, Quaternion.identity);
            }
        }
    }

}
