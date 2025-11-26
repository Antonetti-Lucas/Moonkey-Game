using TMPro;
using UnityEngine;

public class GameManagerStarting : MonoBehaviour
{
    public playerController playerController;

    public int NatureSpheres = 0;

    public void CollectSphere()
    {
        //Increase Nature Sphere Count
        NatureSpheres += 1;

        //Play celebration animation
        playerController.animator.SetTrigger("CollectSphere");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
