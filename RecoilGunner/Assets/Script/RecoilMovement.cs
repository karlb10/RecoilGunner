using UnityEngine;

public class RecoilMovement : MonoBehaviour
{
    [Header("Recoil Settings")]
    public float recoilForce = 10f;            // push strength
    public float projectileSpeed = 15f;        // how fast trash travels
    public GameObject projectilePrefab;        // assign prefab
    public Transform shootPoint;               // where projectile spawns

    private Rigidbody2D rb;
    private Camera cam;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // Left-click or tap
        {
            ShootTowardCursor();
        }
    }

    void ShootTowardCursor()
    {
        // Get mouse position in world space
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // Calculate direction
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        // Spawn projectile
        if (projectilePrefab != null && shootPoint != null)
        {
            GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
            Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
            projRb.linearVelocity = direction * projectileSpeed;
            Destroy(proj, 2f); // clean up after 2 seconds
        }

        // Apply recoil (opposite of shooting direction)
        rb.AddForce(-direction * recoilForce, ForceMode2D.Impulse);
    }
}
