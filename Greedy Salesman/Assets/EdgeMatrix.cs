﻿using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// gets a key uses instance-ids from the start and end gameobjects
public struct PathKey
{
    public readonly int Start;
    public readonly int End;
    public PathKey(int s, int e)
    {
        Start = s;
        End = e;
    }

    // generates a hash code based on the two unique ids. These keys should still be 
    public override int GetHashCode()
    {
        return Start ^ End;
    }
}

public class EdgeMatrix {

    // dictionary of paths. takes a hashable key generated by start and end instance ids
    Dictionary<PathKey, List<GameObject>> pathsDictionary;

    public EdgeMatrix(GameObject start, List<GameObject> tiles, GameObject[,] grid)
    {
        // build a graph
        Graph graph = new Graph();
        graph.Initialize(grid);

        // intialize dictionary
        pathsDictionary = new Dictionary<PathKey, List<GameObject>>();

        // insert start location at 0-index of list
        tiles.Insert(0, start);

        // generate paths for each member of the list
        int count = tiles.Count;

        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                // use instance ids of objects to create a key, then store the a* path between those two objects
                pathsDictionary[new PathKey(tiles[i].GetInstanceID(), tiles[j].GetInstanceID())] = graph.AStarSearch(tiles[i], tiles[j]);
            }
        }

        //// path every cell to every other cell
        // DONT USE THIS CODE AT ANY COST
        //count = GameManagerScript.WORLD_SIZE;

        //for (int i = 0; i < count; i++)
        //{
        //    for (int j = 0; j < count; j++)
        //    {
        //        for (int k = 0; k < count; k++)
        //        {
        //            for (int l = 0; l < count; l++)
        //            {
        //                // use instance ids of objects to create a key, then store the a* path between those two objects
        //                pathsDictionary[new PathKey(grid[i,j].GetInstanceID(), grid[k, l].GetInstanceID())] = graph.AStarSearch(grid[i, j], grid[k, l]);
        //            }
        //        }
        //    }
        //}
    }

    public List<GameObject> this[GameObject start, GameObject end]
    {
        get
        {
            return pathsDictionary[new PathKey(start.GetInstanceID(), end.GetInstanceID())];
        }
    }

    //public int GetPathCount
    //{
    //    get { return pathsDictionary.Count; }
    //}
}
