using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorridorFirstDungeonGenerator : SimpleRandomWalkGenerator
{
    [SerializeField]
    private int corridorLength = 14, corridorCount = 5;
    [SerializeField]
    [Range(0.1f, 1)]
    private float roomPercent = 0.8f;

    public GameObject playerPrefab;
    public GameObject bossPrefab; // Boss prefab
    public List<GameObject> objectPrefabs; // List of object prefabs to spawn in order
    public List<int> objectCounts; // Number of each object to spawn (should be same length as objectPrefabs)

    public HashSet<Vector2Int> usedPositions;
    public Vector2Int spawnPosition;

    private HashSet<Vector2Int> rooms = new HashSet<Vector2Int>();

    // PCG Data
    private Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

    public HashSet<Vector2Int> fPositions, corridorPositions;

    private void Start()
    {
        RunProceduralGeneration();
    }

    protected override void RunProceduralGeneration()
    {
        CorridorFirstGenerator();

        // Find the starting room and place the player
        Vector2Int playerRoom = SelectRandomRoom();
        SpawnPlayer(playerRoom);

        // Find the furthest room from the player's room and place the boss
        Vector2Int bossRoom = FindValidBossRoom(playerRoom);
        SpawnBoss(bossRoom);
    }

    private Vector2Int SelectRandomRoom()
    {
        int randomIndex = UnityEngine.Random.Range(0, rooms.Count);
        Vector2Int[] roomArray = rooms.ToArray();
        return roomArray[randomIndex];
    }

    private Vector2Int FindValidBossRoom(Vector2Int playerRoom)
    {
        Vector2Int bossRoom = FindFurthestRoom(playerRoom);
        if (!IsAreaClear(bossRoom))
        {
            List<Vector2Int> sortedRooms = GetSortedRoomsByDistance(playerRoom);
            foreach (var room in sortedRooms)
            {
                if (room != playerRoom && IsAreaClear(room))
                {
                    return room;
                }
            }
        }
        return bossRoom;
    }

    private Vector2Int FindFurthestRoom(Vector2Int startRoom)
    {
        return FindFurthestRoomFrom(startRoom, excludeRoom: null);
    }

    private Vector2Int FindFurthestRoomFrom(Vector2Int startRoom, Vector2Int? excludeRoom)
    {
        // Initialize distances dictionary
        Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();
        Dictionary<Vector2Int, Vector2Int?> previousRooms = new Dictionary<Vector2Int, Vector2Int?>();

        foreach (var room in rooms)
        {
            distances[room] = int.MaxValue;
            previousRooms[room] = null;
        }

        distances[startRoom] = 0;
        var priorityQueue = new List<Vector2Int> { startRoom };

        while (priorityQueue.Count > 0)
        {
            priorityQueue.Sort((a, b) => distances[a].CompareTo(distances[b]));
            Vector2Int currentRoom = priorityQueue[0];
            priorityQueue.RemoveAt(0);

            if (!roomsDictionary.ContainsKey(currentRoom))
            {
                continue;
            }

            foreach (var neighbor in roomsDictionary[currentRoom])
            {
                int tentativeDistance = distances[currentRoom] + 1; // Assuming each corridor has equal weight
                if (tentativeDistance < distances[neighbor])
                {
                    distances[neighbor] = tentativeDistance;
                    previousRooms[neighbor] = currentRoom;
                    if (!priorityQueue.Contains(neighbor))
                    {
                        priorityQueue.Add(neighbor);
                    }
                }
            }
        }

        // Find the furthest room from the start room, excluding the specified room
        Vector2Int furthestRoom = distances
            .Where(kvp => !excludeRoom.HasValue || kvp.Key != excludeRoom.Value)
            .Aggregate((l, r) => l.Value > r.Value ? l : r).Key;

        return furthestRoom;
    }

    private List<Vector2Int> GetSortedRoomsByDistance(Vector2Int startRoom)
    {
        // Initialize distances dictionary
        Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();

        foreach (var room in rooms)
        {
            distances[room] = int.MaxValue;
        }

        distances[startRoom] = 0;
        var priorityQueue = new List<Vector2Int> { startRoom };

        while (priorityQueue.Count > 0)
        {
            priorityQueue.Sort((a, b) => distances[a].CompareTo(distances[b]));
            Vector2Int currentRoom = priorityQueue[0];
            priorityQueue.RemoveAt(0);

            if (!roomsDictionary.ContainsKey(currentRoom))
            {
                continue;
            }

            foreach (var neighbor in roomsDictionary[currentRoom])
            {
                int tentativeDistance = distances[currentRoom] + 1; // Assuming each corridor has equal weight
                if (tentativeDistance < distances[neighbor])
                {
                    distances[neighbor] = tentativeDistance;
                    if (!priorityQueue.Contains(neighbor))
                    {
                        priorityQueue.Add(neighbor);
                    }
                }
            }
        }

        // Sort rooms by distance
        return distances.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
    }

    private bool IsAreaClear(Vector2Int position)
    {
        int areaSize = 6;
        for (int x = -areaSize / 2; x <= areaSize / 2; x++)
        {
            for (int y = -areaSize / 2; y <= areaSize / 2; y++)
            {
                Vector2Int checkPosition = position + new Vector2Int(x, y);
                if (!rooms.Contains(checkPosition))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void SpawnPlayer(Vector2Int startingRoom)
    {
        Vector3 sp = new Vector3(startingRoom.x, startingRoom.y, 0);
        Instantiate(playerPrefab, sp, Quaternion.identity);
    }

    private void SpawnBoss(Vector2Int bossRoom)
    {
        Vector3 bp = new Vector3(bossRoom.x, bossRoom.y, 0);
        Instantiate(bossPrefab, bp, Quaternion.identity);
    }

    private void CorridorFirstGenerator()
    {
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>();

        List<List<Vector2Int>> corridors = CreateCorridors(floorPositions, potentialRoomPositions);

        HashSet<Vector2Int> roomPositions = CreateRooms(potentialRoomPositions);

        List<Vector2Int> deadEnds = FindAllDeadEnds(floorPositions);
        CreateRoomsAtDeadEnds(deadEnds, roomPositions);

        floorPositions.UnionWith(roomPositions);

        for (int i = 0; i < corridors.Count; i++)
        {
            corridors[i] = IncreaseCorridorSizeBy3b3(corridors[i]);
            floorPositions.UnionWith(corridors[i]);
        }

        tileMapVisualizer.PaintFloorTiles(floorPositions);
        WallGenerator.CreateWalls(floorPositions, tileMapVisualizer);
        SpawnObjects(floorPositions);
    }

    private void SpawnObjects(HashSet<Vector2Int> floorPositions)
    {
        if (objectPrefabs.Count != objectCounts.Count)
        {
            Debug.LogError("The objectPrefabs and objectCounts lists must be of the same length.");
            return;
        }

        // Convert HashSet to list for easier random selection
        List<Vector2Int> floorPositionsList = new List<Vector2Int>(floorPositions);
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

    private List<Vector2Int> IncreaseCorridorSizeBy3b3(List<Vector2Int> corridor)
    {
        List<Vector2Int> newCorridor = new List<Vector2Int>();
        for (int i = 1; i < corridor.Count; i++)
        {
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    newCorridor.Add(corridor[i - 1] + new Vector2Int(x, y));
                }
            }
        }
        return newCorridor;
    }

    private List<Vector2Int> IncreaseCorridorSizeByOne(List<Vector2Int> corridor)
    {
        List<Vector2Int> newCorridor = new List<Vector2Int>();
        Vector2Int previousDirection = Vector2Int.zero;
        for (int i = 1; i < corridor.Count; i++)
        {
            Vector2Int directionFromCell = corridor[i] - corridor[i - 1];
            // For corner
            if (previousDirection != Vector2Int.zero && directionFromCell != previousDirection)
            {
                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        newCorridor.Add(corridor[i - 1] + new Vector2Int(x, y));
                    }
                }
                previousDirection = directionFromCell;
            }
            else
            {
                previousDirection = directionFromCell;
                Vector2Int newCorridorTileOffset = GetDirection90From(directionFromCell);
                newCorridor.Add(corridor[i - 1]);
                newCorridor.Add(corridor[i - 1] + newCorridorTileOffset);
            }
        }
        return newCorridor;
    }

    private Vector2Int GetDirection90From(Vector2Int direction)
    {
        if (direction == Vector2Int.up)
            return Vector2Int.right;
        if (direction == Vector2Int.right)
            return Vector2Int.down;
        if (direction == Vector2Int.down)
            return Vector2Int.left;
        if (direction == Vector2Int.left)
            return Vector2Int.up;

        return Vector2Int.zero;
    }

    private void CreateRoomsAtDeadEnds(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
    {
        foreach (var pos in deadEnds)
        {
            if (!roomFloors.Contains(pos))
            {
                var roomFloor = RunRandomWalk(randomWalkParameters, pos);
                roomFloors.UnionWith(roomFloor);
            }
        }
    }

    private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
    {
        List<Vector2Int> deadEnds = new List<Vector2Int>();
        foreach (var position in floorPositions)
        {
            int neighboursCount = 0;
            foreach (var direction in Direction2D.cardinalDirectionList)
            {
                if (floorPositions.Contains(position + direction))
                    neighboursCount++;
            }
            if (neighboursCount == 1)
                deadEnds.Add(position);
        }
        return deadEnds;
    }

    private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
        int roomToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);

        List<Vector2Int> roomsToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomToCreateCount).ToList();
        ClearRoomData();
        foreach (var roomPosition in roomsToCreate)
        {
            var roomFloor = RunRandomWalk(randomWalkParameters, roomPosition);

            SaveRoomData(roomPosition, roomFloor);
            roomPositions.UnionWith(roomFloor);
        }
        rooms = roomPositions;
        return roomPositions;
    }

    private void ClearRoomData()
    {
        roomsDictionary.Clear();
    }

    private void SaveRoomData(Vector2Int roomPosition, HashSet<Vector2Int> roomFloor)
    {
        roomsDictionary[roomPosition] = roomFloor;
    }

    private List<List<Vector2Int>> CreateCorridors(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> potentialRoomPositions)
    {
        var currentPosition = startPosition;
        potentialRoomPositions.Add(currentPosition);

        List<List<Vector2Int>> corridors = new List<List<Vector2Int>>();

        for (int i = 0; i < corridorCount; i++)
        {
            var path = ProceduralGenerationalAlgorithms.RandomWalkCorridor(currentPosition, corridorLength);
            corridors.Add(path);
            currentPosition = path[path.Count - 1];
            potentialRoomPositions.Add(currentPosition);
            floorPositions.UnionWith(path);

            // Ensure each room is added to the dictionary and linked to its neighbors
            if (!roomsDictionary.ContainsKey(currentPosition))
            {
                roomsDictionary[currentPosition] = new HashSet<Vector2Int>();
            }
            foreach (var position in path)
            {
                if (!roomsDictionary.ContainsKey(position))
                {
                    roomsDictionary[position] = new HashSet<Vector2Int>();
                }
                roomsDictionary[position].Add(currentPosition);
                roomsDictionary[currentPosition].Add(position);
            }
        }
        corridorPositions = new HashSet<Vector2Int>(floorPositions);
        return corridors;
    }
}
