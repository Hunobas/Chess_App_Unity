// MovePlate.cs:
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller;
    public GameObject promotionPlatePrefab;

    private GameObject reference = null;
    private int matrixX;
    private int matrixY;

    public bool isAttack = false;
    public bool isCastling = false;
    public bool isEnPassant = false;

    private void Start()
    {
        if (isAttack)
        {
            // Change to red color for attack moves
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
    }

    private void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        Game game = controller.GetComponent<Game>();

        // Perform the move
        PerformMove();

        // Clean up and end turn
        reference.GetComponent<Chessman>().DestroyMovePlates();
        if (game.GetPromotionComplete())
        {
            game.EndTurn();
        }
    }

    private void PerformMove()
    {
        Game game = controller.GetComponent<Game>();

        if (!isEnPassant && isAttack)
        {
            GameObject pieceToCapture = game.GetPosition(matrixX, matrixY);
            if (pieceToCapture.name == "white_king") game.Winner("black");
            if (pieceToCapture.name == "black_king") game.Winner("white");
            Destroy(pieceToCapture);
        }

        if (isCastling)
        {
            PerformCastling();
        }

        if (isEnPassant)
        {
            PerformEnPassant();
        }

        // save the initialY information for pawn moved last
        int initialY = reference.GetComponent<Chessman>().GetYBoard();

        // Move the piece
        game.SetPositionEmpty(reference.GetComponent<Chessman>().GetXBoard(),
                              reference.GetComponent<Chessman>().GetYBoard());
        reference.GetComponent<Chessman>().SetXBoard(matrixX);
        reference.GetComponent<Chessman>().SetYBoard(matrixY);
        reference.GetComponent<Chessman>().SetCoords();
        game.SetPosition(reference);

        reference.GetComponent<Chessman>().SetHasMoved(true);

        if (reference.name.EndsWith("pawn"))
        {
            game.SetLastMovedPawn(reference, initialY);

            // TODO : implement promotion
            if ((game.GetCurrentPlayer() == "black" && matrixY == 0) || (game.GetCurrentPlayer() == "white" && matrixY == 7))
            {
                game.SetPromotionComplete(false);
                PromotionPlateSpawn(game.GetCurrentPlayer() == "white");
            }
        }
    }

    private void PerformCastling()
    {
        Game game = controller.GetComponent<Game>();
        int direction = (matrixX > reference.GetComponent<Chessman>().GetXBoard()) ? 1 : -1;
        int rookX = (direction == 1) ? 7 : 0;
        int newRookX = (direction == 1) ? 5 : 3;

        GameObject rook = game.GetPosition(rookX, matrixY);
        game.SetPositionEmpty(rookX, matrixY);
        rook.GetComponent<Chessman>().SetXBoard(newRookX);
        rook.GetComponent<Chessman>().SetYBoard(matrixY);
        rook.GetComponent<Chessman>().SetCoords();
        rook.GetComponent<Chessman>().SetHasMoved(true);
        game.SetPosition(rook);
    }

    private void PerformEnPassant()
    {
        Game game = controller.GetComponent<Game>();
        int directionY = (game.GetCurrentPlayer() == "white") ? -1 : 1;
        GameObject pawnToCapture = game.GetPosition(matrixX, matrixY + directionY);

        if (pawnToCapture.name.EndsWith("pawn"))
        {
            Destroy(pawnToCapture);
            game.SetPositionEmpty(matrixX, matrixY + directionY);
        }
    }

    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }

    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

    public GameObject GetReference()
    {
        return reference;
    }

    public void PromotionPlateSpawn(bool isWhite)
    {
        float offsetY = isWhite ? -0.8f : 0.8f;

        float x = matrixX;
        float y = matrixY;

        x *= 0.66f;
        y *= 0.66f;

        x += -2.3f;
        y += (-2.3f + offsetY);

        GameObject mp = Instantiate(promotionPlatePrefab, new Vector3(x, y, -4.0f), Quaternion.identity);
        foreach (Transform child in mp.transform)
        {
            Promotion promotionScript = child.GetComponent<Promotion>();
            if (promotionScript != null)
            {
                promotionScript.SetPromotionPiece(reference);
            }
        }
    }
}