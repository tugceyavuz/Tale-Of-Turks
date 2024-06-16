using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class WallGenerator 
{
    public static void CreateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer)
    {
        var basicWallPositions = FindWallsInDirections(floorPositions, Direction2D.cardinalDirectionList);
        var cornerWallPosition = FindWallsInDirections(floorPositions, Direction2D.diaganalDirectionList);
        CreateBasicWalls(tilemapVisualizer, basicWallPositions, floorPositions);
        CreateCornerWalls(tilemapVisualizer, cornerWallPosition, floorPositions);
    }

    private static void CreateCornerWalls(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> cornerWallPosition, HashSet<Vector2Int> floorPositions)
    {
        foreach (var position in cornerWallPosition)
        {
            string neighBinary = "";
            foreach (var direction in Direction2D.eightDirectionsList)
            {
                var neighPos = position + direction;
                if(floorPositions.Contains(neighPos))
                {
                    neighBinary += "1";
                }
                else
                {
                    neighBinary += "0";
                }
            }
            tilemapVisualizer.PaintSingleCornerWall(position, neighBinary);
        }
    }

    private static void CreateBasicWalls(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> basicWallPositions, HashSet<Vector2Int> floorPositions)
    {
        foreach (var position in basicWallPositions)
        {
            string neighBinaryType = "";
            foreach (var direction in Direction2D.cardinalDirectionList)
            {
                var neighPos = position + direction;
                if(floorPositions.Contains(neighPos))
                {
                    neighBinaryType += "1";
                }
                else
                {
                    neighBinaryType += "0";
                }
            }
            tilemapVisualizer.PaintSingleBasicWall(position,neighBinaryType);

        }
    }

    private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionList)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        
        foreach (var position in floorPositions)
        {
            foreach (var direction in directionList)
            {
                var neighbourPosition = position + direction;
                if(floorPositions.Contains(neighbourPosition) == false) { 
                    wallPositions.Add(neighbourPosition);    
                }
            }
        }

        return wallPositions;
    }
}
