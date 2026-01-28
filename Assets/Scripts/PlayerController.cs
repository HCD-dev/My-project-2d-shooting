using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro için gerekli

public class PlayerController : MonoBehaviour
{
    [Header("UI Referanslarý (Sürükle-Býrak)")]
    public Slider healthSlider;
    public TextMeshProUGUI grenadeText; // TMP tipine güncellendi
    public RawImage grenadeDisplay;

    [Header("Slot 1 Raw Images (Obje olarak)")]
    public RawImage s1_Handgun;
    public RawImage s1_Shotgun;
    public RawImage s1_Rifle;

    [Header("Slot 2 Raw Images (Obje olarak)")]
    public RawImage s2_Handgun;
    public RawImage s2_Shotgun;
    public RawImage s2_Rifle;

    [Header("Oyuncu Ýstatistikleri")]
    public float moveSpeed = 5f;
    public float health = 100f;
    public int grenadeCount = 3;

    public enum WeaponType { None, Handgun, Shotgun, Rifle }

    [Header("Envanter (2 Slot)")]
    public WeaponType[] inventory = new WeaponType[2] { WeaponType.None, WeaponType.None };
    public int currentSlot = 0;
    private bool isHoldingGrenade = false;

    [Header("Silah Görselleri (Karakterin Elindekiler)")]
    public GameObject handgunObject;
    public GameObject shotgunObject;
    public GameObject rifleObject;

    [Header("Yere Atýlacak Prefablar")]
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

        if (healthSlider != null)
        {
            healthSlider.maxValue = 100f;
            healthSlider.value = health;
        }

        // Baþlangýçta 1. slotta tabanca olsun
        inventory[0] = WeaponType.Handgun;
        UpdateWeaponVisuals();
    }

    void Update()
    {
        // Girdiler
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        // Slot Seçimi (1-2-3)
        if (Input.GetKeyDown(KeyCode.Alpha1)) { currentSlot = 0; isHoldingGrenade = false; UpdateWeaponVisuals(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { currentSlot = 1; isHoldingGrenade = false; UpdateWeaponVisuals(); }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (grenadeCount > 0)
            {
                isHoldingGrenade = true;
                UpdateWeaponVisuals();
            }
        }

        // Silah Atma
        if (Input.GetKeyDown(KeyCode.G)) DropWeapon();

        // Ateþ/Bomba
        if (Input.GetButton("Fire1"))
        {
            if (isHoldingGrenade)
            {
                if (Input.GetButtonDown("Fire1")) ThrowGrenade();
            }
            else if (inventory[currentSlot] != WeaponType.None)
            {
                Shoot();
            }
        }

        // Her karede UI ve HUD güncelle
        UpdateUI();
    }

    void FixedUpdate()
    {
        // Hareket ve Sýnýrlandýrma
        Vector2 targetPosition = rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime;
        targetPosition.x = Mathf.Clamp(targetPosition.x, -9f, 9f);
        targetPosition.y = Mathf.Clamp(targetPosition.y, -5f, 5f);
        rb.MovePosition(targetPosition);

        // Bakýþ Yönü
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
    }

    void UpdateUI()
    {
        if (healthSlider != null) healthSlider.value = health;
        if (grenadeText != null) grenadeText.text = "x" + grenadeCount;

        if (grenadeDisplay != null)
            grenadeDisplay.enabled = (grenadeCount > 0);

        UpdateWeaponHUD();
    }

    void UpdateWeaponHUD()
    {
        // SLOT 1 KONTROLÜ
        WeaponType s1 = inventory[0];
        if (s1_Handgun) s1_Handgun.gameObject.SetActive(s1 == WeaponType.Handgun);
        if (s1_Shotgun) s1_Shotgun.gameObject.SetActive(s1 == WeaponType.Shotgun);
        if (s1_Rifle) s1_Rifle.gameObject.SetActive(s1 == WeaponType.Rifle);

        // SLOT 2 KONTROLÜ
        WeaponType s2 = inventory[1];
        if (s2_Handgun) s2_Handgun.gameObject.SetActive(s2 == WeaponType.Handgun);
        if (s2_Shotgun) s2_Shotgun.gameObject.SetActive(s2 == WeaponType.Shotgun);
        if (s2_Rifle) s2_Rifle.gameObject.SetActive(s2 == WeaponType.Rifle);

        // Seçili olaný beyaz yap, olmayaný soluk yap
        ApplySelectionHighlight(0, s1_Handgun, s1_Shotgun, s1_Rifle);
        ApplySelectionHighlight(1, s2_Handgun, s2_Shotgun, s2_Rifle);
    }

    void ApplySelectionHighlight(int slotIndex, params RawImage[] images)
    {
        Color c = (currentSlot == slotIndex && !isHoldingGrenade) ? Color.white : new Color(1, 1, 1, 0.3f);
        foreach (var img in images) { if (img != null) img.color = c; }
    }

    public void PickUpWeapon(WeaponType newWp)
    {
        int targetSlot = -1;
        if (inventory[0] == WeaponType.None) targetSlot = 0;
        else if (inventory[1] == WeaponType.None) targetSlot = 1;

        if (targetSlot != -1) currentSlot = targetSlot;
        else DropWeapon();

        inventory[currentSlot] = newWp;
        isHoldingGrenade = false;
        UpdateWeaponVisuals();
    }

    void DropWeapon()
    {
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
        if (handgunObject) handgunObject.SetActive(false);
        if (shotgunObject) shotgunObject.SetActive(false);
        if (rifleObject) rifleObject.SetActive(false);

        if (!isHoldingGrenade)
        {
            WeaponType activeWp = inventory[currentSlot];
            if (activeWp == WeaponType.Handgun && handgunObject) handgunObject.SetActive(true);
            if (activeWp == WeaponType.Shotgun && shotgunObject) shotgunObject.SetActive(true);
            if (activeWp == WeaponType.Rifle && rifleObject) rifleObject.SetActive(true);
        }
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