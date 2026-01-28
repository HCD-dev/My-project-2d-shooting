using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum ItemType { Grenade, Medkit, Handgun, Shotgun, Rifle }
    public ItemType itemType;

    private bool isPlayerInRange = false;
    private PlayerController playerRef;

    void Update()
    {
        // Medkit ve Grenade otomatik alýndýðý için Update'te E kontrolüne gerek yok
        if (itemType == ItemType.Medkit || itemType == ItemType.Grenade) return;

        // Silahlarý (Handgun, Shotgun, Rifle) E ile al
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (playerRef != null) DoPickup();
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
                // Can 100'den azsa al
                if (playerRef.health < 100f)
                {
                    playerRef.AddHealth(50f);
                }
                else return; // Can doluysa yerde kalsýn
                break;
            case ItemType.Grenade:
                // Bomba sayýsý 3'ten azsa otomatik al
                if (playerRef.grenadeCount < 3)
                {
                    playerRef.grenadeCount++;
                }
                else return; // Bomba doluysa yerde kalsýn
                break;
        }

        // Eðer buraya kadar kod ulaþtýysa (return olmadýysa) objeyi yok et
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerRef = other.GetComponent<PlayerController>();
            isPlayerInRange = true;

            // OTOMATÝK ALMA: Medkit veya Grenade ise çarptýðýn an al
            if (itemType == ItemType.Medkit || itemType == ItemType.Grenade)
            {
                DoPickup();
            }
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