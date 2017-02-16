using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;

public enum SearchType { BreadthFirst, BestFirst, Djikstras, AStar }

namespace Assets.Scripts
{
	class Edge
	{
		public Node Start { get; set; }
		public Node End { get; set; }
		public Edge(Node start, Node end)
		{
			Start = start;
			End = end;
		}
        public float Length
        {
            get
            {
                return (End.Cell.transform.position - Start.Cell.transform.position).magnitude;
            }
        }
	}

	class Node
	{
		public GameObject Cell { get; set; }
		public bool IsVisited { get; set; }
		public List<Edge> NeighborEdges { get; set; }
		public Node BackPath { get; set; }
		public Node(GameObject cell)
		{
			NeighborEdges = new List<Edge>();
			this.Cell = cell;
			IsVisited = false;
			BackPath = null;
		}
	}

	public class Graph
	{
		private List<Node> nodes;
		private List<Edge> edges;
		private GameObject[,] grid;

		/// <summary>
		/// Graph Constructor
		/// </summary>
		public Graph()
		{
			nodes = new List<Node>();
			edges = new List<Edge>();
		}

		/// <summary>
		/// Initialize this graph from a grid
		/// </summary>
		/// <param name="grid"></param>
		public void Initialize(GameObject[,] grid)
		{
			this.grid = grid;

			// Create a node for each gridcell
			foreach (GameObject g in grid)
			{
				Node node = new Node(g);
				nodes.Add(node);
			}

			// Create edges for each neighbor
			foreach (Node n in nodes)
			{
				foreach (GameObject neighbor in n.Cell.GetComponent<GridCellScript>().neighbors)
				{
					Node endNode = nodes.Where(node => node.Cell == neighbor).First();
					Edge edge = new Edge(n, endNode);
					n.NeighborEdges.Add(edge);
					edges.Add(edge);
				}
			}
		}

		/// <summary>
		/// Reset the graph to be unvisited, clear the backpaths, clear the colors
		/// </summary>
		private void ResetGraph()
		{
			foreach (GameObject g in grid)
			{
				g.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
			}

			foreach (Node n in nodes)
			{
				n.IsVisited = false;
				n.BackPath = null;
			}
		}

		/// <summary>
		/// Compute the shortest path to the gridCell using a Breadth-first search
		/// </summary>
		/// <param name="goalCell"></param>
		/// <returns></returns>
		public List<GameObject> BreadthFirstSearch(GameObject startCell, GameObject goalCell)
		{
			ResetGraph();

			List<GameObject> path = new List<GameObject>();

			List<Node> queue = new List<Node>();

			queue.Add(nodes.Where(n => n.Cell == startCell).First());

			// If the goal gridcell doesn't exist in our graph (ERROR!!)
			if (queue[0] == null)
				return path;
			queue[0].IsVisited = true;

            // While there are still nodes to search
            while (queue.Count() > 0)
            {
                // TODO: Complete the code in this while loop - there are some commented-out lines so that it compiles

                // Pop off the first item from the queue (hint: queue is a list!)
                Node currNode = queue[0];
                queue.RemoveAt(0);

                //Check if the currNode is the goal node, if it is, we're done!
                if (currNode.Cell == goalCell)
                {
                    // Add the current cell to the path
                    path.Add(currNode.Cell);

                    // Set the current cell to cyan (so we can see the path)
                    currNode.Cell.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;

                    // While there is a backpath from the current cell to the previous cell
                    while (currNode.BackPath != null)
                    {
                        // Insert the cell into the BEGINNING of the path
                        path.Insert(0, currNode.Cell);

                        // Set the current cell to cyan (so we can see the path)
                        currNode.Cell.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;

                        // Move to the previous node in the backpath
                        currNode = currNode.BackPath;
                    }

                    // Return the path
                    return path;
                }

                // For each edge in the current node's neighbors that is not visited and not occupied
                foreach (Edge edge in currNode.NeighborEdges.Where(neighbor => neighbor.End.IsVisited == false && !neighbor.End.Cell.GetComponent<GridCellScript>().IsOccupied))
                {
                    // Add the neighbor to the queue
                    queue.Add(edge.End);

                    // Set the neighbor's color to yellow (so we can see all nodes considered)
                    edge.End.Cell.gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;

                    // Set the backpath of the neighbor to the current node
                    edge.End.BackPath = currNode;

                    // Set the neighbor to visited
                    edge.End.IsVisited = true;
                }
            }

            // Return the path
            return path;
        }

        /// <summary>
        /// Compute the most efficient path to the gridCell using a Djikstra's algorithm
        /// </summary>
        /// <param name="goalCell"></param>
        /// <returns></returns>
        public List<GameObject> DjikstrasSearch(GameObject startCell, GameObject goalCell)
        {
            ResetGraph();

            List<GameObject> path = new List<GameObject>();
            return path;
        }

        /// <summary>
        /// Compute the most efficient path to the gridCell using a* algorithm
        /// </summary>
        /// <param name="goalCell"></param>
        /// <returns></returns>
        public List<GameObject> AStarSearch(GameObject startCell, GameObject goalCell)
        {
            ResetGraph();

            List<GameObject> path = new List<GameObject>();
            return path;
        }

        /// <summary>
        /// Compute the most efficient path to the gridCell using best first search
        /// </summary>
        /// <param name="goalCell"></param>
        /// <returns></returns>
        public List<GameObject> BestFirstSearch(GameObject startCell, GameObject goalCell)
        {
            ResetGraph();

            List<GameObject> path = new List<GameObject>();
            return path;
        }
    }
}
