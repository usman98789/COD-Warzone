using Photon.Pun;
using Photon.Realtime;
using SpeedTutorBattleRoyaleUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
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

    PlayerManager playerManager;

    [SerializeField] WeaponBob weaponBobScript;

    [SerializeField] Animator smgAnimator;
    [SerializeField] Animator pistolAnimator;
    [SerializeField] Animator shotgunAnimator;
    [SerializeField] Animator sniperAnimator;
    [SerializeField] Animator arAnimator;
    [SerializeField] Animator macAnimator;
    private Animator curr;

    private bool isAiming = false;
    private Animator[] allAnimators = new Animator[] {};

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
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

        allAnimators = new Animator[] { smgAnimator, pistolAnimator, shotgunAnimator, sniperAnimator, arAnimator, macAnimator };
        disableAnimator();
        curr = allAnimators[0];
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
                curr = allAnimators[0];
                EquipItem(0);
            } else
            {
                curr = allAnimators[itemIndex + 1];
                EquipItem(itemIndex + 1);
            }
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex <= 0)
            {
                curr = allAnimators[items.Length - 1];
                EquipItem(items.Length - 1);
            } else
            {
                curr = allAnimators[itemIndex - 1];
                EquipItem(itemIndex - 1);
            }
        }

        if (Input.GetMouseButton(0))
        {
            items[itemIndex].Use();
        }

        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
            SetAnimator();
        }

        if (Input.GetMouseButtonUp(1))
        {
            isAiming = false;
            SetAnimator();
        }

        if (!isAiming)
        {
            weaponBobScript.enabled = true;
            weaponBobScript.currentSpeed = AllowWeaponBob() ? (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed) : 0;
        } else
        {

            weaponBobScript.enabled = false;
        }

    }

    void disableAnimator()
    {
        for (int i = 0; i < allAnimators.Length; i++)
        {
            allAnimators[i].enabled = false;
        }
    }

    Animator getCurrentAnimator()
    {
        var res = new Animator();
        for(int i =0; i < allAnimators.Length; i++)
        {
            if (allAnimators[i].gameObject.activeInHierarchy)
            {
                Debug.Log("FOUND IT: " + allAnimators[i].gameObject.name);
                res = allAnimators[i];
                break;
            }
        }
        return res;
    }

    void SetAnimator()
    {
        curr.SetBool("Aim", isAiming);
        curr.enabled = isAiming;
    }

    public bool AllowWeaponBob()
    {
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            if (grounded) return true;
        }
        return false;
    }

    void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -65f, 90f);

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

    public void TakeDamage(float damage)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage)
    {

        if (!PV.IsMine)
        {
            return;
        }

        if (UIController.instance.currentArmourValue >= 0)
        {
            UIController.instance.currentArmourValue -= Time.deltaTime * damage;
            UIController.instance.UpdateUI();
        }

        if (UIController.instance.currentArmourValue <= 0 && UIController.instance.currentHealthValue >= 0)
        {
            UIController.instance.currentHealthValue -= Time.deltaTime * damage;
            UIController.instance.UpdateUI();
        }

        if (UIController.instance.currentHealthValue <= 0)
        {
            Die();
        }

    }

    void Die()
    {
        playerManager.Die();
    }

}
