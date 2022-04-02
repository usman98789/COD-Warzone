using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform orientation;
    [SerializeField] Transform playerObj;

    private Rigidbody rb;
    PhotonView PV;

    [Header("Sliding")]
    [SerializeField] float maxSlideTime;
    [SerializeField] float slideForce;
    private float slideTimer;

    private KeyCode slideKey = KeyCode.C;
    private float horizontalInput;
    private float verticalInput;

    private bool isSliding = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        PV = playerObj.GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (!PV.IsMine) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
        {
            StartSlide();
        }

        if (Input.GetKeyUp(slideKey) && isSliding) 
        {
            StopSlide();
        }
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine) return;
        if (isSliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        isSliding = true;
        playerObj.GetComponent<Animator>().SetBool("slide", true);
        slideTimer = maxSlideTime;
    }

    private void StopSlide()
    {
        isSliding = false;
        playerObj.GetComponent<Animator>().SetBool("slide", false);
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

        slideTimer -= Time.deltaTime;

        if (slideTimer <= 0)
            StopSlide();
    }
}
