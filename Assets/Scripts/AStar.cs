using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// maze tile includes data for each tile needed during search
public class MazeTile
{
    // type of tile
    // position in grid
    // current distance of tile from starting tile
    // current priority of tile
    // was tile already visited
    // index in heap
    // current tile from which this one was accessed
    public MazeTileType Type { get; }
    public Vector2Int GridPos { get; }
    public float CurrentDistance { get; set; }
    public float Priority { get; set; }
    public bool Visited { get; set; } = false;
    public int IndexInHeap { get; set; } = -1;
    public MazeTile Previous { get; set; } = null;

    public MazeTile(MazeTileType type, Vector2Int gridPos, float currentDistance = float.MaxValue, float priority = float.MaxValue, bool visited = false, MazeTile previous = null)
    {
        Type = type;
        GridPos = gridPos;
        CurrentDistance = currentDistance;
        Priority = priority;
        Visited = visited;
        Previous = previous;
    }
}

// priority queue implemented using a min-heap
public class PriorityQueue
{
    public List<MazeTile> HeapList { get; } = new List<MazeTile>();

    // swaps two tiles in heap at input indices
    private void Swap(int a, int b)
    {
        (HeapList[a], HeapList[b]) = (HeapList[b], HeapList[a]);

        HeapList[a].IndexInHeap = a;
        HeapList[b].IndexInHeap = b;
    }

    // moves tile up in heap until heap properties are restored, can start from input tile index, else uses last tile in heap
    public void HeapifyUp(bool useArg, int i = 0)
    {
        if (!useArg) i = HeapList.Count - 1;
        var parent = (int)Mathf.Floor((i - 1) / 2);

        while (i > 0 && HeapList[parent].Priority > HeapList[i].Priority)
        {
            Swap(i, parent);
            i = parent;
            parent = (int)Mathf.Floor((i - 1) / 2);
        }
    }

    // moves tile down in heap until heap properties are restored
    private void HeapifyDown()
    {
        var i = 0;
        var leftChild = 1;
        var rightChild = 2;

        var smallest = i;
        if (HeapList.Count > leftChild && HeapList[leftChild].Priority < HeapList[i].Priority)
        {
            smallest = leftChild;
        }
        if (HeapList.Count > leftChild && HeapList.Count > rightChild && HeapList[rightChild].Priority < HeapList[i].Priority && 
            HeapList[rightChild].Priority < HeapList[leftChild].Priority)
        {
            smallest = rightChild;
        }

        while (HeapList[i].Priority > HeapList[smallest].Priority)
        {
            Swap(i, smallest);
            i = smallest;
            leftChild = 2 * i + 1;
            rightChild = 2 * i + 2;
            smallest = i;
            if (HeapList.Count > leftChild && HeapList[leftChild].Priority < HeapList[i].Priority)
            {
                smallest = leftChild;
            }
            if (HeapList.Count > leftChild && HeapList.Count > rightChild && HeapList[rightChild].Priority < HeapList[i].Priority && 
                HeapList[rightChild].Priority < HeapList[leftChild].Priority)
            {
                smallest = rightChild;
            }
        }
    }

    // runs heapify up for each leaf node
    public void FixHeap()
    {
        // ReSharper disable once PossibleLossOfFraction
        for (var i = (int)Mathf.Floor(HeapList.Count/2); i < HeapList.Count; i++)
        {
            HeapifyUp(true, i);
        }
    }

    // returns true if heap is empty
    public bool IsEmpty()
    {
        return HeapList.Count <= 0;
    }

    // inserts a new tile to heap
    public void Insert(MazeTile tile)
    {
        tile.IndexInHeap = HeapList.Count;
        HeapList.Add(tile);
        HeapifyUp(false);
    }

    // returns and removes first tile in heap
    public MazeTile Pop()
    {
        if (HeapList.Count == 0) return null;

        var tile = HeapList[0];
        Swap(0, HeapList.Count - 1);
        HeapList.RemoveAt(HeapList.Count - 1);
        if (HeapList.Count > 0) HeapifyDown();
        return tile;
    }
}

// class managing calculation of a*
[Serializable]
public class AStar
{
    // weight of heuristic, 0 for null heuristic, 1 for euclidean distance, over 1 for faster search but path may not be optimal. Set from game manager
    [SerializeField] 
    private float _HeuristicWeight = 1.0f;
    // grid of maze tiles
    private List<List<MazeTile>> _mazeTiles;
    // priority queue for calculation
    private PriorityQueue _queue;
    // current maze tile
    private MazeTile _current;
    // input maze
    private Maze _maze;
    // start and end positions
    private Vector2Int _start;
    private Vector2Int _end;

    // gets all valid neighbour tiles for input grid of maze tiles and tile position
    private static List<MazeTile> Neighbours(List<List<MazeTile>> mazeTiles, Vector2Int pos)
    {
        var neighbours = new List<MazeTile>();
        // left, down, right, up tiles
        if (pos.x > 0 && mazeTiles[pos.y][pos.x - 1].Type == MazeTileType.Free) neighbours.Add(mazeTiles[pos.y][pos.x - 1]);
        if (pos.y > 0 && mazeTiles[pos.y - 1][pos.x].Type == MazeTileType.Free) neighbours.Add(mazeTiles[pos.y - 1][pos.x]);
        if (pos.x < mazeTiles[0].Count - 1 && mazeTiles[pos.y][pos.x + 1].Type == MazeTileType.Free) neighbours.Add(mazeTiles[pos.y][pos.x + 1]);
        if (pos.y < mazeTiles.Count - 1 && mazeTiles[pos.y + 1][pos.x].Type == MazeTileType.Free) neighbours.Add(mazeTiles[pos.y + 1][pos.x]);

        // diagonal tiles
        if (
            pos.x > 0 && pos.y > 0 &&
            mazeTiles[pos.y - 1][pos.x - 1].Type == MazeTileType.Free &&
            mazeTiles[pos.y][pos.x - 1].Type == MazeTileType.Free &&
            mazeTiles[pos.y - 1][pos.x].Type == MazeTileType.Free
            )
        {
            neighbours.Add(mazeTiles[pos.y - 1][pos.x - 1]);
        }
        if (
            pos.x < mazeTiles[0].Count - 1 && pos.y > 0 &&
            mazeTiles[pos.y - 1][pos.x + 1].Type == MazeTileType.Free &&
            mazeTiles[pos.y][pos.x + 1].Type == MazeTileType.Free &&
            mazeTiles[pos.y - 1][pos.x].Type == MazeTileType.Free
            )
        {
            neighbours.Add(mazeTiles[pos.y - 1][pos.x + 1]);
        }
        if (
            pos.x > 0 && pos.y < mazeTiles.Count - 1 &&
            mazeTiles[pos.y + 1][pos.x - 1].Type == MazeTileType.Free &&
            mazeTiles[pos.y][pos.x - 1].Type == MazeTileType.Free &&
            mazeTiles[pos.y + 1][pos.x].Type == MazeTileType.Free
            )
        {
            neighbours.Add(mazeTiles[pos.y + 1][pos.x - 1]);
        }
        if (
            pos.x < mazeTiles[0].Count - 1 && pos.y < mazeTiles.Count - 1 &&
            mazeTiles[pos.y + 1][pos.x + 1].Type == MazeTileType.Free &&
            mazeTiles[pos.y][pos.x + 1].Type == MazeTileType.Free &&
            mazeTiles[pos.y + 1][pos.x].Type == MazeTileType.Free
            )
        {
            neighbours.Add(mazeTiles[pos.y + 1][pos.x + 1]);
        }

        return neighbours;
    }

    // perform A* search and return final path
    public List<Vector2Int> CalculatePath(Maze maze, Vector2Int start, Vector2Int end)
    {
        InitSearch(maze, start, end);
        var path = new List<Vector2Int>();
        var search = true;

        while (search)
        {
            search = !SearchStep();
            // if search is finished, get path
            if (!search)
            {
                path = ConstructPath();
            }
        }
        return path;
    }

    public float CalculatePathLength(Maze maze, Vector2Int start, Vector2Int end)
    {
        InitSearch(_maze, _start, _end);
        var search = true;

        while (search)
        {
            search = !SearchStep();
            // if search is finished, get length
            if (!search)
            {
                return _current.CurrentDistance;
            }
        }
        return 0;
    }

    // calculates heuristic value for position
    private float Heuristic(Vector2Int pos, Vector2Int end) 
    {
        return (end - pos).magnitude * _HeuristicWeight;
    }

    // initializes values for search
    private void InitSearch(Maze maze, Vector2Int start, Vector2Int end)
    {
        // sets default values
        _mazeTiles = new List<List<MazeTile>>();
        _queue = new PriorityQueue();
        _maze = maze;
        _start = start;
        _end = end;

        // constructs grid of maze tiles from input maze
        for (var y = 0; y < _maze.MazeTiles.Count; y++)
        {
            _mazeTiles.Add(new List<MazeTile>());
            for (var x = 0; x < _maze.MazeTiles[y].Count; x++)
            {
                _mazeTiles[y].Add(new MazeTile(_maze.MazeTiles[y][x], new Vector2Int(x, y)));
            }
        }

        // insert starting tile to queue
        _queue.Insert(_mazeTiles[_start.y][_start.x]);
        // set current tile values
        var current = _mazeTiles[_start.y][_start.x];
        current.CurrentDistance = 0;
        current.Priority = 0;
    }

    // calculates one step of search, returns true if search ended (got to end tile or no solution) and false otherwise
    private bool SearchStep()
    {
        if (!_queue.IsEmpty())
        {
            // get first tile in queue
            _current = _queue.Pop();
            if (_current == _mazeTiles[_end.y][_end.x])
            {
                return true;
            }
            // mark current tile as visited
            _current.Visited = true;
            // get neighbours for current tile
            var neighbours = Neighbours(_mazeTiles, _current.GridPos);

            // calculate distances and priorities to neighbours
            foreach (var neighbour in neighbours.Where(n => !n.Visited))
            {
                // get priority for neighbour
                var priority = _current.CurrentDistance + Heuristic(neighbour.GridPos, _end);
                // if neighbours current saved priority is higher, update neighbours priority, distance and previous tile
                if (!(neighbour.Priority > priority)) 
                    continue;
                
                neighbour.CurrentDistance = _current.CurrentDistance + (_current.GridPos - neighbour.GridPos).magnitude;
                neighbour.Priority = priority;
                neighbour.Previous = _current;
                if (!_queue.HeapList.Contains(neighbour)) _queue.Insert(neighbour);
                else _queue.HeapifyUp(true, neighbour.IndexInHeap);
            }            
        }
        else
        {
            return true;
        }
        return false;
    }

    // construct path starting from end position back to first and then reversing it
    private List<Vector2Int> ConstructPath()
    {
        var endTile = _current;
        var path = new List<Vector2Int>();

        // return empty if there is no solution
        if (endTile.GridPos != _end) return path;

        path.Add(endTile.GridPos);
        while (endTile.GridPos != _start)
        {
            endTile = endTile.Previous;
            path.Add(endTile.GridPos);
        }
        path.Reverse();
        return path;
    }
}
