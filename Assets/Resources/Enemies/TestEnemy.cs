using UnityEngine;
using UnityEngine.AI;

public class TestEnemy : Enemy
{
    protected override void AI()
    {
        agent.SetDestination(player.transform.position);
    }
}
