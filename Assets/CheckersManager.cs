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
    }

    private void InitBlue()
    {
        GameObject temp = Instantiate(pawnBluePrefab, board.transform);
        temp.transform.localPosition = new Vector3(0.05f, 0.07f, 0.05f);
        bluePawnList.Add(temp);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
