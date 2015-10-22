using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Displaystuff : MonoBehaviour {

    // displays the action states in real time

    public Text actstate;


    // function is used at end of the player script
    public void Displayvariables (string type, string value)
    {
        if (type == "as1")
        {
            actstate.text = value;
        }
        else if (type == "as2")
        {
            actstate.text = value;
        }
        else if (type == "asn1")
        {
            actstate.text = value;
        }
        else if (type == "asn2")
        {
            actstate.text = value;
        }
    }
}
