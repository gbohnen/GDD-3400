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

    public Vector3 Velocity
    { get; private set; }

    public Vector3 Orientation
    { get; private set;}

    #endregion

    #region Methods

    public void Initialize(GridCell cell)
    {
        CurrentCell = cell;
        transform.position = new Vector3(cell.Position.x, GameConstants.AGENT_HEIGHT, cell.Position.y);
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



        #region Gridstuff
        // grid based movement
        //// move
        //if (Moving)
        //{
        //    // calculate direction vector
        //    Vector3 direction = TargetCell.transform.position - transform.position;

        //    // reset the vertical height
        //    direction.y = 0;

        //    // if the agent is far enough away from the point
        //    if (direction.sqrMagnitude > .01)
        //    {

        //        // normalize
        //        direction.Normalize();

        //        Direction = direction;

        //        // move based on time and velocity
        //        transform.position += direction * velocity * Time.deltaTime;
        //    }

        //    else
        //    {
        //        Moving = false;
        //        Vector3 place = TargetCell.transform.position;
        //        place.y = 1;
        //        transform.position = place;
        //        TargetCell.State = CellState.Occupied;
        //        CurrentCell = TargetCell;
        //    }
        //}

        //// set current cell occupied
        //CurrentCell.State = CellState.Occupied;

        //// set current tile occupied
        //RaycastHit hit;
        //if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        //{
        //    if (hit.collider.tag == "Tile")
        //    {
        //        hit.collider.gameObject.GetComponent<GridCell>().State = CellState.Occupied;
        //        CurrentCell = hit.collider.gameObject.GetComponent<GridCell>();
        //    }
        //}
        #endregion
    }

    #endregion
}
