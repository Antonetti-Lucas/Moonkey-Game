using UnityEngine;

public class WayVisibilityScript : MonoBehaviour
{
    public GameObject objectsToToggle;

    private void Start()
    {
        if (objectsToToggle != null)
        {
            objectsToToggle.SetActive(false);   //Inicia com as fases desativadas para não vermos de fora
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (objectsToToggle != null)
            {
                objectsToToggle.SetActive(true); // Ativa os objetos
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Verificamos se quem saiu é o Player
        if (other.CompareTag("Player"))
        {
            if (objectsToToggle != null)
            {
                objectsToToggle.SetActive(false); // Desativa os objetos
            }
        }
    }
}
