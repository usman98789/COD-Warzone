using Photon.Pun;
using Photon.Realtime;
using SpeedTutorBattleRoyaleUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject cameraHolder;
    [SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime;

    [SerializeField] Item[] items;

    int itemIndex;
    int previousItemIndex = -1;

    float verticalLookRotation;
    bool grounded;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;

    Rigidbody rb;
    PhotonView PV;

    private Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (PV.IsMine)
        {
            anim = GetComponent<Animator>();
            EquipItem(0);
        } else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }

    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        Look();
        Move();
        Animate();
        Jump();

        for (int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }

        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if (itemIndex >= items.Length - 1)
            {
                EquipItem(0);
            } else
            {
                EquipItem(itemIndex + 1);
            }
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex <= 0)
            {
                EquipItem(items.Length - 1);
            } else
            {
                EquipItem(itemIndex - 1);
            }
        }

    }

    void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -55f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    void Move()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);

    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(transform.up * jumpForce);
        }
    }

    void EquipItem(int _index)
    {
        if (_index == previousItemIndex)
        {
            return;
        }

        itemIndex = _index;
        items[itemIndex].itemGameObject.SetActive(true);

        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;

        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    void Animate()
    {
        if (Input.GetKey(KeyCode.A) && grounded)
        {
            anim.SetFloat("Move", 0.5f);
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            anim.SetFloat("Move", 0f);
        }

        if (Input.GetKey(KeyCode.D) && grounded)
        {
            anim.SetFloat("Move", 0.5f);
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            anim.SetFloat("Move", 0f);
        }

        if (Input.GetKey(KeyCode.S) && grounded)
        {
            anim.SetFloat("Move", 0.5f);
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            anim.SetFloat("Move", 0f);
        }

        if (Input.GetKey(KeyCode.W) && grounded)
        {
            anim.SetFloat("Move", 0.5f);
        }
        else if (Input.GetKeyUp(KeyCode.W))
        {
            anim.SetFloat("Move", 0f);
        }

        if (Input.GetKey(KeyCode.LeftShift) && grounded)
        {
            anim.SetFloat("Move", 1f);
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            anim.SetFloat("Move", 0f);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(!PV.IsMine && targetPlayer == PV.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine)
            return;

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

}
