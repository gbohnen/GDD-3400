using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {

    #region Fields

    float velocity = 1;

    #endregion

    #region Properties

    Vector3 Target
    { get; set; }

    public bool Moving
    { get; set; }

    public GridCell CurrentCell
    { get; set; }

    public GridCell TargetCell
    { get; set; }

    #endregion

    #region Methods

    public void NavigateTo(int x, int z)
    {
        if (!Moving)
        {
            Target = new Vector3(x, gameObject.transform.position.y, z);
            Moving = true;
        }
    }

    public void Update()
    {
        // move
        if (Moving)
        {
            // calculate direction vector
            Vector3 direction = Target - gameObject.transform.position;

            // normalize
            direction.Normalize();

            // move based on time and velocity
            gameObject.transform.position += direction * velocity * Time.deltaTime;

            // check if agent is within acceptable margins of the target
            if (gameObject.transform.position.x >= Target.x - .1f
                && gameObject.transform.position.x <= Target.x + .1f
                && gameObject.transform.position.z >= Target.z - .1f
                && gameObject.transform.position.z <= Target.z + .1f)
            {
                Moving = false;
                gameObject.transform.position = Target;
            }
        }


        // set current tile occupied
        RaycastHit hit;
        if (Physics.Raycast(gameObject.transform.position, Vector3.down, out hit, 1f))
        {
            if (hit.collider.tag == "Tile")
            {
                hit.collider.gameObject.GetComponent<GridCell>().State = CellState.Occupied;
            }
        }
    }

    #endregion
}
