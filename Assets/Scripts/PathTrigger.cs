using UnityEngine;

public class PathTrigger : MonoBehaviour
{
    public GameObject objectsToToggle;

    public playerController playerController;

    public Transform respawn;

    private bool isInside = false;

    private void Start()
    {
        if (objectsToToggle != null)
        {
            objectsToToggle.SetActive(false); //Faz as fases desaparecerem para não serem vistas de fora
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Verifica se o jogador passou pelo corredor e se sim faz aparecer a segunda metade do corredor
        if (other.CompareTag("Player"))
        {
            if (objectsToToggle != null)
            {
                isInside = !isInside;   //Verifica o sentido que o jogador está indo para ativar e desativar os objetos
                objectsToToggle.SetActive(isInside); 
            }

            playerController.respawn = respawn; //Marca onde o jogador deve renascer se cair
        }
    }

}
