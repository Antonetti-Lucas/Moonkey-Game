using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    public Text scoreTxt;

    private float inf = float.PositiveInfinity;

    public void Start()
    {
        if (!PlayerPrefs.HasKey("Top1"))
        {
            PlayerPrefs.SetFloat("Top1", inf);
        }

        if (!PlayerPrefs.HasKey("Top2"))
        {
            PlayerPrefs.SetFloat("Top2", inf);
        }

        if (!PlayerPrefs.HasKey("Top3"))
        {
            PlayerPrefs.SetFloat("Top3", inf);
        }

        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        UnityEngine.Cursor.visible = true;
    }

    public void PlayGame()
    {
        StartCoroutine(playRoutine());
    }
    IEnumerator playRoutine()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("IntroScene");   //Toca a custcene inicial do jogo
    }

    public void getScore()
    {
        float t1, t2, t3;
        t1 = PlayerPrefs.GetFloat("Top1");
        t2 = PlayerPrefs.GetFloat("Top2");
        t3 = PlayerPrefs.GetFloat("Top3");

        int minutes1 = Mathf.FloorToInt(t1 / 60);
        int seconds1 = Mathf.FloorToInt(t1 % 60);

        int minutes2 = Mathf.FloorToInt(t2 / 60);
        int seconds2 = Mathf.FloorToInt(t2 % 60);

        int minutes3 = Mathf.FloorToInt(t3 / 60);
        int seconds3 = Mathf.FloorToInt(t3 % 60);

        string top1, top2, top3;

        if (t1 != inf)
        {
            top1 = string.Format("{0:00}:{1:00}", minutes1, seconds1);
        }
        else
        {
            top1 = "-- : --";
        }

        if (t2 != inf)
        {
            top2 = string.Format("{0:00}:{1:00}", minutes2, seconds2);
        }
        else
        {
            top2 = "-- : --";
        }

        if (t3 != inf)
        {
            top3 = string.Format("{0:00}:{1:00}", minutes3, seconds3);
        }
        else
        {
            top3 = "-- : --";
        }

        scoreTxt.text = "Top1 - " + top1 + '\n' + "Top2 - " + top2 + '\n' + "Top3 - " + top3;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
