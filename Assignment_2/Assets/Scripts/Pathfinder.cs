using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    protected int moveDiagonalCost = 14;
    protected int moveStraigtCost = 10;

    public bool euclideanHeuristics = true;
    public bool nullHeuristics = false;

    public Vector2Int currentTile;
    public Vector2Int targetTile;

    public List<Vector2Int> path;

    protected Maze parentMaze;

    protected List<List<MazeTile>> costMaze;

    public PathFinder(Maze maze, bool euclideanH, bool nullH)
    {
        euclideanHeuristics = euclideanH;
        nullHeuristics = nullH;
        parentMaze = maze;

        costMaze = new List<List<MazeTile>>();

        for (int i = 0; i < parentMaze.MazeTiles.Count; i++)
        {
            costMaze.Add(new List<MazeTile>());

            for (int j = 0; j < parentMaze.MazeTiles[i].Count; j++)
            {
                //costMaze[i][j] = new MazeTile(new Vector2Int(i, j));
                MazeTile tile = new MazeTile(new Vector2Int(i, j));
                
                Debug.Log("Hashes");
                Debug.Log(tile.GetHashCode());
                costMaze[i].Add(tile);
            }
        }
    }

    public List<Vector2Int> FindPath(Vector2Int currentTile, Vector2Int targetTile)
    {
        // Main code

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

                if (current.tile == targetTile)
                {
                    return RetracePath(start, target);
                }

                List<Vector2Int> neighbourTiles = parentMaze.GetNeighbourTiles(current.tile);

                foreach (Vector2Int neighbourTile in neighbourTiles)
                {
                    MazeTile neighbour = costMaze[neighbourTile.x][neighbourTile.y];

                    if (!parentMaze.IsValidTileOfType(neighbourTile, MazeTileType.Free) || closeSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = current.gCost + ComputeHeuristics(current.tile, neighbourTile);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = ComputeHeuristics(neighbourTile, targetTile);
                        neighbour.parentTile = current;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                            parentMaze.SetFreeTileColor(neighbour.tile, Color.green);
                        }
                    }
                }
            }
        } else if (nullHeuristics)
        {
            while (openSet.Count > 0 && openSet.Count < 1000)
            {
                MazeTile current = openSet[0];

                openSet.Remove(current);
                closeSet.Add(current);
                parentMaze.SetFreeTileColor(current.tile, Color.red);

                if (current.tile == targetTile)
                {
                    return RetracePath(start, target);
                }

                List<Vector2Int> neighbourTiles = parentMaze.GetNeighbourTiles(current.tile);

                foreach (Vector2Int neighbourTile in neighbourTiles)
                {
                    MazeTile neighbour = costMaze[neighbourTile.x][neighbourTile.y];

                    if (!parentMaze.IsValidTileOfType(neighbourTile, MazeTileType.Free) || closeSet.Contains(neighbour))
                    {
                        continue;
                    }

                    openSet.Add(neighbour);
                    neighbour.parentTile = current;
                    parentMaze.SetFreeTileColor(neighbour.tile, Color.green);
                }
            }
        } else
        {
            Debug.LogError("None of the heuristics is chosen");
        }

        throw new Exception("Unable to find optimal path");
    }

    protected int ComputeHeuristics(Vector2Int currentTile, Vector2Int targetTile)
    {
        if (euclideanHeuristics)
        {
            return this.ComputeEuclideanHeuristics(currentTile, targetTile);
        }
        else if (nullHeuristics)
        {
            return this.ComputeNullHeuristics(currentTile, targetTile);
        }
        else
        {
            Debug.LogError("None of the heuristics is chosen");
        }
        return 0;
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

    protected int ComputeNullHeuristics(Vector2Int currentTile, Vector2Int targetTile)
    {
        // TODO right now this is as Euclidean - change to normal
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

        path.Reverse();

        return path;
    }
}
