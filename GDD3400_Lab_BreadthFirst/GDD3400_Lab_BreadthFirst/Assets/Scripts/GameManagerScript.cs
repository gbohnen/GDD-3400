using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
	public class GameManagerScript : MonoBehaviour
	{

		public static int WORLD_SIZE = 25;
		private const int AGENT_NUMBER = 1;
		private const float MAX_TIMER = 1.0f;

		public Camera mainCamera;

		public GameObject gridPrefab;
		public GameObject agentPrefab;
		public GameObject coinPrefab;
		public GameObject obstaclePrefab;

		private GameObject[,] grid;
		private List<GameObject> agents;
		private List<GameObject> obstacles;

		private float timer;

		public GameManagerScript()
		{
			agents = new List<GameObject>();
			obstacles = new List<GameObject>();
			grid = new GameObject[WORLD_SIZE, WORLD_SIZE];
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
					grid[i, j] = Instantiate(gridPrefab, new Vector3(i, 0, j), Quaternion.identity);
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
							grid[i, j].GetComponent<GridCellScript>().neighbors.Add(grid[m, j]);
						}
					}
					for (int n = j - 1; n <= j + 1; n += 2)
					{
						// If this is the same cell, don't add it as a neighbor
						if (n >= 0 && n < WORLD_SIZE)
						{
							grid[i, j].GetComponent<GridCellScript>().neighbors.Add(grid[i, n]);
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
				} while (grid[row, col].GetComponent<GridCellScript>().IsOccupied);

				GameObject obstacle = Instantiate(obstaclePrefab, new Vector3(row, 0.5f, col), Quaternion.identity);
				obstacle.GetComponent<ObstacleScript>().Initialize(grid[row, col]);
				grid[row, col].GetComponent<GridCellScript>().IsOccupied = true;
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
				} while (grid[row, col].GetComponent<GridCellScript>().IsOccupied);

				// Create a new agent
				GameObject newAgent = Instantiate(agentPrefab, new Vector3(row, 0.5f, col), Quaternion.identity);
				newAgent.GetComponent<AgentScript>().Initialize(grid, grid[row, col]);
				agents.Add(newAgent);
			}

			// Center the camera above the map
			mainCamera.transform.position = new Vector3(WORLD_SIZE * 0.5f, WORLD_SIZE, WORLD_SIZE * 0.5f);

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
				} while (grid[row, col].GetComponent<GridCellScript>().IsOccupied);

				// Create a new coin, reset the timer
				GameObject coin = Instantiate(coinPrefab, new Vector3(row, 0.5f, col), Quaternion.identity);
				coin.GetComponent<CoinScript>().currentCell = grid[row, col];
				grid[row, col].GetComponent<GridCellScript>().IsCoin = true;
				timer = MAX_TIMER;
			}
		}
	}
}