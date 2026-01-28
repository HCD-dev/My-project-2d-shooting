using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("UI Referanslarý")]
    public Slider healthSlider;
    public TextMeshProUGUI grenadeText;
    public RawImage grenadeDisplay;
    public GameObject gameOverPanel;

    [Header("Slot Raw Images")]
    public RawImage s1_Handgun; public RawImage s1_Shotgun; public RawImage s1_Rifle;
    public RawImage s2_Handgun; public RawImage s2_Shotgun; public RawImage s2_Rifle;

    [Header("Oyuncu Ayarlarý")]
    public float moveSpeed = 5f;
    public float health = 100f;
    public int grenadeCount = 3;
    private bool isDead = false;

    public enum WeaponType { None, Handgun, Shotgun, Rifle }
    [Header("Envanter")]
    public WeaponType[] inventory = new WeaponType[2] { WeaponType.None, WeaponType.None };
    public int currentSlot = 0;
    private bool isHoldingGrenade = false;

    [Header("Silah Objeleri (Karakter)")]
    public GameObject handgunObject; public GameObject shotgunObject; public GameObject rifleObject;

    [Header("Pickup Prefablarý")]
    public GameObject handgunPickupPrefab; public GameObject shotgunPickupPrefab; public GameObject riflePickupPrefab;

    [Header("Referanslar")]
    public GameObject bulletPrefab;
    public GameObject grenadePrefab;
    public Transform firePoint;
    public Camera cam;
    public float rifleFireRate = 0.15f;
    private float nextFireTime = 0f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 mousePos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (healthSlider != null) healthSlider.maxValue = 100f;
        inventory[0] = WeaponType.Handgun;
        UpdateWeaponVisuals();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (isDead) return;

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetKeyDown(KeyCode.Alpha1)) { currentSlot = 0; isHoldingGrenade = false; UpdateWeaponVisuals(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { currentSlot = 1; isHoldingGrenade = false; UpdateWeaponVisuals(); }
        if (Input.GetKeyDown(KeyCode.Alpha3) && grenadeCount > 0) { isHoldingGrenade = true; UpdateWeaponVisuals(); }
        if (Input.GetKeyDown(KeyCode.G)) DropWeapon();

        if (Input.GetButton("Fire1"))
        {
            if (isHoldingGrenade) { if (Input.GetButtonDown("Fire1")) ThrowGrenade(); }
            else if (inventory[currentSlot] != WeaponType.None) Shoot();
        }

        UpdateUI();
    }

    void FixedUpdate()
    {
        if (isDead) return;
        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
        Vector2 lookDir = mousePos - rb.position;
        rb.rotation = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
    }

    void UpdateUI()
    {
        if (healthSlider != null) healthSlider.value = health;
        if (grenadeText != null) grenadeText.text = "x" + grenadeCount;
        if (grenadeDisplay != null) grenadeDisplay.enabled = (grenadeCount > 0);
        UpdateWeaponHUD();
    }

    void UpdateWeaponHUD()
    {
        s1_Handgun.gameObject.SetActive(inventory[0] == WeaponType.Handgun);
        s1_Shotgun.gameObject.SetActive(inventory[0] == WeaponType.Shotgun);
        s1_Rifle.gameObject.SetActive(inventory[0] == WeaponType.Rifle);

        s2_Handgun.gameObject.SetActive(inventory[1] == WeaponType.Handgun);
        s2_Shotgun.gameObject.SetActive(inventory[1] == WeaponType.Shotgun);
        s2_Rifle.gameObject.SetActive(inventory[1] == WeaponType.Rifle);

        ApplyHighlight(0, s1_Handgun, s1_Shotgun, s1_Rifle);
        ApplyHighlight(1, s2_Handgun, s2_Shotgun, s2_Rifle);
    }

    void ApplyHighlight(int slot, params RawImage[] imgs)
    {
        Color c = (currentSlot == slot && !isHoldingGrenade) ? Color.white : new Color(1, 1, 1, 0.3f);
        foreach (var img in imgs) if (img != null) img.color = c;
    }

    public void PickUpWeapon(WeaponType newWp)
    {
        if (isDead) return;
        int targetSlot = (inventory[0] == WeaponType.None) ? 0 : (inventory[1] == WeaponType.None ? 1 : -1);
        if (targetSlot != -1) currentSlot = targetSlot; else DropWeapon();
        inventory[currentSlot] = newWp;
        isHoldingGrenade = false;
        UpdateWeaponVisuals();
    }

    void DropWeapon()
    {
        if (isHoldingGrenade || isDead || inventory[currentSlot] == WeaponType.None) return;
        GameObject p = (inventory[currentSlot] == WeaponType.Handgun) ? handgunPickupPrefab : (inventory[currentSlot] == WeaponType.Shotgun ? shotgunPickupPrefab : riflePickupPrefab);
        if (p) Instantiate(p, transform.position + (Vector3)Random.insideUnitCircle * 0.5f, Quaternion.identity);
        inventory[currentSlot] = WeaponType.None;
        UpdateWeaponVisuals();
    }

    public void UpdateWeaponVisuals()
    {
        handgunObject.SetActive(false); shotgunObject.SetActive(false); rifleObject.SetActive(false);
        if (!isHoldingGrenade && !isDead)
        {
            WeaponType active = inventory[currentSlot];
            if (active == WeaponType.Handgun) handgunObject.SetActive(true);
            else if (active == WeaponType.Shotgun) shotgunObject.SetActive(true);
            else if (active == WeaponType.Rifle) rifleObject.SetActive(true);
        }
    }

    void Shoot()
    {
        if (Time.time < nextFireTime) return;
        WeaponType type = inventory[currentSlot];
        if (type == WeaponType.Shotgun)
        {
            for (int i = 0; i < 5; i++) Instantiate(bulletPrefab, firePoint.position, firePoint.rotation * Quaternion.Euler(0, 0, Random.Range(-15f, 15f)));
            nextFireTime = Time.time + 0.6f;
        }
        else
        {
            float r = type == WeaponType.Rifle ? Random.Range(-5f, 5f) : 0;
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation * Quaternion.Euler(0, 0, r));
            nextFireTime = Time.time + (type == WeaponType.Rifle ? rifleFireRate : 0.25f);
        }
    }

    void ThrowGrenade()
    {
        grenadeCount--;
        GameObject g = Instantiate(grenadePrefab, firePoint.position, Quaternion.identity);
        g.GetComponent<Grenade>().Launch(mousePos);
        if (grenadeCount <= 0) { isHoldingGrenade = false; UpdateWeaponVisuals(); }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        health = Mathf.Max(health - amount, 0);
        if (health <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        handgunObject.SetActive(false); shotgunObject.SetActive(false); rifleObject.SetActive(false);
    }
    public void AddHealth(float amount)
    {
        if (!isDead)
        {
            health = Mathf.Min(health + amount, 100f);
            // Can barýný hemen güncellemek için:
            if (healthSlider != null) healthSlider.value = health;
        }
    }
}