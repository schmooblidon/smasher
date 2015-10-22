using UnityEngine;
using System.Collections;

public class CollisionProjection : MonoBehaviour {

    // For the Projected ECB. Plats are kinda broken atm

    private Player player;
    public GameObject plat;

    void Start()
    {
        player = gameObject.GetComponentInParent<Player>();
    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.isTrigger)
        {

            if (col.CompareTag("plat"))
            {
                player.platCollide = true;
            }
            else
            {
                player.platCollide = false;
            }
                player.grounded = true;
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (!col.isTrigger)
        {
            player.grounded = true;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        player.grounded = false;
        player.platCollide = false;
    }

}
