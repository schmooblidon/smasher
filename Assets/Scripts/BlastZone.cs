using UnityEngine;
using System.Collections;

public class BlastZone : MonoBehaviour {

    private Player player;

    void Start()
    {
        player = gameObject.GetComponentInParent<Player>();
    }

    void OnTriggerExit2D(Collider2D col)
    {

        // When exiting collider, and collider is not a trigger, reset level. Once more chars and stuff are in, needs to be changed to remove stock, reset position, or w/e
        if (!col.isTrigger)
        {
            Application.LoadLevel(Application.loadedLevel);
        }
    }
}
