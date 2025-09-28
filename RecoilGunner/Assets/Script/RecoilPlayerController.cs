using UnityEngine;

public class RecoilPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float recoilForce = 10f;
    public float maxSpeed = 15f;
    public float drag = 2f;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.1f;
    public float bulletSpeed = 20f;
    public int bulletsPerShot = 1;
    public float spreadAngle = 10f;

    [Header("Charge Shot Settings")]
    public float maxChargeTime = 2f;
    public float chargedRecoilMultiplier = 3f;
    public int chargedBulletsPerShot = 8;
    public float chargedSpreadAngle = 45f;
    public float chargedBulletSpeed = 25f;

    [Header("Visual Effects")]
    public LineRenderer aimLine;
    public Transform chargeCircle;
    public float aimLineLength = 5f;

    [Header("Input Settings")]
    public bool useMouseAiming = true;

    private Rigidbody2D rb;
    private Camera mainCamera;
    private float nextFireTime;
    private Vector2 aimDirection;
    private float chargeTime;
    private bool isCharging = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        // Set up physics
        rb.linearDamping = drag;

        // Set up visual effects
        SetupVisualEffects();
    }

    void Update()
    {
        // CHECK IF GAME IS OVER - Stop all input processing
        if (GameManager.Instance != null && GameManager.Instance.gameOver)
        {
            // Hide all visual effects when game is over
            if (aimLine != null && aimLine.enabled)
            {
                aimLine.enabled = false;
            }
            if (chargeCircle != null && chargeCircle.gameObject.activeInHierarchy)
            {
                chargeCircle.gameObject.SetActive(false);
            }
            return; // Exit early - no input processing
        }

        HandleInput();
        HandleAiming();
        UpdateVisualEffects();
        LimitSpeed();
    }

    void LimitSpeed()
    {
        // Don't limit speed if game is over
        if (GameManager.Instance != null && GameManager.Instance.gameOver) return;

        // Limit maximum speed
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    void HandleInput()
    {
        // Double check - no input if game is over
        if (GameManager.Instance != null && GameManager.Instance.gameOver) return;

        // Check if starting to charge
        if (Input.GetButtonDown("Fire1"))
        {
            StartCharging();
        }

        // Check if holding to charge
        if (Input.GetButton("Fire1") && isCharging)
        {
            ContinueCharging();
        }

        // Check if releasing to shoot
        if (Input.GetButtonUp("Fire1") && isCharging)
        {
            ReleaseShotgun();
        }
    }

    void HandleAiming()
    {
        // Don't aim if game is over
        if (GameManager.Instance != null && GameManager.Instance.gameOver) return;

        if (useMouseAiming)
        {
            // Aim towards mouse position
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            aimDirection = (mousePos - transform.position).normalized;
        }
        else
        {
            // Aim with WASD or arrow keys
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (horizontal != 0 || vertical != 0)
            {
                aimDirection = new Vector2(horizontal, vertical).normalized;
            }
        }

        // Rotate player to face aim direction
        if (aimDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void StartCharging()
    {
        // Don't charge if game is over
        if (GameManager.Instance != null && GameManager.Instance.gameOver) return;

        isCharging = true;
        chargeTime = 0f;

        // Apply instant recoil movement when starting to charge
        if (aimDirection != Vector2.zero)
        {
            Vector2 recoilDirection = -aimDirection;
            rb.AddForce(recoilDirection * (recoilForce * 0.5f), ForceMode2D.Impulse);
        }

        // Show aim line immediately
        if (aimLine != null)
        {
            aimLine.enabled = true;
        }
    }

    void ContinueCharging()
    {
        // Don't continue charging if game is over
        if (GameManager.Instance != null && GameManager.Instance.gameOver)
        {
            isCharging = false;
            return;
        }

        chargeTime += Time.deltaTime;
        chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);
    }

    void ReleaseShotgun()
    {
        // Don't shoot if game is over
        if (GameManager.Instance != null && GameManager.Instance.gameOver)
        {
            isCharging = false;
            return;
        }

        isCharging = false;

        // Hide visual effects
        if (aimLine != null)
        {
            aimLine.enabled = false;
        }
        if (chargeCircle != null)
        {
            chargeCircle.gameObject.SetActive(false);
        }

        // Fire based on charge level
        if (chargeTime >= 0.1f) // Minimum charge time to prevent accidental shots
        {
            FireChargedShotgun();
        }

        chargeTime = 0f;
    }

    void FireChargedShotgun()
    {
        // Don't fire if game is over
        if (GameManager.Instance != null && GameManager.Instance.gameOver) return;
        if (aimDirection == Vector2.zero) return;

        // Calculate charge multiplier (0 to 1)
        float chargeMultiplier = chargeTime / maxChargeTime;

        // Calculate shot properties based on charge
        float currentRecoilForce = recoilForce + (chargedRecoilMultiplier * recoilForce * chargeMultiplier);
        int currentBulletCount = Mathf.RoundToInt(Mathf.Lerp(bulletsPerShot, chargedBulletsPerShot, chargeMultiplier));
        float currentSpread = Mathf.Lerp(spreadAngle, chargedSpreadAngle, chargeMultiplier);
        float currentBulletSpeed = Mathf.Lerp(bulletSpeed, chargedBulletSpeed, chargeMultiplier);

        // Apply recoil force (opposite to aim direction)
        Vector2 recoilDirection = -aimDirection;
        rb.AddForce(recoilDirection * currentRecoilForce, ForceMode2D.Impulse);

        // Spawn bullets
        for (int i = 0; i < currentBulletCount; i++)
        {
            // Calculate spread
            float spread = 0f;
            if (currentBulletCount > 1)
            {
                spread = Mathf.Lerp(-currentSpread / 2f, currentSpread / 2f, (float)i / (currentBulletCount - 1));
            }

            // Create bullet direction with spread
            float bulletAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg + spread;
            Vector2 bulletDirection = new Vector2(
                Mathf.Cos(bulletAngle * Mathf.Deg2Rad),
                Mathf.Sin(bulletAngle * Mathf.Deg2Rad)
            );

            // Spawn bullet
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = bulletDirection * currentBulletSpeed;
            }
        }

        Debug.Log($"Fired shotgun! Charge: {chargeMultiplier:F2}, Bullets: {currentBulletCount}, Recoil: {currentRecoilForce:F1}");
    }

    void SetupVisualEffects()
    {
        // Create aim line if not assigned
        if (aimLine == null)
        {
            GameObject lineObj = new GameObject("AimLine");
            lineObj.transform.SetParent(transform);
            aimLine = lineObj.AddComponent<LineRenderer>();
            aimLine.material = new Material(Shader.Find("Sprites/Default"));
            aimLine.startColor = Color.red;
            aimLine.endColor = Color.red;
            aimLine.startWidth = 0.05f;
            aimLine.endWidth = 0.02f;
            aimLine.positionCount = 2;
            aimLine.sortingOrder = 1; // Set render order to 1
            aimLine.enabled = false;
        }

        // Create charge circle if not assigned - multiple concentric circles like Recoil Gunner
        if (chargeCircle == null)
        {
            GameObject circleObj = new GameObject("ChargeCircle");
            circleObj.transform.SetParent(transform);
            circleObj.transform.localPosition = Vector3.zero;

            // Create multiple circles for the charging effect
            for (int circleIndex = 0; circleIndex < 3; circleIndex++)
            {
                GameObject singleCircle = new GameObject($"Circle_{circleIndex}");
                singleCircle.transform.SetParent(circleObj.transform);
                singleCircle.transform.localPosition = Vector3.zero;

                LineRenderer circleRenderer = singleCircle.AddComponent<LineRenderer>();
                circleRenderer.material = new Material(Shader.Find("Sprites/Default"));
                circleRenderer.sortingOrder = 1; // Set render order to 1

                // Different colors and sizes for each circle
                Color circleColor = Color.Lerp(Color.cyan, Color.white, (float)circleIndex / 3f);
                circleRenderer.startColor = circleColor;
                circleRenderer.endColor = circleColor;

                float width = 0.15f - (circleIndex * 0.03f);
                circleRenderer.startWidth = width;
                circleRenderer.endWidth = width;
                circleRenderer.loop = true;
                circleRenderer.useWorldSpace = false;

                // Create circle points
                int segments = 24;
                circleRenderer.positionCount = segments;
                float baseRadius = 0.8f + (circleIndex * 0.4f);

                for (int i = 0; i < segments; i++)
                {
                    float angle = (float)i / segments * Mathf.PI * 2f;
                    Vector3 pos = new Vector3(Mathf.Cos(angle) * baseRadius, Mathf.Sin(angle) * baseRadius, 0f);
                    circleRenderer.SetPosition(i, pos);
                }
            }

            chargeCircle = circleObj.transform;
            circleObj.SetActive(false);
        }
    }

    void UpdateVisualEffects()
    {
        // Don't update visuals if game is over
        if (GameManager.Instance != null && GameManager.Instance.gameOver) return;

        // Update aim line
        if (aimLine != null && aimLine.enabled && aimDirection != Vector2.zero)
        {
            Vector3 startPos = firePoint.position;
            Vector3 endPos = startPos + (Vector3)aimDirection * aimLineLength;

            aimLine.SetPosition(0, startPos);
            aimLine.SetPosition(1, endPos);
        }

        // Update charge circle - follows player and animates like Recoil Gunner
        if (isCharging && chargeCircle != null)
        {
            // Show circle when charging
            if (!chargeCircle.gameObject.activeInHierarchy)
            {
                chargeCircle.gameObject.SetActive(true);
            }

            // Make circles follow player position (world space)
            chargeCircle.position = transform.position;

            // Calculate charge progress
            float chargeProgress = chargeTime / maxChargeTime;

            // Animate each circle independently like in Recoil Gunner
            for (int i = 0; i < chargeCircle.childCount; i++)
            {
                Transform circle = chargeCircle.GetChild(i);
                LineRenderer renderer = circle.GetComponent<LineRenderer>();

                if (renderer != null)
                {
                    // Pulsing scale effect
                    float pulseSpeed = 3f + (i * 2f);
                    float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.2f;
                    float finalScale = (0.3f + chargeProgress * 1.2f) * pulse;
                    circle.localScale = Vector3.one * finalScale;

                    // Rotating circles in different directions
                    float rotationSpeed = 90f + (i * 45f);
                    if (i % 2 == 0) rotationSpeed *= -1; // Alternate rotation direction
                    circle.Rotate(0, 0, rotationSpeed * Time.deltaTime);

                    // Color intensity changes with charge
                    Color baseColor = i == 0 ? Color.cyan : (i == 1 ? Color.white : Color.yellow);
                    Color chargedColor = Color.Lerp(baseColor, Color.red, chargeProgress);
                    chargedColor.a = 0.6f + (chargeProgress * 0.4f);

                    renderer.startColor = chargedColor;
                    renderer.endColor = chargedColor;

                    // Width grows with charge
                    float width = (0.1f + chargeProgress * 0.1f) * (1f + pulse * 0.3f);
                    renderer.startWidth = width;
                    renderer.endWidth = width;
                }
            }
        }
    }
}