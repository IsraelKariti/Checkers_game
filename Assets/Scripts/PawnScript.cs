using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnScript : MonoBehaviour
{
    private CheckersManager checkersManager;

    public void setCheckersManager(CheckersManager cm)
    {
        checkersManager = cm;   
    }
    public void ResolveMovement()
    {
        Debug.Log("movementttttttttttttttt");
    }
}
