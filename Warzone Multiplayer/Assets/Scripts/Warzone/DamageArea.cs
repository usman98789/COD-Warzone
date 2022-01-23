using Photon.Pun;
using UnityEngine;

namespace SpeedTutorBattleRoyaleUI
{
    public class DamageArea : MonoBehaviour
    {
        [SerializeField] [Range(0, 50)] private float damageMult;

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("GONNA DAMAGE PLAYER");
                UIController.instance.inDamageArea = true;
                UIController.instance.regenHealth = false;

                if (UIController.instance.currentArmourValue >= 0)
                {
                    UIController.instance.currentArmourValue -= Time.deltaTime * damageMult;
                    //other.gameObject.GetComponent<PlayerController>().CallDamage(Time.deltaTime * damageMult, true);
                    //Debug.Log("UI controller instance currentArmourValue" + UIController.instance.currentArmourValue);
                    UIController.instance.UpdateUI();
                }

                if (UIController.instance.currentArmourValue <= 0 && UIController.instance.currentHealthValue >= 0)
                {
                    UIController.instance.currentHealthValue -= Time.deltaTime * damageMult;
                    //other.gameObject.GetComponent<PlayerController>().CallDamage(Time.deltaTime * damageMult, false);
                    //Debug.Log("UI controller instance currentHealthValue" + UIController.instance.currentArmourValue);
                    UIController.instance.UpdateUI();
                }
               
            } 
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                //FindObjectOfType<AudioManager>().Stop("GasCough");
                UIController.instance.inDamageArea = false;
                UIController.instance.regenHealth = true;
            }
        }
    }
}
