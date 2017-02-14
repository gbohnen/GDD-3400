using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
	public class GridCellScript : MonoBehaviour
	{

		private bool isOccupied;
		private bool isTargeted;

		public List<GameObject> neighbors;

		public GridCellScript()
		{
			neighbors = new List<GameObject>();
		}

		public bool IsCoin { get; set; }

		/// <summary>
		/// Is occupied by an agent, obstacle, or other object
		/// </summary>
		public bool IsOccupied
		{
			get
			{
				return isOccupied;
			}
			set
			{
				isOccupied = value;
				if (isOccupied)
				{
					gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
				}
				else
				{
					gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
				}
			}
		}

		/// <summary>
		/// Is targeted by an agent
		/// </summary>
		public bool IsTargeted
		{
			get
			{
				return isTargeted;
			}
			set
			{
				isTargeted = value;
				if (isTargeted)
				{
					gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
				}
				else
				{
					gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
				}
			}
		}
	}
}