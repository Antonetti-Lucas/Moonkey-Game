using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioCheck : MonoBehaviour
{
    public AudioSource myAudioSource;

    public GameObject player;

    private bool check = true;

    void Update()
    {
        if (!myAudioSource.isPlaying && check)
        {
            check = false;
            player.SetActive(true); 
            StartCoroutine(endRoutine());
        }
    }

    IEnumerator endRoutine()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("Main Menu");
    }
}