// Game.cs:
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public enum PlayerColor { White, Black }
public enum ChessPieceType { Pawn, Rook, Knight, Bishop, Queen, King }

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    public GameObject ChessPiecePrefab;
    public GameObject LastMovedPawn { get; private set; }
    public int LastMovedPawnInitialY { get; private set; }

    private const int BoardSize = 8;
    private GameObject[,] positions = new GameObject[BoardSize, BoardSize];
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];

    private PlayerColor currentPlayer = PlayerColor.White;
    private bool isGameOver = false;
    private bool isPromotionComplete = true;

    public event Action<PlayerColor> OnTurnChanged;
    public event Action<PlayerColor> OnGameOver;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        playerWhite = CreateTeam(PlayerColor.White);
        playerBlack = CreateTeam(PlayerColor.Black);

        for (int i = 0; i < playerBlack.Length; i++)
        {
            SetPosition(playerBlack[i]);
            SetPosition(playerWhite[i]);
        }
    }

    private GameObject[] CreateTeam(PlayerColor color)
    {
        return new GameObject[]
        {
            CreateChessPiece(ChessPieceType.Rook, 0, color),
            CreateChessPiece(ChessPieceType.Knight, 1, color),
            CreateChessPiece(ChessPieceType.Bishop, 2, color),
            CreateChessPiece(ChessPieceType.Queen, 3, color),
            CreateChessPiece(ChessPieceType.King, 4, color),
            CreateChessPiece(ChessPieceType.Bishop, 5, color),
            CreateChessPiece(ChessPieceType.Knight, 6, color),
            CreateChessPiece(ChessPieceType.Rook, 7, color),
            CreateChessPiece(ChessPieceType.Pawn, 0, color),
            CreateChessPiece(ChessPieceType.Pawn, 1, color),
            CreateChessPiece(ChessPieceType.Pawn, 2, color),
            CreateChessPiece(ChessPieceType.Pawn, 3, color),
            CreateChessPiece(ChessPieceType.Pawn, 4, color),
            CreateChessPiece(ChessPieceType.Pawn, 5, color),
            CreateChessPiece(ChessPieceType.Pawn, 6, color),
            CreateChessPiece(ChessPieceType.Pawn, 7, color)
        };
    }

    public GameObject CreateChessPiece(ChessPieceType type, int x, PlayerColor color)
    {
        GameObject obj = Instantiate(ChessPiecePrefab, new Vector3(0, 0, -1), Quaternion.identity);
        Chessman cm = obj.GetComponent<Chessman>();
        cm.name = $"{color.ToString().ToLower()}_{type.ToString().ToLower()}";
        cm.XBoard = x;

        if (type == ChessPieceType.Pawn) cm.YBoard = color == PlayerColor.White ? 1 : 6;
        else cm.YBoard = color == PlayerColor.White ? 0 : 7;

        cm.Activate();
        return obj;
    }

    public void SetLastMovedPawn(GameObject pawn, int initialY)
    {
        LastMovedPawn = pawn;
        LastMovedPawnInitialY = initialY;
    }

    public void SetPosition(GameObject obj)
    {
        Chessman cm = obj.GetComponent<Chessman>();
        positions[cm.XBoard, cm.YBoard] = obj;
    }

    public void SetPositionEmpty(int x, int y)
    {
        positions[x, y] = null;
    }

    public GameObject GetPosition(int x, int y)
    {
        return positions[x, y];
    }

    public bool IsPositionOnBoard(int x, int y)
    {
        return x >= 0 && y >= 0 && x < BoardSize && y < BoardSize;
    }

    public PlayerColor GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void SetPromotionComplete(bool value)
    {
        isPromotionComplete = value;
        if (value)
        {
            EndTurn();
        }
    }

    public bool GetPromotionComplete() { return isPromotionComplete; }

    public void EndTurn()
    {
        NextTurn();
        ResetLastMovedPawn();
    }

    private void ResetLastMovedPawn()
    {
        if (LastMovedPawn != null && LastMovedPawn.name.EndsWith("pawn"))
        {
            Chessman pawnScript = LastMovedPawn.GetComponent<Chessman>();
            if (Mathf.Abs(pawnScript.YBoard - LastMovedPawnInitialY) != 2)
            {
                LastMovedPawn = null;
            }
        }
        else
        {
            LastMovedPawn = null;
        }
    }

    public void NextTurn()
    {
        currentPlayer = (currentPlayer == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;
        OnTurnChanged?.Invoke(currentPlayer);
    }

    public void Update()
    {
        if (isGameOver && Input.GetMouseButtonDown(0))
        {
            RestartGame();
        }
    }

    private void RestartGame()
    {
        isGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Winner(PlayerColor winningPlayer)
    {
        isGameOver = true;
        DisplayWinnerText(winningPlayer);
        OnGameOver?.Invoke(winningPlayer);
    }

    private void DisplayWinnerText(PlayerColor winningPlayer)
    {
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<TextMeshProUGUI>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<TextMeshProUGUI>().text = $"{winningPlayer} is the winner!";
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<TextMeshProUGUI>().enabled = true;
    }
}