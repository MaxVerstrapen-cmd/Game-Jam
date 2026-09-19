using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{

    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal"); // A = -1, D = +1

        anim.SetBool("IsRunning", x != 0);

        // Flip to face the direction he's moving
        if (x != 0)
        {
            float size = Mathf.Abs(transform.localScale.x);
            transform.localScale = new Vector3(x < 0 ? -size : size, transform.localScale.y, transform.localScale.z);
        }
    }
}
