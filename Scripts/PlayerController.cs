using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun
{
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed;
    public int gold;
    public int curHp;
    public int maxHp;
    public bool dead;

    [Header("Attack")]
    public int damage;
    public float attackRange;
    public float attackRate;
    private float lastAttackTime;

    [Header("Components")]
    public Rigidbody2D rig;
    public Player photonPlayer;
    public SpriteRenderer sr;
    public Animator weaponAnim;

    // Local Player
    public static PlayerController me;

    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;
        
        // Initialize the health bar
        if (player.IsLocal)
            me = this;
        else
            rig.isKinematic = true;

        GameManager.instance.players[id - 1] = this;
    }

    void Update()
    {
        if (!photonView.IsMine)
            return;

        Move();

        if (Input.GetMouseButtonDown(0) && Time.time - lastAttackTime > attackRate)
            Attack();

        float mouseX = (Screen.width / 2) - Input.mousePosition.x;

        if (mouseX < 0) // If mouse is left of character
            weaponAnim.transform.parent.localScale = new Vector3(1, 1, 1);
        else // If mouse is right of character
            weaponAnim.transform.parent.localScale = new Vector3(-1, 1, 1);
    }

    void Move()
    {
        // Get the horizontal and vertical inputs
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        // Apply that to our velocity
        rig.velocity = new Vector2(x, y) * moveSpeed;
    }

    // Melee attacks towards the mouse
    void Attack()
    {
        lastAttackTime = Time.time;
        
        // Calculate the direction
        Vector3 dir = (Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)).normalized;
        
        // Shoot a raycast in the direction
        RaycastHit2D hit = Physics2D.Raycast(transform.position + dir, dir, attackRange);
        
        // Did we hit an enemy?
        if (hit.collider != null && hit.collider.gameObject.CompareTag("Enemy"))
        {
            // Get the enemy and damage them
        }
        
        // Play attack animation
        weaponAnim.SetTrigger("Attack");
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        curHp -= damage;
        
        // Update the health bar
        if (curHp <= 0)
            Die();
        else
        {
            StartCoroutine(DamageFlash());
            IEnumerator DamageFlash()
            {
                sr.color = Color.red;
                yield return new WaitForSeconds(0.05f);
                sr.color = Color.white;
            }
        }
    }

    void Die()
    {
        dead = true;
        rig.isKinematic = true;
        
        transform.position = new Vector3(0, 99, 0);
        Vector3 spawnPos = GameManager.instance.spawnPoints[Random.Range(0, GameManager.instance.spawnPoints.Length)].position;
        
        StartCoroutine(Spawn(spawnPos, GameManager.instance.respawnTime));
    }
    
    IEnumerator Spawn(Vector3 spawnPos, float timeToSpawn)
    {
        yield return new WaitForSeconds(timeToSpawn);
        
        dead = false;
        transform.position = spawnPos;
        
        curHp = maxHp;
        rig.isKinematic = false;
        
        // Update the health bar
    }

    [PunRPC]
    void Heal(int amountToHeal)
    {
        curHp = Mathf.Clamp(curHp + amountToHeal, 0, maxHp);

        // Update the health bar
    }

    [PunRPC]
    void GiveGold(int goldToGive)
    {
        gold += goldToGive;

        // Update the UI
    }
}
