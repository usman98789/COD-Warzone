using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FPS/New Gun")]
public class GunInfo : ItemInfo
{
    public float damage;
    public float fireRate = 0.1f;
    public int clipSize = 30;
    public float kickback;

}
