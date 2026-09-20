using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxScript : MonoBehaviour
{
    private playerMovement owner;

    void Awake()
    {
        owner = GetComponentInParent<playerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner == null)
        {
            return;
        }

        playerMovement otherPlayer =
            other.GetComponentInParent<playerMovement>();

        //the floor, the walls and the arena tilemap all overlap this trigger,
        //and none of them have a playerMovement to damage
        if (otherPlayer == null)
        {
            return;
        }

        //our own body collider / our own hitbox
        if (otherPlayer == owner)
        {
            return;
        }

        owner.Hitbox(otherPlayer);
    }
}
