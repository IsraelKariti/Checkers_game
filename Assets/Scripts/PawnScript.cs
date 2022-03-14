using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PawnType
{
    RED_PAWN,
    BLUE_PAWN,
    RED_KING,
    BLUE_KING
}

public class PawnScript : MonoBehaviour
{
    private CheckersManager checkersManager;
    private PawnType type;

    public PawnType Type { get => type; set => type = value; }

    public void setCheckersManager(CheckersManager cm)
    {
        checkersManager = cm;   
    }
    public void ResolveMovement()
    {
        Debug.Log("movementttttttttttttttt");
    }
}
