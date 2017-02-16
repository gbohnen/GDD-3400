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
		List<GameObject> path;

        SearchType type;

		private PlayMakerFSM basicMovementFSM;
		public Graph graph;

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
            // TODO: Modify this method to take the Target Coin object from Playmaker and do a breadthfirstsearch for it

            // get the closest coin object
            GameObject closestCoin = basicMovementFSM.FsmVariables.GetFsmGameObject("Target Coin").Value;

            // set target equal to that cell
            basicMovementFSM.FsmVariables.GetFsmGameObject("Target Cell").Value = closestCoin.GetComponent<CoinScript>().currentCell;

            switch (type)
            {
                case SearchType.BreadthFirst:
                    path = graph.BreadthFirstSearch(currentCell, basicMovementFSM.FsmVariables.GetFsmGameObject("Target Cell").Value);
                    break;
                case SearchType.Djikstras:
                    path = graph.DjikstrasSearch(currentCell, basicMovementFSM.FsmVariables.GetFsmGameObject("Target Cell").Value);
                    break;
                case SearchType.AStar:
                    path = graph.AStarSearch(currentCell, basicMovementFSM.FsmVariables.GetFsmGameObject("Target Cell").Value);
                    break;
                case SearchType.BestFirst:
                    path = graph.BestFirstSearch(currentCell, basicMovementFSM.FsmVariables.GetFsmGameObject("Target Cell").Value);
                    break;
                default:
                    path = graph.BreadthFirstSearch(currentCell, basicMovementFSM.FsmVariables.GetFsmGameObject("Target Cell").Value);
                    break;
            }

            // get the path to the cell
            path = graph.BreadthFirstSearch(currentCell, basicMovementFSM.FsmVariables.GetFsmGameObject("Target Cell").Value);

			GetNextPoint();
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
	}
}