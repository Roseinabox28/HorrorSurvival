using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectEntity : MonoBehaviour
{

    Player player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
     {

        
        player.isEntityInWay = true;
        
     }

     void OnTriggerExit(Collider other)
     {
        
        player.isEntityInWay = false;
     }
}
