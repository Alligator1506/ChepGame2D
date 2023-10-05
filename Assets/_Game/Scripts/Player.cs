using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

public class Player : Character
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float speed = 5;

    [SerializeField] private float jumpForce = 8;

    [SerializeField] private Kunai kunaiPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private GameObject attackArea;

    private bool isGrounded = true;
    private bool isJumping = false;
    private bool isAttack = false;
    private bool isDeath = false;

    private float horizontal;

    int numberOfJumps = 0;

    private int coin = 0;

    private Vector3 savePoint;

    private void Awake()
    {
        coin = PlayerPrefs.GetInt("coin", 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDead) return;

        isGrounded = CheckGrounded();

        //-1 -> 0 -> 1
        //horizontal = Input.GetAxisRaw("Horizontal");
        //vertical = Input.GetAxisRaw("Vertical");
        
        if (isAttack)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (isGrounded)
        {
            if (isJumping)
            {
                return;
            }

            //jump
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                Jump();
            }
            
            //change anim run
            if (Mathf.Abs(horizontal) > 0.1f)
            {
                ChangeAnim("run");
            }

            //attack
            if (Input.GetKeyDown(KeyCode.C) && isGrounded)
            {
                Attack();
            }
            //throw
            if (Input.GetKeyDown(KeyCode.V) && isGrounded)
            {
                Throw();
            }
            
        }
        //check falling
        if (!isGrounded && rb.velocity.y < 0)
        {
            ChangeAnim("fall");
            isJumping = false;
        }

        //Moving
        if (Mathf.Abs(horizontal) > 0.1f) 
        {
            rb.velocity = new Vector2 (horizontal * speed, rb.velocity.y);
            transform.rotation = Quaternion.Euler(new Vector3(0, horizontal > 0 ? 0 : 180, 0));
            //transform.localScale = new Vector3(horizontal, 1, 1);
        } 
        else if (isGrounded)
        {
            ChangeAnim("idle");
            rb.velocity = Vector2.zero;
        }
    }

    public override void OnInit()
    {
        base.OnInit();
        isAttack = false;

        transform.position = savePoint;
        ChangeAnim("idle");
        DeActiveAttack();

        SavePoint();
        UIManager.instance.SetCoin(coin);
    }

    public override void OnDespawn()
    {
        base.OnDespawn();

        OnInit();
    }

    protected override void OnDeath()
    {
        base.OnDeath();
    }

    private bool CheckGrounded()
    {
        Debug.DrawLine(transform.position, transform.position + Vector3.down * 1.1f, Color.red);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, groundLayer);

        return hit.collider != null;
    }

    public void Attack()
    {
        ChangeAnim("attack");
        isAttack = true;
        Invoke(nameof(ResetAttack), 0.5f);
        ActiveAttack();
        Invoke(nameof(DeActiveAttack), 0.5f);
    }

    public void Throw()
    {   
        ChangeAnim("throw");
        isAttack = true;
        Invoke(nameof(ResetAttack), 0.5f);

        Instantiate(kunaiPrefab, throwPoint.position, throwPoint.rotation);
    }

    private void ResetAttack()
    {
        isAttack = false;
        ChangeAnim("idle");
    }

    public void Jump()
    {

        if (isGrounded)
        {
            numberOfJumps = 0;
            isJumping = true;
            ChangeAnim("jump");
            //rb.AddForce(jumpForce * Vector2.up);
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            numberOfJumps++;
        }
        else
        {
            if (numberOfJumps == 1)
            {
                isJumping = true;
                ChangeAnim("jump");
                //rb.AddForce(jumpForce * Vector2.up);
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                numberOfJumps++;
            }
        }
    }
   
    internal void SavePoint()
    {
        savePoint = transform.position;
    }

    private void ActiveAttack()
    {
        attackArea.SetActive(true);
    }

    private void DeActiveAttack()
    {
        attackArea.SetActive(false);
    }

    public void SetMove(float horizontal)
    {
        this.horizontal = horizontal;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Coin")
        {
            coin++;
            PlayerPrefs.SetInt("coin", coin);
            UIManager.instance.SetCoin(coin);
            Destroy(collision.gameObject);
        }
        if (collision.tag == "DeathZone")
        {
            ChangeAnim("die");
            Invoke(nameof(OnInit), 1f);
        }
    }

    //public void AttackTouch()
    //{
    //    ChangeAnim("attack");
    //    isAttack = true;
    //    Invoke(nameof(ResetAttack), 0.5f);
    //    ActiveAttack();
    //    Invoke(nameof(DeActiveAttack), 0.5f);
    //}

    //public void ThrowTouch()
    //{
    //    ChangeAnim("throw");
    //    isAttack = true;
    //    Invoke(nameof(ResetAttack), 0.5f);

    //    Instantiate(kunaiPrefab, throwPoint.position, throwPoint.rotation);
    //}
    //public void JumpTouch()
    //{
    //    if (isGrounded)
    //    {
    //        isJumping = true;
    //        ChangeAnim("jump");
    //        rb.AddForce(jumpForce * Vector2.up);
    //    }
    //}
}
