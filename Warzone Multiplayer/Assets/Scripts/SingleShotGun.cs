using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam;
    [SerializeField] Text ammoDisplay;

    [SerializeField] Image muzzleFlash;
    [SerializeField] Sprite[] flashes;

    private AudioSource audioSource;

    bool canShoot;
    int currAmmoInClip;
    public PhotonView PV;
    public bool reload = false;
    public GameObject gunObj;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (PV.IsMine)
        {
            currAmmoInClip = ((GunInfo)itemInfo).clipSize;
        }
        canShoot = true;
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
        reload = true;
        Vector3 curr = gunObj.transform.localScale;
        gunObj.transform.localScale = new Vector3(0, 0, 0);
        audioSource.volume = 0.5f;
        audioSource.PlayOneShot(((GunInfo)itemInfo).reloadSound);
        audioSource.volume = 1f;
        yield return new WaitForSeconds(((GunInfo)itemInfo).reloadSound.length);
        reload = false;
        gunObj.transform.localScale = curr;
        currAmmoInClip = ((GunInfo)itemInfo).clipSize;
        ammoDisplay.gameObject.GetComponent<UpdateAmmo>().UpdateAmount(currAmmoInClip.ToString(), ((GunInfo)itemInfo).clipSize.ToString());
    }

    public void UpdateAmmo()
    {
        if (!PV.IsMine) return;
        currAmmoInClip = ((GunInfo)itemInfo).clipSize;
        ammoDisplay.gameObject.GetComponent<UpdateAmmo>().UpdateAmount(currAmmoInClip.ToString(), ((GunInfo)itemInfo).clipSize.ToString());
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
        ammoDisplay.gameObject.GetComponent<UpdateAmmo>().UpdateAmount(currAmmoInClip.ToString(), ((GunInfo)itemInfo).clipSize.ToString());
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
            hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
        }
    }
}
