using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kunai : MonoBehaviour
{
    public GameObject hitVFX;
    public Rigidbody2D rb;

    public LineRenderer lineRenderer;

    public float force = 10f; // Lực bắn
    public float speed = 20f;
    public float angle = 50f; // Góc bay của đạn (độ)
    public float gravity = 9.8f; // Gia tốc trọng trường

    private float radianAngle; // Góc bay của đạn (rad)
    private Vector2 initialVelocity; // Vận tốc ban đầu của đạn
    private float timeInterval = 0.1f;

    public float x;
    public bool lateStart;

    public float horizontal;

    // Start is called before the first frame update
    void Start()
    {
        OnInit();
    }

    void FixedUpdate()
    {
        // Áp dụng gia tốc trọng trường lên đạn
        rb.AddForce(Vector2.down * gravity, ForceMode2D.Force);
    }

    public void OnInit()
    {
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.enabled = true;

        rb = GetComponent<Rigidbody2D>();

        radianAngle = angle * Mathf.Deg2Rad;
        float horizontalVelocity = speed * Mathf.Cos(radianAngle);
        float verticalVelocity = speed * Mathf.Sin(radianAngle);
        initialVelocity = new Vector2(horizontalVelocity, verticalVelocity);

        // Điều chỉnh hướng của velocity tùy thuộc vào góc quay
        if (transform.rotation.y < 0f) // Nếu quay về phía bên phải
        {
            initialVelocity = new Vector2(-initialVelocity.x, initialVelocity.y);
            rb.AddForce(initialVelocity.normalized * force, ForceMode2D.Impulse);
            
        }
        else // Nếu quay về phía bên trái
        {
            // Đảo ngược hướng velocity theo chiều ngang
            rb.AddForce(initialVelocity.normalized * force, ForceMode2D.Impulse);
        }

        StartCoroutine(DrawTrajectory());

        //rb.velocity = transform.right * force;
        Invoke(nameof(OnDespawn), 4f);
    }

    IEnumerator DrawTrajectory()
    {
        Vector2 currentPosition = transform.position;
        float elapsedTime = 0f;

        while (true)
        {
            elapsedTime += timeInterval;
            float x = currentPosition.x + initialVelocity.x * elapsedTime;
            float y = currentPosition.y + initialVelocity.y * elapsedTime - 0.5f * gravity * elapsedTime * elapsedTime;
            Vector2 newPosition = new Vector2(x, y);

            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, newPosition);

            yield return new WaitForSeconds(timeInterval);
        }
    }

    public void OnDespawn()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            collision.GetComponent<Character>().OnHit(30f);
            //Instantiate(hitVFX, transform.position, transform.rotation);
            OnDespawn();
        }
    }
}
