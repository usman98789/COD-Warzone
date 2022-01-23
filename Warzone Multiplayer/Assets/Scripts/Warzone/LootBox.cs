using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LootBox : MonoBehaviour
{
    private bool triggerEntered = false;
    private bool isOpened = false;
    private Animator anim;
    private PhotonView PV;

    void Start()
    {
        anim = GetComponent<Animator>();
        PV = GetComponent<PhotonView>();
    }

    void Update()
    {
        var lootBoxPos = gameObject.transform.position;
        if (triggerEntered && !isOpened)  
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                FindObjectOfType<AudioManager>().Play("ChestOpen");
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Cash_Pickup"), new Vector3(lootBoxPos.x + Random.Range(1f,2.5f), lootBoxPos.y, lootBoxPos.z + Random.Range(8.5f, 9.5f)), Quaternion.identity);
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ArmourPlate_Pickup"), new Vector3(lootBoxPos.x + Random.Range(0.5f, 1.5f), lootBoxPos.y + 0.4f, lootBoxPos.z + Random.Range(0.5f, 1f)), Quaternion.identity);
                PV.RPC("RPC_HasOpened", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void RPC_HasOpened()
    {
        anim.SetBool("chestOpen", true);
        isOpened = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        triggerEntered = true;
    }
}
