using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected float health = 100f;
    [SerializeField] protected float speed = 3.5f;
    [SerializeField] protected float attack = 10f;
    [SerializeField] public float curHealth;

    [Header("Target")]
    [SerializeField] protected string playerTag = "Player";

    protected GameObject player;
    protected NavMeshAgent agent;

    protected virtual void Awake()
    {
        curHealth = health;
        player = GameObject.FindGameObjectWithTag(playerTag);
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = speed;
        }
    }

    protected virtual void Update()
    {
        AI();
    }

    protected abstract void AI();

    protected virtual void TakeDamage(float damage)
    {
        curHealth -= damage;

        if (curHealth <= 0f)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter()
    {
    }
}
