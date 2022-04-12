using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideOnAim : MonoBehaviour
{
    [SerializeField] PhotonView PV;

    void Update()
    {
        if (!PV.IsMine) return;
        if (Input.GetMouseButtonDown(1))
        {
            Debug.LogError("HIDE");
            for (int j = 0; j < gameObject.transform.childCount; j++)
            {
                gameObject.transform.GetChild(j).gameObject.SetActive(false);
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            Debug.LogError("SHOW");
            for (int j = 0; j < gameObject.transform.childCount; j++)
            {
                gameObject.transform.GetChild(j).gameObject.SetActive(true);
            }
        }

    }
}
