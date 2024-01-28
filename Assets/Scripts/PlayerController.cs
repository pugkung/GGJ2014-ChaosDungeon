using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    private const float ATTACK_TIME = 0.4f;
    private const int MAX_HP = 100;

    public int hp;
    public float jumpForce;
    public float moveSpeed;
    public float damageCooldown;
    public GameObject playerAttack;
    public float baseAttack;
    public float attackDmgVariance;
    public AudioClip attackSE;

    public GameObject hpBar;

    private AudioSource audioSource;
    private Rigidbody2D rigidBody;
    private float inputX;

    private Animator anim;
    private Slider hpBarSlider;
    private TMPro.TextMeshProUGUI hpDisplayValue;
    private bool playerIsDeath = false;
    private bool isFacingRight = true;
    private bool isJumping = true;
    private bool isAttacking = false;
    private bool isTakingDamage = false;
    
    void Start()
    {
        hp = MAX_HP;
        
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        hpBarSlider = hpBar.GetComponent<Slider>();
        hpDisplayValue = hpBar.GetComponentInChildren<TMPro.TextMeshProUGUI>();
    }

    void Update()
    {
        if (!playerIsDeath)
        {
            getControllerInput();
            updateAnimation();
            updateHUD();
            
            if (hp <= 0)
            {
                hp = 0;
                playerIsDeath = true;
            }
        }
    }

    private void getControllerInput()
    {
        inputX = Input.GetAxis("Horizontal");
        rigidBody.position = new Vector2(rigidBody.position.x + (inputX * moveSpeed), rigidBody.position.y);

        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            isJumping = true;
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y + jumpForce);
        }

        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            // reject spam input
            StartCoroutine(performAttack(ATTACK_TIME));
        }
    }

    private void updateHUD()
    {
        hpBarSlider.value = (1.0f * hp) / MAX_HP;
        hpDisplayValue.text = hp + " / " + MAX_HP;
    }

    private void updateAnimation()
    {
        anim.SetBool("isRunning", inputX != 0);
        if (inputX < 0 && isFacingRight)
        {
            isFacingRight = false;
            flipFacingDirection();
        }
        else if (inputX > 0 && !isFacingRight)
        {
            isFacingRight = true;
            flipFacingDirection();
        }
    }

    private void flipFacingDirection()
    {
        gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * -1,
            gameObject.transform.localScale.y, gameObject.transform.localScale.z);
    }
    
    IEnumerator performAttack(float waitTime)
    {
        isAttacking = true;
        audioSource.PlayOneShot(attackSE);
        anim.SetBool("isAttacking", isAttacking);
        GameObject attack = GameObject.Instantiate(playerAttack, transform.position, Quaternion.identity);
        attack.GetComponent<PlayerAttack>().setAttackProperties(
            baseAttack + Random.Range(-attackDmgVariance, attackDmgVariance), 
            0.1f, 
            0, 
            new Vector3(0,0,0));
        yield return new WaitForSeconds(waitTime);
        
        isAttacking = false;
        anim.SetBool("isAttacking", isAttacking);
    }

    // using Collision2D, not Collision (3D)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            isJumping = false;
        }

        if (!isTakingDamage && collision.gameObject.tag == "Enemy")
        {
            int damage = collision.gameObject.GetComponent <EnemyAIController>().getDamageOnHit();
            hp -= damage;
            StartCoroutine(waitForDamageCooldown(damageCooldown));
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Bullet")
        {
            Destroy(collider.gameObject);

            if (!isTakingDamage)
            {
                hp -= 10;
                StartCoroutine(waitForDamageCooldown(damageCooldown));
            }
        }
    }

    IEnumerator waitForDamageCooldown(float damageCooldownTime)
    {
        isTakingDamage = true;
        yield return new WaitForSeconds(damageCooldownTime);

        isTakingDamage = false;
    }

    public bool isPlayerDeath()
    {
        return playerIsDeath;
    }

    public void addHealth(int value)
    {
        hp += value;
    }

    public void changeSpeed(float duration, float modifier)
    {
        StartCoroutine(changeSpeedForGivenTime(duration, modifier));
    }

    IEnumerator changeSpeedForGivenTime(float duration, float modifier)
    {
        moveSpeed *= modifier;
        Debug.Log("new speed: " + moveSpeed);
        yield return new WaitForSeconds(duration);
        moveSpeed /= modifier;
        Debug.Log("rollback speed: " + moveSpeed);
    }

    public void changeGravity(float duration, float modifier)
    {
        StartCoroutine(changeGravityForGivenTime(duration, modifier));
    }

    IEnumerator changeGravityForGivenTime(float duration, float modifier)
    {
        Physics2D.gravity *= modifier;
        jumpForce *= modifier;
        yield return new WaitForSeconds(duration);
        Physics2D.gravity *= modifier;
        jumpForce *= modifier;
    }
}
