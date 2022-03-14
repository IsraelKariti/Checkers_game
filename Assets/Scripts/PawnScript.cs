using System;
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
    private PawnType pawnType;
    private int xIndex;
    private int zIndex;
    private float yBoardPos = 0.07f;
    public PawnType Type { get => pawnType; set => pawnType = value; }
    public int XIndex { get => xIndex; set => xIndex = value; }
    public int ZIndex { get => zIndex; set => zIndex = value; }
    public CheckersManager CheckersManager { get => checkersManager; set => checkersManager = value; }

    
    // this function is called when the player picks up a soldier
    public void ResolveStartMovement()
    {
        Debug.Log("movementttttttttttttttt");
        // regular blue move
        if(CheckersManager.GameStatus == GameStatus.BLUE_TURN && pawnType == PawnType.BLUE_PAWN )
        {
            regularBlueMove();
        }
        // repeated blue move
        else if (CheckersManager.GameStatus == GameStatus.BLUE_REPEAT && pawnType == PawnType.BLUE_PAWN)
        {

        }
        // king move
        else if (CheckersManager.GameStatus == GameStatus.BLUE_TURN && pawnType == PawnType.BLUE_KING)
        {

        }

        
    }
    public void ResolveEndMovement()
    {
        //STEP 1: get the position of the drop
        float dropX = transform.localPosition.x;
        float dropZ = transform.localPosition.z;
        //STEP 2: check if the drop is inside the board borders
        if(dropX<0 || dropX > 0.8 || dropZ<0 || dropZ > 0.8)
        {
            Debug.Log("pawn dropped out of bounds!");
            // snap back to original position
            transform.localPosition = SnapBack();
        }
        //STEP 3: find the respective matrix index of the drop position

    }

    private Vector3 SnapBack()
    {
        float x = ConvertIndexToPos(xIndex);
        float z = ConvertIndexToPos(zIndex);
        return new Vector3(x, yBoardPos, z);
    }

    // this function converts the index position in the board matrix to the position
    private float ConvertIndexToPos(int index)
    {
        float f = (float)index;
        f += 0.5f;
        f /= 10;
        return f;
    }


    // first time in this turn that the blue pawn is moving
    private void regularBlueMove()
    {
        //STEP 1: find all possible destinations

    }
}
