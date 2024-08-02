// Chessman.cs:
using UnityEngine;
using Photon.Pun;

public class Chessman : MonoBehaviourPunCallbacks
{
    public GameObject MovePlatePrefab;
    public GameObject PromotionPlatePrefab;
    private PhotonView photonView;

    public int XBoard { get; set; } = -1;
    public int YBoard { get; set; } = -1;
    public bool HasMoved { get; set; } = false;
    private PlayerColor player;

    public Sprite BlackQueen, BlackKnight, BlackBishop, BlackKing, BlackRook, BlackPawn;
    public Sprite WhiteQueen, WhiteKnight, WhiteBishop, WhiteKing, WhiteRook, WhitePawn;

    private const float ScaleFactor = 0.66f;
    private const float OffsetX = -2.3f;
    private const float OffsetY = -2.3f;

    public void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void Initialize(ChessPieceType type, int x, PlayerColor color)
    {
        name = $"{color.ToString().ToLower()}_{type.ToString().ToLower()}";
        XBoard = x;
        YBoard = (type == ChessPieceType.Pawn) ? (color == PlayerColor.White ? 1 : 6) : (color == PlayerColor.White ? 0 : 7);
        SetSprite();
        SetCoords();
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
            MovePlateSpawn(XBoard + 2, YBoard, false, true);
        }
        // Queenside castling
        if (CanCastle(-1, 4))
        {
            MovePlateSpawn(XBoard - 2, YBoard, false, true);
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
                MovePlateSpawn(XBoard, YBoard + directionY);

                if (!HasMoved && game.GetPosition(XBoard, YBoard + directionY * 2) == null)
                {
                    MovePlateSpawn(XBoard, YBoard + directionY * 2);
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
                    MovePlateSpawn(XBoard + directionX, YBoard + directionY, true);
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
                MovePlateSpawn(lastMovedPawnScript.XBoard, YBoard + directionY, true, false, true);
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
            MovePlateSpawn(x, y);
            x += xIncrement;
            y += yIncrement;
        }

        if (game.IsPositionOnBoard(x, y) && game.GetPosition(x, y).GetComponent<Chessman>().player != player)
        {
            MovePlateSpawn(x, y, true);
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
                MovePlateSpawn(x, y);
            }
            else if (piece.GetComponent<Chessman>().player != player)
            {
                MovePlateSpawn(x, y, true);
            }
        }
    }

    public void MovePlateSpawn(int matrixX, int matrixY, bool isAttack = false, bool isCastling = false, bool isEnPassant = false)
    {
        if (Game.Instance.LocalPlayerColor != player) return;

        float x = matrixX * ScaleFactor + OffsetX;
        float y = matrixY * ScaleFactor + OffsetY;

        GameObject mp = Instantiate(MovePlatePrefab, new Vector3(x, y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.IsAttack = isAttack;
        mpScript.IsCastling = isCastling;
        mpScript.IsEnPassant = isEnPassant;
        mpScript.SetReference(gameObject, photonView);
        mpScript.SetCoords(matrixX, matrixY);
    }

    private void OnMouseUp()
    {
        Game game = Game.Instance;
        if (!game.IsGameOver() && game.GetCurrentPlayer() == player && game.IsPromotionComplete && game.LocalPlayerColor == player)
        {
            DestroyMovePlates();
            InitiateMovePlates();
        }
    }

    [PunRPC]
    private void PerformMoveRPC(int targetX, int targetY, bool isAttack, bool isCastling, bool isEnPassant)
    {
        Debug.Log("#########PerformMoveRPC ½ÇÇàµÊ.");
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
        PhotonNetwork.Destroy(pieceToCapture);
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
            PhotonNetwork.Destroy(pawnToCapture);
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
            if (photonView.IsMine)
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
    private void PerformPromotionRPC(string newPieceName, Sprite newSprite)
    {
        name = newPieceName;
        GetComponent<SpriteRenderer>().sprite = newSprite;

        if (PhotonNetwork.IsMasterClient)
        {
            Game.Instance.IsPromotionComplete = true;
        }
    }
}