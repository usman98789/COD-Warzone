using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateGun : MonoBehaviour
{
    [SerializeField] GameObject gun1;
    [SerializeField] GameObject gun2;
    [SerializeField] GameObject gun3;
    [SerializeField] GameObject gun4;
    [SerializeField] GameObject gun5;
    [SerializeField] GameObject gun6;
    [SerializeField] PhotonView PV;

    void Update()
    {
        if (!PV.IsMine) return;

        if (gun1.activeInHierarchy)
        {
            Debug.Log("active" + gameObject.transform.GetChild(0).name);
            gameObject.transform.GetChild(0).gameObject.SetActive(true);
            HideRest(0);
        } 
        else if (gun2.activeInHierarchy)
        {
            Debug.Log("active" + gameObject.transform.GetChild(1).name);
            gameObject.transform.GetChild(1).gameObject.SetActive(true);
            HideRest(1);
        }
        else if (gun3.activeInHierarchy)
        {
            Debug.Log("active" + gameObject.transform.GetChild(2).name);
            gameObject.transform.GetChild(2).gameObject.SetActive(true);
            HideRest(2);
        }
        else if (gun4.activeInHierarchy)
        {
            Debug.Log("active" + gameObject.transform.GetChild(3).name);
            gameObject.transform.GetChild(3).gameObject.SetActive(true);
            HideRest(3);
        }
        else if (gun5.activeInHierarchy)
        {
            Debug.Log("active" + gameObject.transform.GetChild(4).name);
            gameObject.transform.GetChild(4).gameObject.SetActive(true);
            HideRest(4);
        }
        else if (gun6.activeInHierarchy)
        {
            Debug.Log("active" + gameObject.transform.GetChild(5).name);
            gameObject.transform.GetChild(5).gameObject.SetActive(true);
            HideRest(5);
        }
    }

    void HideRest(int keep)
    {
        for (int i = 0; i < 6; i++)
        {
            if (i != keep)
            {
                Debug.Log("gonna hide " + gameObject.transform.GetChild(i).name);
                gameObject.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
