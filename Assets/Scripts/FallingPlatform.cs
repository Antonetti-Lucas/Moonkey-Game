using System;
using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    private float fallDelay = 0.3f; //tempo entre a plataforma tremer e cair
    private float returnDelay = 5f; //tempo para ela voltar onde estava

    private float shakeTime = 2f; 
    public float shakeAmount = 1f;

    private bool isFalling = false;

    private Vector3 startPos;

    [SerializeField] private Rigidbody rb;

    private void Start()
    {
        startPos = transform.position;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isFalling)
        {
            StartCoroutine(Fall());
        }
    }

    private IEnumerator Fall()
    {
        isFalling = true;

        float timer = 0;
        while (timer < shakeTime)
        {
            //Teleporta a plataforma para pontos aleatórios em uma área para simular tremor
            Vector3 randomPoint = startPos + UnityEngine.Random.insideUnitSphere * shakeAmount; 

            transform.position = new Vector3(randomPoint.x, startPos.y, randomPoint.z);


            timer += Time.deltaTime;  
            yield return null;
        }
        yield return new WaitForSeconds(fallDelay);

        transform.position = startPos;

        //Ativa a gravidade da plataforma
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        yield return new WaitForSeconds(returnDelay);

        //Desativa a gravidade da plataforma
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;


        rb.isKinematic = true;
        rb.useGravity = false; 


        rb.MovePosition(startPos);


        isFalling = false;
    }
}
