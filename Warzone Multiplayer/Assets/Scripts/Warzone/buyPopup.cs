using Photon.Pun;
using SpeedTutorBattleRoyaleUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buyPopup : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        ShowBuy(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        ShowBuy(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag.Contains("Player"))
        {
            collision.gameObject.transform.GetChild(2).gameObject.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    void ShowBuy(Collision collision)
    {
        if (collision.gameObject.tag.Contains("Player") && Input.GetKeyDown(KeyCode.E))
        {
            collision.gameObject.transform.GetChild(2).gameObject.transform.GetChild(1).gameObject.SetActive(true);
            UIController.instance.UpdateArmourAmount(2, true);
        }
    }
}
