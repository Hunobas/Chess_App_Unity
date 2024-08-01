// MovePlate.cs:
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class MovePlate : MonoBehaviourPunCallbacks
{
    public GameObject PromotionPlatePrefab;

    private GameObject reference = null;
    private PhotonView photonView;
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
        if (!photonView.IsMine) return;
        
        Game game = Game.Instance;

        // Perform the move
        photonView.RPC("PerformMoveRPC", RpcTarget.All, matrixX, matrixY, IsAttack, IsCastling, IsEnPassant);

        // Clean up and end turn
        reference.GetComponent<Chessman>().DestroyMovePlates();
    }

    [PunRPC]
    private void PerformMoveRPC(int targetX, int targetY, bool isAttack, bool isCastling, bool isEnPassant)
    {
        Game game = Game.Instance;

        if (!isEnPassant && isAttack)
        {
            Capturepiece(game, targetX, targetY);
        }

        if (isCastling)
        {
            PerformCastling(game, targetX, targetY);
        }

        if (isEnPassant)
        {
            PerformEnPassant(game, targetX, targetY);
        }

        int initialY = reference.GetComponent<Chessman>().YBoard;

        MovePiece(game, targetX, targetY);

        if (reference.name.EndsWith("pawn"))
        {
            HandlePawnMove(game, initialY, targetX, targetY);
        }

        if (game.IsPromotionComplete)
        {
            game.EndTurn();
        }
    }

    private void Capturepiece(Game game, int x, int y)
    {
        GameObject pieceToCapture = game.GetPosition(x, y);
        if (pieceToCapture.name == "white_king") game.Winner(PlayerColor.Black);
        if (pieceToCapture.name == "black_king") game.Winner(PlayerColor.White);
        PhotonNetwork.Destroy(pieceToCapture);
    }

    private void PerformCastling(Game game, int targetX, int targetY)
    {
        int direction = (targetX > reference.GetComponent<Chessman>().XBoard) ? 1 : -1;
        int rookX = (direction == 1) ? 7 : 0;
        int newRookX = (direction == 1) ? 5 : 3;

        GameObject rook = game.GetPosition(rookX, targetY);
        Chessman rookComponent = rook.GetComponent<Chessman>();
        game.SetPositionEmpty(rookX, targetY);
        rookComponent.XBoard = newRookX;
        rookComponent.YBoard = targetY;
        rookComponent.SetCoords();
        rookComponent.HasMoved = true;
        game.SetPosition(rook);
    }

    private void PerformEnPassant(Game game, int targetX, int targetY)
    {
        int directionY = (game.GetCurrentPlayer() == PlayerColor.White) ? -1 : 1;
        GameObject pawnToCapture = game.GetPosition(targetX, targetY + directionY);

        if (pawnToCapture.name.EndsWith("pawn"))
        {
            PhotonNetwork.Destroy(pawnToCapture);
            game.SetPositionEmpty(targetX, targetY + directionY);
        }
    }

    private void MovePiece(Game game, int targetX, int targetY)
    {
        Chessman cm = reference.GetComponent<Chessman>();
        game.SetPositionEmpty(cm.XBoard, cm.YBoard);
        cm.XBoard = targetX;
        cm.YBoard = targetY;
        cm.SetCoords();
        game.SetPosition(reference);
        cm.HasMoved = true;
    }

    private void HandlePawnMove(Game game, int initialY, int targetX, int targetY)
    {
        game.SetLastMovedPawn(reference, initialY);

        if ((game.GetCurrentPlayer() == PlayerColor.Black && targetY == 0) || (game.GetCurrentPlayer() == PlayerColor.White && targetY == 7))
        {
            game.IsPromotionComplete = false;
            if (photonView.IsMine)
            {
                PromotionPlateSpawn(game.GetCurrentPlayer() == PlayerColor.White);
            }
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
        photonView = obj.GetComponent<PhotonView>();
    }

    public GameObject GetReference()
    {
        return reference;
    }
}