using UnityEngine;

public class TestEnemy : Enemy
{
    [Header("Chase Settings")]
    [SerializeField] private float detectionRange = 30f;
    [SerializeField] private float stoppingDistance = 1.2f;

    protected override void Awake()
    {
        base.Awake();
        if (agent != null)
        {
            agent.stoppingDistance = stoppingDistance;
        }
    }

    protected override void AI()
    {
        if (agent == null || player == null)
        {
            return;
        }

        float sqrDist = (player.transform.position - transform.position).sqrMagnitude;
        if (sqrDist <= detectionRange * detectionRange)
        {
            agent.SetDestination(player.transform.position);
        }
    }
}
