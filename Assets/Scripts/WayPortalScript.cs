using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Audio;

public class WayPortalScript : MonoBehaviour
{
    private Transform playerPai;
    public CinemachineCamera camVirtual;
    public playerController playerController;
    public AudioSource audioSource;

    private void OnTriggerStay(Collider other)
    {
        playerPai = other.transform.root;   //Seleciona tudo que faz parte do jogador
        if(other.CompareTag("Player") && other.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Spin"))
        {
            if (audioSource != null)
            {
                audioSource.Play();
            }
            Vector3 destino = playerController.respawn.position;
            playerPai.transform.position = destino; //Teleporta o jogador de volta para o início da fase
            camVirtual.PreviousStateIsValid = false;
        }
    }

}
