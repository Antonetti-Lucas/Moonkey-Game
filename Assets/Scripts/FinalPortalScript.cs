using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalPortalScript : MonoBehaviour
{
    public float time;

    private float inf = float.PositiveInfinity; //para retornar infinito caso não haja registros

    private bool issaved = false;

    public GameManagerTower GameManagerTower;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Spin") && !issaved)
        {
            time = GameManagerTower.getTime(); // pega o tempo exato que o personagem terminou o jogo

            //Lógica de organização para o top 3 melhores tempos
            if(time < PlayerPrefs.GetFloat("Top1",inf))
            {
                PlayerPrefs.SetFloat("Top3", PlayerPrefs.GetFloat("Top2"));
                PlayerPrefs.SetFloat("Top2", PlayerPrefs.GetFloat("Top1"));
                PlayerPrefs.SetFloat("Top1", time);
            }
            else if(time < PlayerPrefs.GetFloat("Top2",inf))
            {
                PlayerPrefs.SetFloat("Top3", PlayerPrefs.GetFloat("Top2"));
                PlayerPrefs.SetFloat("Top2", time);
            }
            else if(time < PlayerPrefs.GetFloat("Top3",inf))
            {
                PlayerPrefs.SetFloat("Top3", time);
            }

            PlayerPrefs.Save(); //Salva o tempo feito nas preferences da unity
            issaved = true; //Para não salvar várias vezes o mesmo tempo
            SceneManager.LoadScene("FinalScene");
        }
    }
}
