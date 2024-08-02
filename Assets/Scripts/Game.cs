// Game.cs:
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using Photon.Pun;
using System.Drawing;

public enum PlayerColor { White, Black }
public enum ChessPieceType { Pawn, Rook, Knight, Bishop, Queen, King }

public class Game : MonoBehaviourPunCallbacks
{
    public static Game Instance { get; private set; }

    public bool IsMasterClientLocal => PhotonNetwork.IsMasterClient && photonView.IsMine;

    public GameObject ChessPiecePrefab;
    public GameObject LastMovedPawn { get; private set; }
    public int LastMovedPawnInitialY { get; private set; }
    public PlayerColor LocalPlayerColor { get; private set; }

    private bool _isPromotionComplete = true;
    public bool IsPromotionComplete
    {
        get => _isPromotionComplete;
        set
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("SetPromotionCompleteRPC", RpcTarget.All, value);
            }
        }
    }
    private const int BoardSize = 8;
    private GameObject[,] positions = new GameObject[BoardSize, BoardSize];
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];
    private int[] playerScores;

    private PlayerColor currentPlayer = PlayerColor.White;
    private bool isGameOver = false;

    private TextMeshProUGUI waitingText;
    private TextMeshProUGUI winnerText;
    private TextMeshProUGUI restartText;
    private GameObject[] menuObjects;

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
        playerScores = new[] { 0, 0 };
        LocalPlayerColor = (PhotonNetwork.LocalPlayer.ActorNumber == 1) ? PlayerColor.White : PlayerColor.Black;
        waitingText = GameObject.FindGameObjectWithTag("WaitingText").GetComponent<TextMeshProUGUI>();
        winnerText = GameObject.FindGameObjectWithTag("WinnerText").GetComponent<TextMeshProUGUI>();
        restartText = GameObject.FindGameObjectWithTag("RestartText").GetComponent<TextMeshProUGUI>();
        menuObjects = GameObject.FindGameObjectsWithTag("Menu");

        if (PhotonNetwork.IsMasterClient)
        {
            waitingText.enabled = true;
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            waitingText.enabled = false;
            photonView.RPC("InitializeBoardRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    private void InitializeBoardRPC()
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
        GameObject obj = PhotonNetwork.Instantiate(ChessPiecePrefab.name, new Vector3(0, 0, -1), Quaternion.identity);
        Chessman cm = obj.GetComponent<Chessman>();
        cm.Initialize(type, x, color);

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
        if (IsPositionOnBoard(x, y))
        {
            positions[x, y] = null;
        }
        else
        {
            Debug.LogWarning($"Attempted to set empty position outside board: {x}, {y}");
        }
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

    [PunRPC]
    private void ChangeTurnRPC(PlayerColor newCurrentPlayer)
    {
        currentPlayer = newCurrentPlayer;
        OnTurnChanged?.Invoke(currentPlayer);
    }

    public void EndTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerColor nextPlayer = (currentPlayer == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;
            photonView.RPC("ChangeTurnRPC", RpcTarget.All, nextPlayer);
        }
        ResetLastMovedPawn();
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
        StartCoroutine(LeaveRoomCoroutine());
    }

    public void Winner(PlayerColor winningPlayer)
    {
        isGameOver = true;
        DisplayWinnerText(winningPlayer);
        OnGameOver?.Invoke(winningPlayer);
    }

    public void QuitGame()
    {
        if (PhotonNetwork.IsMessageQueueRunning)
        {
            photonView.RPC("PlayerQuitRPC", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
        }
        StartCoroutine(LeaveRoomCoroutine());
    }

    [PunRPC]
    private void PlayerQuitRPC(int playerActorNumber)
    {
        PlayerColor quittingPlayerColor = (playerActorNumber == 1) ? PlayerColor.White : PlayerColor.Black;
        PlayerColor winningPlayerColor = (quittingPlayerColor == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;

        if (PhotonNetwork.LocalPlayer.ActorNumber != playerActorNumber)
        {
            // 상대방이 나갔을 때 승리 처리
            Winner(winningPlayerColor);
        }
    }

    private IEnumerator LeaveRoomCoroutine()
    {
        PhotonNetwork.LeaveRoom();
        while (PhotonNetwork.InRoom)
            yield return null;
        SceneManager.LoadScene("Lobby");
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

    private void DisplayWinnerText(PlayerColor winningPlayer)
    {
        winnerText.enabled = true;
        winnerText.text = $"{winningPlayer} is the winner!";
        restartText.enabled = true;
    }

    [PunRPC]
    private void SetPromotionCompleteRPC(bool value)
    {
        _isPromotionComplete = value;
        if (value)
        {
            EndTurn();
        }
    }

    public void OpenMenu()
    {
        foreach (GameObject menuObject in menuObjects)
        {
            menuObject.SetActive(true);
        }
    }

    public void CloseMenu()
    {
        foreach(GameObject menuObject in menuObjects)
        {
            menuObject.SetActive(false);
        }
    }
}