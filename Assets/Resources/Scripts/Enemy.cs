using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float speed;
    [SerializeField] protected float attack;
    protected GameObject player;
    protected NavMeshAgent agent;

    public float curHealth;

    protected virtual void Awake()
    {
        curHealth = health;
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
    }

    protected virtual void Update()
    {
        AI();
    }

    protected abstract void AI();

    protected virtual void TakeDamage(float damage)
    {
        curHealth -= damage;

        if (curHealth <=0)
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
