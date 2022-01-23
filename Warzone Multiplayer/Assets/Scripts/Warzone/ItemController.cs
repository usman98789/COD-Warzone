using Photon.Pun;
using UnityEngine;

namespace SpeedTutorBattleRoyaleUI
{
    public class ItemController : MonoBehaviour
    {
        [Header("Item Type")]
        [SerializeField] private bool armour;
        [SerializeField] private bool cash;

        [Header("Item Parameters")]
        [SerializeField] private float itemValue;

        public void ObjectInteraction()
        {
            if (armour)
            {
                Debug.Log("Armor Update amount");
                UIController.instance.UpdateArmourAmount(itemValue, false);
                GetComponent<PhotonView>().RPC("RPC_SetNotActive", RpcTarget.All);
                FindObjectOfType<AudioManager>().Play("ArmorPickup");
            }

            if (cash)
            {
                UIController.instance.UpdateCashUI(itemValue);
                GetComponent<PhotonView>().RPC("RPC_SetNotActive", RpcTarget.All);
                FindObjectOfType<AudioManager>().Play("CashPickup");
            }
        }

        [PunRPC]
        public void RPC_SetNotActive()
        {
            Debug.Log("Set not active RPC");
            gameObject.SetActive(false);
        }
    }
}
