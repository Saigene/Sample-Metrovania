using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _playermovement : MonoBehaviour
{
    public _controller2d controller2d;
    public Animator animator;
    public float _runspeed = 40f;
    float _horizontalspeed = 0f;
    bool _jump = false;
    bool _dash = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _horizontalspeed = Input.GetAxisRaw("Horizontal") * _runspeed;
        animator.SetFloat("speed", Mathf.Abs(_horizontalspeed));
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jump = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _dash = true;
        }
    }

    public void OnFall()
    {
        animator.SetBool("jumpup", true);
   //     animator.SetBool("jumpfalling", false);
    }

    public void OnLanding()
    {
        animator.SetBool("jumpup", false);
  //     animator.SetBool("jumpfalling", true);
    }

    private void FixedUpdate()
    {
        controller2d._move(_horizontalspeed * Time.fixedDeltaTime, _jump, _dash);
        _jump = false;
        _dash = false;
    }
}
