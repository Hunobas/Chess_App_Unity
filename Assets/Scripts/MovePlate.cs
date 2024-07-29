// MovePlate.cs:
using UnityEngine;
using System.Collections;

public class MovePlate : MonoBehaviour
{
    public GameObject PromotionPlatePrefab;

    private GameObject reference = null;
    private int matrixX;
    private int matrixY;

    public bool IsAttack { get; set; } = false;
    public bool IsCastling { get; set; } = false;
    public bool IsEnPassant { get; set; } = false;

    private const float ScaleFactor = 0.66f;
    private const float OffsetX = -2.3f;
    private const float OffsetY = -2.3f;

    private void Start()
    {
        if (IsAttack)
        {
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    private void OnMouseUp()
    {
        Game game = Game.Instance;

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
        Game game = Game.Instance;

        if (!IsEnPassant && IsAttack)
        {
            Capturepiece(game);
        }

        if (IsCastling)
        {
            PerformCastling(game);
        }

        if (IsEnPassant)
        {
            PerformEnPassant(game);
        }

        // save the initialY information for pawn moved last
        int initialY = reference.GetComponent<Chessman>().YBoard;

        // Move the piece
        MovePiece(game);

        if (reference.name.EndsWith("pawn"))
        {
            HandlePawnMove(game, initialY);
        }
    }

    private void Capturepiece(Game game)
    {
        GameObject pieceToCapture = game.GetPosition(matrixX, matrixY);
        if (pieceToCapture.name == "white_king") game.Winner(PlayerColor.Black);
        if (pieceToCapture.name == "black_king") game.Winner(PlayerColor.White);
        Destroy(pieceToCapture);
    }

    private void PerformCastling(Game game)
    {
        int direction = (matrixX > reference.GetComponent<Chessman>().XBoard) ? 1 : -1;
        int rookX = (direction == 1) ? 7 : 0;
        int newRookX = (direction == 1) ? 5 : 3;

        GameObject rook = game.GetPosition(rookX, matrixY);
        game.SetPositionEmpty(rookX, matrixY);
        rook.GetComponent<Chessman>().XBoard = newRookX;
        rook.GetComponent<Chessman>().YBoard = matrixY;
        rook.GetComponent<Chessman>().SetCoords();
        rook.GetComponent<Chessman>().HasMoved = true;
        game.SetPosition(rook);
    }

    private void PerformEnPassant(Game game)
    {
        int directionY = (game.GetCurrentPlayer() == PlayerColor.White) ? -1 : 1;
        GameObject pawnToCapture = game.GetPosition(matrixX, matrixY + directionY);

        if (pawnToCapture.name.EndsWith("pawn"))
        {
            Destroy(pawnToCapture);
            game.SetPositionEmpty(matrixX, matrixY + directionY);
        }
    }

    private void MovePiece(Game game)
    {
        game.SetPositionEmpty(reference.GetComponent<Chessman>().XBoard,
                              reference.GetComponent<Chessman>().YBoard);
        reference.GetComponent<Chessman>().XBoard = matrixX;
        reference.GetComponent<Chessman>().YBoard = matrixY;
        reference.GetComponent<Chessman>().SetCoords();
        game.SetPosition(reference);

        reference.GetComponent<Chessman>().HasMoved = true;
    }

    private void HandlePawnMove(Game game, int initialY)
    {
        game.SetLastMovedPawn(reference, initialY);

        if ((game.GetCurrentPlayer() == PlayerColor.Black && matrixY == 0) || (game.GetCurrentPlayer() == PlayerColor.White && matrixY == 7))
        {
            game.SetPromotionComplete(false);
            PromotionPlateSpawn(game.GetCurrentPlayer() == PlayerColor.White);
        }
    }

    public void PromotionPlateSpawn(bool isWhite)
    {
        float offsetY = isWhite ? -0.8f : 0.8f;

        float x = matrixX * ScaleFactor + OffsetX;
        float y = matrixY * ScaleFactor + OffsetY + offsetY;

        GameObject mp = Instantiate(PromotionPlatePrefab, new Vector3(x, y, -4.0f), Quaternion.identity);
        foreach (Transform child in mp.transform)
        {
            Promotion promotionScript = child.GetComponent<Promotion>();
            if (promotionScript != null)
            {
                promotionScript.SetPromotionPiece(reference);
            }
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
}