using UnityEngine;
using UnityEngine.AI;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshObstacle))]
public class KaijuAI : MonoBehaviour
{
    [Header("Kaiju Settings")]
    public float detectionRadius = 15f;
    public float attackRange = 3f;
    public float moveSpeed = 2.5f;
    public float rotationSpeed = 2f;
    public float stoppingDistance = 6f;
    public LayerMask unitLayer;

    private Transform currentTarget;
    private Rigidbody rb;
    private NavMeshObstacle obstacle;

    private void Awake()
    {
        InitializeComponents();
        ConfigureRigidbody();
        ConfigureNavMeshObstacle();
    }

    private void FixedUpdate()
    {
        DetectNearestUnit();
        if (currentTarget == null) return;

        Vector3 currentPos = transform.position;
        Vector3 targetPos = new Vector3(currentTarget.position.x, currentPos.y, currentTarget.position.z);
        Vector3 direction = (targetPos - currentPos).normalized;
        float distance = Vector3.Distance(currentPos, targetPos);

        RotateTowards(direction);

        if (distance > attackRange)
        {
            MoveTowards(direction, distance);
        }
        else
        {
            StopAndAttack();
        }
    }

    /// <summary>
    /// Tries to get required components or adds them if missing.
    /// </summary>
    private void InitializeComponents()
    {
        if (!TryGetComponent(out rb))
            rb = gameObject.AddComponent<Rigidbody>();

        if (!TryGetComponent(out obstacle))
            obstacle = gameObject.AddComponent<NavMeshObstacle>();
    }

    /// <summary>
    /// Configures the rigidbody physics parameters.
    /// </summary>
    private void ConfigureRigidbody()
    {
        rb.mass = 999f;
        rb.linearDamping = 5f; // Controls drag when moving
        rb.angularDamping = 0.05f; // Controls drag when rotating
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    /// <summary>
    /// Configures the NavMeshObstacle to interact with pathfinding.
    /// </summary>
    private void ConfigureNavMeshObstacle()
    {
        obstacle.carving = false;
        obstacle.carveOnlyStationary = true;
        obstacle.enabled = true;
    }

    /// <summary>
    /// Finds the nearest unit in range based on layer mask.
    /// </summary>
    private void DetectNearestUnit()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, unitLayer);
        currentTarget = hits
            .Select(h => h.transform)
            .OrderBy(h => Vector3.Distance(transform.position, h.position))
            .FirstOrDefault();
    }

    /// <summary>
    /// Rotates the Kaiju toward the given direction smoothly.
    /// </summary>
    private void RotateTowards(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Moves the Kaiju toward the target with speed interpolation.
    /// </summary>
    private void MoveTowards(Vector3 direction, float distance)
    {
        float t = Mathf.InverseLerp(attackRange, stoppingDistance, distance);
        float speed = Mathf.Lerp(0f, moveSpeed, t);

        Vector3 movement = direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        obstacle.carving = false; // Disables carving while moving
    }

    /// <summary>
    /// Stops the Kaiju movement and initiates attack behavior.
    /// </summary>
    private void StopAndAttack()
    {
        rb.linearVelocity = Vector3.zero; // Stops movement completely
        obstacle.carving = true; // Allows carving the nav mesh while stationary
        Attack();
    }

    /// <summary>
    /// Attack logic for the Kaiju.
    /// </summary>
    private void Attack()
    {
        Debug.Log("Kaiju Attack!");
        // TODO: Add actual attack logic (damage, animation, etc.)
    }

    /// <summary>
    /// Draws detection and attack radii for debugging in the editor.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}


