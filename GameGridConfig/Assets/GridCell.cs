using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellState { Empty = 0, Occupied = 1, Targeted = 2 }

public class GridCell: MonoBehaviour {

    #region Methods

    void Update()
    {
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

    public void Initialize()
    {
        State = CellState.Empty;
        Neighbors = new List<GridCell>();
    }

    #endregion

    #region Properties

    public CellState State
    { get; set; }

    public List<GridCell> Neighbors
    { get; set; }

    #endregion

}
