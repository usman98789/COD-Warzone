using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam;

    bool canShoot;
    int currAmmoInClip;

    public bool randomRecoil;
    public Vector2 randomRecoilConstraints;
    public Vector2 recoilPattern;

    private void Start()
    {
        currAmmoInClip = ((GunInfo)itemInfo).clipSize;
        canShoot = true;
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && currAmmoInClip < ((GunInfo)itemInfo).clipSize)
        {
            Debug.Log("RELOAD");
            currAmmoInClip = ((GunInfo)itemInfo).clipSize;
        }
    }

    public override void Use()
    {
        Shoot();
    }

    void Shoot()
    {
        if (canShoot && currAmmoInClip > 0)
        {
            canShoot = false;
            currAmmoInClip -= 1;
            //Debug.Log("CURR AMMO IN CLIP NOW IS" + currAmmoInClip);
            //Debug.Log("CLIP SIZE IS" + ((GunInfo)itemInfo).clipSize);
            StartCoroutine(ShootGun());
        }

    }

    void DetermineRecoil()
    {
        Debug.Log("INSIDE" + GameObject.Find(itemInfo.ItemName).transform.parent.name);
        //GameObject.Find(itemInfo.ItemName).transform.parent.transform.localPosition -= Vector3.forward * 0.1f;
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
