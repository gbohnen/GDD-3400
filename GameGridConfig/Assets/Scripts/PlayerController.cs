using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float maxVelocity;
    public float maxAccel;
    public float arriveThreshold;
    public float arriveRadius;

    Vector3 linearAccel;
    Vector3 velocity;

    public Vector3 Target
    { get; private set; }

    void Start()
    {
        
    }

    void Update()
    {
        // get target
        Target = GetPositionFromMouse();

        // set acceleration, zero y value
        Vector3 movement = Target - transform.position;
        movement.y = 0;
        linearAccel = movement;

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

    }

    Vector3 GetPositionFromMouse()
    {
        Vector3 mouse = Input.mousePosition;
        mouse = Camera.main.ScreenToWorldPoint(mouse);

        mouse.y = GameConstants.AGENT_HEIGHT;

        return mouse;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Edge")
        {

        }
    }
}
