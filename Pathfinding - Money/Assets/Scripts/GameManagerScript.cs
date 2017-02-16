using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
	public class GameManagerScript : MonoBehaviour
	{

		public static int WORLD_SIZE = 25;
        public static int WORLD_COUNT = 3;
		private const int AGENT_NUMBER = 1;
		private const float MAX_TIMER = 1.0f;
        private const float WORLD_OFFSET = 50;
        private const float GRIDS_NUMBER = 3;

		public Camera mainCamera;

		public GameObject gridPrefab;
		public GameObject agentPrefab;
		public GameObject coinPrefab;
		public GameObject obstaclePrefab;

        private GameObject[,] grid0;
        private GameObject[,] grid1;
        private GameObject[,] grid2;
        private List<GameObject> agents;
		private List<GameObject> obstacles;

		private float timer;

		public GameManagerScript()
		{
			agents = new List<GameObject>();
			obstacles = new List<GameObject>();
            grid0 = new GameObject[WORLD_SIZE, WORLD_SIZE];
            grid1 = new GameObject[WORLD_SIZE, WORLD_SIZE];
            grid2 = new GameObject[WORLD_SIZE, WORLD_SIZE];
        }

		// Use this for initialization
		void Start()
		{
			Initialize();
		}
		private void Initialize()
		{
            // Setup grid
            for (int i = 0; i < WORLD_SIZE; ++i)
            {
                for (int j = 0; j < WORLD_SIZE; ++j)
                {
                    grid0[i, j] = Instantiate(gridPrefab, new Vector3(i + 0 * WORLD_OFFSET, 0, j), Quaternion.identity);
                    grid1[i, j] = Instantiate(gridPrefab, new Vector3(i + 1 * WORLD_OFFSET, 0, j), Quaternion.identity);
                    grid2[i, j] = Instantiate(gridPrefab, new Vector3(i + 2 * WORLD_OFFSET, 0, j), Quaternion.identity);
                }
            }

            // Setup neighbors
            for (int i = 0; i < WORLD_SIZE; ++i)
            {
                for (int j = 0; j < WORLD_SIZE; ++j)
                {
                    for (int m = i - 1; m <= i + 1; m += 2)
                    {
                        // If this is the same cell, don't add it as a neighbor
                        if (m >= 0 && m < WORLD_SIZE)
                        {
                            grid0[i, j].GetComponent<GridCellScript>().neighbors.Add(grid0[m, j]);
                            grid1[i, j].GetComponent<GridCellScript>().neighbors.Add(grid1[m, j]);
                            grid2[i, j].GetComponent<GridCellScript>().neighbors.Add(grid2[m, j]);
                        }
                    }
                    for (int n = j - 1; n <= j + 1; n += 2)
                    {
                        // If this is the same cell, don't add it as a neighbor
                        if (n >= 0 && n < WORLD_SIZE)
                        {
                            grid0[i, j].GetComponent<GridCellScript>().neighbors.Add(grid0[i, n]);
                            grid1[i, j].GetComponent<GridCellScript>().neighbors.Add(grid1[i, n]);
                            grid2[i, j].GetComponent<GridCellScript>().neighbors.Add(grid2[i, n]);
                        }
                    }
                }
            }

            // Create a bunch of obstacles and put on empty cells
            int nbrCells = WORLD_SIZE * WORLD_SIZE;
            int nbrObstacles = (int)Random.Range(nbrCells * .2f, nbrCells * .3f);
            for (int i = 0; i < nbrObstacles; ++i)
            {
                int row;
                int col;
                do
                {
                    row = (int)(Random.value * WORLD_SIZE);
                    col = (int)(Random.value * WORLD_SIZE);
                } while (grid0[row, col].GetComponent<GridCellScript>().IsOccupied);

                GameObject obstacle = Instantiate(obstaclePrefab, new Vector3(row + 0 * WORLD_OFFSET, 0.5f, col), Quaternion.identity);
                obstacle.GetComponent<ObstacleScript>().Initialize(grid0[row, col]);
                grid0[row, col].GetComponent<GridCellScript>().IsOccupied = true;
                obstacles.Add(obstacle);
            }
            for (int i = 0; i < nbrObstacles; ++i)
            {
                int row;
                int col;
                do
                {
                    row = (int)(Random.value * WORLD_SIZE);
                    col = (int)(Random.value * WORLD_SIZE);
                } while (grid1[row, col].GetComponent<GridCellScript>().IsOccupied);

                GameObject obstacle = Instantiate(obstaclePrefab, new Vector3(row + 1 * WORLD_OFFSET, 0.5f, col), Quaternion.identity);
                obstacle.GetComponent<ObstacleScript>().Initialize(grid1[row, col]);
                grid1[row, col].GetComponent<GridCellScript>().IsOccupied = true;
                obstacles.Add(obstacle);
            }
            for (int i = 0; i < nbrObstacles; ++i)
            {
                int row;
                int col;
                do
                {
                    row = (int)(Random.value * WORLD_SIZE);
                    col = (int)(Random.value * WORLD_SIZE);
                } while (grid2[row, col].GetComponent<GridCellScript>().IsOccupied);

                GameObject obstacle = Instantiate(obstaclePrefab, new Vector3(row + 2 * WORLD_OFFSET, 0.5f, col), Quaternion.identity);
                obstacle.GetComponent<ObstacleScript>().Initialize(grid2[row, col]);
                grid2[row, col].GetComponent<GridCellScript>().IsOccupied = true;
                obstacles.Add(obstacle);
            }

            // Create agents and put on empty cells
            for (int i = 0; i < AGENT_NUMBER; ++i)
            {
                int row;
                int col;
                do
                {
                    row = (int)(Random.value * WORLD_SIZE);
                    col = (int)(Random.value * WORLD_SIZE);
                } while (grid0[row, col].GetComponent<GridCellScript>().IsOccupied);

                // Create a new agent
                GameObject newAgent = Instantiate(agentPrefab, new Vector3(row + 0 * WORLD_OFFSET, 0.5f, col), Quaternion.identity);
                newAgent.GetComponent<AgentScript>().Initialize(grid0, grid0[row, col], SearchType.Djikstras);
                agents.Add(newAgent);
            }

            // Create agents and put on empty cells
            for (int i = 0; i < AGENT_NUMBER; ++i)
            {
                int row;
                int col;
                do
                {
                    row = (int)(Random.value * WORLD_SIZE);
                    col = (int)(Random.value * WORLD_SIZE);
                } while (grid1[row, col].GetComponent<GridCellScript>().IsOccupied);

                // Create a new agent
                GameObject newAgent = Instantiate(agentPrefab, new Vector3(row + 1 * WORLD_OFFSET, 0.5f, col), Quaternion.identity);
                newAgent.GetComponent<AgentScript>().Initialize(grid1, grid1[row, col], SearchType.BestFirst);
                agents.Add(newAgent);
            }

            // Create agents and put on empty cells
            for (int i = 0; i < AGENT_NUMBER; ++i)
            {
                int row;
                int col;
                do
                {
                    row = (int)(Random.value * WORLD_SIZE);
                    col = (int)(Random.value * WORLD_SIZE);
                } while (grid2[row, col].GetComponent<GridCellScript>().IsOccupied);

                // Create a new agent
                GameObject newAgent = Instantiate(agentPrefab, new Vector3(row + 2 * WORLD_OFFSET, 0.5f, col), Quaternion.identity);
                newAgent.GetComponent<AgentScript>().Initialize(grid2, grid2[row, col], SearchType.AStar);
                agents.Add(newAgent);
            }

            // Center the camera above the map
            mainCamera.transform.position = new Vector3((WORLD_SIZE + WORLD_OFFSET * 2) * 0.5f, WORLD_SIZE * 3 + WORLD_OFFSET * 0.5f, -WORLD_SIZE * 0.5f);

			// Setup the gold coin timer
			timer = MAX_TIMER;
		}

		// Update is called once per frame
		void Update()
		{
			timer -= Time.deltaTime;

			// If the timer has expired, create a gold coin
			if (timer <= 0.0f)
            {
                Debug.Log("Creating coin");
                int row;
                int col;
                do
                {
                    row = (int)(Random.value * WORLD_SIZE);
                    col = (int)(Random.value * WORLD_SIZE);
                } while (grid0[row, col].GetComponent<GridCellScript>().IsOccupied);

                // Create a new coin, reset the timer
                GameObject coin0 = Instantiate(coinPrefab, new Vector3(row + (0 * WORLD_OFFSET), 0.5f, col), Quaternion.identity);
				coin0.GetComponent<CoinScript>().currentCell = grid0[row, col];
				grid0[row, col].GetComponent<GridCellScript>().IsCoin = true;
				timer = MAX_TIMER;

                Debug.Log("Creating coin");
                do
                {
                    row = (int)(Random.value * WORLD_SIZE);
                    col = (int)(Random.value * WORLD_SIZE);
                } while (grid1[row, col].GetComponent<GridCellScript>().IsOccupied);

                // Create a new coin, reset the timer
                GameObject coin1 = Instantiate(coinPrefab, new Vector3(row + (1 * WORLD_OFFSET), 0.5f, col), Quaternion.identity);
                coin1.GetComponent<CoinScript>().currentCell = grid1[row, col];
                grid1[row, col].GetComponent<GridCellScript>().IsCoin = true;
                timer = MAX_TIMER;

                Debug.Log("Creating coin");
                do
                {
                    row = (int)(Random.value * WORLD_SIZE);
                    col = (int)(Random.value * WORLD_SIZE);
                } while (grid2[row, col].GetComponent<GridCellScript>().IsOccupied);

                // Create a new coin, reset the timer
                GameObject coin2 = Instantiate(coinPrefab, new Vector3(row + (2 * WORLD_OFFSET), 0.5f, col), Quaternion.identity);
                coin2.GetComponent<CoinScript>().currentCell = grid2[row, col];
                grid2[row, col].GetComponent<GridCellScript>().IsCoin = true;
                timer = MAX_TIMER;
            }
		}
	}
}