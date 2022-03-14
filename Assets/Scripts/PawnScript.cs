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
    private int insideAnotherPawnCounter = 0;
    private GameObject[,] boardMatrix;
    private int dropXIndex;
    private int dropZIndex;
    public PawnType PawnType { get => pawnType; set => pawnType = value; }
    public int XIndex { get => xIndex; set => xIndex = value; }
    public int ZIndex { get => zIndex; set => zIndex = value; }
    public CheckersManager CheckersManager { get => checkersManager; set => checkersManager = value; }
    public GameObject[,] BoardMatrix { get => boardMatrix; set => boardMatrix = value; }

    private void OnTriggerEnter(Collider other)
    {
        insideAnotherPawnCounter++;
    }
    private void OnTriggerExit(Collider other)
    {
        insideAnotherPawnCounter--;
    }

    // this function is called when the player picks up a soldier
    public void ResolveStartMovement()
    {
        Debug.Log("movementttttttttttttttt");
    }
    // This function is called when the user release the grip 
    public void ResolveEndMovement()
    {
        // STEP 0: Check if this pawn color is even suppose to play now
        if (!IsCorrectPlayerMoving())
        {
            Debug.Log("Sorry, it's the other guys' turn!");
            // snap back to original position
            transform.localPosition = SnapBack();
            return;
        }
        //STEP 1: get the position of the drop
        float dropX = transform.localPosition.x;
        float dropZ = transform.localPosition.z;
        //STEP 2: check if the drop is inside the board borders
        if(dropX<0 || dropX > 0.8 || dropZ<0 || dropZ > 0.8)
        {
            Debug.Log("pawn dropped out of bounds!");
            // snap back to original position
            transform.localPosition = SnapBack();
            return;
        }
        //STEP 3: check if pawn droppen on top of another pawn
        if (CheckIfOnTopAnotherPawn()>0)
        {
            Debug.Log("pawn dropped on top of another pawn!");
            // snap back to original position
            transform.localPosition = SnapBack();
            return;
        }
        //STEP 4: find the respective matrix indices of the drop position
        dropXIndex = ConvertPosToIndex(dropX);
        dropZIndex = ConvertPosToIndex(dropZ);
        //STEP 5: Check if the position is legal move for this pawn
        if (!CheckIfLegalMove())
        {
            Debug.Log("That's an illagal move!");
            // snap back to original position
            transform.localPosition = SnapBack();
            return;
        }
    }

    // if we have got this far it means that the correct pawn was picked, and then released in bounds, on an empty square
    // so now we only have to check if that movement is legal according to checkers rules
    private bool CheckIfLegalMove()
    {
        // check if the movement is a diagonal movement
        if (!CheckIfMoveIsDiagonal())
            return false;

        // check if the player dropped the pawn back in its original position
        if(!CheckIfOrigAndDestDiffer())
        {
            return false;
        }

        // 
        if(checkersManager.GameStatus==GameStatus.BLUE_TURN && pawnType == PawnType.BLUE_PAWN)
        {
            if(!AttemptBluePawnFirstStep())
            {
                return false;
            }
        }
        else if(checkersManager.GameStatus == GameStatus.BLUE_REPEAT && pawnType == PawnType.BLUE_PAWN)
        {
            if (!AttemptBluePawnRepeatStep())
            {
                return false;
            }
        }
        else if (checkersManager.GameStatus == GameStatus.BLUE_TURN && pawnType == PawnType.BLUE_KING)
        {
            if (!AttemptBlueKingStep())
            {
                return false;
            }
        }
        if(checkersManager.GameStatus==GameStatus.RED_TURN && pawnType == PawnType.RED_PAWN)
        {

        }
        else if(checkersManager.GameStatus == GameStatus.RED_REPEAT && pawnType == PawnType.RED_PAWN)
        {

        }
        else if (checkersManager.GameStatus == GameStatus.RED_TURN && pawnType == PawnType.RED_KING)
        {

        }
        return true;
    }

    private bool AttemptBlueKingStep()
    {
        // check how many blue and red pawns between src and dst
        int redCounter = 0;
        int blueCounter = 0;
        // the direction of the diagonal from src to dst
        int xDir = xIndex < dropXIndex ? 1 : -1;
        int zDir = zIndex < dropZIndex ? 1 : -1;
        // run on all the squares between the src and dst
        for(int x = xIndex, z = zIndex; x < dropXIndex; x+=xDir, z += zDir)
        {
            if (boardMatrix[z, x] == null)
                continue;
            PawnType pt = boardMatrix[z, x].GetComponent<PawnScript>().PawnType;
            if (pt == PawnType.BLUE_KING || pt == PawnType.BLUE_PAWN)
                blueCounter++;
            if (pt == PawnType.RED_KING || pt == PawnType.RED_PAWN)
                redCounter++;
            
        }
        // there are foes in the way OR there are more than one rival
        if (blueCounter > 0 || redCounter > 1)
            return false;


    }



    // check if the blue pawn first step is legal: move a square OR eat a square
    private bool AttemptBluePawnFirstStep()
    {
        // check if the blue player first step is move with no eating
        if(AttemptBlueMoveRightUp() || AttemptBlueMoveLeftUp() )
        {
            return true;
        }
        // check if the blue first step is an eating step
        if(AttemptBlueEatRightUp() || AttemptBlueEatLeftUp())
        {
            return true;
        }
        
        return false;
    }
    // check if the blue pawn first step is legal: move a square OR eat a square
    private bool AttemptBluePawnRepeatStep()
    {
        // check if the blue player first step is move with no eating
        if (AttemptBlueEatRightUp() || AttemptBlueEatLeftUp() || AttemptBlueEatLeftDown() || AttemptBlueEatRightDown())
        {
            return true;
        }

        return false;
    }
    private bool AttemptBlueMoveRightUp()
    {
        // check up right move
        if (xIndex + 1 == dropXIndex && zIndex + 1 == dropZIndex)
        {
            boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
            boardMatrix[zIndex, xIndex] = null;
            return true;
        }
        return false;
    }
    private bool AttemptBlueMoveLeftUp()
    {
        // check up right move
        if (xIndex - 1 == dropXIndex && zIndex + 1 == dropZIndex)
        {
            boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
            boardMatrix[zIndex, xIndex] = null;
            return true;
        }
        return false;
    }
    private bool AttemptBlueMoveRightDown()
    {
        // check up right move
        if (xIndex + 1 == dropXIndex && zIndex - 1 == dropZIndex)
        {
            boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
            boardMatrix[zIndex, xIndex] = null;
            return true;
        }
        return false;
    }
    private bool AttemptBlueMoveLeftDown()
    {
        // check up right move
        if (xIndex - 1 == dropXIndex && zIndex - 1 == dropZIndex)
        {
            boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
            boardMatrix[zIndex, xIndex] = null;
            return true;
        }
        return false;
    }
    private bool AttemptBlueEatRightUp()
    {
        // check up right eat
        if (xIndex + 2 == dropXIndex && zIndex + 2 == dropZIndex)
        {
            // check if there is an enemy between src and dest positions
            PawnScript middlePawn = checkersManager.BoardMatrix[xIndex + 1, zIndex + 1]?.GetComponent<PawnScript>();
            if (middlePawn == null || middlePawn.pawnType == PawnType.BLUE_PAWN || middlePawn.pawnType == PawnType.BLUE_KING)
                return false;

            // move this pawn on the board matrix
            boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
            boardMatrix[zIndex, xIndex] = null;

            // eat the pawn in the middle
            Destroy(checkersManager.BoardMatrix[xIndex + 1, zIndex + 1]);
            checkersManager.BoardMatrix[xIndex + 1, zIndex + 1] = null;
            // update number of enemies
            checkersManager.decRed();
            //TODO: enter repeat mode
            return true;
        }
        return false;
    }
    private bool AttemptBlueEatLeftUp()
    {
        // check up right eat
        if (xIndex - 2 == dropXIndex && zIndex + 2 == dropZIndex)
        {
            // check if there is an enemy between src and dest positions
            PawnScript middlePawn = checkersManager.BoardMatrix[xIndex - 1, zIndex + 1]?.GetComponent<PawnScript>();
            if (middlePawn == null || middlePawn.pawnType == PawnType.BLUE_PAWN || middlePawn.pawnType == PawnType.BLUE_KING)
                return false;

            // move this pawn on the board matrix
            boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
            boardMatrix[zIndex, xIndex] = null;

            // eat the pawn in the middle
            Destroy(checkersManager.BoardMatrix[xIndex - 1, zIndex + 1]);
            checkersManager.BoardMatrix[xIndex - 1, zIndex + 1] = null;
            // update number of enemies
            checkersManager.decRed();
            //TODO: enter repeat mode
            return true;
        }
        return false;
    }
    private bool AttemptBlueEatRightDown()
    {
        // check up right eat
        if (xIndex + 2 == dropXIndex && zIndex - 2 == dropZIndex)
        {
            // check if there is an enemy between src and dest positions
            PawnScript middlePawn = checkersManager.BoardMatrix[xIndex + 1, zIndex - 1]?.GetComponent<PawnScript>();
            if (middlePawn == null || middlePawn.pawnType == PawnType.BLUE_PAWN || middlePawn.pawnType == PawnType.BLUE_KING)
                return false;

            // move this pawn on the board matrix
            boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
            boardMatrix[zIndex, xIndex] = null;

            // eat the pawn in the middle
            Destroy(checkersManager.BoardMatrix[xIndex + 1, zIndex - 1]);
            checkersManager.BoardMatrix[xIndex + 1, zIndex - 1] = null;
            // update number of enemies
            checkersManager.decRed();
            //TODO: enter repeat mode
            return true;
        }
        return false;
    }
    private bool AttemptBlueEatLeftDown()
    {
        // check up right eat
        if (xIndex - 2 == dropXIndex && zIndex - 2 == dropZIndex)
        {
            // check if there is an enemy between src and dest positions
            PawnScript middlePawn = checkersManager.BoardMatrix[xIndex - 1, zIndex - 1]?.GetComponent<PawnScript>();
            if (middlePawn == null || middlePawn.pawnType == PawnType.BLUE_PAWN || middlePawn.pawnType == PawnType.BLUE_KING)
                return false;

            // move this pawn on the board matrix
            boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
            boardMatrix[zIndex, xIndex] = null;

            // eat the pawn in the middle
            Destroy(checkersManager.BoardMatrix[xIndex - 1, zIndex - 1]);
            checkersManager.BoardMatrix[xIndex - 1, zIndex - 1] = null;
            // update number of enemies
            checkersManager.decRed();
            //TODO: enter repeat mode
            return true;
        }
        return false;
    }

    // This function check if the player didn't actually move the pawn
    private bool CheckIfOrigAndDestDiffer()
    {
        if (dropXIndex == xIndex && dropZIndex == zIndex)
            return false;
        return true;
    }

    

    // check if the pawn movement on the x and z axis has the same absolute value
    private bool CheckIfMoveIsDiagonal()
    {
        if (Math.Abs(xIndex - dropXIndex) == Math.Abs(zIndex - dropZIndex))
            return true;
        return false;
    }

    // check if the correct player is playing in the right turn
    private bool IsCorrectPlayerMoving()
    {
        // blue supposed to move but this is a red pawn
        if ((CheckersManager.GameStatus == GameStatus.BLUE_TURN || CheckersManager.GameStatus == GameStatus.BLUE_REPEAT) &&
            (pawnType == PawnType.RED_PAWN || pawnType == PawnType.RED_KING))
        {
            return false;
        }
        // red supposed to move but this is a blue pawn
        if ((CheckersManager.GameStatus == GameStatus.RED_TURN || CheckersManager.GameStatus == GameStatus.RED_REPEAT) &&
            (pawnType == PawnType.BLUE_PAWN || pawnType == PawnType.BLUE_KING))
        {
            return false;
        }
        return true;
    }

    

    // Convert the local position of the drop to the respective index in the board matrix
    private int ConvertPosToIndex(float dropX)
    {
        dropX *= 10;// ex. 0.234 -> 2.34
        return (int)dropX;// 2.34 -> 2
    }

    private int CheckIfOnTopAnotherPawn()
    {
        return insideAnotherPawnCounter;
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

}
