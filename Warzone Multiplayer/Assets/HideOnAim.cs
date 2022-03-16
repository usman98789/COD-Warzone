using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideOnAim : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            for (int j = 0; j < gameObject.transform.childCount; j++)
            {
                gameObject.transform.GetChild(j).gameObject.SetActive(false);
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            for (int j = 0; j < gameObject.transform.childCount; j++)
            {
                gameObject.transform.GetChild(j).gameObject.SetActive(true);
            }
        }

    }
}
