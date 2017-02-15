using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Assets.Scripts
{
	public class ObstacleScript : MonoBehaviour
	{
		private const float MAX_TIME = 10.0f;
		private const float MIN_TIME = 5.0f;
		private float timer;

		private GameObject currentCell;
		public GameObject CurrentCell {
			get
			{
				return currentCell;
			}
			set
			{
				currentCell.GetComponent<GridCellScript>().IsOccupied = false;
				currentCell = value;
				currentCell.GetComponent<GridCellScript>().IsOccupied = true;
			}
		}

		/// <summary>
		/// Initialize the obstacles
		/// </summary>
		/// <param name="currentCell"></param>
		public void Initialize(GameObject currentCell)
		{
			this.currentCell = currentCell;
		}

		/// <summary>
		/// Use this for initialization
		/// </summary>
		/// <param name=""></param>
		/// <param name="Start"></param>
		/// <returns></returns>
		void Start()
		{
		}

		/// <summary>
		/// Update the obstacles - if the timer has elapsed, move the obstacle randomly
		/// </summary>
		void Update()
		{
		}
	}
}