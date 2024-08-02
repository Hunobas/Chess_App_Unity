// MovePlate.cs:
using UnityEngine;
using System.Collections;
using Photon.Pun;
using Unity.VisualScripting;

public class MovePlate : MonoBehaviourPunCallbacks
{
    private GameObject reference = null;
    private PhotonView photonView;
    private int matrixX;
    private int matrixY;

    public bool IsAttack { get; set; } = false;
    public bool IsCastling { get; set; } = false;
    public bool IsEnPassant { get; set; } = false;

    private void Start()
    {
        if (IsAttack)
        {
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }

    public void SetReference(GameObject obj, PhotonView pv)
    {
        reference = obj;
        photonView = pv;
    }

    public GameObject GetReference()
    {
        return reference;
    }

    private void OnMouseUp()
    {
        if (photonView == null) {
            Debug.LogError("MovePlate Error: photonView is null");
            return;
        }

        photonView.RPC("PerformMoveRPC", RpcTarget.All, matrixX, matrixY, IsAttack, IsCastling, IsEnPassant);
        reference.GetComponent<Chessman>().DestroyMovePlates();
    }
}