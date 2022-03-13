using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckersManager : MonoBehaviour
{
    public GameObject pawnRedPrefab;
    public GameObject pawnBluePrefab;
    public GameObject board;

    private List<GameObject> redPawnList;
    private List<GameObject> bluePawnList;
    // Start is called before the first frame update
    void Start()
    {
        redPawnList = new List<GameObject>();
        bluePawnList = new List<GameObject>();
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
        InitLine(xPos, zPos, pawnRedPrefab);

        // initialize the first line
        InitLine(xPos - 0.1f, zPos + 0.1f, pawnRedPrefab);

        // initialize the first line
        InitLine(xPos, zPos + 0.2f, pawnRedPrefab);
    }

    private void InitBlue()
    {
        // position of the bottom left corner
        float xPos = 0.05f;
        float zPos = 0.05f;

        // initialize the first line
        InitLine(xPos, zPos, pawnBluePrefab);

        // initialize the first line
        InitLine(xPos+0.1f, zPos+0.1f, pawnBluePrefab);

        // initialize the first line
        InitLine(xPos, zPos+0.2f, pawnBluePrefab);

    }

    private void InitLine(float xPos, float zPos, GameObject prefab)
    {
        GameObject temp;

        for (int i = 0; i < 4; i++)
        {
            temp = Instantiate(prefab, board.transform);
            temp.transform.localPosition = new Vector3(xPos, 0.07f, zPos);
            bluePawnList.Add(temp);
            xPos += 0.2f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
