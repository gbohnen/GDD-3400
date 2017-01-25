﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    // prefabs
    public GameObject tilePrefab;
    public GameObject agentPrefab;

    // game objects
    public Camera mainCam;
    public GridCell[,] graph;
    public List<Agent> agents;


    // game constants
    int graphWidth = 25;
    int graphHeight = 25;
    int maxAgents = 10;
    float agentHeight = .7f;

    void Start()
    {
        Initialize();
    }

     void Initialize()
    {
        // initialize collections
        graph = new GridCell[graphWidth + 1, graphHeight + 1];
        agents = new List<Agent>();

        for (int i = 1; i <= graphWidth; i++)
        {
            for (int j = 1; j <= graphHeight; j++)
            {
                graph[i, j] = Instantiate(tilePrefab).GetComponent<GridCell>();

                graph[i, j].gameObject.transform.position = new Vector3(i, 0, j);
                graph[i, j].Initialize();
            }
        }

        // startup
        ComputeNeighbors();
        PositionCamera();
        PlaceAgents();
    }

    void PositionCamera()
    {
        mainCam.transform.position = new Vector3(graphWidth / 2, graphWidth / 2, graphWidth / 2);
        mainCam.orthographicSize = graphWidth / 2 + 3;
    }

    void ComputeNeighbors()
    {
        for (int i = 1; i <= graphWidth; i++)
        {
            for (int j = 1; j <= graphHeight; j++)
            {
                // check corners
                // bottom-left
                if (i == 0 && j == 0)
                {
                    graph[i, j].Neighbors.Add(graph[i + 1, j]);
                    graph[i, j].Neighbors.Add(graph[i, j + 1]);
                }
                // top-left
                else if (i == 0 && j == graphHeight)
                {
                    graph[i, j].Neighbors.Add(graph[i + 1, j]);
                    graph[i, j].Neighbors.Add(graph[i, j - 1]);
                }
                // bottom-right
                else if (i == graphWidth && j == 0)
                {
                    graph[i, j].Neighbors.Add(graph[i - 1, j]);
                    graph[i, j].Neighbors.Add(graph[i, j + 1]);
                }
                // top-right
                else if (i == graphWidth && j == graphHeight)
                {
                    graph[i, j].Neighbors.Add(graph[i - 1, j]);
                    graph[i, j].Neighbors.Add(graph[i, j - 1]);
                }

                // check edges
                // left edge
                else if (i == 0 && j != 0 && j != graphHeight)
                {
                    graph[i, j].Neighbors.Add(graph[i, j - 1]);
                    graph[i, j].Neighbors.Add(graph[i, j + 1]);
                    graph[i, j].Neighbors.Add(graph[i + 1, j]);
                }
                // right edge
                else if (i == graphWidth && j != 0 && j != graphHeight)
                {
                    graph[i, j].Neighbors.Add(graph[i - 1, j]);
                    graph[i, j].Neighbors.Add(graph[i, j - 1]);
                    graph[i, j].Neighbors.Add(graph[i, j + 1]);
                }
                // bottom edge
                else if (j == 0 && !(i == 0 || i == graphWidth))
                {
                    graph[i, j].Neighbors.Add(graph[i - 1, j]);
                    graph[i, j].Neighbors.Add(graph[i, j + 1]);
                    graph[i, j].Neighbors.Add(graph[i + 1, j]);
                }
                // top edge
                else if (j == graphHeight && !(i == 0 || i == graphWidth))
                {
                    graph[i, j].Neighbors.Add(graph[i - 1, j]);
                    graph[i, j].Neighbors.Add(graph[i, j - 1]);
                    graph[i, j].Neighbors.Add(graph[i + 1, j]);
                }
                // internal
                else
                {
                    graph[i, j].Neighbors.Add(graph[i - 1, j]);
                    graph[i, j].Neighbors.Add(graph[i, j - 1]);
                    graph[i, j].Neighbors.Add(graph[i, j + 1]);
                    graph[i, j].Neighbors.Add(graph[i + 1, j]);
                }


                // print  data
                Debug.Log(i + " " + j + " " + graph[i,j].Neighbors.Count);
            }
        }
    }

    void PlaceAgents()
    {
        // build agent list
        for (int i = 0; i < maxAgents; i++)
        {
            agents.Add(Instantiate(agentPrefab).GetComponent<Agent>());
        }

        // place agents on random tiles, no overlap
        foreach (Agent agent in agents)
        {
            // random coords
            int i = Random.Range(0, graphWidth) + 1;
            int j = Random.Range(0, graphHeight) + 1;

            while (graph[i, j].State == CellState.Occupied)
            {
                i = Random.Range(0, graphWidth) + 1;
                j = Random.Range(0, graphHeight) + 1;
            }

            // place the agent
            agent.gameObject.transform.position = new Vector3(i, agentHeight, j);
            graph[i, j].State = CellState.Occupied;
        }
    }
}
