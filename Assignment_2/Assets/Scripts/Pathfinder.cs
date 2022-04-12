using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    protected int moveDiagonalCost = 14;
    protected int moveStraigtCost = 10;
    protected float secondDelay = 0.2F;

    public bool euclideanHeuristics = true;
    public bool nullHeuristics = false;

    public Vector2Int currentTile;
    public Vector2Int targetTile;

    public List<Vector2Int> path;

    protected Maze parentMaze;

    protected List<List<MazeTile>> costMaze;

    protected bool isWorking = false;
    protected bool isFinished = false;
    public bool IsWorking
    {
        get
        {
            return isWorking; 
        }
    }

    public bool IsFinished
    {
        get
        {
            return isFinished;
        }
    }

    Coroutine routine;
    public void InitializeData(Maze maze, bool euclideanH, bool nullH)
    {
        euclideanHeuristics = euclideanH;
        nullHeuristics = nullH;
        parentMaze = maze;

        costMaze = new List<List<MazeTile>>();

        // we assume that maze is always a square or a rectangle
        for (int i = 0; i < parentMaze.MazeTiles[0].Count; i++)
        {
            costMaze.Add(new List<MazeTile>());

            for (int j = 0; j < parentMaze.MazeTiles.Count; j++)
            {
                MazeTile tile = new MazeTile(new Vector2Int(i, j));
                costMaze[i].Add(tile);
            }
        }

    }

    public void FindPath(Vector2Int current, Vector2Int target)
    {
        currentTile = current;
        targetTile = target;

        routine = StartCoroutine(FindPathCoroutine());

        //return path;
    }

    public void StopFindPathCoroutine()
    {
        StopCoroutine(routine);
        isWorking = false;
        isFinished = false;
    }

    IEnumerator FindPathCoroutine()
    {
        // Main code

        isWorking = true;
        isFinished = false;

        MazeTile start = costMaze[currentTile.x][currentTile.y];
        MazeTile target = costMaze[targetTile.x][targetTile.y];

        List<MazeTile> openSet = new List<MazeTile>();
        List<MazeTile> closeSet = new List<MazeTile>();
        openSet.Add(start);
        if (euclideanHeuristics)
        {
            while (openSet.Count > 0)
            {
                MazeTile current = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost <= current.fCost)
                    {
                        if (openSet[i].hCost < current.hCost)
                            current = openSet[i];
                    }
                }

                openSet.Remove(current);
                closeSet.Add(current);
                parentMaze.SetFreeTileColor(current.tile, Color.red);
                yield return new WaitForSeconds(secondDelay);

                if (current.tile == targetTile)
                {
                    path = RetracePath(start, target);
                    foreach (Vector2Int tile in path)
                    {
                        parentMaze.SetFreeTileColor(tile, Color.blue);
                        yield return new WaitForSeconds(secondDelay);
                    }
                    isWorking = false;
                    isFinished = true;
                    yield break;
                }

                List<Vector2Int> neighbourTiles = parentMaze.GetNeighbourTiles(current.tile);

                foreach (Vector2Int neighbourTile in neighbourTiles)
                {
                    MazeTile neighbour = costMaze[neighbourTile.x][neighbourTile.y];

                    if (!parentMaze.IsValidTileOfType(neighbourTile, MazeTileType.Free) || closeSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = current.gCost + ComputeEuclideanHeuristics(current.tile, neighbourTile);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = ComputeEuclideanHeuristics(neighbourTile, targetTile);
                        neighbour.parentTile = current;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                            parentMaze.SetFreeTileColor(neighbour.tile, Color.green);
                            //yield return new WaitForSeconds(secondDelay);
                        }
                    }
                }
            }
        } else if (nullHeuristics)
        {
            while (openSet.Count > 0)
            {
                MazeTile current = openSet[0];

                openSet.Remove(current);
                closeSet.Add(current);
                parentMaze.SetFreeTileColor(current.tile, Color.red);
                yield return new WaitForSeconds(secondDelay);

                if (current.tile == targetTile)
                {
                    path = RetracePath(start, target);
                    foreach (Vector2Int tile in path)
                    {
                        parentMaze.SetFreeTileColor(tile, Color.blue);
                        yield return new WaitForSeconds(secondDelay);
                    }
                    isWorking = false;
                    isFinished = true;
                    yield break;
                }

                List<Vector2Int> neighbourTiles = parentMaze.GetNeighbourTiles(current.tile, false);

                foreach (Vector2Int neighbourTile in neighbourTiles)
                {
                    MazeTile neighbour = costMaze[neighbourTile.x][neighbourTile.y];

                    if (!parentMaze.IsValidTileOfType(neighbourTile, MazeTileType.Free) || closeSet.Contains(neighbour) || openSet.Contains(neighbour))
                    {
                        continue;
                    }

                    openSet.Add(neighbour);
                    neighbour.parentTile = current;
                    parentMaze.SetFreeTileColor(neighbour.tile, Color.green);
                    //yield return new WaitForSeconds(secondDelay);
                }
            }
        } else
        {
            Debug.LogError("None of the heuristics is chosen");
            yield break;
        }
        throw new Exception("Unable to find optimal path");
    }

    protected int ComputeEuclideanHeuristics(Vector2Int currentTile, Vector2Int targetTile)
    {
        int distX = Mathf.Abs(currentTile.x - targetTile.x);
        int distY = Mathf.Abs(currentTile.y - targetTile.y);

        if (distX > distY)
        {
            return moveDiagonalCost * distY + moveStraigtCost * (distX - distY);
        }
        return moveDiagonalCost * distX + moveStraigtCost * (distY - distX);
    }

    public void ResetCostMaze()
    {
        for (int i = 0; i < costMaze.Count; i++)
        {
            for (int j = 0; j < costMaze[i].Count; j++)
            {
                costMaze[i][j].gCost = 0;
                costMaze[i][j].hCost = 0;
            }
        }
    }

    public List<Vector2Int> RetracePath(MazeTile startTile, MazeTile endTile)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        MazeTile currentTile = endTile;

        while (currentTile != startTile)
        {
            path.Add(currentTile.tile);
            currentTile = currentTile.parentTile;
        }

        path.Add(currentTile.tile);
        path.Reverse();

        return path;
    }
}
