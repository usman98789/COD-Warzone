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

    [SerializeField] GameObject cameraHolder;
    private Vector3 cameraPos;
    private float distToGround;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        PV = playerObj.GetComponent<PhotonView>();
        cameraPos = cameraHolder.transform.localPosition;
        distToGround = playerObj.GetComponent<Collider>().bounds.extents.y;
    }

    private void Update()
    {
        if (!PV.IsMine) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0) && !IsGrounded() && !isSliding)
        {
            StartSlide();
        }

        if (Input.GetKeyUp(slideKey) && isSliding) 
        {
            StopSlide();
        }
    }

    bool IsGrounded() {
        return Physics.Raycast(transform.position, -Vector3.up, (float)(distToGround + 0.1));
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
        cameraHolder.transform.localPosition = new Vector3(cameraPos.x, cameraPos.y - 0.65f, cameraPos.z);
        playerObj.GetComponent<Animator>().SetBool("slide", true);
        slideTimer = maxSlideTime;
    }

    private void StopSlide()
    {
        isSliding = false;
        cameraHolder.transform.localPosition = new Vector3(cameraPos.x, cameraPos.y, cameraPos.z);
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
