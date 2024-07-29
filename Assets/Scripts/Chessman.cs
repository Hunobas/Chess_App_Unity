// Chessman.cs:
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Chessman : MonoBehaviour
{
    public GameObject controller;
    public GameObject movePlatePrefab;

    private int xBoard = -1;
    private int yBoard = -1;
    private bool hasMoved = false;
    private string player;

    public Sprite black_queen, black_knight, black_bishop, black_king, black_rook, black_pawn;
    public Sprite white_queen, white_knight, white_bishop, white_king, white_rook, white_pawn;

    public void Activate()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        SetCoords();
        SetSprite();
    }

    private void SetSprite()
    {
        switch (this.name)
        {
            case "black_queen": this.GetComponent<SpriteRenderer>().sprite = black_queen; player = "black"; break;
            case "black_knight": this.GetComponent<SpriteRenderer>().sprite = black_knight; player = "black"; break;
            case "black_bishop": this.GetComponent<SpriteRenderer>().sprite = black_bishop; player = "black"; break;
            case "black_king": this.GetComponent<SpriteRenderer>().sprite = black_king; player = "black"; break;
            case "black_rook": this.GetComponent<SpriteRenderer>().sprite = black_rook; player = "black"; break;
            case "black_pawn": this.GetComponent<SpriteRenderer>().sprite = black_pawn; player = "black"; break;
            case "white_queen": this.GetComponent<SpriteRenderer>().sprite = white_queen; player = "white"; break;
            case "white_knight": this.GetComponent<SpriteRenderer>().sprite = white_knight; player = "white"; break;
            case "white_bishop": this.GetComponent<SpriteRenderer>().sprite = white_bishop; player = "white"; break;
            case "white_king": this.GetComponent<SpriteRenderer>().sprite = white_king; player = "white"; break;
            case "white_rook": this.GetComponent<SpriteRenderer>().sprite = white_rook; player = "white"; break;
            case "white_pawn": this.GetComponent<SpriteRenderer>().sprite = white_pawn; player = "white"; break;
        }
    }

    public void SetCoords()
    {
        float x = xBoard;
        float y = yBoard;

        x *= 0.66f;
        y *= 0.66f;

        x += -2.3f;
        y += -2.3f;

        this.transform.position = new Vector3(x, y, -1.0f);
    }

    public int GetXBoard() { return xBoard; }
    public int GetYBoard() { return yBoard; }
    public void SetXBoard(int x) { xBoard = x; }
    public void SetYBoard(int y) { yBoard = y; }
    public void SetHasMoved(bool value) { hasMoved = value; }

    public void DestroyMovePlates()
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        foreach (GameObject mp in movePlates)
        {
            Destroy(mp);
        }
    }

    public void InitiateMovePlates()
    {
        switch (this.name.Split('_')[1])
        {
            case "queen": InitiateQueenMoves(); break;
            case "knight": InitiateKnightMoves(); break;
            case "bishop": InitiateBishopMoves(); break;
            case "king": InitiateKingMoves(); break;
            case "rook": InitiateRookMoves(); break;
            case "pawn": InitiatePawnMoves(); break;
        }
    }

    private void InitiateQueenMoves()
    {
        LineMovePlate(1, 0);
        LineMovePlate(0, 1);
        LineMovePlate(1, 1);
        LineMovePlate(-1, 0);
        LineMovePlate(0, -1);
        LineMovePlate(-1, -1);
        LineMovePlate(-1, 1);
        LineMovePlate(1, -1);
    }

    private void InitiateKnightMoves()
    {
        PointMovePlate(xBoard + 1, yBoard + 2);
        PointMovePlate(xBoard - 1, yBoard + 2);
        PointMovePlate(xBoard + 2, yBoard + 1);
        PointMovePlate(xBoard + 2, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard - 2);
        PointMovePlate(xBoard - 1, yBoard - 2);
        PointMovePlate(xBoard - 2, yBoard + 1);
        PointMovePlate(xBoard - 2, yBoard - 1);
    }

    private void InitiateBishopMoves()
    {
        LineMovePlate(1, 1);
        LineMovePlate(1, -1);
        LineMovePlate(-1, 1);
        LineMovePlate(-1, -1);
    }

    private void InitiateKingMoves()
    {
        PointMovePlate(xBoard, yBoard + 1);
        PointMovePlate(xBoard, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard);
        PointMovePlate(xBoard - 1, yBoard + 1);
        PointMovePlate(xBoard + 1, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard);
        PointMovePlate(xBoard + 1, yBoard + 1);

        // Castling
        InitiateCastling();
    }

    private void InitiateCastling()
    {
        if (hasMoved) return;

        Game sc = controller.GetComponent<Game>();
        // Kingside castling
        if (CanCastle(1, 3))
        {
            MovePlateSpawn(xBoard + 2, yBoard, false, true);
        }
        // Queenside castling
        if (CanCastle(-1, 4))
        {
            MovePlateSpawn(xBoard - 2, yBoard, false, true);
        }
    }

    private bool CanCastle(int direction, int distance)
    {
        Game sc = controller.GetComponent<Game>();
        for (int i = 1; i < distance; i++)
        {
            if (sc.GetPosition(xBoard + i * direction, yBoard) != null) return false;
        }
        GameObject rook = sc.GetPosition(xBoard + distance * direction, yBoard);
        return rook != null && !rook.GetComponent<Chessman>().hasMoved && rook.name.EndsWith("rook");
    }

    private void InitiateRookMoves()
    {
        LineMovePlate(1, 0);
        LineMovePlate(0, 1);
        LineMovePlate(-1, 0);
        LineMovePlate(0, -1);
    }

    private void InitiatePawnMoves()
    {
        Game sc = controller.GetComponent<Game>();
        int directionY = player == "white" ? 1 : -1;
        if (sc.IsPositionOnBoard(xBoard, yBoard))
        {
            if (sc.GetPosition(xBoard, yBoard + directionY) == null)
            {
                MovePlateSpawn(xBoard, yBoard + directionY);

                if (hasMoved == false && sc.GetPosition(xBoard, yBoard + directionY * 2) == null)
                {
                    MovePlateSpawn(xBoard, yBoard + directionY * 2);
                }
            }

            // common pawn attack
            int[] attackDirections = { -1, 1 };
            foreach (int directionX in attackDirections)
            {
                if (sc.IsPositionOnBoard(xBoard + directionX, yBoard + directionY) && sc.GetPosition(xBoard + directionX, yBoard + directionY) != null &&
                    sc.GetPosition(xBoard + directionX, yBoard + directionY).GetComponent<Chessman>().player != player)
                {
                    MovePlateSpawn(xBoard + directionX, yBoard + directionY, true);
                }
            }

            // en passant attack
            if (sc.lastMovedPawn != null)
            {
                Chessman lastMovedPawnScript = sc.lastMovedPawn.GetComponent<Chessman>();
                if (Mathf.Abs(lastMovedPawnScript.GetYBoard() - sc.lastMovedPawnInitialY) == 2 &&
                    Mathf.Abs(lastMovedPawnScript.GetXBoard() - xBoard) == 1 &&
                    lastMovedPawnScript.GetYBoard() == yBoard)
                {
                    MovePlateSpawn(lastMovedPawnScript.GetXBoard(), yBoard + directionY, true, false, true);
                }
            }
        }
    }

    public void LineMovePlate(int xIncrement, int yIncrement)
    {
        Game sc = controller.GetComponent<Game>();
        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        while (sc.IsPositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);
            x += xIncrement;
            y += yIncrement;
        }

        if (sc.IsPositionOnBoard(x, y) && sc.GetPosition(x, y).GetComponent<Chessman>().player != player)
        {
            MovePlateSpawn(x, y, true);
        }
    }

    public void PointMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        if (sc.IsPositionOnBoard(x, y))
        {
            GameObject cp = sc.GetPosition(x, y);
            if (cp == null)
            {
                MovePlateSpawn(x, y);
            }
            else if (cp.GetComponent<Chessman>().player != player)
            {
                MovePlateSpawn(x, y, true);
            }
        }
    }

    public void MovePlateSpawn(int matrixX, int matrixY, bool isAttack = false, bool isCastling = false, bool isEnPassant = false)
    {
        float x = matrixX;
        float y = matrixY;

        x *= 0.66f;
        y *= 0.66f;

        x += -2.3f;
        y += -2.3f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(x, y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        if (isAttack) mpScript.isAttack = true;
        if (isCastling) mpScript.isCastling = true;
        if (isEnPassant) mpScript.isEnPassant = true;
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }

    private void OnMouseUp()
    {
        if (!controller.GetComponent<Game>().IsGameOver() && controller.GetComponent<Game>().GetCurrentPlayer() == player)
        {
            DestroyMovePlates();

            InitiateMovePlates();
        }
    }
}
