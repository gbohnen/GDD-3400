using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class GameManagerScript : MonoBehaviour
	{

		public static int WORLD_SIZE = 25;
        public static int WORLD_COUNT = 3;
		private const int AGENT_NUMBER = 1;
		private const float MAX_TIMER = 1.0f;
        private const float WORLD_OFFSET = 1;
        private const float GRIDS_NUMBER = 3;

		public Camera mainCamera;

		public GameObject gridPrefab;
		public GameObject agentPrefab;
		public GameObject coinPrefab;
		public GameObject obstaclePrefab;

        private GameObject[,] grid0;
        private List<GameObject> agents;
        private List<GameObject> coins;
		private List<GameObject> obstacles;

        EdgeMatrix edgeMatrix;

        public CanvasGroup coinsWindow;
        public InputField coinInput;
        public Text calcTimeText;
        public Text circuitLengthText;

		public GameManagerScript()
		{
			agents = new List<GameObject>();
			obstacles = new List<GameObject>();
            grid0 = new GameObject[WORLD_SIZE, WORLD_SIZE];
        }

		// Use this for initialization
		void Start()
		{
            // setup first calculation window
            if (!coinsWindow.interactable)
            {
                ToggleStartWindow();
            }

            // Center the camera above the map
            mainCamera.transform.position = new Vector3(5, WORLD_SIZE * 1.1f, WORLD_SIZE / 2);
		}
		private void Initialize()
		{
            // Setup grid
            for (int i = 0; i < WORLD_SIZE; ++i)
            {
                for (int j = 0; j < WORLD_SIZE; ++j)
                {
                    grid0[i, j] = Instantiate(gridPrefab, new Vector3(i + 0 * WORLD_OFFSET, 0, j), Quaternion.identity);
                }
            }

            // Setup neighbors
            for (int i = 0; i < WORLD_SIZE; ++i)
            {
                for (int j = 0; j < WORLD_SIZE; ++j)
                {
                    // get verts
                    for (int n = j - 1; n <= j + 1; n += 2)
                    {
                        // If this is the same cell, don't add it as a neighbor
                        if (n >= 0 && n < WORLD_SIZE)
                        {
                            grid0[i, j].GetComponent<GridCellScript>().neighbors.Add(grid0[i, n]);
                        }
                    }
                    // get horizonts
                    for (int m = i - 1; m <= i + 1; m += 2)
                    {
                        // If this is the same cell, don't add it as a neighbor
                        if (m >= 0 && m < WORLD_SIZE)
                        {
                            grid0[i, j].GetComponent<GridCellScript>().neighbors.Add(grid0[m, j]);

                            // check for below diagonals
                            if (j - 1 >= 0)
                            {
                                grid0[i, j].GetComponent<GridCellScript>().neighbors.Add(grid0[m, j - 1]);
                            }
                            // check above diagonals
                            if (j + 1 < WORLD_SIZE)
                            {
                                grid0[i, j].GetComponent<GridCellScript>().neighbors.Add(grid0[m, j + 1]);
                            }
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

            // create coins
            int totalCoins = int.Parse(coinInput.text);
            List<GameObject> coinPlacements = new List<GameObject>();

            for (int i = 0; i < totalCoins; i++)
            {
                //Debug.Log("Creating coin");
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

                coinPlacements.Add(grid0[row, col]);
            }

            GameObject agentStart = grid0[0,0];

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
                newAgent.GetComponent<AgentScript>().Initialize(grid0, grid0[row, col]);
                agentStart = grid0[row, col];
                agents.Add(newAgent);
            }

            Stopwatch st = new Stopwatch();

            st.Start();

            // initialize edge matrix
            
            edgeMatrix = new EdgeMatrix(agentStart, coinPlacements, grid0);

            st.Stop();

            calcTimeText.text = st.ElapsedMilliseconds.ToString();

            // pass the edgeMatrix off to the agent
            agents[0].GetComponent<AgentScript>().matrix = edgeMatrix;
        }

		// Update is called once per frame
		void Update()
		{
			//timer -= Time.deltaTime;

			//// If the timer has expired, create a gold coin
			//if (timer <= 0.0f)
   //         {
   //             Debug.Log("Creating coin");
   //             int row;
   //             int col;
   //             do
   //             {
   //                 row = (int)(Random.value * WORLD_SIZE);
   //                 col = (int)(Random.value * WORLD_SIZE);
   //             } while (grid0[row, col].GetComponent<GridCellScript>().IsOccupied);

   //             // Create a new coin, reset the timer
   //             GameObject coin0 = Instantiate(coinPrefab, new Vector3(row + (0 * WORLD_OFFSET), 0.5f, col), Quaternion.identity);
			//	coin0.GetComponent<CoinScript>().currentCell = grid0[row, col];
			//	grid0[row, col].GetComponent<GridCellScript>().IsCoin = true;
			//	timer = MAX_TIMER;
   //         }
        }

        public void StartButton()
        {
            ToggleStartWindow();
            Initialize();
        }

        void ToggleStartWindow()
        {
            if (coinsWindow.interactable)
            {
                coinsWindow.interactable = false;
                coinsWindow.alpha = 0;
                coinsWindow.blocksRaycasts = false;
            }
            else
            {
                coinsWindow.interactable = true;
                coinsWindow.alpha = 1;
                coinsWindow.blocksRaycasts = true;
            }
        }

        public void UpdateCircuit(int i)
        {
            int count = int.Parse(circuitLengthText.text);
            count += i;
            circuitLengthText.text = count.ToString();
        } 

        public void ResetGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        //public void UpdateNodeCount(SearchType type, int enq, int deq, int coins)
        //{
        //    switch (type)
        //    {
        //        case SearchType.Djikstras:
        //            DjikEnq += enq;
        //            DjikDeq += deq;
        //            DjikCoin += coins;
        //            DjikstrasEnqueued.text = enqueueBase + DjikEnq;
        //            DjikstrasDequeued.text = dequeueBase + DjikDeq;
        //            DjikstraCoins.text = coinsBase + DjikCoin;
        //            break;
        //        case SearchType.BestFirst:
        //            BeFiEnq += enq;
        //            BeFiDeq += deq;
        //            BeFiCoin += coins;
        //            BestFirstEnqueued.text = enqueueBase + BeFiEnq;
        //            BestFirstDequeued.text = dequeueBase + BeFiDeq;
        //            BestFirstCoins.text = coinsBase + BeFiCoin;
        //            break;
        //        case SearchType.AStar:
        //            AStaEnq += enq;
        //            AStaDeq += deq;
        //            AStaCoin += coins;
        //            AStarEnqueued.text = enqueueBase + AStaEnq;
        //            AStarDequeued.text = dequeueBase + AStaDeq;
        //            AStarCoins.text = coinsBase + AStaCoin;
        //            break;
        //    }
        //}
    }
}