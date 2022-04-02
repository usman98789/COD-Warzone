using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateAmmo : MonoBehaviour
{
    public Text ammoDisplay;

    // Start is called before the first frame update
    void Start()
    {
        ammoDisplay = GetComponent<Text>();
    }

    public void UpdateAmount(string ammo, string clipSize)
    {
        ammoDisplay.text = ammo + " / " + clipSize;
    }
}
