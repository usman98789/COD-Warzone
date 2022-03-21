using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateAmmo : MonoBehaviour
{
    public PhotonView PV;
    public Text ammoDisplay;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        ammoDisplay = GetComponent<Text>();
    }

    public void UpdateAmount(string ammo, string clipSize)
    {
        if (!PV.IsMine) return;

        ammoDisplay.text = ammo + " / " + clipSize;
    }
}
