// Promotion.cs:
using UnityEngine;
using System.Collections;

public class Promotion : MonoBehaviour
{
    public GameObject PromotionPiece { get; private set; }

    public Sprite BlackQueen, BlackKnight, BlackBishop, BlackRook;
    public Sprite WhiteQueen, WhiteKnight, WhiteBishop, WhiteRook;

    void Start()
    {
        SetSprite();
    }

    private void SetSprite()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        PlayerColor currentPlayer = Game.Instance.GetCurrentPlayer();

        switch (name)
        {
            case "PromotionPiece_1":
                spriteRenderer.sprite = currentPlayer == PlayerColor.White ? WhiteQueen : BlackBishop;
                name = currentPlayer == PlayerColor.White ? "white_queen" : "black_bishop";
                break;
            case "PromotionPiece_2":
                spriteRenderer.sprite = currentPlayer == PlayerColor.White ? WhiteKnight : BlackRook;
                name = currentPlayer == PlayerColor.White ? "white_knight" : "black_rook";
                break;
            case "PromotionPiece_3":
                spriteRenderer.sprite = currentPlayer == PlayerColor.White ? WhiteRook : BlackKnight;
                name = currentPlayer == PlayerColor.White ? "white_rook" : "black_knight";
                break;
            case "PromotionPiece_4":
                spriteRenderer.sprite = currentPlayer == PlayerColor.White ? WhiteBishop : BlackQueen;
                name = currentPlayer == PlayerColor.White ? "white_bishop" : "black_queen";
                break;
        }
    }

    public void SetPromotionPiece(GameObject piece)
    {
        PromotionPiece = piece;
    }

    private void OnMouseUp()
    {
        PromotionPiece.name = name;
        PromotionPiece.GetComponent<SpriteRenderer>().sprite = GetComponent<SpriteRenderer>().sprite;
        Game.Instance.SetPromotionComplete(true);
        StartCoroutine(DestroyParentNextFrame());
    }

    private IEnumerator DestroyParentNextFrame()
    {
        yield return null;
        Destroy(transform.parent.gameObject);
    }
}