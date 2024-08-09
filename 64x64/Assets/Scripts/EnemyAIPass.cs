using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIPass : MonoBehaviour
{
    public void Attack()
    {
        this.gameObject.transform.parent.gameObject.GetComponent<EnemyAI>().StartAttack();
    }

    public void Projectile()
    {
        this.gameObject.transform.parent.gameObject.GetComponent<EnemyAI>().ShootAtPlayer();
    }
}
