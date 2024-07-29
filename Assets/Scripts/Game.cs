// Game.cs:
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public GameObject chessPiecePrefab;
    public GameObject lastMovedPawn;
    public int lastMovedPawnInitialY;

    private GameObject[,] positions = new GameObject[8, 8];
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];

    private string currentPlayer = "white";
    private bool isGameOver = false;
    private bool isPromotionComplete = true;

    void Start()
    {
        playerWhite = CreateTeam("white");
        playerBlack = CreateTeam("black");

        // Set all piece positions on the board
        for (int i = 0; i < playerBlack.Length; i++)
        {
            SetPosition(playerBlack[i]);
            SetPosition(playerWhite[i]);
        }
    }

    private GameObject[] CreateTeam(string color)
    {
        return new GameObject[] {
            CreateChessPiece($"{color}_rook", 0, color == "white" ? 0 : 7),
            CreateChessPiece($"{color}_knight", 1, color == "white" ? 0 : 7),
            CreateChessPiece($"{color}_bishop", 2, color == "white" ? 0 : 7),
            CreateChessPiece($"{color}_queen", 3, color == "white" ? 0 : 7),
            CreateChessPiece($"{color}_king", 4, color == "white" ? 0 : 7),
            CreateChessPiece($"{color}_bishop", 5, color == "white" ? 0 : 7),
            CreateChessPiece($"{color}_knight", 6, color == "white" ? 0 : 7),
            CreateChessPiece($"{color}_rook", 7, color == "white" ? 0 : 7),
            CreateChessPiece($"{color}_pawn", 0, color == "white" ? 1 : 6),
            CreateChessPiece($"{color}_pawn", 1, color == "white" ? 1 : 6),
            CreateChessPiece($"{color}_pawn", 2, color == "white" ? 1 : 6),
            CreateChessPiece($"{color}_pawn", 3, color == "white" ? 1 : 6),
            CreateChessPiece($"{color}_pawn", 4, color == "white" ? 1 : 6),
            CreateChessPiece($"{color}_pawn", 5, color == "white" ? 1 : 6),
            CreateChessPiece($"{color}_pawn", 6, color == "white" ? 1 : 6),
            CreateChessPiece($"{color}_pawn", 7, color == "white" ? 1 : 6)
        };
    }

    public GameObject CreateChessPiece(string name, int x, int y)
    {
        GameObject obj = Instantiate(chessPiecePrefab, new Vector3(0, 0, -1), Quaternion.identity);
        Chessman cm = obj.GetComponent<Chessman>();
        cm.name = name;
        cm.SetXBoard(x);
        cm.SetYBoard(y);
        cm.Activate();
        return obj;
    }

    public void SetLastMovedPawn(GameObject pawn, int initialY)
    {
        lastMovedPawn = pawn;
        lastMovedPawnInitialY = initialY;
    }

    public void SetPosition(GameObject obj)
    {
        Chessman cm = obj.GetComponent<Chessman>();
        positions[cm.GetXBoard(), cm.GetYBoard()] = obj;
    }

    public void SetPositionEmpty(int x, int y)
    {
        positions[x, y] = null;
    }

    public GameObject GetPosition(int x, int y)
    {
        return positions[x, y];
    }

    public bool IsPositionOnBoard(int x, int y)
    {
        return x >= 0 && y >= 0 && x < 8 && y < 8;
    }

    public string GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void SetPromotionComplete(bool value) { isPromotionComplete = value; }
    public bool GetPromotionComplete() { return isPromotionComplete; }

    public void EndTurn()
    {
        NextTurn();

        // reset information of pawn moved last
        if (lastMovedPawn != null && lastMovedPawn.name.EndsWith("pawn"))
        {
            Chessman pawnScript = lastMovedPawn.GetComponent<Chessman>();
            if (Mathf.Abs(pawnScript.GetYBoard() - lastMovedPawnInitialY) != 2)
            {
                lastMovedPawn = null;
            }
        }
        else
        {
            lastMovedPawn = null;
        }
    }

    public void NextTurn()
    {
        currentPlayer = (currentPlayer == "white") ? "black" : "white";
    }

    public void Update()
    {
        if (isGameOver && Input.GetMouseButtonDown(0))
        {
            isGameOver = false;
            SceneManager.LoadScene("Game");
        }
    }

    public void Winner(string playerWinner)
    {
        isGameOver = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<TextMeshProUGUI>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<TextMeshProUGUI>().text = $"{playerWinner} is the winner!";
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<TextMeshProUGUI>().enabled = true;
    }
}