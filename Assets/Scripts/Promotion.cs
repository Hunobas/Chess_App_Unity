// Promotion.cs:
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class Promotion : MonoBehaviourPunCallbacks
{
    public GameObject PromotionPiece { get; private set; }
    private PhotonView photonView;

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
        photonView = piece.GetComponent<PhotonView>();
    }

    private void OnMouseUp()
    {
        if (!photonView.IsMine) return;

        photonView.RPC("PerformPromotion", RpcTarget.All, name);
    }

    [PunRPC]
    private void PerformPromotion(string newPieceName)
    {
        PromotionPiece.name = newPieceName;
        PromotionPiece.GetComponent<SpriteRenderer>().sprite = GetComponent<SpriteRenderer>().sprite;

        if (PhotonNetwork.IsMasterClient)
        {
            Game.Instance.IsPromotionComplete = true;
        }

        StartCoroutine(DestroyParentNextFrame());
    }

    private IEnumerator DestroyParentNextFrame()
    {
        yield return null;
        Destroy(transform.parent.gameObject);
    }
}