using Photon.Pun;
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

    private bool isAiming = false;
    private GameObject[] allObjects = new GameObject[] {};

    public float aimSmooth = 10;
    private AudioSource audioSrc;

    [SerializeField] AudioClip[] footsteps;
    private float footsteptimer = 0;
    private float GetCurrOffset => Input.GetKey(KeyCode.LeftShift) ? 0.1f * 0.8f : 0.1f;

    [SerializeField] Image redSplatterImage;
    [SerializeField] Image redialCircle;

    [SerializeField] AudioClip armorBreak;

    private bool firstBreak = false;

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
            anim = GetComponent<Animator>();
            EquipItem(0);
        } else
        {
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
            curr.transform.parent.transform.parent.transform.GetComponent<SingleShotGun>().UpdateAmmo();
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
            curr.transform.parent.transform.parent.transform.GetComponent<SingleShotGun>().UpdateAmmo();
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

        if (moveAmount.x > 0 || moveAmount.y > 0 || moveAmount.z > 0 && audioSrc.isPlaying == false)
        {
            PV.RPC("RPC_PlayFootStep", RpcTarget.All);
        } 

    }

    [PunRPC]
    public void RPC_PlayFootStep()
    {
        if (!grounded) return;
        if (!Input.anyKey) return;

        footsteptimer -= Time.deltaTime;

        if (footsteptimer <= 0)
        {
            audioSrc.volume = Random.Range(0.5f, 0.7f);
            //audioSrc.PlayOneShot(footsteps[Random.Range(0, footsteps.Length - 1)]);
            footsteptimer = GetCurrOffset;
        }

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
        Color splatterAlpha = redSplatterImage.color;
        if (!PV.IsMine)
        {
            return;
        }

        if (UIController.instance.currentArmourValue >= 0)
        {
            splatterAlpha.a = 1 - ((UIController.instance.currentArmourValue + UIController.instance.currentHealthValue) / 400);
            firstBreak = false;
            UIController.instance.currentArmourValue -= Time.deltaTime * damage;
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

    void Die()
    {
        playerManager.Die();
    }

}
