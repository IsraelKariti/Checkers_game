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
    private PawnType rivalPawn;
    private PawnType rivalKing;
    private int xIndex;
    private int zIndex;
    private float yBoardPos = 0.07f;
    //private int insideAnotherPawnCounter = 0;
    private GameObject[,] boardMatrix;
    private int dropXIndex;
    private int dropZIndex;
    private int fwd;// for blue this means +1, and for red this is -1 as they are heading in opposite directions
    //private bool isHeld = false;
    private bool rivalEaten;
    public PawnType PawnType { get => pawnType; 
            set { 
            pawnType = value;
            if(pawnType == PawnType.BLUE_PAWN || pawnType == PawnType.BLUE_KING)
            {
                fwd = 1;
                rivalPawn = PawnType.RED_PAWN;
                rivalKing = PawnType.RED_KING;
            }
            else
            {
                fwd = -1;
                rivalPawn = PawnType.BLUE_PAWN;
                rivalKing = PawnType.BLUE_KING;
            }
        } }
    public int XIndex { get => xIndex; set => xIndex = value; }
    public int ZIndex { get => zIndex; set => zIndex = value; }
    public CheckersManager CheckersManager { get => checkersManager; set => checkersManager = value; }
    public GameObject[,] BoardMatrix { get => boardMatrix; set => boardMatrix = value; }
    public int ForwardMovement { get => fwd; set => fwd = value; }
    public GameObject crown;
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (isHeld && other.gameObject.CompareTag("pawn")){
    //        insideAnotherPawnCounter++;
    //        Debug.Log(gameObject.ToString()+gameObject.transform.localPosition+" Enter collider: " + other.gameObject.ToString() + " " + other.transform.parent.localPosition + " count: "+ insideAnotherPawnCounter);
    //    }

    //}
    //private void OnTriggerExit(Collider other)
    //{
    //    if (isHeld && other.gameObject.CompareTag("pawn"))
    //    {
    //        insideAnotherPawnCounter--;
    //        Debug.Log(gameObject.ToString() + gameObject.transform.localPosition + " Exit collider: " + other.gameObject.ToString() + " " + other.transform.parent.localPosition + " count: " + insideAnotherPawnCounter);

    //    }
    //}

    // this function is called when the player picks up a soldier
    public void ResolveStartMovement()
    {
        Debug.Log("movementttttttttttttttt ");
        //isHeld = true;
        rivalEaten = false;
    }
    // This function is called when the user release the grip 
    public void ResolveEndMovement()
    {
        //isHeld = false;
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
        //STEP 4: find the respective matrix indices of the drop position
        dropXIndex = ConvertPosToIndex(dropX);
        dropZIndex = ConvertPosToIndex(dropZ);

        if (!CheckIfDroppedOnLegalSquare())
        {
            Debug.Log("pawn dropped on unavailable board square!");
            // snap back to original position
            transform.localPosition = SnapBack();
            return;
        }
        //STEP 3: check if pawn droppen on top of another pawn
        if (!CheckIfDroppedOnClearSquare())
        {
            Debug.Log("pawn dropped on top of another pawn!");
            // snap back to original position
            transform.localPosition = SnapBack();
            return;
        }
        
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
            Debug.Log("You havn't moved, try again!");
            return false;
        }

        // 
        if(checkersManager.GameStatus == GameStatus.BLUE_TURN || checkersManager.GameStatus == GameStatus.RED_TURN )
        {
            if(!AttemptFirstStep())
            {
                return false;
            }
        }
        else if(checkersManager.GameStatus == GameStatus.BLUE_REPEAT || checkersManager.GameStatus == GameStatus.RED_REPEAT)
        {
            // Check if the repeater is not me
            if (CheckersManager.Repeater != this)
                return false;

            if (!AttemptPawnRepeatStep())
            {
                return false;
            }
        }
        HandlePostStep();
        

        return true;
    }

    private void HandlePostStep()
    {
        // check if game is finished and update status
        if (checkersManager.RedCounter == 0)
        {
            checkersManager.GameStatus = GameStatus.BLUE_WON;
        }
        else if (checkersManager.BlueCounter == 0)
        {
            checkersManager.GameStatus = GameStatus.RED_WON;
        }
        // After the step has finished (move or eat) check if the pawn is located at the opposite edge
        else if ((pawnType == PawnType.BLUE_KING || pawnType == PawnType.BLUE_PAWN) && zIndex == 7)
        {
            crown.SetActive(true);
        }
        else if ((pawnType == PawnType.RED_KING || pawnType == PawnType.RED_PAWN) && zIndex == 0)
        {
            crown.SetActive(true);
        }
        // in case step included eating 
        else if (rivalEaten && IsPossibleToEatAgain())// check if this pawn can eat again
        {
            checkersManager.EnableRepeat();
        }
    }

    private bool IsPossibleToEatAgain()
    {
        if(IsPossibleToEatAgainFwdRight() || IsPossibleToEatAgainFwdLeft() || IsPossibleToEatAgainBwdRight() || IsPossibleToEatAgainBwdLeft())
        {
            return true;
        }
        return false;
    }

    private bool IsPossibleToEatAgainFwdRight()
    {
        int nearX = xIndex + 1;
        int nearZ = zIndex + 1 * fwd;
        int farX = xIndex + 2;
        int farZ = zIndex + 2 * fwd;

        return IsPossibleToEatAdjacentPawn(nearX, nearZ, farX, farZ);

    }
    private bool IsPossibleToEatAgainFwdLeft()
    {
        int nearX = xIndex - 1;
        int nearZ = zIndex + 1 * fwd;
        int farX = xIndex - 2;
        int farZ = zIndex + 2 * fwd;

        return IsPossibleToEatAdjacentPawn(nearX, nearZ, farX, farZ);

    }
    private bool IsPossibleToEatAgainBwdRight()
    {
        int nearX = xIndex + 1;
        int nearZ = zIndex - 1 * fwd;
        int farX = xIndex + 2;
        int farZ = zIndex - 2 * fwd;

        return IsPossibleToEatAdjacentPawn(nearX, nearZ, farX, farZ);

    }
    private bool IsPossibleToEatAgainBwdLeft()
    {
        int nearX = xIndex - 1;
        int nearZ = zIndex - 1 * fwd;
        int farX = xIndex - 2;
        int farZ = zIndex - 2 * fwd;

        return IsPossibleToEatAdjacentPawn(nearX, nearZ, farX, farZ);
    }
    private bool IsPossibleToEatAdjacentPawn(int nearX, int nearZ, int farX, int farZ)
    {
        if (nearX < 0 || nearX > 7 || nearZ < 0 || nearZ > 7)
            return false;
        if (farX < 0 || farX > 7 || farZ < 0 || farZ > 7)
            return false;
        if (boardMatrix[farZ, farX] != null)
            return false;
        PawnType nearPawnType = boardMatrix[nearZ, nearX].GetComponent<PawnScript>().pawnType;
        if (nearPawnType != rivalPawn && nearPawnType != rivalKing)
            return false;

        return true;
    }
    
    private bool AttemptFirstStep()
    {
        if(pawnType == PawnType.RED_PAWN || pawnType == PawnType.BLUE_PAWN)
        {
            return AttemptPawnFirstStep();
        }
        else
        {
            return AttemptKingStep();
        }
    }

    private bool AttemptKingStep()
    {
        return AttemptKingMove() || AttemptKingEat();
           
    }
    private bool AttemptKingMove()
    {
        // check how many blue and red pawns between src and dst
        int rivalCounter = 0;
        int rivalX = 0;
        int rivalZ = 0;
        int foeCounter = 0;
        // the direction of the diagonal from src to dst
        int xDir = xIndex < dropXIndex ? 1 : -1;
        int zDir = zIndex < dropZIndex ? 1 : -1;
        // run on all the squares between the src and dst
        for (int x = xIndex, z = zIndex; x < dropXIndex; x += xDir, z += zDir)
        {
            if (boardMatrix[z, x] == null)
                continue;
            PawnType pt = boardMatrix[z, x].GetComponent<PawnScript>().PawnType;
            if (pt == rivalPawn || pt == rivalKing)
            {
                rivalCounter++;
                // save the matrix indexes of the rival
                rivalX = x;
                rivalZ = z;
            }
            else
            {
                foeCounter++;
            }
        }
        // there are foes in the way OR there are more than one rival
        if (foeCounter > 0 || rivalCounter > 0)
            return false;

        // move this pawn on the board matrix
        boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
        boardMatrix[zIndex, xIndex] = null;

        // update local indices
        xIndex = dropXIndex;
        zIndex = dropZIndex;

        //TODO: enter repeat mode
        checkersManager.SwitchPlayer();
        return true;
    }
    private bool AttemptKingEat()
    {
        // check how many blue and red pawns between src and dst
        int rivalCounter = 0;
        int rivalX = 0;
        int rivalZ = 0;
        int foeCounter = 0;
        // the direction of the diagonal from src to dst
        int xDir = xIndex < dropXIndex ? 1 : -1;
        int zDir = zIndex < dropZIndex ? 1 : -1;
        // run on all the squares between the src and dst
        for (int x = xIndex, z = zIndex; x < dropXIndex; x += xDir, z += zDir)
        {
            if (boardMatrix[z, x] == null)
                continue;
            PawnType pt = boardMatrix[z, x].GetComponent<PawnScript>().PawnType;
            if (pt == rivalPawn || pt == rivalKing)
            {
                rivalCounter++;
                // save the matrix indexes of the rival
                rivalX = x;
                rivalZ = z;
            }
            else
            {
                foeCounter++;
            }
        }
        // there are foes in the way OR there are more than one rival
        if (foeCounter > 0 || rivalCounter != 1)
            return false;

        // move this pawn on the board matrix
        boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
        boardMatrix[zIndex, xIndex] = null;


        // update local indices
        xIndex = dropXIndex;
        zIndex = dropZIndex;

        // check if this step is eat 
        if (rivalCounter > 0)
        {
            // eat the pawn in the middle
            Destroy(boardMatrix[rivalZ, rivalX]);
            boardMatrix[rivalZ, rivalX] = null;
            // update number of enemies
            DecRival();
        }

        //TODO: enter repeat mode
        return true;
    }

    // decrease the number count of rivals
    private void DecRival()
    {
        if (rivalPawn == PawnType.RED_PAWN)
            checkersManager.decRed();
        else
            checkersManager.decBlue();

        rivalEaten = true;
    }

    // check if the blue pawn first step is legal: move a square OR eat a square
    private bool AttemptPawnFirstStep()
    {
        // check if the blue player first step is move with no eating
        if(AttemptMoveFwdRight() || AttemptMoveFwdLeft() )
        {
            
            return true;
        }
        // check if the blue first step is an eating step
        if(AttemptEatFwdRight() || AttemptEatFwdLeft())
        {
            return true;
        }
        
        return false;
    }
    // check if the blue pawn first step is legal: move a square OR eat a square
    private bool AttemptPawnRepeatStep()
    {
        // check if the blue player first step is move with no eating
        if (AttemptEatFwdRight() || AttemptEatFwdLeft() || AttemptEatBwdLeft() || AttemptEatBwdRight())
        {
            return true;
        }

        return false;
    }
    // try to move the pawn forward (+1 for blue, -1 for red) and positive on Z axis
    private bool AttemptMoveFwdRight()
    {
        // check up right move
        if (xIndex + 1 == dropXIndex && zIndex + fwd == dropZIndex)
        {
            boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
            boardMatrix[zIndex, xIndex] = null;
            checkersManager.SwitchPlayer();


            // update local indices
            xIndex = dropXIndex;
            zIndex = dropZIndex;

            return true;
        }
        return false;
    }
    private bool AttemptMoveFwdLeft()
    {
        // check up right move
        if (xIndex - 1 == dropXIndex && zIndex + fwd == dropZIndex)
        {
            boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
            boardMatrix[zIndex, xIndex] = null;
            checkersManager.SwitchPlayer();

            // update local indices
            xIndex = dropXIndex;
            zIndex = dropZIndex;
            return true;
        }
        return false;
    }
    private bool AttemptMoveBwdRight()
    {
        // check up right move
        if (xIndex + 1 == dropXIndex && zIndex - fwd == dropZIndex)
        {
            boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
            boardMatrix[zIndex, xIndex] = null;
            checkersManager.SwitchPlayer();

            // update local indices
            xIndex = dropXIndex;
            zIndex = dropZIndex;
            return true;
        }
        return false;
    }
    private bool AttemptMoveBwdLeft()
    {
        // check up right move
        if (xIndex - 1 == dropXIndex && zIndex - fwd == dropZIndex)
        {
            boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
            boardMatrix[zIndex, xIndex] = null;
            checkersManager.SwitchPlayer();

            // update local indices
            xIndex = dropXIndex;
            zIndex = dropZIndex;
            return true;
        }
        return false;
    }
    private bool AttemptEatFwdRight()
    {
        // check up right eat
        if (xIndex + 2 == dropXIndex && zIndex + 2*fwd == dropZIndex)
        {
            // check if there is an enemy between src and dest positions
            PawnScript middlePawn = boardMatrix[zIndex + fwd, xIndex + 1]?.GetComponent<PawnScript>();
            if (middlePawn == null || middlePawn.pawnType != rivalPawn && middlePawn.pawnType == rivalKing)
                return false;

            // move this pawn on the board matrix
            boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
            boardMatrix[zIndex, xIndex] = null;

            
            // eat the pawn in the middle
            Destroy(boardMatrix[zIndex + fwd, xIndex + 1]);
            boardMatrix[zIndex + fwd, xIndex + 1] = null;
            // update local indices
            xIndex = dropXIndex;
            zIndex = dropZIndex;
            // update number of enemies
            DecRival();
            //TODO: enter repeat mode
            return true;
        }
        return false;
    }
    private bool AttemptEatFwdLeft()
    {
        // check up right eat
         if (xIndex - 2 == dropXIndex && zIndex + 2*fwd == dropZIndex)
        {
            // check if there is an enemy between src and dest positions
            PawnScript middlePawn = boardMatrix[zIndex + fwd, xIndex - 1 ]?.GetComponent<PawnScript>();
            if (middlePawn == null || middlePawn.pawnType != rivalPawn && middlePawn.pawnType == rivalKing)
                return false;

            // move this pawn on the board matrix
            boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
            boardMatrix[zIndex, xIndex] = null;

            
            // eat the pawn in the middle
            Destroy(boardMatrix[zIndex + fwd, xIndex - 1]);
            boardMatrix[zIndex + fwd, xIndex - 1] = null;
            // update local indices
            xIndex = dropXIndex;
            zIndex = dropZIndex;
            // update number of enemies
            DecRival();
            //TODO: enter repeat mode
            return true;
        }
        return false;
    }
    private bool AttemptEatBwdRight()
    {
        // check up right eat
        if (xIndex + 2 == dropXIndex && zIndex - 2*fwd == dropZIndex)
        {
            // check if there is an enemy between src and dest positions
            PawnScript middlePawn = boardMatrix[ zIndex - fwd, xIndex + 1]?.GetComponent<PawnScript>();
            if (middlePawn == null || middlePawn.pawnType != rivalPawn && middlePawn.pawnType == rivalKing)
                return false;

            // move this pawn on the board matrix
            boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
            boardMatrix[zIndex, xIndex] = null;

            

            // eat the pawn in the middle
            Destroy(boardMatrix[zIndex - fwd,  xIndex + 1]);
            boardMatrix[zIndex - fwd, xIndex + 1] = null;
            // update local indices
            xIndex = dropXIndex;
            zIndex = dropZIndex;
            // update number of enemies
            DecRival();
            //TODO: enter repeat mode
            return true;
        }
        return false;
    }
    private bool AttemptEatBwdLeft()
    {
        // check up right eat
        if (xIndex - 2 == dropXIndex && zIndex - 2*fwd == dropZIndex)
        {
            // check if there is an enemy between src and dest positions
            PawnScript middlePawn = boardMatrix[zIndex - fwd,xIndex - 1]?.GetComponent<PawnScript>();
            if (middlePawn == null || middlePawn.pawnType != rivalPawn && middlePawn.pawnType == rivalKing)
                return false;

            // move this pawn on the board matrix
            boardMatrix[dropZIndex, dropXIndex] = boardMatrix[zIndex, xIndex];
            boardMatrix[zIndex, xIndex] = null;

            

            // eat the pawn in the middle
            Destroy(boardMatrix[zIndex - fwd,xIndex - 1 ]);
            boardMatrix[zIndex - fwd, xIndex - 1 ] = null;
            // update local indices
            xIndex = dropXIndex;
            zIndex = dropZIndex;
            // update number of enemies
            DecRival();
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

    private bool CheckIfDroppedOnLegalSquare()
    {
        // 
        if(dropZIndex%2 == dropXIndex%2)
            return true;
        return false;
    }
    private bool CheckIfDroppedOnClearSquare()
    {
        // 
        if(boardMatrix[dropZIndex,dropXIndex] == null)
            return true;
        return false;
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
