﻿using Photon.Pun;
using Photon.Realtime;
using SpeedTutorBattleRoyaleUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;

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

    [SerializeField] GameObject smg;
    [SerializeField] GameObject pistol;
    [SerializeField] GameObject shotgun;
    [SerializeField] GameObject sniper;
    [SerializeField] GameObject ar;
    [SerializeField] GameObject mac;
    private GameObject curr;

    [SerializeField] GameObject scopeOverlay;
    [SerializeField] Camera cameraFOV;


    [SerializeField] GameObject arms;

    private bool isAiming = false;
    private GameObject[] allObjects = new GameObject[] {};

    public float aimSmooth = 10;
    private AudioSource audioSrc;

    [SerializeField] AudioClip footsteps;
    private float footsteptimer = 0;
    private float GetCurrOffset => Input.GetKey(KeyCode.LeftShift) ? 0.1f * 0.8f : 0.1f;

    [SerializeField] Image redSplatterImage;
    [SerializeField] Image redialCircle;

    [SerializeField] AudioClip armorBreak;

    private bool firstBreak = false;

    [SerializeField] GameObject rozeSkinGraphics;
    [SerializeField] GameObject gunContainer;

    private Vector3 origPos;

    private float elapsedTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        audioSrc = GetComponent<AudioSource>();
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (PV.IsMine)
        {
            HideLayer(rozeSkinGraphics, LayerMask.NameToLayer("DontDraw"));
            anim = GetComponent<Animator>();
            EquipItem(0);

        } else
        {
            HideLayer(gunContainer, LayerMask.NameToLayer("DontDraw"));
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }

        allObjects = new GameObject[] { smg, pistol, shotgun, sniper, ar, mac };
        curr = allObjects[0];
        if (PV.IsMine)
        {
            curr.transform.parent.transform.parent.transform.GetComponent<SingleShotGun>().UpdateAmmo();
        }

    }

    void HideLayer(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            HideLayer(child.gameObject, newLayer);
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
                curr = allObjects[0];
                EquipItem(0);
            } else
            {
                curr = allObjects[itemIndex + 1];
                EquipItem(itemIndex + 1);
            }
            if (PV.IsMine)
            {
                curr.transform.parent.transform.parent.transform.GetComponent<SingleShotGun>().UpdateAmmo();
            }
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex <= 0)
            {
                curr = allObjects[items.Length - 1];
                EquipItem(items.Length - 1);
            } else
            {
                curr = allObjects[itemIndex - 1];
                EquipItem(itemIndex - 1);
            }
            if (PV.IsMine)
            {
                curr.transform.parent.transform.parent.transform.GetComponent<SingleShotGun>().UpdateAmmo();
            }
        }

        if (Input.GetMouseButton(0))
        {
            items[itemIndex].Use();
        }

        DetermineAim();

        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;

            if (curr.gameObject.name == "Kar98")
            {
                StartCoroutine(OnScoped());
            }
        }


        if (Input.GetMouseButtonUp(1))
        {
            if (curr.gameObject.name == "Kar98")
            {
                StartCoroutine(UnScoped());
            }
            isAiming = false;
        }

        if (!isAiming)
        {
            weaponBobScript.enabled = true;
            weaponBobScript.currentSpeed = AllowWeaponBob() ? (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed) : 0;
        }
        else
        {

            weaponBobScript.enabled = false;
        }

        if (UIController.instance.regenHealth)
        {
            Color splatterAlpha = redSplatterImage.color;
            splatterAlpha.a = 1 - UIController.instance.currentHealthValue / 100;
            redSplatterImage.color = splatterAlpha;
        }

    }

    void DetermineAim()
    {
        Vector3 target;
        if (isAiming)
        {
            target = curr.gameObject.GetComponent<AimPosition>().normalPos;
        } else
        {
            target = curr.transform.localPosition;
        }

        if (Input.GetMouseButton(1))
        {
            weaponBobScript.enabled = false;
            target = curr.gameObject.GetComponent<AimPosition>().aimPos;
        }

        Vector3 desiredPos = Vector3.Lerp(curr.transform.localPosition, target, Time.deltaTime * aimSmooth);
        curr.transform.localPosition = desiredPos;
    }

    IEnumerator OnScoped()
    {
        yield return new WaitForSeconds(0.15f);
        scopeOverlay.SetActive(true);
        curr.gameObject.SetActive(false);
        cameraFOV.fieldOfView = 35;
    }

    IEnumerator UnScoped()
    {
        yield return new WaitForSeconds(0.15f);
        scopeOverlay.SetActive(false);
        curr.gameObject.SetActive(true);
        cameraFOV.fieldOfView = 75;
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
        arms.transform.GetChild(itemIndex).gameObject.SetActive(true);

        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
            arms.transform.GetChild(previousItemIndex).gameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;

        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
        origPos = new Vector3(arms.transform.GetChild(itemIndex).transform.localPosition.x, arms.transform.GetChild(itemIndex).transform.localPosition.y, arms.transform.GetChild(itemIndex).transform.localPosition.z);
    }

    void MoveGunDown()
    {
        if (arms.transform.GetChild(itemIndex).gameObject.transform.localPosition.y == origPos.y && !Input.GetKeyDown(KeyCode.C))
        {
            arms.transform.GetChild(itemIndex).gameObject.transform.localPosition = new Vector3(origPos.x, origPos.y - 0.15f, origPos.z + 0.06f);
        }
    }

    void MoveGunDownSlide()
    {
        arms.transform.GetChild(itemIndex).gameObject.transform.localPosition = Vector3.Lerp(origPos, new Vector3(origPos.x, origPos.y - 0.65f, origPos.z + 0.06f), 1);
    }

    void Animate()
    {

        if (Input.GetKey(KeyCode.A) && grounded)
        {
            anim.SetFloat("Move", 0.5f);
            MoveGunDown();
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            anim.SetFloat("Move", 0f);
            arms.transform.GetChild(itemIndex).gameObject.transform.localPosition = new Vector3(origPos.x, origPos.y, origPos.z);
        }

        if (Input.GetKey(KeyCode.D) && grounded)
        {
            anim.SetFloat("Move", 0.5f);
            MoveGunDown();
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            anim.SetFloat("Move", 0f);
            arms.transform.GetChild(itemIndex).gameObject.transform.localPosition = new Vector3(origPos.x, origPos.y, origPos.z);
        }

        if (Input.GetKey(KeyCode.S) && grounded)
        {
            anim.SetFloat("Move", 0.5f);
            MoveGunDown();
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            anim.SetFloat("Move", 0f);
            arms.transform.GetChild(itemIndex).gameObject.transform.localPosition = new Vector3(origPos.x, origPos.y, origPos.z);
        }

        if (Input.GetKey(KeyCode.W) && grounded)
        {
            anim.SetFloat("Move", 0.5f);
            MoveGunDown();
        }
        else if (Input.GetKeyUp(KeyCode.W))
        {
            anim.SetFloat("Move", 0f);
            arms.transform.GetChild(itemIndex).gameObject.transform.localPosition = new Vector3(origPos.x, origPos.y, origPos.z);
        }

        if (Input.GetKey(KeyCode.LeftShift) && grounded)
        {
            anim.SetFloat("Move", 1f);
            MoveGunDown();
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            anim.SetFloat("Move", 0f);
            arms.transform.GetChild(itemIndex).gameObject.transform.localPosition = new Vector3(origPos.x, origPos.y, origPos.z);
        }

        if (Input.GetKey(KeyCode.C) && grounded && (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0))
        {
            MoveGunDownSlide();
        } else if (Input.GetKeyUp(KeyCode.C))
        {
            arms.transform.GetChild(itemIndex).gameObject.transform.localPosition = Vector3.Lerp(arms.transform.GetChild(itemIndex).gameObject.transform.localPosition, new Vector3(origPos.x, origPos.y, origPos.z), 1);
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
        if (!PV.IsMine) return;

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
        Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (targetVelocity.x != 0 || targetVelocity.z != 0 && grounded)
        {
            var isSprinting = false;
            audioSrc.volume = Random.Range(0.7f, 0.9f);
            if (Input.GetKey(KeyCode.LeftShift))
            {
                isSprinting = true;
                audioSrc.pitch = Random.Range(1.7f, 1.9f);
            }
            else
            {
                audioSrc.pitch = Random.Range(0.8f, 1f);
            }
            if (!audioSrc.isPlaying && targetVelocity.y == 0)
            {
                audioSrc.Play();
            }

            if (!grounded)
            {
                if (isSprinting)
                {
                    audioSrc.PlayDelayed(0.001f);
                }
                else
                {
                    audioSrc.PlayDelayed(0.1f);
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        Color splatterAlpha = redSplatterImage.color;
        if (!PV.IsMine)
        {
            return;
        }

        if (UIController.instance.currentArmourValue >= 0)
        {
            firstBreak = false;
            UIController.instance.currentArmourValue -= Time.deltaTime * damage;
            splatterAlpha.a = 1 - (UIController.instance.currentArmourValue / 400);
            StartCoroutine(ArmorHurt());
            UIController.instance.UpdateUI();
        }

        if (UIController.instance.currentArmourValue <= 0 && UIController.instance.currentHealthValue >= 0)
        {
            if (!firstBreak)
            {
                firstBreak = true;
                PV.RPC("RPC_ArmorBreak", RpcTarget.All);
            }
            StartCoroutine(HurtFlash());
            splatterAlpha.a = 1 - (UIController.instance.currentHealthValue / 400);

            UIController.instance.currentHealthValue -= Time.deltaTime * damage;
            UIController.instance.UpdateUI();
        }

        redSplatterImage.color = splatterAlpha;

        if (UIController.instance.currentHealthValue <= 0)
        {
            Die();
        }

    }

    [PunRPC]
    public void RPC_ArmorBreak()
    {
        AudioSource.PlayClipAtPoint(armorBreak, transform.localPosition);
    }

    IEnumerator HurtFlash()
    {
        redialCircle.enabled = true;
        yield return new WaitForSeconds(0.3f);
        redialCircle.enabled = false;
    }


    IEnumerator ArmorHurt()
    {
        Color splatterAlpha = redSplatterImage.color;
        splatterAlpha.a = 1 - (UIController.instance.currentArmourValue / 400);
        redSplatterImage.color = splatterAlpha;

        yield return new WaitForSeconds(1f);

        for (float i = splatterAlpha.a - (float) 0.1; i >= -0.1;  i-= (float) 0.1)
        {
            yield return new WaitForSeconds(0.2f);
            splatterAlpha.a = i;
        }

        redSplatterImage.color = splatterAlpha;
    }

    void Die()
    {
        playerManager.Die();
    }

}
