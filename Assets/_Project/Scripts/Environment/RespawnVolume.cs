using Project.Player;
using UnityEngine;

namespace Project.Environment
{
    [RequireComponent(typeof(BoxCollider))]
    public class RespawnVolume : MonoBehaviour
    {
        private void Reset()
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerRespawnController respawnController = other.GetComponent<PlayerRespawnController>();

            if (respawnController != null)
            {
                respawnController.Respawn();
            }
        }
    }
}
