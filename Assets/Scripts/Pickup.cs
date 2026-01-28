using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum ItemType { Grenade, Medkit, Handgun, Shotgun, Rifle }
    public ItemType itemType;

    private bool isPlayerInRange = false;
    private PlayerController playerRef;

    void Update()
    {
        // Menzildeysek ve E'ye basarsak al
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (playerRef != null)
            {
                DoPickup();
            }
        }
    }

    void DoPickup()
    {
        switch (itemType)
        {
            case ItemType.Handgun:
                playerRef.PickUpWeapon(PlayerController.WeaponType.Handgun);
                break;
            case ItemType.Shotgun:
                playerRef.PickUpWeapon(PlayerController.WeaponType.Shotgun);
                break;
            case ItemType.Rifle:
                playerRef.PickUpWeapon(PlayerController.WeaponType.Rifle);
                break;
            case ItemType.Medkit:
                if (playerRef.health < 100) playerRef.AddHealth(50);
                else return; // Can doluysa yok etme
                break;
            case ItemType.Grenade:
                if (playerRef.grenadeCount < 3) playerRef.grenadeCount++;
                else return; // Bomba doluysa yok etme
                break;
        }
        Destroy(gameObject); // Alým baþarýlýysa yerdeki objeyi sil
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerRef = other.GetComponent<PlayerController>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerRef = null;
        }
    }
}