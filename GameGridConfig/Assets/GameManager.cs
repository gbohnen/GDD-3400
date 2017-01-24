using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public GameObject tilePrefab;
    public GridCell[,] graph;

    int graphWidth = 25;
    int graphHeight = 25;
    int timer;

    void Start()
    {
        graph = new GridCell[graphWidth, graphHeight];

        for (int i = 0; i < graphWidth; i++)
        {
            for (int j = 0; j < graphHeight; j++)
            {
                graph[i, j] = Instantiate(tilePrefab).GetComponent<GridCell>();

                graph[i, j].gameObject.transform.position = new Vector3(i, 0, j);
                graph[i, j].Initialize();
            }
        }

        ComputeNeighbors();
    }

    void ComputeNeighbors()
    {
        for (int i = 0; i < graphWidth; i++)
        {
            for (int j = 0; j < graphHeight; j++)
            {
                // check corners
                // bottom-left
                if (i == 0 && j == 0)
                {
                    graph[i, j].Neighbors.Add(graph[i + 1, j]);
                    graph[i, j].Neighbors.Add(graph[i, j + 1]);
                }
                // top-left
                if (i == 0 && j == graphHeight)
                {
                    graph[i, j].Neighbors.Add(graph[i + 1, j]);
                    graph[i, j].Neighbors.Add(graph[i, j - 1]);
                }
                // bottom-right
                if (i == graphWidth && j == 0)
                {
                    graph[i, j].Neighbors.Add(graph[i - 1, j]);
                    graph[i, j].Neighbors.Add(graph[i, j + 1]);
                }
                // top-right
                if (i == graphWidth && j == graphHeight)
                {
                    graph[i, j].Neighbors.Add(graph[i - 1, j]);
                    graph[i, j].Neighbors.Add(graph[i, j - 1]);
                }
            }
        }
    }

}
