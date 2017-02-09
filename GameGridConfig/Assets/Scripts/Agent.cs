using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {

    #region Fields

    public float playerFleeWeight = 0.3f;
    public float alignToAlliesWeight = 0.3f;
    public float avoidAlliesWeight = 0.3f;
    public float joinHerdWeight = 0.1f;

    public float maxVelocity;
    public float maxAccel;
    public float arriveThreshold;
    public float arriveRadius;

    Vector3 linearAccel;
    Vector3 velocity;

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

    public Vector3 Target
    { get; private set; }

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

    public void SetMovementFactors(Vector3 playerPosition, Vector3 avgVelocity, Vector3 avgPosition)
    {
        Vector3 avgMovement = Vector3.zero;

        // add direction away from player
        avgMovement += ((transform.position - playerPosition));
        Debug.Log("PlayerPos" + playerPosition);
        Debug.Log("AvoidPlayer" + avgMovement);

        //// add average flock velocity
        //avgMovement += (avgVelocity.normalized);
        //Debug.Log("avgVelocity" + avgVelocity);

        //// add average flock position
        //avgMovement += (avgPosition);
        //Debug.Log("avgposition" + avgPosition);
        //Debug.Log("AvgMovement" + avgMovement);

        avgMovement.y = 0;

        Target = avgMovement;
    }

    public void Update()
    {
        // set acceleration, zero y value
        Vector3 movement = Target + transform.position;
        movement.y = 0;
        linearAccel = movement;

        Debug.Log("Target: " + Target);

        // update and cap acceleration
        if (linearAccel.magnitude > maxAccel)
        {
            linearAccel.Normalize();
            linearAccel *= maxAccel;
        }

        // update and cap velocity
        velocity += linearAccel * Time.deltaTime;
        if (velocity.magnitude > maxVelocity)
        {
            velocity.Normalize();
            velocity *= maxVelocity;
        }

        // arrive behavior
        if (movement.magnitude < arriveThreshold)
        {
            float distance = movement.magnitude;

            if (distance <= arriveRadius)
                velocity = Vector3.zero;

            if (distance < arriveThreshold)
            {
                velocity.Normalize();
                velocity *= (maxVelocity * distance / arriveThreshold);
            }
        }

        // update position
        GetComponent<Rigidbody>().velocity = velocity;

        //transform.position += velocity * Time.deltaTime + .5f * linearAccel * Time.deltaTime * Time.deltaTime;

        // update orientation
        if (velocity.sqrMagnitude > 0)
        {
            transform.rotation = Quaternion.Euler(0, Mathf.Rad2Deg * Mathf.Atan2(velocity.x, velocity.z), 0);
        }





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
