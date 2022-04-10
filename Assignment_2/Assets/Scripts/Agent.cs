using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class Agent : MonoBehaviour
{
    public Vector2Int CurrentTile { get; private set; }

    private Sprite _sprite;
    public Sprite Sprite
    {
        get
        {
            if(_sprite == null)
            {
                _sprite = GetComponentInChildren<SpriteRenderer>()?.sprite;
            }

            return _sprite;
        }
    }

    protected float movementSpeed;
    protected Maze parentMaze;
    protected bool isInitialized = false;

    [SerializeField]
    protected bool euclideanHeuristics = true;

    [SerializeField]
    protected bool nullHeuristics = false;

    public PathFinder pathFinder;

    public float speed = 5F;
    protected bool isWalking = false;
    Coroutine routine;
    Coroutine routine2;

    protected virtual void Start()
    {
        GameManager.Instance.DestinationChanged += OnDestinationChanged;
    }

    protected virtual void Update()
    {
        // TODO Assignment 2 ... this function might be of your interest. :-)
        // You are free to add new functions, create new classes, etc.
        // ---
        // The CurrentTile property should held the current location (tile-based) of an agent
        //
        // Have a look at Maze class, it contains several useful properties and functions.
        // For example, Maze.MazeTiles stores the information about the tiles of the maze.
        // Then, there are several functions for conversion/retrieval of tile positions, as well as for changing tile colors.
        // 
        // Finally, you can also have a look at GameManager to see what it provides.

        // NOTE
        // The code below is just a simple demonstration of some of the functionality / functions
        // You will need to replace it / change it

        //var destWorld = parentMaze.GetWorldPositionForMazeTile(GameManager.Instance.DestinationTile);

        //if(destWorld.x > transform.position.x && parentMaze.IsValidTileOfType(new Vector2Int(CurrentTile.x + 1, CurrentTile.y), MazeTileType.Free))
        //{
        //    transform.Translate(Vector3.right * movementSpeed * Time.deltaTime);
        //} 
        //else if(destWorld.x < transform.position.x && parentMaze.IsValidTileOfType(new Vector2Int(CurrentTile.x - 1, CurrentTile.y), MazeTileType.Free))
        //{
        //    transform.Translate(-Vector3.right * movementSpeed * Time.deltaTime);
        //}

        //var oldTile = CurrentTile;
        // Notice on the player's behavior that using this approach, a new tile is computed for a player
        // as soon as his origin crosses the tile border. Therefore, the player now often stops somehow "in the middle".
        // For this demo code, it does not really matter but just keep this in mind when dealing with movement.
        CurrentTile = parentMaze.GetMazeTileForWorldPosition(transform.position);

        //if(oldTile != afterTranslTile)
        //{
        //    parentMaze.SetFreeTileColor(oldTile, Color.red);
        //    CurrentTile = afterTranslTile;
        //}

        //if(CurrentTile == GameManager.Instance.DestinationTile)
        //{
        //    parentMaze.ResetTileColors();
        //    Debug.Log("YESSS");
        //}

        if (CurrentTile != GameManager.Instance.DestinationTile && pathFinder.IsFinished && !isWalking)
        {
            StartMovement();
        }
    }


    // This function is called every time the user sets a new destination using a left mouse button
    protected virtual void OnDestinationChanged(Vector2Int newDestinationTile)
    {
        // TODO Assignment 2 ... this function might be of your interest. :-)
        // The destination tile index is also accessible via GameManager.Instance.DestinationTile

        if (pathFinder.IsWorking)
        {
            pathFinder.StopFindPathCoroutine();
        }

        if (isWalking)
        {
            StopMovement();
        }

        pathFinder.ResetCostMaze();
        parentMaze.ResetTileColors();
        pathFinder.FindPath(CurrentTile, newDestinationTile);
    }

    IEnumerator Moving(Vector2Int tile)
    {
        var destWorld = parentMaze.GetWorldPositionForMazeTile(tile);

        while (transform.position != destWorld)
        {
            transform.position = Vector3.MoveTowards(transform.position, destWorld, speed * Time.deltaTime);
            yield return null;
        }

    }


    IEnumerator HandleMovement()
    {
        List<Vector2Int> path = pathFinder.path;
        isWalking = true;

        foreach (var tile in path)
        {
            //var destWorld = parentMaze.GetWorldPositionForMazeTile(tile);
            //transform.position += (destWorld - transform.position).normalized * speed * Time.deltaTime;
            //CurrentTile = tile;

            routine2 = StartCoroutine(Moving(tile));
            yield return routine2;
        }

        isWalking = false;
        yield return null;
    }

    protected void StopMovement()
    {
        isWalking = false;
        StopCoroutine(routine);
        StopCoroutine(routine2);
    }

    protected void StartMovement()
    {
        routine = StartCoroutine(HandleMovement());
    }

    public virtual void InitializeData(Maze parentMaze, float movementSpeed, Vector2Int spawnTilePos)
    {
        this.parentMaze = parentMaze;

        // The multiplication below ensures that movement speed is considered in tile-units so it stays
        // consistent across different scales of the maze
        this.movementSpeed = movementSpeed * parentMaze.GetElementsScale().x; 

        transform.position = parentMaze.GetWorldPositionForMazeTile(spawnTilePos.x, spawnTilePos.y);
        transform.localScale = parentMaze.GetElementsScale();

        CurrentTile = spawnTilePos;

        isInitialized = true;

        pathFinder = gameObject.AddComponent<PathFinder>();

        pathFinder.InitializeData(parentMaze, euclideanHeuristics, nullHeuristics);
    }
}
