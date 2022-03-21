using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam;
    [SerializeField] Text ammoDisplay;

    bool canShoot;
    int currAmmoInClip;

    private void Start()
    {
        currAmmoInClip = ((GunInfo)itemInfo).clipSize;
        canShoot = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && currAmmoInClip < ((GunInfo)itemInfo).clipSize && !Input.GetMouseButton(0) && !Input.GetKeyDown(KeyCode.F))
        {
            currAmmoInClip = ((GunInfo)itemInfo).clipSize;
            ammoDisplay.gameObject.GetComponent<UpdateAmmo>().UpdateAmount(currAmmoInClip.ToString(), ((GunInfo)itemInfo).clipSize.ToString());
        }
    }

    public void UpdateAmmo()
    {
        currAmmoInClip = ((GunInfo)itemInfo).clipSize;
        ammoDisplay.gameObject.GetComponent<UpdateAmmo>().UpdateAmount(currAmmoInClip.ToString(), ((GunInfo)itemInfo).clipSize.ToString());
    }

    public override void Use()
    {
        Shoot();
    }

    void DetermineRecoil()
    {
        Debug.Log(transform.GetChild(0).GetChild(0).name);
        transform.GetChild(0).GetChild(0).transform.localPosition -= Vector3.forward * ((GunInfo)itemInfo).kickback;
    }

    void Shoot()
    {
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
        DetermineRecoil();
        RayCastEnemy();
        yield return new WaitForSeconds(((GunInfo)itemInfo).fireRate);
        canShoot = true;

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
