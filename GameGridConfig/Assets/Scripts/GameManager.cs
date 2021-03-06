﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    // prefabs
    public GameObject tilePrefab;
    public GameObject agentPrefab;

    public PlayerController player;

    // game objects
    public Camera mainCam;
    public GridCell[,] graph;
    public List<Agent> agents;

    // game constants
    int graphWidth = 25;
    int graphHeight = 25;
    int maxAgents = 10;

    public Collider penBounds;

    // average agent info
    Vector3 AvgVelocity
    { get; set; }

    Vector3 AvgOrientation
    { get; set; }

    Vector3 AvgPosition
    { get; set; }

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        // zero averages to be recalculated next frame
        AvgVelocity = Vector3.zero;
        AvgPosition = Vector3.zero;
        AvgOrientation = Vector3.zero;

        foreach (Agent agent in agents)
        {
            // add agent velocity
            AvgVelocity += agent.GetComponent<Rigidbody>().velocity;

            // add agent position
            AvgPosition += agent.transform.position;

            // add agent orientation
            AvgOrientation += agent.transform.rotation.eulerAngles;


            #region Gridstuff
            // grid based movement
            //if (!agent.Moving)
            //{
            //    // get random empty neighbor of cell under this agent
            //    GridCell rand;
            //    do
            //    {
            //        rand = graph[(int)Mathf.Round(agent.gameObject.transform.position.x), (int)Mathf.Round(agent.gameObject.transform.position.z)].Neighbors[Random.Range(0, graph[(int)Mathf.Round(agent.gameObject.transform.position.x), (int)Mathf.Round(agent.gameObject.transform.position.z)].Neighbors.Count)];

            //        } while (graph[(int)rand.Position.x, (int)rand.Position.y].State != CellState.Empty);

            //    // set cell as target
            //    agent.NavigateTo(graph[(int)rand.Position.x, (int)rand.Position.y]);
            //    graph[(int)rand.Position.x, (int)rand.Position.y].State = CellState.Targeted;
            //}
            //// if the agent is already moving, update its occupation
            //else
            //{
            //    graph[(int)Mathf.Round(agent.gameObject.transform.position.x), (int)Mathf.Round(agent.gameObject.transform.position.z)].State = CellState.Occupied;
            //}
            #endregion
        }

        // divide out each average
        AvgVelocity /= agents.Count;
        AvgPosition /= agents.Count;
        AvgOrientation /= agents.Count;

        // get player position
        Vector3 playerPos = player.gameObject.transform.position;

        // apply average to each agent
        foreach (Agent agent in agents)
        {
            agent.SetMovementFactors(playerPos, AvgVelocity, AvgPosition);
        }

        // end game if all cats are captured
        if (agents.All(c => penBounds.bounds.Contains(c.transform.position)))
        {
            Debug.Log("GameOver");
            SceneManager.LoadScene(0);
        }
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
                graph[i, j].Initialize(i, j);
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
                //Debug.Log(i + " " + j + " " + graph[i,j].Neighbors.Count);
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
            agent.Initialize(graph[i, j]);
        }
    }
}
