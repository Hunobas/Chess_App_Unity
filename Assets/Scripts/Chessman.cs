// Chessman.cs:
using UnityEngine;

public class Chessman : MonoBehaviour
{
    public GameObject MovePlatePrefab;

    public int XBoard { get; set; } = -1;
    public int YBoard { get; set; } = -1;
    public bool HasMoved { get; set; } = false;
    private PlayerColor player;

    public Sprite BlackQueen, BlackKnight, BlackBishop, BlackKing, BlackRook, BlackPawn;
    public Sprite WhiteQueen, WhiteKnight, WhiteBishop, WhiteKing, WhiteRook, WhitePawn;

    private const float ScaleFactor = 0.66f;
    private const float OffsetX = -2.3f;
    private const float OffsetY = -2.3f;

    public void Activate()
    {
        SetCoords();
        SetSprite();
    }

    private void SetSprite()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        string[] nameParts = name.Split('_');
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
        float x = matrixX * ScaleFactor + OffsetX;
        float y = matrixY * ScaleFactor + OffsetY;

        GameObject mp = Instantiate(MovePlatePrefab, new Vector3(x, y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.IsAttack = isAttack;
        mpScript.IsCastling = isCastling;
        mpScript.IsEnPassant = isEnPassant;
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }

    private void OnMouseUp()
    {
        if (!Game.Instance.IsGameOver() && Game.Instance.GetCurrentPlayer() == player)
        {
            DestroyMovePlates();
            InitiateMovePlates();
        }
    }
}