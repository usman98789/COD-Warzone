using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SpeedTutorBattleRoyaleUI;


public class SingleShotGun : Gun
{
    [SerializeField] Camera cam;

    [SerializeField] Image muzzleFlash;
    [SerializeField] Sprite[] flashes;

    private AudioSource audioSource;

    bool canShoot;
    int currAmmoInClip;
    public PhotonView PV;
    public bool reload = false;
    public GameObject gunObj;

    [SerializeField] GameObject hitmarker;
    [SerializeField] AudioClip hitmarkerSound;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        canShoot = true;
        UpdateAmmo();
    }

    private void Update()
    {
        if (!PV.IsMine) return;

        if (Input.GetKeyDown(KeyCode.R) && currAmmoInClip < ((GunInfo)itemInfo).clipSize && !Input.GetMouseButton(0) && !Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(ReloadGun());
        }
    }

    IEnumerator ReloadGun()
    {
        if (!PV.IsMine) yield return null;
        reload = true;
        Vector3 curr = gunObj.transform.localScale;
        gunObj.transform.localScale = new Vector3(0, 0, 0);
        audioSource.clip = ((GunInfo)itemInfo).reloadSound;
        audioSource.volume = 0.2f;
        audioSource.PlayOneShot(((GunInfo)itemInfo).reloadSound);
        yield return new WaitForSeconds(((GunInfo)itemInfo).reloadSound.length);
        reload = false;
        gunObj.transform.localScale = curr;
        currAmmoInClip = ((GunInfo)itemInfo).clipSize;
        Debug.Log("SingleShotGun ReloadGun() currAmmoInClip" + currAmmoInClip);
        UIController.instance.UpdateAmmo(currAmmoInClip.ToString(), ((GunInfo)itemInfo).clipSize.ToString());
        UIController.instance.UpdateUI();
    }

    public void UpdateAmmo()
    {
        if (!PV.IsMine) return;
        currAmmoInClip = ((GunInfo)itemInfo).clipSize;
        Debug.Log("SingleShotGun UpdateAmmo() currAmmoInClip" + currAmmoInClip);
        UIController.instance.UpdateAmmo(currAmmoInClip.ToString(), ((GunInfo)itemInfo).clipSize.ToString());
        UIController.instance.UpdateUI();
    }

    public override void Use()
    {
        Shoot();
    }

    void DetermineRecoil()
    {
        transform.GetChild(0).GetChild(0).transform.localPosition -= Vector3.forward * ((GunInfo)itemInfo).kickback;
    }

    void Shoot()
    {
        if (!PV.IsMine) return;
        if (reload) return;
        Debug.Log("SingleShotGun Shoot() currAmmoInClip" + currAmmoInClip);
        UIController.instance.UpdateAmmo(currAmmoInClip.ToString(), ((GunInfo)itemInfo).clipSize.ToString());
        UIController.instance.UpdateUI();
        if (canShoot && currAmmoInClip > 0)
        {
            canShoot = false;
            currAmmoInClip -= 1;
            StartCoroutine(ShootGun());
        }

    }

    IEnumerator ShootGun()
    {
        PV.RPC("RPC_ShootAudio", RpcTarget.All);
        StartCoroutine(MuzzleFlash());
        DetermineRecoil();
        RayCastEnemy();
        yield return new WaitForSeconds(((GunInfo)itemInfo).fireRate);
        canShoot = true;
    }

    [PunRPC]
    public void RPC_ShootAudio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            if (!PV.IsMine)
            {
                AudioSource.PlayClipAtPoint(((GunInfo)itemInfo).shootSound, transform.position);
            } else
            {
                audioSource.clip = ((GunInfo)itemInfo).shootSound;
                audioSource.volume = 0.5f;
                audioSource.PlayOneShot(((GunInfo)itemInfo).shootSound);
            }
        }
        else
        {
            if (!PV.IsMine)
            {
                AudioSource.PlayClipAtPoint(((GunInfo)itemInfo).shootSound, transform.position);
            }
            else 
            {
                audioSource.clip = ((GunInfo)itemInfo).shootSound;
                audioSource.volume = 0.5f;
                audioSource.PlayOneShot(((GunInfo)itemInfo).shootSound);
            }
        }
    }

    IEnumerator MuzzleFlash()
    {
        muzzleFlash.sprite = flashes[Random.Range(0, flashes.Length - 1)];
        muzzleFlash.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        muzzleFlash.sprite = null;
        muzzleFlash.color = new Color(0, 0, 0, 0);
    }

    void RayCastEnemy()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if(hit.collider.gameObject.GetComponent<IDamageable>() != null)
            {
                hitmarker.SetActive(true);
                gameObject.transform.GetChild(0).GetComponent<AudioSource>().Play();
                Invoke("disableHitmarker", 0.2f);
            }

            hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
        }
    }

    void disableHitmarker()
    {
        hitmarker.SetActive(false);
    }
}
