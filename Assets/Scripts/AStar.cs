using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// maze tile includes data for each tile needed during search
public class MazeTile
{
    public MazeTile(MazeTileType _type, Vector2Int _gridPos, float _currentDistance = float.MaxValue, float _priority = float.MaxValue, bool _visited = false, MazeTile _previous = null)
    {
        type = _type;
        gridPos = _gridPos;
        currentDistance = _currentDistance;
        priority = _priority;
        visited = _visited;
        previous = _previous;
    }

    // type of tile
    public MazeTileType type;
    // position in grid
    public Vector2Int gridPos;
    // current distance of tile from starting tile
    public float currentDistance = float.MaxValue;
    // current priority of tile
    public float priority = float.MaxValue;
    // was tile already visited
    public bool visited = false;
    // index in heap
    public int indexInHeap = -1;
    // current tile from which this one was accessed
    public MazeTile previous = null;
}

// priority queue implemented using a min-heap
public class PriorityQueue
{
    public List<MazeTile> heapList = new List<MazeTile>();

    // swaps two tiles in heap at input indices
    private void Swap(int a, int b)
    {
        MazeTile temp = heapList[a];
        heapList[a] = heapList[b];
        heapList[b] = temp;

        heapList[a].indexInHeap = a;
        heapList[b].indexInHeap = b;
    }

    // moves tile up in heap until heap properties are restored, can start from input tile index, else uses last tile in heap
    public void HeapifyUp(bool useArg, int i = 0)
    {
        if (!useArg) i = heapList.Count - 1;
        int parent = (int)Mathf.Floor((i - 1) / 2);

        while (i > 0 && heapList[parent].priority > heapList[i].priority)
        {
            Swap(i, parent);
            i = parent;
            parent = (int)Mathf.Floor((i - 1) / 2);
        }
    }

    // moves tile down in heap until heap properties are restored
    public void HeapifyDown()
    {
        int i = 0;
        int leftChild = 2 * i + 1;
        int rightChild = 2 * i + 2;

        int smallest = i;
        if (heapList.Count > leftChild && heapList[leftChild].priority < heapList[i].priority)
        {
            smallest = leftChild;
        }
        if (heapList.Count > leftChild && heapList.Count > rightChild && heapList[rightChild].priority < heapList[i].priority && 
            heapList[rightChild].priority < heapList[leftChild].priority)
        {
            smallest = rightChild;
        }

        while (heapList[i].priority > heapList[smallest].priority)
        {
            Swap(i, smallest);
            i = smallest;
            leftChild = 2 * i + 1;
            rightChild = 2 * i + 2;
            smallest = i;
            if (heapList.Count > leftChild && heapList[leftChild].priority < heapList[i].priority)
            {
                smallest = leftChild;
            }
            if (heapList.Count > leftChild && heapList.Count > rightChild && heapList[rightChild].priority < heapList[i].priority && 
                heapList[rightChild].priority < heapList[leftChild].priority)
            {
                smallest = rightChild;
            }
        }
    }

    // runs heapify up for each leaf node
    public void FixHeap()
    {
        for (int i = (int)Mathf.Floor(heapList.Count/2); i < heapList.Count; i++)
        {
            HeapifyUp(true, i);
        }
    }

    // returns true if heap is empty
    public bool IsEmpty()
    {
        return heapList.Count <= 0;
    }

    // inserts a new tile to heap
    public void Insert(MazeTile tile)
    {
        tile.indexInHeap = heapList.Count;
        heapList.Add(tile);
        HeapifyUp(false);
    }

    // returns and removes first tile in heap
    public MazeTile Pop()
    {
        if (heapList.Count == 0) return null;

        MazeTile tile = heapList[0];
        Swap(0, heapList.Count - 1);
        heapList.RemoveAt(heapList.Count - 1);
        if (heapList.Count > 0) HeapifyDown();
        return tile;
    }
}

// class managing calculation of a*
public class AStar
{
    // weight of heuristic, 0 for null heuristic, 1 for euclidean distance, over 1 for faster search but path may not be optimal. Set from game manager
    public float heuristicWeight = 1.0f;
    // grid of maze tiles
    List<List<MazeTile>> mazeTiles;
    // priority queue for calculation
    PriorityQueue queue;
    // current maze tile
    MazeTile current;
    // input maze
    Maze maze;
    // start and end positions
    Vector2Int start;
    Vector2Int end;

    // gets all valid neighbour tiles for input grid of maze tiles and tile position
    public static List<MazeTile> Neighbours(List<List<MazeTile>> mazeTiles, Vector2Int pos)
    {
        List<MazeTile> neighbours = new List<MazeTile>();
        // left, down, right, up tiles
        if (pos.x > 0 && mazeTiles[pos.y][pos.x - 1].type == MazeTileType.Free) neighbours.Add(mazeTiles[pos.y][pos.x - 1]);
        if (pos.y > 0 && mazeTiles[pos.y - 1][pos.x].type == MazeTileType.Free) neighbours.Add(mazeTiles[pos.y - 1][pos.x]);
        if (pos.x < mazeTiles[0].Count - 1 && mazeTiles[pos.y][pos.x + 1].type == MazeTileType.Free) neighbours.Add(mazeTiles[pos.y][pos.x + 1]);
        if (pos.y < mazeTiles.Count - 1 && mazeTiles[pos.y + 1][pos.x].type == MazeTileType.Free) neighbours.Add(mazeTiles[pos.y + 1][pos.x]);

        // diagonal tiles
        if (
            pos.x > 0 && pos.y > 0 &&
            mazeTiles[pos.y - 1][pos.x - 1].type == MazeTileType.Free &&
            mazeTiles[pos.y][pos.x - 1].type == MazeTileType.Free &&
            mazeTiles[pos.y - 1][pos.x].type == MazeTileType.Free
            )
        {
            neighbours.Add(mazeTiles[pos.y - 1][pos.x - 1]);
        }
        if (
            pos.x < mazeTiles[0].Count - 1 && pos.y > 0 &&
            mazeTiles[pos.y - 1][pos.x + 1].type == MazeTileType.Free &&
            mazeTiles[pos.y][pos.x + 1].type == MazeTileType.Free &&
            mazeTiles[pos.y - 1][pos.x].type == MazeTileType.Free
            )
        {
            neighbours.Add(mazeTiles[pos.y - 1][pos.x + 1]);
        }
        if (
            pos.x > 0 && pos.y < mazeTiles.Count - 1 &&
            mazeTiles[pos.y + 1][pos.x - 1].type == MazeTileType.Free &&
            mazeTiles[pos.y][pos.x - 1].type == MazeTileType.Free &&
            mazeTiles[pos.y + 1][pos.x].type == MazeTileType.Free
            )
        {
            neighbours.Add(mazeTiles[pos.y + 1][pos.x - 1]);
        }
        if (
            pos.x < mazeTiles[0].Count - 1 && pos.y < mazeTiles.Count - 1 &&
            mazeTiles[pos.y + 1][pos.x + 1].type == MazeTileType.Free &&
            mazeTiles[pos.y][pos.x + 1].type == MazeTileType.Free &&
            mazeTiles[pos.y + 1][pos.x].type == MazeTileType.Free
            )
        {
            neighbours.Add(mazeTiles[pos.y + 1][pos.x + 1]);
        }

        return neighbours;
    }

    // calculates heuristic value for position
    public float Heuristic(Vector2Int pos, Vector2Int end) 
    {
        return (end - pos).magnitude * heuristicWeight;
    }

    // initializes values for search
    public void InitSearch(Maze _maze, Vector2Int _start, Vector2Int _end)
    {
        // sets default values
        mazeTiles = new List<List<MazeTile>>();
        queue = new PriorityQueue();
        maze = _maze;
        start = _start;
        end = _end;

        // constructs grid of maze tiles from input maze
        for (int y = 0; y < maze.MazeTiles.Count; y++)
        {
            mazeTiles.Add(new List<MazeTile>());
            for (int x = 0; x < maze.MazeTiles[y].Count; x++)
            {
                mazeTiles[y].Add(new MazeTile(maze.MazeTiles[y][x], new Vector2Int(x, y)));
            }
        }

        // insert starting tile to queue
        queue.Insert(mazeTiles[start.y][start.x]);
        // set current tile values
        MazeTile current = mazeTiles[start.y][start.x];
        current.currentDistance = 0;
        current.priority = 0;
    }

    // calculates one step of search, returns true if search ended (got to end tile or no solution) and false otherwise
    public bool SearchStep()
    {
        if (!queue.IsEmpty())
        {
            // get first tile in queue
            current = queue.Pop();
            if (current == mazeTiles[end.y][end.x])
            {
                return true;
            }
            // mark current tile as visited
            current.visited = true;
            // get neighbours for current tile
            List<MazeTile> neighbours = Neighbours(mazeTiles, current.gridPos);

            // calculate distances and priorities to neighbours
            foreach (MazeTile neighbour in neighbours)
            {
                // get priority for neighbour
                float priority = current.currentDistance + Heuristic(neighbour.gridPos, end);
                // if neighbour hasn't already been visited
                if (!neighbour.visited)
                {
                    // if neighbours current saved priority is higher, update neighbours priority, distance and previous tile
                    if (neighbour.priority > priority)
                    {
                        neighbour.currentDistance = current.currentDistance + (current.gridPos - neighbour.gridPos).magnitude;
                        neighbour.priority = priority;
                        neighbour.previous = current;
                        if (!queue.heapList.Contains(neighbour)) queue.Insert(neighbour);
                        else queue.HeapifyUp(true, neighbour.indexInHeap);
                    }
                }
            }            
        }
        else
        {
            return true;
        }
        return false;
    }

    // construct path starting from end position back to first and then reversing it
    public List<Vector2Int> ConstructPath()
    {
        MazeTile endTile = current;
        List<Vector2Int> path = new List<Vector2Int>();

        // return empty if there is no solution
        if (endTile.gridPos != end) return path;

        path.Add(endTile.gridPos);
        while (endTile.gridPos != start)
        {
            endTile = endTile.previous;
            path.Add(endTile.gridPos);
        }
        path.Reverse();
        return path;
    }

    // perform A* search and return final path
    public List<Vector2Int> CalculatePath(Maze _maze, Vector2Int _start, Vector2Int _end)
    {
        InitSearch(_maze, _start, _end);
        List<Vector2Int> path = new List<Vector2Int>();
        bool search = true;

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

    public float CalculatePathLength(Maze _maze, Vector2Int _start, Vector2Int _end)
    {
        InitSearch(_maze, _start, _end);
        bool search = true;

        while (search)
        {
            search = !SearchStep();
            // if search is finished, get length
            if (!search)
            {
                return current.currentDistance;
            }
        }
        return 0;
    }
}
