using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MazeTile 
{
    public Vector2Int tile;

    //public int gCost = 0;
    //public int hCost = 0;
    public int gCost;
    public int hCost;
    public MazeTile parentTile;

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public MazeTile(Vector2Int inputTile)
    {
        tile = inputTile;
    }

    public bool Equals(MazeTile other)
    {
        if (other == null)
        {
            return false;
        }

        if (tile != other.tile) return false;

        return true;
    }
}
