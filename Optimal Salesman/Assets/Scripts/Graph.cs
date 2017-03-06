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
        float length;
		public Node Start { get; set; }
		public Node End { get; set; }
		public Edge(Node start, Node end)
		{
			Start = start;
			End = end;
            length = (End.Cell.transform.position - Start.Cell.transform.position).magnitude;
        }
        public float Length
        {
            get { return length; }
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

            Coords = new Vector2(cell.transform.position.x, cell.transform.position.z);
        }
        public float Priority { get; set; }
        public float Distance { get; set; }
        public Vector2 Coords
        { get; set; }
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
                n.Priority = 0;
                n.Distance = 0;
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

			queue.Add(nodes.FirstOrDefault(n => n.Cell == startCell));

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

            List<Node> priorityQueue = new List<Node>();

            // data members
            int enq = 0;
            int deq = 0;

            priorityQueue.Add(nodes.FirstOrDefault(n => n.Cell == startCell));

            // If the goal gridcell doesn't exist in our graph (ERROR!!)
            if (priorityQueue[0] == null)
                return path;
            priorityQueue[0].IsVisited = true;
            priorityQueue[0].Priority = 0;

            while (priorityQueue.Count > 0)
            {
                // pop off first item in queue, store as current node
                Node currNode = priorityQueue[0];
                priorityQueue.RemoveAt(0);
                //currNode.Cell.gameObject.GetComponent<MeshRenderer>().material.color = Color.magenta;
                deq++;

                // if goal, calculate path
                //Check if the currNode is the goal node, if it is, we're done!
                if (currNode.Cell == goalCell)
                {
                    // Add the current cell to the path
                    path.Add(currNode.Cell);

                    // Set the current cell to cyan (so we can see the path)
                    //currNode.Cell.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;

                    // While there is a backpath from the current cell to the previous cell
                    while (currNode.BackPath != null)
                    {
                        // Insert the cell into the BEGINNING of the path
                        path.Insert(0, currNode.Cell);

                        // Set the current cell to cyan (so we can see the path)
                        //currNode.Cell.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;

                        // Move to the previous node in the backpath
                        currNode = currNode.BackPath;
                    }

                    // Return the path
                    //GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManagerScript>().UpdateNodeCount(SearchType.Djikstras, enq, deq, 0);
                    return path;
                }

                // for each neighbor, check and update distances
                foreach (Edge edge in currNode.NeighborEdges.Where(n => !n.End.Cell.GetComponent<GridCellScript>().IsOccupied))
                {
                    if (!edge.End.IsVisited)
                    {
                        edge.End.IsVisited = true;                                  // mark visited
                        edge.End.Priority = currNode.Priority + edge.Length;        // update distance
                        edge.End.BackPath = currNode;                               // set back-pointer
                        priorityQueue.Add(edge.End);                                // add neighbor to queue
                        edge.End.Cell.gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;
                        enq++;
                    }
                    else
                    {
                        // update the distance if it was shorter
                        if (currNode.Priority + edge.Length < edge.End.Priority)
                        {
                            var newNode = priorityQueue.FirstOrDefault(n => n == edge.End);

                            if (newNode != null)
                            {
                                newNode.Priority = currNode.Priority + edge.Length;
                                newNode.BackPath = currNode;
                            }
                        }
                    }
                }

                // sort priority queue
                priorityQueue = PrioritizeList(priorityQueue);
            }            

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

            List<Node> priorityQueue = new List<Node>();

            // data members
            int enq = 0;
            int deq = 0;

            // if start and destination are the same, return a blank path
            if (startCell == goalCell)
                return path;

            priorityQueue.Add(nodes.FirstOrDefault(n => n.Cell == startCell));

            // If the goal gridcell doesn't exist in our graph (ERROR!!)
            if (priorityQueue[0] == null)
                return path;
            priorityQueue[0].IsVisited = true;
            priorityQueue[0].Priority = 0;
            priorityQueue[0].Distance = 0;

            while (priorityQueue.Count > 0)
            {
                // pop off first item in queue, store as current node
                Node currNode = priorityQueue[0];
                priorityQueue.RemoveAt(0);
                //currNode.Cell.gameObject.GetComponent<MeshRenderer>().material.color = Color.magenta;
                deq++;

                // if goal, calculate path
                //Check if the currNode is the goal node, if it is, we're done!
                if (currNode.Cell == goalCell)
                {
                    // Add the current cell to the path
                    path.Add(currNode.Cell);

                    // Set the current cell to cyan (so we can see the path)
                    //currNode.Cell.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;

                    // While there is a backpath from the current cell to the previous cell
                    while (currNode.BackPath != null)
                    {
                        // Insert the cell into the BEGINNING of the path
                        path.Insert(0, currNode.Cell);

                        // Set the current cell to cyan (so we can see the path)
                        //currNode.Cell.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;

                        // Move to the previous node in the backpath
                        currNode = currNode.BackPath;
                    }

                    // Return the path
                    //GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManagerScript>().UpdateNodeCount(SearchType.AStar, enq, deq, 0);
                    return path;
                }

                // for each neighbor, check and update distances
                foreach (Edge edge in currNode.NeighborEdges.Where(n => !n.End.Cell.GetComponent<GridCellScript>().IsOccupied))
                {
                    if (!edge.End.IsVisited)
                    {
                        edge.End.IsVisited = true;                                  // mark visited

                        edge.End.Priority = currNode.Distance + edge.Length + EstimateDistance(goalCell, edge.End.Cell);     // update priority
                        edge.End.Distance = currNode.Distance + edge.Length;        // update distance
                        edge.End.BackPath = currNode;                               // set back-pointer
                        priorityQueue.Add(edge.End);                                // add neighbor to queue
                        //edge.End.Cell.gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;
                        enq++;
                    }
                    else
                    {
                        // update the distance if it was shorter
                        if (currNode.Distance + edge.Length + EstimateDistance(goalCell, edge.End.Cell) < edge.End.Priority)
                        {
                            var newNode = priorityQueue.FirstOrDefault(n => n == edge.End);

                            if (newNode != null)
                            {
                                newNode.Priority = currNode.Distance + edge.Length + EstimateDistance(goalCell, edge.End.Cell);
                                newNode.Distance = currNode.Priority + edge.Length;
                                newNode.BackPath = currNode;
                            }
                        }
                    }
                }

                // sort priority queue
                priorityQueue = PrioritizeList(priorityQueue);
            }

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

            List<Node> priorityQueue = new List<Node>();

            // data members
            int enq = 0;
            int deq = 0;

            priorityQueue.Add(nodes.FirstOrDefault(n => n.Cell == startCell));

            // If the goal gridcell doesn't exist in our graph (ERROR!!)
            if (priorityQueue[0] == null)
                return path;
            priorityQueue[0].IsVisited = true;
            priorityQueue[0].Priority = EstimateDistance(goalCell, priorityQueue[0].Cell);

            while (priorityQueue.Count > 0)
            {
                // pop off first item in queue, store as current node
                Node currNode = priorityQueue[0];
                priorityQueue.RemoveAt(0);
                //currNode.Cell.gameObject.GetComponent<MeshRenderer>().material.color = Color.magenta;
                deq++;

                // if goal, calculate path
                //Check if the currNode is the goal node, if it is, we're done!
                if (currNode.Cell == goalCell)
                {
                    // Add the current cell to the path
                    path.Add(currNode.Cell);

                    // Set the current cell to cyan (so we can see the path)
                    //currNode.Cell.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;

                    // While there is a backpath from the current cell to the previous cell
                    while (currNode.BackPath != null)
                    {
                        // Insert the cell into the BEGINNING of the path
                        path.Insert(0, currNode.Cell);

                        // Set the current cell to cyan (so we can see the path)
                        //currNode.Cell.gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;

                        // Move to the previous node in the backpath
                        currNode = currNode.BackPath;
                    }

                    // Return the path
                    //GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManagerScript>().UpdateNodeCount(SearchType.BestFirst, enq, deq, 0);
                    return path;
                }

                // for each neighbor, check and update distances
                foreach (Edge edge in currNode.NeighborEdges.Where(n => !n.End.Cell.GetComponent<GridCellScript>().IsOccupied))
                {
                    if (!edge.End.IsVisited)
                    {
                        edge.End.IsVisited = true;                                  // mark visited
                        edge.End.Priority = EstimateDistance(goalCell, edge.End.Cell);        // update distance
                        edge.End.BackPath = currNode;                               // set back-pointer
                        priorityQueue.Add(edge.End);                                // add neighbor to queue
                        //edge.End.Cell.gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;
                        enq++;
                    }
                    else
                    {
                        // update the distance if it was shorter
                        if (EstimateDistance(goalCell, edge.End.Cell) < edge.End.Priority)
                        {
                            var newNode = priorityQueue.FirstOrDefault(n => n == edge.End);

                            if (newNode != null)
                            {
                                newNode.Priority = EstimateDistance(goalCell, edge.End.Cell);
                                newNode.BackPath = currNode;
                            }
                        }
                    }
                }

                // sort priority queue
                priorityQueue = PrioritizeList(priorityQueue);
            }

            return path;
        }


        /// <summary>
        /// sorts a list based on the distance value of each node
        /// </summary>
        /// <param name="list"> the list to be sorted  </param>
        /// <returns></returns>
        List<Node> PrioritizeList(List<Node> list)
        {
            return list.OrderBy(n => n.Priority).ToList();
        }

        float EstimateDistance(GameObject target, GameObject current)
        {
            // manhattan distance
            return Math.Abs(target.transform.position.x - current.transform.position.x) + Math.Abs(target.transform.position.z - current.transform.position.z);
        }
    }
}
