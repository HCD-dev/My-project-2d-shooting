using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float health = 100f;
    public int grenadeCount = 3;

    public enum WeaponType { None, Handgun, Shotgun, Rifle }

    [Header("Envanter (2 Slot)")]
    public WeaponType[] inventory = new WeaponType[2] { WeaponType.None, WeaponType.None };
    public int currentSlot = 0;
    private bool isHoldingGrenade = false;

    [Header("Silah Görselleri (Karakterin Altýndaki Objeler)")]
    public GameObject handgunObject;
    public GameObject shotgunObject;
    public GameObject rifleObject;

    [Header("Yere Atýlacak Prefablar (Pickup Prefablarý)")]
    public GameObject handgunPickupPrefab;
    public GameObject shotgunPickupPrefab;
    public GameObject riflePickupPrefab;

    [Header("Referanslar")]
    public GameObject bulletPrefab;
    public GameObject grenadePrefab;
    public Transform firePoint;
    public Camera cam;

    [Header("Silah Ayarlarý")]
    public float rifleFireRate = 0.15f;
    private float nextFireTime = 0f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 mousePos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // TEST ÝÇÝN: Oyuna baþlarken eline silah ver
        inventory[0] = WeaponType.Handgun;
        UpdateWeaponVisuals();
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        // Slot Seçimi (1 ve 2)
        if (Input.GetKeyDown(KeyCode.Alpha1)) { currentSlot = 0; isHoldingGrenade = false; UpdateWeaponVisuals(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { currentSlot = 1; isHoldingGrenade = false; UpdateWeaponVisuals(); }

        // 3'e Basýnca Bombayý Seç
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (grenadeCount > 0)
            {
                isHoldingGrenade = true;
                UpdateWeaponVisuals();
            }
        }

        // G ile Silahý Yere At
        if (Input.GetKeyDown(KeyCode.G)) DropWeapon();

        // Ateþ Etme veya Bomba Atma (Rifle için GetButton kullanarak basýlý tutma özelliði eklendi)
        if (Input.GetButton("Fire1"))
        {
            if (isHoldingGrenade)
            {
                if (Input.GetButtonDown("Fire1")) ThrowGrenade(); // Bomba tek týkla atýlýr
            }
            else if (inventory[currentSlot] != WeaponType.None)
            {
                Shoot();
            }
        }
    }

    void FixedUpdate()
    {
        // 1. Önce nereye gitmek istediðimizi hesaplýyoruz
        Vector2 targetPosition = rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime;

        // 2. Gidilecek yeri sýnýrlar (-9 ile 9 ve -5 ile 5) içerisinde kýsýtlýyoruz
        targetPosition.x = Mathf.Clamp(targetPosition.x, -9f, 9f);
        targetPosition.y = Mathf.Clamp(targetPosition.y, -5f, 5f);

        // 3. Rigidbody'yi sadece bu "güvenli" alana taþýyoruz
        rb.MovePosition(targetPosition);

        // Bakýþ yönü (Mouse takibi)
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
    }

    public void PickUpWeapon(WeaponType newWp)
    {
        int targetSlot = -1;

        // 1. Önce boþ slot ara
        if (inventory[0] == WeaponType.None) targetSlot = 0;
        else if (inventory[1] == WeaponType.None) targetSlot = 1;

        // 2. Boþ slot yoksa þu anki slotu yere at
        if (targetSlot != -1)
        {
            currentSlot = targetSlot;
        }
        else
        {
            DropWeapon(); // Elindekini fýrlat (inventory[currentSlot] None olur)
        }

        // 3. Silahý yerleþtir ve GÖRSELÝ ZORLA GÜNCELLE
        inventory[currentSlot] = newWp;
        isHoldingGrenade = false;

        // Debug ekleyelim, Console ekranýnda hangi slota ne aldýðýný gör:
        Debug.Log("Slot " + currentSlot + " içine þu silah alýndý: " + newWp);

        UpdateWeaponVisuals();
    }
    void DropWeapon()
    {
        // Bomba elindeyken silah atamazsýn
        if (isHoldingGrenade) return;

        WeaponType toDrop = inventory[currentSlot];
        if (toDrop == WeaponType.None) return;

        GameObject prefab = null;
        if (toDrop == WeaponType.Handgun) prefab = handgunPickupPrefab;
        else if (toDrop == WeaponType.Shotgun) prefab = shotgunPickupPrefab;
        else if (toDrop == WeaponType.Rifle) prefab = riflePickupPrefab;

        if (prefab) Instantiate(prefab, transform.position + (Vector3)Random.insideUnitCircle * 0.5f, Quaternion.identity);

        inventory[currentSlot] = WeaponType.None;
        UpdateWeaponVisuals();
    }

    public void UpdateWeaponVisuals()
    {
        // Önce hepsini kapat (Temizle)
        if (handgunObject) handgunObject.SetActive(false);
        if (shotgunObject) shotgunObject.SetActive(false);
        if (rifleObject) rifleObject.SetActive(false);

        // Sadece bomba modunda deðilsek ve slot doluysa aç
        if (!isHoldingGrenade)
        {
            WeaponType activeWp = inventory[currentSlot];
            if (activeWp == WeaponType.Handgun && handgunObject) handgunObject.SetActive(true);
            if (activeWp == WeaponType.Shotgun && shotgunObject) shotgunObject.SetActive(true);
            if (activeWp == WeaponType.Rifle && rifleObject) rifleObject.SetActive(true);
        }
        // Ýstersen buraya else { bomba_gorseli.SetActive(true); } ekleyebilirsin
    }

    void Shoot()
    {
        if (Time.time < nextFireTime) return;

        WeaponType type = inventory[currentSlot];

        if (type == WeaponType.Shotgun)
        {
            for (int i = 0; i < 5; i++)
            {
                float spread = Random.Range(-15f, 15f);
                Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, spread);
                Instantiate(bulletPrefab, firePoint.position, rot);
            }
            nextFireTime = Time.time + 0.6f;
        }
        else if (type == WeaponType.Rifle)
        {
            float recoil = Random.Range(-5f, 5f);
            Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, recoil);
            Instantiate(bulletPrefab, firePoint.position, rot);
            nextFireTime = Time.time + rifleFireRate;
        }
        else if (type == WeaponType.Handgun)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            nextFireTime = Time.time + 0.25f;
        }
    }

    void ThrowGrenade()
    {
        if (grenadeCount > 0)
        {
            grenadeCount--;
            GameObject g = Instantiate(grenadePrefab, firePoint.position, Quaternion.identity);
            g.GetComponent<Grenade>().Launch(mousePos);

            if (grenadeCount <= 0)
            {
                isHoldingGrenade = false;
                UpdateWeaponVisuals();
            }
        }
    }

    public void AddHealth(float amount) => health = Mathf.Min(health + amount, 100f);
    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0) { health = 0; Debug.Log("Game Over"); }
    }
}