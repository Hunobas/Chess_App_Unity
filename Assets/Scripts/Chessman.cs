// Chessman.cs:
using UnityEngine;
using Photon.Pun;
using static UnityEngine.GraphicsBuffer;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using System;

[Flags]
public enum MoveType
{
    Normal = 1 << 0,
    Attack = 1 << 1,
    Castling = 1 << 2,
    EnPassant = 1 << 3
}

public class Chessman : MonoBehaviourPunCallbacks
{
    public GameObject MovePlatePrefab;
    public GameObject PromotionPlatePrefab;

    public int XBoard { get; set; } = -1;
    public int YBoard { get; set; } = -1;
    public bool HasMoved { get; set; } = false;
    private PlayerColor player;

    public Sprite BlackQueen, BlackKnight, BlackBishop, BlackKing, BlackRook, BlackPawn;
    public Sprite WhiteQueen, WhiteKnight, WhiteBishop, WhiteKing, WhiteRook, WhitePawn;

    private List<Vector3Int> ValidMovePlates = null;

    private const float ScaleFactor = 0.66f;
    private const float OffsetX = -2.3f;
    private const float OffsetY = -2.3f;

    public void Initialize(ChessPieceType type, int x, PlayerColor color)
    {
        photonView.RPC("InitializeRPC", RpcTarget.All, type, x, color);
    }

    [PunRPC]
    public void InitializeRPC(ChessPieceType type, int x, PlayerColor color)
    {
        ValidMovePlates = new List<Vector3Int>();
        name = $"{color.ToString().ToLower()}_{type.ToString().ToLower()}";
        XBoard = x;
        YBoard = (type == ChessPieceType.Pawn) ? (color == PlayerColor.White ? 1 : 6) : (color == PlayerColor.White ? 0 : 7);
        SetSprite();
        SetCoords();
    }

    private void DestroyPiece(GameObject pieceToCapture)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(pieceToCapture);
        }
    }

    public void SetSprite()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        string[] nameParts = name.Split('_');

        if (nameParts.Length < 2)
        {
            Debug.LogWarning($"Chess piece name not properly set: {name}");
            return;
        }

        player = nameParts[0] == "white" ? PlayerColor.White : PlayerColor.Black;
        switch (nameParts[1])
        {
            case "queen": spriteRenderer.sprite = player == PlayerColor.White ? WhiteQueen : BlackQueen; break;
            case "knight": spriteRenderer.sprite = player == PlayerColor.White ? WhiteKnight : BlackKnight; break;
            case "bishop": spriteRenderer.sprite = player == PlayerColor.White ? WhiteBishop : BlackBishop; break;
            case "king": spriteRenderer.sprite = player == PlayerColor.White ? WhiteKing : BlackKing; break;
            case "rook": spriteRenderer.sprite = player == PlayerColor.White ? WhiteRook : BlackRook; break;
            case "pawn": spriteRenderer.sprite = player == PlayerColor.White ? WhitePawn : BlackPawn; break;
        }
    }

    public void SetCoords()
    {
        float x = XBoard * ScaleFactor + OffsetX;
        float y = YBoard * ScaleFactor + OffsetY;

        transform.position = new Vector3(x, y, -1.0f);
    }

    public void DestroyMovePlates()
    {
        ValidMovePlates.Clear();
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        foreach (GameObject mp in movePlates)
        {
            Destroy(mp);
        }
    }

    public void DestroyPromotionPlates()
    {
        GameObject[] promotionPlate = GameObject.FindGameObjectsWithTag("PromotionPlate");
        foreach (GameObject pp in promotionPlate)
        {
            Destroy(pp);
        }
    }

    public void InitiateMovePlates()
    {
        string pieceType = name.Split('_')[1];
        switch (pieceType)
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
        PointMovePlate(XBoard + 1, YBoard + 2);
        PointMovePlate(XBoard - 1, YBoard + 2);
        PointMovePlate(XBoard + 2, YBoard + 1);
        PointMovePlate(XBoard + 2, YBoard - 1);
        PointMovePlate(XBoard + 1, YBoard - 2);
        PointMovePlate(XBoard - 1, YBoard - 2);
        PointMovePlate(XBoard - 2, YBoard + 1);
        PointMovePlate(XBoard - 2, YBoard - 1);
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
        PointMovePlate(XBoard, YBoard + 1);
        PointMovePlate(XBoard, YBoard - 1);
        PointMovePlate(XBoard - 1, YBoard - 1);
        PointMovePlate(XBoard - 1, YBoard);
        PointMovePlate(XBoard - 1, YBoard + 1);
        PointMovePlate(XBoard + 1, YBoard - 1);
        PointMovePlate(XBoard + 1, YBoard);
        PointMovePlate(XBoard + 1, YBoard + 1);

        InitiateCastling();
    }

    private void InitiateCastling()
    {
        if (HasMoved) return;

        Game game = Game.Instance;
        // Kingside castling
        if (CanCastle(1, 3))
        {
            ValidMovePlates.Add(new Vector3Int(XBoard + 2, YBoard, (int)(MoveType.Castling) ));
        }
        // Queenside castling
        if (CanCastle(-1, 4))
        {
            ValidMovePlates.Add(new Vector3Int(XBoard - 2, YBoard, (int)(MoveType.Castling)));
        }
    }

    private bool CanCastle(int direction, int distance)
    {
        Game game = Game.Instance;
        for (int i = 1; i < distance; i++)
        {
            if (game.GetPosition(XBoard + i * direction, YBoard) != null) return false;
        }
        GameObject rook = game.GetPosition(XBoard + distance * direction, YBoard);
        return rook != null && !rook.GetComponent<Chessman>().HasMoved && rook.name.EndsWith("rook");
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
        Game game = Game.Instance;
        int directionY = player == PlayerColor.White ? 1 : -1;
        if (game.IsPositionOnBoard(XBoard, YBoard + directionY))
        {
            if (game.GetPosition(XBoard, YBoard + directionY) == null)
            {
                ValidMovePlates.Add(new Vector3Int(XBoard, YBoard + directionY, (int)(MoveType.Normal) ));

                if (!HasMoved && game.GetPosition(XBoard, YBoard + directionY * 2) == null)
                {
                    ValidMovePlates.Add(new Vector3Int(XBoard, YBoard + directionY * 2, (int)(MoveType.Normal) ));
                }
            }

            CheckPawnAttacks(game, directionY);
            CheckEnPassant(game, directionY);
        }
    }

    private void CheckPawnAttacks(Game game, int directionY)
    {
        int[] attackDirections = { -1, 1 };
        foreach (int directionX in attackDirections)
        {
            if (game.IsPositionOnBoard(XBoard + directionX, YBoard + directionY))
            {
                GameObject piece = game.GetPosition(XBoard + directionX, YBoard + directionY);
                if (piece != null && piece.GetComponent<Chessman>().player != player)
                {
                    ValidMovePlates.Add(new Vector3Int(XBoard + directionX, YBoard + directionY, (int)(MoveType.Attack) ));
                }
            }
        }
    }

    private void CheckEnPassant(Game game, int directionY)
    {
        if (game.LastMovedPawn != null)
        {
            Chessman lastMovedPawnScript = game.LastMovedPawn.GetComponent<Chessman>();
            if (Mathf.Abs(lastMovedPawnScript.YBoard - game.LastMovedPawnInitialY) == 2 &&
                Mathf.Abs(lastMovedPawnScript.XBoard - XBoard) == 1 &&
                lastMovedPawnScript.YBoard == YBoard)
            {
                ValidMovePlates.Add(new Vector3Int(lastMovedPawnScript.XBoard, YBoard + directionY, (int)(MoveType.Attack | MoveType.EnPassant) ));
            }
        }
    }

    public void LineMovePlate(int xIncrement, int yIncrement)
    {
        Game game = Game.Instance;
        int x = XBoard + xIncrement;
        int y = YBoard + yIncrement;

        while (game.IsPositionOnBoard(x, y) && game.GetPosition(x, y) == null)
        {
            ValidMovePlates.Add(new Vector3Int( x, y, (int)(MoveType.Normal) ));
            x += xIncrement;
            y += yIncrement;
        }

        if (game.IsPositionOnBoard(x, y) && game.GetPosition(x, y).GetComponent<Chessman>().player != player)
        {
            ValidMovePlates.Add(new Vector3Int( x, y, (int)(MoveType.Attack)));
        }
    }

    public void PointMovePlate(int x, int y)
    {
        Game game = Game.Instance;
        if (game.IsPositionOnBoard(x, y))
        {
            GameObject piece = game.GetPosition(x, y);
            if (piece == null)
            {
                ValidMovePlates.Add(new Vector3Int( x, y, (int)(MoveType.Normal)));
            }
            else if (piece.GetComponent<Chessman>().player != player)
            {
                ValidMovePlates.Add(new Vector3Int( x, y, (int)(MoveType.Attack)));
            }
        }
    }

    public void MovePlateSpawn()
    {
        if (Game.Instance.LocalPlayerColor != player) return;

        foreach (Vector3Int ValidMovePlate in ValidMovePlates)
        {
            float x = ValidMovePlate.x * ScaleFactor + OffsetX;
            float y = ValidMovePlate.y * ScaleFactor + OffsetY;

            GameObject mp = Instantiate(MovePlatePrefab, new Vector3(x, y, -3.0f), Quaternion.identity);

            MovePlate mpScript = mp.GetComponent<MovePlate>();
            mpScript.IsAttack = ((MoveType)ValidMovePlate.z).HasFlag(MoveType.Attack);
            mpScript.IsCastling = ((MoveType)ValidMovePlate.z).HasFlag(MoveType.Castling);
            mpScript.IsEnPassant = ((MoveType)ValidMovePlate.z).HasFlag(MoveType.EnPassant);
            mpScript.SetReference(gameObject, photonView);
            mpScript.SetCoords(ValidMovePlate.x, ValidMovePlate.y);
        }
    }

    private void OnMouseUp()
    {
        Game game = Game.Instance;
        if (!game.IsGameOver() && game.GetCurrentPlayer() == player && game.IsPromotionComplete && game.LocalPlayerColor == player)
        {
            DestroyMovePlates();
            if (PhotonNetwork.IsMasterClient)
            {
                InitiateMovePlates();
                MovePlateSpawn();
            }
            else
            {
                photonView.RPC("RequestMovePlatesRPC", RpcTarget.MasterClient, photonView.ViewID);
            }
        }
    }

    [PunRPC]
    private void RequestMovePlatesRPC(int viewID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Chessman chessman = PhotonView.Find(viewID).GetComponent<Chessman>();
            chessman.ValidMovePlates.Clear();
            chessman.InitiateMovePlates();

            int[] flattenedMoves = new int[chessman.ValidMovePlates.Count * 3];
            for (int i = 0; i < chessman.ValidMovePlates.Count; i++)
            {
                flattenedMoves[i * 3] = chessman.ValidMovePlates[i].x;
                flattenedMoves[i * 3 + 1] = chessman.ValidMovePlates[i].y;
                flattenedMoves[i * 3 + 2] = chessman.ValidMovePlates[i].z;
            }

            photonView.RPC("ReceiveMovePlatesRPC", RpcTarget.All, viewID, flattenedMoves);
        }
    }

    [PunRPC]
    private void ReceiveMovePlatesRPC(int viewID, int[] flattenedMoves)
    {
        if (photonView.ViewID == viewID)
        {
            ValidMovePlates = new List<Vector3Int>();
            for (int i = 0; i < flattenedMoves.Length; i += 3)
            {
                ValidMovePlates.Add(new Vector3Int(flattenedMoves[i], flattenedMoves[i + 1], flattenedMoves[i + 2]));
            }
            MovePlateSpawn();
        }
    }

    [PunRPC]
    public void PerformMoveRPC(int targetX, int targetY, bool isAttack, bool isCastling, bool isEnPassant)
    {
        Game game = Game.Instance;

        if (!game.IsPositionOnBoard(targetX, targetY))
        {
            Debug.LogError($"Invalid move target: {targetX}, {targetY}");
            return;
        }

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

        int initialY = YBoard;

        MovePiece(game, targetX, targetY);

        if (name.EndsWith("pawn"))
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
        if (pieceToCapture == null)
        {
            Debug.LogError("Chessman Error: pieceToCapture is null");
            return;
        }
        if (pieceToCapture.name == "white_king") game.Winner(PlayerColor.Black);
        if (pieceToCapture.name == "black_king") game.Winner(PlayerColor.White);

        DestroyPiece(pieceToCapture);
    }

    private void PerformCastling(Game game, int targetX, int targetY)
    {
        int direction = (targetX > XBoard) ? 1 : -1;
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
            DestroyPiece(pawnToCapture);
            game.SetPositionEmpty(targetX, targetY + directionY);
        }
    }

    private void MovePiece(Game game, int targetX, int targetY)
    {
        game.SetPositionEmpty(XBoard, YBoard);
        XBoard = targetX;
        YBoard = targetY;
        SetCoords();
        game.SetPosition(gameObject);
        HasMoved = true;
    }

    private void HandlePawnMove(Game game, int initialY, int targetX, int targetY)
    {
        game.SetLastMovedPawn(gameObject, initialY);

        if ((game.GetCurrentPlayer() == PlayerColor.Black && targetY == 0) || (game.GetCurrentPlayer() == PlayerColor.White && targetY == 7))
        {
            game.IsPromotionComplete = false;
            if (game.LocalPlayerColor == game.GetCurrentPlayer())
            {
                PromotionPlateSpawn(game.GetCurrentPlayer() == PlayerColor.White);
            }
        }
    }

    public void PromotionPlateSpawn(bool isWhite)
    {
        float offsetY = isWhite ? -0.8f : 0.8f;

        float x = XBoard * ScaleFactor + OffsetX;
        float y = YBoard * ScaleFactor + OffsetY + offsetY;

        GameObject mp = Instantiate(PromotionPlatePrefab, new Vector3(x, y, -4.0f), Quaternion.identity);
        foreach (Transform child in mp.transform)
        {
            Promotion promotionScript = child.GetComponent<Promotion>();
            if (promotionScript != null)
            {
                promotionScript.SetPromotionPiece(gameObject, photonView);
            }
        }
    }

    [PunRPC]
    public void PerformPromotionRPC(string newPieceName)
    {
        name = newPieceName;
        SetSprite();

        if (PhotonNetwork.IsMasterClient)
        {
            Game.Instance.IsPromotionComplete = true;
        }
    }
}