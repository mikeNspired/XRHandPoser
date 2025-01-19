using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class Zombie : MonoBehaviour, IEnemy
{
    [SerializeField] float maxSpeed = 1f;
    [SerializeField] float timeToMaxSpeed = 2f;
    Animator animator;
    Vector3 destination;
    Transform player;
    float accelerateTimer;
    bool movingToDestination = true;
    private bool isDead;
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Attack = Animator.StringToHash("Attack");

    // Action event for when the zombie dies
    public static event Action<Zombie> OnZombieDied;
    public static event Action OnZombieAttacked;

    void Awake() => animator = GetComponent<Animator>();
    void Start() => player = Camera.main.transform;

    public void Initialize(float speed, float timeToSpeed, Vector3 finalDestination)
    {
        maxSpeed = speed;
        timeToMaxSpeed = timeToSpeed;
        destination = finalDestination;
        accelerateTimer = 0f;
        movingToDestination = true;
        transform.LookAt(finalDestination);
        if (animator) animator.SetFloat(Speed, 0f);
    }

    void Update()
    {
        if (movingToDestination) MoveToDestination();
        else FacePlayer();
    }

    void MoveToDestination()
    {
        accelerateTimer += Time.deltaTime;
        float currentSpeed = Mathf.Lerp(0f, maxSpeed, Mathf.Clamp01(accelerateTimer / timeToMaxSpeed));
        float distance = Vector3.Distance(transform.position, destination);

        Vector3 dir = destination - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);

        if (distance < 1f) currentSpeed = Mathf.Lerp(0f, currentSpeed, distance);
        if (distance < 0.5f)
        {
            currentSpeed = 0f;
            movingToDestination = false;
        }

        if (animator) animator.SetFloat(Speed, currentSpeed);
    }

    void FacePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized; // Direction to player
        dir.y = 0f; // Ignore vertical differences

        if (dir.sqrMagnitude > 0.001f) // Avoid rotating for negligible distances
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 2f);
        }

        float angleToPlayer = Vector3.Angle(transform.forward, dir); // Angle between forward direction and player direction

        if (angleToPlayer < 10f && !isDead) // Check if zombie is within attack angle
        {
            animator.SetTrigger("Attack");
        }
    }

    public void AttackCompleted()
    {
        OnZombieAttacked?.Invoke(); // Trigger the death event
        Debug.Log("Attack animation completed!");
    }

    public void Die()
    {
        OnZombieDied?.Invoke(this); // Trigger the death event

        if (animator)
        {
            int deathType = Random.Range(0, 2); // Randomly choose between 0 (Death1) and 1 (Death2)
            animator.SetTrigger(deathType == 0 ? "Death1" : "Death2");
        }

        Destroy(gameObject, 2f); // Allow the animation to play before destruction
    }

}

public interface IEnemy
{
    void Die();
}
