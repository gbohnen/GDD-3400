using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {

    #region Fields

    float velocity = 1;

    #endregion

    #region Properties
    
    public bool Moving
    { get; set; }

    GridCell CurrentCell
    { get; set; }

    GridCell TargetCell
    { get; set; }

    Vector3 Direction
    { get; set;}

    #endregion

    #region Methods

    public void Initialize(GridCell cell)
    {
        CurrentCell = cell;
        gameObject.transform.position = new Vector3(cell.Position.x, .7f, cell.Position.y);
    }

    public void NavigateTo(GridCell cell)
    {
        if (!Moving)
        {
            TargetCell = cell;
            Moving = true;
        }
    }

    public void Update()
    {
        // move
        if (Moving)
        {
            // calculate direction vector
            Vector3 direction = TargetCell.gameObject.transform.position - gameObject.transform.position;

            // normalize
            direction.Normalize();

            Direction = direction;

            // move based on time and velocity
            gameObject.transform.position += direction * velocity * Time.deltaTime;

            // check if agent is within acceptable margins of the target
            if (gameObject.transform.position.x >= TargetCell.gameObject.transform.position.x - .1f
                && gameObject.transform.position.x <= TargetCell.gameObject.transform.position.x + .1f
                && gameObject.transform.position.z >= TargetCell.gameObject.transform.position.z - .1f
                && gameObject.transform.position.z <= TargetCell.gameObject.transform.position.z + .1f)
            {
                Moving = false;
                gameObject.transform.position = TargetCell.gameObject.transform.position;
                TargetCell.State = CellState.Occupied;
                CurrentCell = TargetCell;
            }
        }

        // set current cell occupied
        CurrentCell.State = CellState.Occupied;

        // set current tile occupied
        RaycastHit hit;
        if (Physics.Raycast(gameObject.transform.position, Vector3.down, out hit, 2f))
        {
            if (hit.collider.tag == "Tile")
            {
                hit.collider.gameObject.GetComponent<GridCell>().State = CellState.Occupied;
                CurrentCell = hit.collider.gameObject.GetComponent<GridCell>();
            }
        }
    }

    #endregion
}
