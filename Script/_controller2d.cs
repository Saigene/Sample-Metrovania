using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class _controller2d : MonoBehaviour
{

    [SerializeField] private float _mjumpforce = 400f;
    [Range(0, .3f)] [SerializeField] private float _msmoothmovement = .05f;
    [SerializeField] private bool _maircontrol = false;
    [SerializeField] private LayerMask _mwhatisground;
    [SerializeField] private Transform _mgroundcheck;
    [SerializeField] private Transform _mwallcheck;

    const float _kgroundedradius = .2f;
    private bool _mgrounded;
    private Rigidbody2D _mrigidbody2d;
    private bool _mfacingright = true;
    private Vector3 velocity = Vector3.zero;
    private float limitfallspeed = 25f;

    public bool _candoublejump = true;
    [SerializeField] private float _mdashforce = 25f;
    private bool candash = true;
    private bool isdashing = false;
    private bool _miswall = false;
    private bool iswallsliding = false;
    private bool oldwallslidding = false;
    private float prevVelocityX = 0f;
    private bool canCheck = false;

    public float life = 10f;
    public bool invicible = false;
    private bool canmove = true;

    private Animator animator;
    public ParticleSystem particleJumpUp;
    public ParticleSystem particleJumpDown;

    private float jumpWallStartX = 0;
    private float jumpWallDistX = 0;
    private bool limitVelonwalljump = false;

    [Header("Events")]
    [Space]

    public UnityEvent OnFallEvent;
    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    private void Awake()
    {
        _mrigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (OnFallEvent == null)
        {
            OnFallEvent = new UnityEvent();
        }

        if(OnLandEvent == null)
        {
            OnLandEvent = new UnityEvent();
        }

    }

    private void FixedUpdate()
    {
        bool wasgrounded = _mgrounded;
        _mgrounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(_mgroundcheck.position, _kgroundedradius, _mwhatisground);
        for (int i = 0; i < colliders.Length; i++)
        { 
            if(colliders[i].gameObject != gameObject)
                _mgrounded = true;
            if (!wasgrounded)
            {
                OnLandEvent.Invoke();
                if (!_miswall && !isdashing)
                    //particlejump play
                    particleJumpDown.Play();
                    _candoublejump = true;
                if (_mrigidbody2d.velocity.y < 0f)
                    limitVelonwalljump = false;

            }
        }

        _miswall = false;

        if (!_mgrounded)
        {
            OnFallEvent.Invoke();
            Collider2D[] colliderswall = Physics2D.OverlapCircleAll(_mwallcheck.position, _kgroundedradius, _mwhatisground);
            for(int i = 0; i < colliderswall.Length; i++)
            {
                if (colliderswall[i].gameObject != null)
                {
                    isdashing = false;
                    _miswall = true;
                }
            }
            prevVelocityX = _mrigidbody2d.velocity.x;
        }

        if (limitVelonwalljump)
        {
            if (_mrigidbody2d.velocity.y < -0.5f)
                limitVelonwalljump = false;
            jumpWallDistX = (jumpWallStartX - transform.position.x) * transform.localScale.x;
            if(jumpWallDistX < -0.5f && jumpWallDistX > -1f)
            {
                canmove = true;
            }
            else if (jumpWallDistX < -1f && jumpWallDistX >= -2f)
            {
                canmove = true;
                _mrigidbody2d.velocity = new Vector2(10f * transform.localScale.x, _mrigidbody2d.velocity.y);
            }
            else if(jumpWallDistX < -2f)
            {
                limitVelonwalljump = false;
                _mrigidbody2d.velocity = new Vector2(0, _mrigidbody2d.velocity.y);
            }
            else if (jumpWallDistX > 0)
            {
                limitVelonwalljump = false;
                _mrigidbody2d.velocity = new Vector2(0, _mrigidbody2d.velocity.y);
            }

        }
    }

    public void _move(float move, bool jump, bool dash)
    {
        if (canmove)
        {
            if (dash && candash && !iswallsliding)
            {
                StartCoroutine(Dashcooldown());
            }


            if (isdashing)
            {
                _mrigidbody2d.velocity = new Vector2(transform.localScale.x * _mdashforce, 0);
            }
            else if (_mgrounded || _maircontrol)
            {
                if (_mrigidbody2d.velocity.y < -limitfallspeed)
                    _mrigidbody2d.velocity = new Vector2(_mrigidbody2d.velocity.x, -limitfallspeed);
                Vector3 targetVelocity = new Vector2(move * 10f, _mrigidbody2d.velocity.y);
                _mrigidbody2d.velocity = Vector3.SmoothDamp(_mrigidbody2d.velocity, targetVelocity, ref velocity, _msmoothmovement);

                if (move > 0 && !_mfacingright && !iswallsliding)
                {
                    Flip();
                }
                else if (move < 0 && _mfacingright && !iswallsliding)
                {
                    Flip();
                }
            }

            if (_mgrounded && jump)
            {
                //animator jump
                animator.SetBool("jumpup", true);
                _mgrounded = false;
                _mrigidbody2d.AddForce(new Vector2(0f, _mjumpforce));
                _candoublejump = true;
                //particle jump
                particleJumpDown.Play();
                particleJumpUp.Play();
            }
            else if (!_mgrounded && jump && _candoublejump && !iswallsliding)
            {
                _candoublejump = false;
                _mrigidbody2d.velocity = new Vector2(_mrigidbody2d.velocity.x, 0);
                _mrigidbody2d.AddForce(new Vector2(0f, _mjumpforce / 1.2f));
                animator.SetBool("doublejump", true);
                //animator double jump
            }
            else if (_miswall && !_mgrounded)
            {
                if (!oldwallslidding && _mrigidbody2d.velocity.y < 0 || isdashing)
                {
                    iswallsliding = true;
                    _mwallcheck.localPosition = new Vector3(-_mwallcheck.localPosition.x, _mwallcheck.localPosition.y, 0);
                    Flip();
                    StartCoroutine(WaitToCheck(0.1f));
                    _candoublejump = true;
                    animator.SetBool("wallsliding", true);
                    //animator wallslidding
                }
                isdashing = false;
                if (iswallsliding)
                {
                    if (move * transform.localScale.x > 0.1f)
                    {
                        StartCoroutine(WaitToEndSliding());
                    }
                    else
                    {
                        oldwallslidding = true;
                        _mrigidbody2d.velocity = new Vector2(-transform.localScale.x * 2, -5);
                    }
                }

                if (jump && iswallsliding)
                {
                    //animator jumping true;
                    animator.SetBool("jumpup", true);
                    _mrigidbody2d.velocity = new Vector2(0f, 0f);
                    _mrigidbody2d.AddForce(new Vector2(transform.localScale.x * _mjumpforce * 1.2f, _mjumpforce));
                    jumpWallStartX = transform.position.x;
                    limitVelonwalljump = true;
                    _candoublejump = true;
                    iswallsliding = false;
                    //animator iswallsliding false
                    animator.SetBool("wallsliding", false);
                    oldwallslidding = false;
                    _mwallcheck.localPosition = new Vector3(Mathf.Abs(_mwallcheck.localPosition.x), _mwallcheck.localPosition.y, 0);
                    canmove = false;
                }
                else if (dash && candash)
                {
                    iswallsliding = false;
                    //animator is wall sliding false;
                    animator.SetBool("wallsliding",false);
                    oldwallslidding = false;
                    _mwallcheck.localPosition = new Vector3(Mathf.Abs(_mwallcheck.localPosition.x), _mwallcheck.localPosition.y, 0);
                    _candoublejump = true;
                    StartCoroutine(Dashcooldown());
                }

            }
            else if (iswallsliding && !_miswall && canCheck)
            {
                iswallsliding = false;
                //animator iswallslidding false
                animator.SetBool("wallsliding", false);
                oldwallslidding = false;
                _mwallcheck.localPosition = new Vector3(Mathf.Abs(_mwallcheck.localPosition.x), _mwallcheck.localPosition.y, 0);
                _candoublejump = true;
            }
        }
    }

    private void Flip()
    {
        _mfacingright = !_mfacingright;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }




    IEnumerator WaitToCheck(float time)
    {
        canCheck = false;
        yield return new WaitForSeconds(time);
        canCheck = true;
    }

    IEnumerator WaitToEndSliding()
    {
        yield return new WaitForSeconds(0.1f);
        _candoublejump = true;
        iswallsliding = false;
        //animator iswallsliding false
        oldwallslidding = false;
        _mwallcheck.localPosition = new Vector3(Mathf.Abs(_mwallcheck.localPosition.x), _mwallcheck.localPosition.y, 0);
    }

    IEnumerator Dashcooldown()
    {
        //animator isdashing true;
        isdashing = true;
        candash = false;
        yield return new WaitForSeconds(0.1f);
        isdashing = false;
        yield return new WaitForSeconds(0.5f);
        candash = true;
    }

}
