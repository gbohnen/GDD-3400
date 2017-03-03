using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker;
using System.Linq;

namespace Assets.Scripts
{
	public class AgentScript : MonoBehaviour
	{
		public GameObject currentCell;
        public GameObject homeCell;
		List<GameObject> path;

        SearchType type;

		private PlayMakerFSM basicMovementFSM;
		public Graph graph;
        public EdgeMatrix matrix;

		/// <summary>
		/// Get the next target object to move toward
		/// </summary>
		public void GetNextPoint()
		{
			// If the target object variable exists in the FSM
			if (basicMovementFSM.FsmVariables.FindFsmGameObject("Target Cell") != null)
			{
				currentCell.GetComponent<GridCellScript>().IsOccupied = false;
				FsmGameObject targetCell = basicMovementFSM.FsmVariables.GetFsmGameObject("Target Cell");
				currentCell = targetCell.Value;
				currentCell.GetComponent<GridCellScript>().IsOccupied = true;
                currentCell.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;
                if (path.Count > 0)
				{
					// If the next node is occupied, terminate movement
					if (path[0].GetComponent<GridCellScript>().IsOccupied)
					{
						if (basicMovementFSM.FsmVariables.FindFsmBool("Finished Moving") != null)
						{
							FsmBool isFinishedMoving = basicMovementFSM.FsmVariables.GetFsmBool("Finished Moving");
							isFinishedMoving.Value = true;
						}
						path = new List<GameObject>();
						return;
					}
					targetCell.Value = path[0];
					path.RemoveAt(0);
					targetCell.Value.GetComponent<GridCellScript>().IsTargeted = true;
				}
				else if (basicMovementFSM.FsmVariables.FindFsmBool("Finished Moving") != null)
				{
					FsmBool isFinishedMoving = basicMovementFSM.FsmVariables.GetFsmBool("Finished Moving");
					isFinishedMoving.Value = true;
				}
			}
		}

		/// <summary>
		/// Calculate the shortest path to the closest coin
		/// </summary>
		public void CalculateCoinPath()
		{
            // get the closest coin object
            GameObject closestCoin = basicMovementFSM.FsmVariables.GetFsmGameObject("Target Coin").Value;

            // set target equal to that cell
            basicMovementFSM.FsmVariables.GetFsmGameObject("Target Cell").Value = closestCoin.GetComponent<CoinScript>().currentCell;

            // guard against grid conflicts
            // if the distance is less than the world offset
            //if ((basicMovementFSM.FsmVariables.GetFsmGameObject("Target Cell").Value.transform.position - currentCell.transform.position).magnitude < GameManagerScript.WORLD_SIZE)
            //{
                // set path equal to next coin, using the pre-computed edge matrix
                // get the path to the closest coin
                path = matrix[currentCell, closestCoin.GetComponent<CoinScript>().currentCell];
                GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManagerScript>().UpdateCircuit(path.Count);
            //}

            if (basicMovementFSM.FsmVariables.FindFsmBool("Finished Moving") != null)
            {
                FsmBool isFinishedMoving = basicMovementFSM.FsmVariables.GetFsmBool("Finished Moving");
                isFinishedMoving.Value = false;
            }

            GetNextPoint();
        }

        /// <summary>
        /// Calculate the shortest path to the closest coin
        /// </summary>
        public void CalculateHomePath()
        {
            // set target equal to that cell
            basicMovementFSM.FsmVariables.GetFsmGameObject("Target Cell").Value = homeCell;

            if (homeCell != currentCell)
            {
                // guard against grid conflicts
                // if the distance is less than the world offset
                //if ((basicMovementFSM.FsmVariables.GetFsmGameObject("Target Cell").Value.transform.position - currentCell.transform.position).magnitude < GameManagerScript.WORLD_SIZE)
                //{
                    // set path equal to next coin, using the pre-computed edge matrix
                    // get the path to the closest coin
                    path = matrix[currentCell, homeCell];
                    GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManagerScript>().UpdateCircuit(path.Count);
                //}

                if (basicMovementFSM.FsmVariables.FindFsmBool("Finished Moving") != null)
                {
                    FsmBool isFinishedMoving = basicMovementFSM.FsmVariables.GetFsmBool("Finished Moving");
                    isFinishedMoving.Value = false;
                }

                GetNextPoint();
            }
        }
        /// <summary>
        /// Initialize the agent
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="startCell"></param>
        public void Initialize(GameObject[,] grid, GameObject startCell, SearchType searchType)
		{
            type = searchType;

			path = new List<GameObject>();
			currentCell = startCell;
            homeCell = startCell;
			currentCell.GetComponent<GridCellScript>().IsOccupied = true;

			graph = new Graph();
			graph.Initialize(grid);

			// Initialize the target cell as the current cell
			basicMovementFSM = PlayMakerFSM.FindFsmOnGameObject(gameObject, "BasicMovementFSM");
			if (basicMovementFSM.FsmVariables.FindFsmGameObject("Target Cell") != null)
			{
				FsmGameObject targetCell = basicMovementFSM.FsmVariables.GetFsmGameObject("Target Cell");
				targetCell.Value = currentCell;
			}
		}

        public void CoinCollectFlag()
        {
            //GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManagerScript>().UpdateNodeCount(type, 0, 0, 1);
        }
	}
}