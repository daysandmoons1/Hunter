using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSlime : MonoBehaviour
{
    Transform tr;
    Rigidbody2D rigid;
    SpriteRenderer sr;
    Animator anim;
    CapsuleCollider2D collider;

    private int nextMove;
    private float moveSpeed = 5.0f;
    private Vector2 boxCastSize = new Vector2(1.0f, 0.5f);

    private bool jumping = false, falling = false;
    private float curPos, lastPos;

    void Awake()
    {
        tr = GetComponent<Transform>();
        rigid = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        collider = GetComponent<CapsuleCollider2D>();

        NextMove();
        StartCoroutine("Jump");
    }

    void Update()
    {
        Move();
        DetectPlayer();
    }

    private void Move()
    {
        curPos = tr.position.y;

        // nextMove�� flip ����ߵ�
        if(nextMove == 1)
            sr.flipX = true;
        else
            sr.flipX = false;

        // move
        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);
        
        // ���� ����, �ڽ� ũ��, �ڽ� ����, �ڽ� ����, �ڽ� �ִ� �Ÿ�, ����
        RaycastHit2D raycastHit = Physics2D.BoxCast(tr.position, boxCastSize, 0f, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
        
        // �ϰ�
        if(raycastHit.collider == null && curPos < lastPos && jumping == true)
        {
            jumping = false;
            falling = true;
            anim.SetBool("jumping", false);

            // ���� ����
            // nextMove != 1 ����
            sr.flipX = nextMove != 1;
            anim.SetBool("falling", true);
            //Debug.Log("�ϰ�");
        }

        // ����
        else if (raycastHit.collider != null && falling == true)
        {
            falling = false;
            anim.SetBool("falling", false);
            anim.SetBool("landing", true);
            
            // 1�� ���� �� Idle �ִϸ��̼����� ����
            CancelInvoke();
            nextMove = 0;
            Invoke("SetIdle", 0.5f);
            Invoke("NextMove", 0.5f);

            //Debug.Log("����");
        }

        lastPos = curPos;
    }

    private void DetectPlayer()
    {
        // �ݰ� ���� �ȿ� �÷��̾� ������ �����ϸ� ����
    }

    private void NextMove()
    {
        //set Next Active
        nextMove  = Random.Range(-1, 2);

        //Recursive
        float nextChangingTime = Random.Range(1f, 3f);
        Invoke("NextMove", nextChangingTime);
    }

    private void SetIdle()
    {
        anim.SetBool("jumping", false);
        anim.SetBool("landing", false);
        anim.SetBool("falling", false);
        //Debug.Log("SetIdle");
    }

    private void JumpAnim()
    {
    }

    IEnumerator Jump()
    {
        WaitForSeconds wait = new WaitForSeconds(5f);
        while(true)
        {
            yield return wait;
            jumping = true;
            anim.SetBool("jumping", true);
            CancelInvoke();
            rigid.AddForce(new Vector3(nextMove, 1, 0) * 300);//Vector2.up
            rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

            //Debug.Log("����");
        }
    }
}