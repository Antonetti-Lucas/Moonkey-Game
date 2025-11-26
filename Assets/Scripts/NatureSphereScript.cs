using UnityEngine;

public class NatureSphereScript : MonoBehaviour
{
    public Transform Sphere;

    public GameManagerTower Manager;

    private string way;

    public Animator animator;

    private void OnTriggerEnter(Collider other)
    {

        if(other.CompareTag("Player")) //Verifica se foi o player que colidiu
        {
            way = gameObject.tag; //Verifica qual orb foi coletada

            Destroy(gameObject);

            Manager.CollectSphere(way);
        }
    }

    void Update()
    {
        Sphere.transform.rotation = Camera.main.transform.rotation; //mantém o png sempre olhando para a câmera
    }
}
