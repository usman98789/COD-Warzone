using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject headLock;

    // Update is called once per frame
    void Update()
    {
        var posX = player.transform.position.x + 0.0183f + 0.03f;
        var posY = player.transform.position.y + 0.1358f - 0.03f;
        var posZ = player.transform.position.z + 0.4372f - 0.02f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            var newPosY = posY - 0.03f;
            transform.position = new Vector3(posX, newPosY, posZ);
        }
        else
        {
            transform.position = new Vector3(posX, posY, posZ);
        }
    }
}
