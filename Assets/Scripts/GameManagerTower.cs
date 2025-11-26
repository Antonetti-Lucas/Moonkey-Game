using TMPro;
using UnityEngine;

public class GameManagerTower : MonoBehaviour
{
    public playerController playerController;

    public int NatureSpheres = 0;

    public GameObject earthPortal;
    public GameObject waterPortal;
    public GameObject airPortal;
    public GameObject endPortal;

    private int minutes;
    private int seconds;
    public TextMeshProUGUI timerText;

    private float elapsedTime=0;

    public void CollectSphere(string way)
    {
        //Verifica qual orb foi coletada para ativar o portal certo
        if(way == "Earth")
        {
            earthPortal.SetActive(true);
        }
        else if(way == "Water")
        {
            waterPortal.SetActive(true);
        }
        else if(way == "Air")
        {
            airPortal.SetActive(true);
        }

        //Aumenta o número de orbs coletados
        NatureSpheres += 1;

        //Animação de comemoração
        playerController.animator.SetTrigger("CollectSphere");
    }

    public float getTime()
    {
        return elapsedTime;
    }

    void Update()
    {
        if (NatureSpheres >= 3 && endPortal != null) //Caso pegue 3 orbs o portal para o fim do jogo é ativado
        {
            endPortal.SetActive(true);
        }

        //Separa o tempo em minutos e segundos
        if (playerController.notPaused == 1)
        {
            elapsedTime += Time.deltaTime;
            minutes = Mathf.FloorToInt(elapsedTime / 60);
            seconds = Mathf.FloorToInt(elapsedTime % 60);
        }

        timerText.text = string.Format("{0:00}:{1:00}",minutes,seconds); //display do timer na tela
    }
}
