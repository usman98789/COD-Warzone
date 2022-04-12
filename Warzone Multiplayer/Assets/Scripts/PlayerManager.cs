using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;

public class PlayerManager : MonoBehaviour
{
    PhotonView PV;

    GameObject controller;
    private Player[] allPlayers;
    private int pos;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    // Start is called before the first frame update
    void Start()
    {
        pos = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        if (PV.IsMine)
        {
            CreateController();
        }
    }

    void CreateController()
    {
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), SpawnManager.Instance.spawnpoints[pos].position, SpawnManager.Instance.spawnpoints[pos].rotation, 0, new object[] { PV.ViewID });
    }

    public void Die()
    {
        if (controller != null)
        {
            PhotonNetwork.Destroy(controller);
            CreateController();
        }
    }
}
