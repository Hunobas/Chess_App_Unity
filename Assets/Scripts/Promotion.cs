// Promotion.cs:
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Promotion : MonoBehaviour
{
    public GameObject controller;
    public GameObject promotionPiece;

    void Start()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        SetSprite();
    }

    public Sprite black_queen, black_knight, black_bishop, black_rook;
    public Sprite white_queen, white_knight, white_bishop, white_rook;

    private void SetSprite()
    {
        Game game = controller.GetComponent<Game>();

        if (game.GetCurrentPlayer() == "white")
        {
            switch (this.name)
            {
                case "PromotionPiece_1": this.GetComponent<SpriteRenderer>().sprite = white_queen; this.name = "white_queen"; break;
                case "PromotionPiece_2": this.GetComponent<SpriteRenderer>().sprite = white_knight; this.name = "white_knight"; break;
                case "PromotionPiece_3": this.GetComponent<SpriteRenderer>().sprite = white_rook; this.name = "white_rook"; break;
                case "PromotionPiece_4": this.GetComponent<SpriteRenderer>().sprite = white_bishop; this.name = "white_bishop"; break;
            }
        } else
        {
            switch (this.name)
            {
                case "PromotionPiece_1": this.GetComponent<SpriteRenderer>().sprite = black_bishop; this.name = "black_bishop"; break;
                case "PromotionPiece_2": this.GetComponent<SpriteRenderer>().sprite = black_rook; this.name = "black_rook"; break;
                case "PromotionPiece_3": this.GetComponent<SpriteRenderer>().sprite = black_knight; this.name = "black_knight"; break;
                case "PromotionPiece_4": this.GetComponent<SpriteRenderer>().sprite = black_queen; this.name = "black_queen"; break;
            }
        }
    }

    public void SetPromotionPiece(GameObject piece) { promotionPiece = piece; }

    private void OnMouseUp()
    {
        Game game = controller.GetComponent<Game>();

        promotionPiece.name = this.name;
        promotionPiece.GetComponent<SpriteRenderer>().sprite = this.GetComponent<SpriteRenderer>().sprite;
        game.SetPromotionComplete(true);
        game.EndTurn();
        StartCoroutine(DestroyParentNextFrame());
    }

    private IEnumerator DestroyParentNextFrame()
    {
        yield return null;
        Destroy(transform.parent.gameObject);
    }
}
