using UnityEngine;
using System.Collections;

public class platform : MonoBehaviour {

    // i dno what im doin lol

    private Player player;

    void Start()
    {
        player = gameObject.GetComponentInParent<Player>();

    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.isTrigger && player.rb2d.velocity.y < 0 && player.ignorePlatforms == false)
        {
            //Debug.Log("collisionscript1");
            player.grounded = true;
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (!col.isTrigger)
        {
            // Debug.Log("collisionscript2");
            player.grounded = true;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        //Debug.Log("collisionscript3");
        player.grounded = false;
    }






}
