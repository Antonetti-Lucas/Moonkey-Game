using UnityEngine;

public class FinalCameraScript : MonoBehaviour
{
    public float velocidade = 1f;
    public Transform destinoTransform;
    private Vector3 destino;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        destino = destinoTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //Move a câmera lentamente para trás
        transform.position = Vector3.MoveTowards(transform.position, destino, velocidade * Time.deltaTime);
    }
}
