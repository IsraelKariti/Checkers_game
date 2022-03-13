using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckersManager : MonoBehaviour
{
    public GameObject pawnRedPrefab;
    public GameObject pawnBluePrefab;
    public GameObject visualBoard;
    //public CheckersManager checkersManager;
  

    private List<GameObject> redPawnList;
    private List<GameObject> bluePawnList;
    private int[,] backendBoard;
    // Start is called before the first frame update
    void Start()
    {
        redPawnList = new List<GameObject>();
        bluePawnList = new List<GameObject>();
        backendBoard = new int[8, 8] { 
            { 1,0,1,0,1,0,1,0},// 1 - blue
            { 0,1,0,1,0,1,0,1},
            { 1,0,1,0,1,0,1,0},
            { 0,0,0,0,0,0,0,0},
            { 0,0,0,0,0,0,0,0},
            { 0,2,0,2,0,2,0,2},// 2 - red
            { 2,0,2,0,2,0,2,0},
            { 0,2,0,2,0,2,0,2}
        };
        InitBoard();
    }

    private void InitBoard()
    {
        InitBlue();
        InitRed();
    }



    private void InitRed()
    {
        // position of the red left bottom corner
        float xPos = 0.15f;
        float zPos = 0.55f;

        // initialize the first line
        InitLine(xPos, zPos, pawnRedPrefab, redPawnList);

        // initialize the first line
        InitLine(xPos - 0.1f, zPos + 0.1f, pawnRedPrefab, redPawnList);

        // initialize the first line
        InitLine(xPos, zPos + 0.2f, pawnRedPrefab, redPawnList);
    }

    private void InitBlue()
    {
        // position of the bottom left corner
        float xPos = 0.05f;
        float zPos = 0.05f;

        // initialize the first line
        InitLine(xPos, zPos, pawnBluePrefab, bluePawnList);

        // initialize the first line
        InitLine(xPos+0.1f, zPos+0.1f, pawnBluePrefab, bluePawnList);

        // initialize the first line
        InitLine(xPos, zPos+0.2f, pawnBluePrefab, bluePawnList);

    }

    private void InitLine(float xPos, float zPos, GameObject prefab, List<GameObject> list)
    {
        GameObject temp;

        for (int i = 0; i < 4; i++)
        {
            // create the pawn
            temp = Instantiate(prefab, visualBoard.transform);
            // give the pawn ref to this script
            temp.GetComponent<PawnScript>().setCheckersManager(this);
            // set the pawn position
            temp.transform.localPosition = new Vector3(xPos, 0.07f, zPos);
            // save pawn to a list
            list.Add(temp);
            xPos += 0.2f;
        }
    }

   
}
