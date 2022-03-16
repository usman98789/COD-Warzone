using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBob : MonoBehaviour
{

    [SerializeField] BobOverride[] bobOverrides;
    [SerializeField] Transform[] weapon;
    private Transform currWeapon;

    [HideInInspector]
    public float currentSpeed;

    private float currentTimeX;
    private float currentTimeY;
    private float xPos;
    private float yPos;
    private Vector3 smoothV;

    private void Update()
    {
        foreach(BobOverride bob in bobOverrides)
        {
            if (currentSpeed >= bob.minSpeed && currentSpeed <= bob.maxSpeed)
            {
                float bobMultiplayer = (currentSpeed == 0) ? 1 : currentSpeed;

                currentTimeX += bob.speedX / 10 * Time.deltaTime * bobMultiplayer;
                currentTimeY += bob.speedY / 10 * Time.deltaTime * bobMultiplayer;

                xPos = bob.bobX.Evaluate(currentTimeX) * bob.intensityX;
                yPos = bob.bobY.Evaluate(currentTimeY) * bob.intensityY;
            }
        }
    }

    private void FixedUpdate()
    {
        Vector3 target = new Vector3(xPos, yPos, 0);

        foreach(Transform weapon in weapon)
        {
            if (weapon.gameObject.activeInHierarchy)
            {
                currWeapon = weapon;
            }
        }

        Vector3 desiredPos = Vector3.SmoothDamp(currWeapon.localPosition, target, ref smoothV, 0.1f);
        currWeapon.localPosition = desiredPos;
    }

    [System.Serializable]
    public struct BobOverride
    {
        public float minSpeed;
        public float maxSpeed;

        [Header("X Settings")]
        public float speedX;
        public float intensityX;
        public AnimationCurve bobX;

        [Header("Y Settings")]
        public float speedY;
        public float intensityY;
        public AnimationCurve bobY;
    }

}
