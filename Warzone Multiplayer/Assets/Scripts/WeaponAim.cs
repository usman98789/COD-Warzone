using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAim : MonoBehaviour
{
    [SerializeField] private Vector3 normalLocalPosition;
    [SerializeField] private Vector3 aimingLocalPoistion;
    private float aimSmooth = 1;

    private void Update()
    {
        Vector3 target = normalLocalPosition;
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("AIMING LOCAL IS" + aimingLocalPoistion);
            target = aimingLocalPoistion;
        }


        Vector3 desiredPos = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * aimSmooth);
        transform.localPosition = desiredPos;
        
    }

}
