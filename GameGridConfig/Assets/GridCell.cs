using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellState { Empty = 0, Occupied = 1, Targeted = 2 }

public class GridCell: MonoBehaviour {

    #region Fields

    float occupationTimer = 0;
    CellState state;

    #endregion

    #region Methods

    void Update()
    {
        // update timer, reset
        
        if (occupationTimer > 0f)
        {
            occupationTimer -= Time.deltaTime;
            Debug.Log(occupationTimer);
        }
        else if (state != CellState.Targeted)
        {
            State = CellState.Empty;
        }

        // update cell color
        switch (State)
        {
            case CellState.Empty:
                gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                break;
            case CellState.Occupied:
                gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                break;
            case CellState.Targeted:
                gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
                break;
        }
    }

    public void Initialize(int x, int z)
    {
        State = CellState.Empty;
        Neighbors = new List<GridCell>();
        Position = new Vector2(x, z);
    }

    #endregion

    #region Properties

    public Vector2 Position
    { get; set; }

    public CellState State
    {
        get
        {
            return state;
        }
        set
        {
            if (value == CellState.Occupied)
            {
                occupationTimer = .01f;
            }
            state = value;
        }
    }


    public List<GridCell> Neighbors
    { get; set; }

    #endregion

}
