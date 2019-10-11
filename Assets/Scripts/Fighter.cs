using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter : MonoBehaviour
{
    // state variables
    [SerializeField] int hitPoints = 100;
    [SerializeField] float walkSpeed = 3f;
    [SerializeField] float runSpeed = 10f;
    [SerializeField] float airDashSpeed = 5f;
    [SerializeField] float accelRate = 2f;
    [SerializeField] float decelRate = 0.5f;

    // conditional variables
    bool airborne =     new bool();
    bool stunned =      new bool();
    bool blocking =     new bool();
    bool attacking =    new bool();
    bool dashing =      new bool();
    bool airdash =      new bool();
    bool bairdash =     new bool();
    bool bdashing =     new bool();
    bool walking =      new bool();
    bool bwalking =     new bool();
    bool crouching =    new bool();
    bool jumping =      new bool();

    // placeholder variables
    Vector2 speed;

    // cache variables
    Rigidbody2D gameBody;
    InputController buttons;
    [SerializeField] Animator animator;

    // action function delegate
    public delegate void ActionDelegate();
    ActionDelegate act;
    float timer;

    // Start is called before the first frame update
    void Start()
    {
        gameBody = GetComponent<Rigidbody2D>();
        buttons = FindObjectOfType<InputController>();
    }

    // Update is called once per frame
    void Update()
    {
        speed = gameBody.velocity;

        if (timer > 0)
        {
            timer--;
            act();
        }
        else
        {
            if (airborne)
            {
                if (buttons.isDoubleRight() && !airdash)        airDashRight();
                else if (buttons.isDoubleLeft() && !bairdash)   airDashLeft();
            }
            else
            {
                if (buttons.isHoldUp()) jump();
                else if (buttons.giveDir() == "down" ||
                    buttons.giveDir() == "downright" ||
                    buttons.giveDir() == "downleft")
                {
                    setMovement("crouch");
                    halt();
                }
                else if (buttons.isDoubleRight())
                {
                    if (!dashing)
                    {
                        lock_movement(5f, dashRight);
                        setMovement("dash");
                    }
                    dashRight();
                }
                else if (buttons.giveDir() == "right") walkRight();
                else if (buttons.isDoubleLeft())
                {
                    if (!dashing)
                    {
                        lock_movement(5f, dashLeft);
                        setMovement("bdash");
                    }
                    dashLeft();
                }
                else if (buttons.giveDir() == "left") walkLeft();
                else
                {
                    setMovement("none");
                    halt();
                }
            }
        }

        // handle the animator
        updateAnimation();
    }

    //----------------------------
    // COLLISION
    //----------------------------

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            setMovement("none");
            airborne = false;
        }
    }

    //----------------------------
    // MOVEMENT
    //----------------------------

    private void jump()
    {
        setMovement("jump");
        if (buttons.giveDir() == "up")
        {
            speed.x = 0f;
            speed.y = 10f;
        }
        else if (buttons.giveDir() == "upright")
        {
            if (orientation() == 'L') speed.x = 3f;
            speed.y = 10f;
        }
        else if (buttons.giveDir() == "upleft")
        {
            if (orientation() == 'R') speed.x = -3f;
            speed.y = 10f;
        }
        gameBody.velocity = speed;
        airborne = true;
    }

    private void setXSpeed(float val)
    {
        speed.x = val;
        gameBody.velocity = speed;
    }

    private void addXSpeed(float val)
    {
        speed.x += val;
        gameBody.velocity = speed;
    }

    private void setYSpeed(float val)
    {
        speed.y = val;
        gameBody.velocity = speed;
    }

    private void addYSpeed(float val)
    {
        speed.y += val;
        gameBody.velocity = speed;
    }

    private void walkRight()
    {
        setXSpeed(walkSpeed);
        setMovement("walk");
    }

    private void walkLeft()
    {
        setXSpeed(-walkSpeed);
        setMovement("bwalk");
    }

    private void dashRight()
    {
        accelerateRight(runSpeed, accelRate);
    }

    private void dashLeft()
    {
        accelerateLeft(runSpeed, accelRate);
    }

    private void airDashRight()
    {
        if (speed.x >= airDashSpeed)    addXSpeed(airDashSpeed);
        else                            setXSpeed(runSpeed);
        setYSpeed(1f);
        setMovement("adash");
    }

    private void airDashLeft()
    {
        if (speed.x <= -airDashSpeed)    addXSpeed(-airDashSpeed);
        else                            setXSpeed(-runSpeed);
        setYSpeed(1f);
        setMovement("badash");
    }

    private void halt()
    {
        if (orientation() == 'R') decelerateRight(decelRate);
        if (orientation() == 'L') decelerateLeft(decelRate);
    }

    private void accelerateRight(float fvelocity, float accel)
    {
        // if we have reached the desired speed, stop acceleration
        if (gameBody.velocity.x >= fvelocity) return;

        // otherwise, accelerate
        if (speed.x + accel >= fvelocity) speed.x = fvelocity;
        else speed.x += accel;

        gameBody.velocity = speed;
    }

    private void accelerateLeft(float fvelocity, float accel)
    {
        // if we have reached the desired speed, stop acceleration
        if (gameBody.velocity.x <= -fvelocity) return;

        // otherwise, accelerate
        if (speed.x - accel <= -fvelocity) speed.x = -fvelocity;
        else speed.x -= accel;

        gameBody.velocity = speed;
    }

    private void decelerateRight(float decel)
    {
        // if we have reached the desired speed, stop acceleration
        if (gameBody.velocity.x <= 0) return;

        // otherwise, accelerate
        if (speed.x - decel <= 0) speed.x = 0;
        else speed.x -= decel;

        gameBody.velocity = speed;
    }

    private void decelerateLeft(float decel)
    {
        // if we have reached the desired speed, stop acceleration
        if (gameBody.velocity.x >= 0) return;

        // otherwise, accelerate
        if (speed.x + decel >= 0) speed.x = 0;
        else speed.x += decel;

        gameBody.velocity = speed;
    }

    //----------------------------
    // ANIMATION
    //----------------------------

    private void updateAnimation()
    {
        if (walking) animator.SetBool("isWalking", true); else animator.SetBool("isWalking", false);
        if (bwalking) animator.SetBool("isBackWalk", true); else animator.SetBool("isBackWalk", false);
        if (dashing) animator.SetBool("isDashing", true); else animator.SetBool("isDashing", false);
        if (bdashing) animator.SetBool("isBackDash", true); else animator.SetBool("isBackDash", false);
        if (crouching) animator.SetBool("isCrouching", true); else animator.SetBool("isCrouching", false);
        if (jumping) animator.SetBool("isJumping", true); else animator.SetBool("isJumping", false);
        if (airdash) animator.SetBool("isAirDash", true); else animator.SetBool("isAirDash", false);
        if (bairdash) animator.SetBool("isBairDash", true); else animator.SetBool("isBairDash", false);
    }

    //----------------------------
    // STATUS
    //----------------------------

    // lock movement for specified number of frames
    // sets an action to continue under movement lock
    public void lock_movement(float amt, ActionDelegate tmp)
    {
        timer = amt;
        act = tmp;
    }

    // tells if object is currently moving left or right
    private char orientation()
    {
        return gameBody.velocity.x > 0 ? 'R' : 'L';
    }

    // we can set our act delegate to do no action
    private void nothing()
    {
        return;
    }

    // we set our movement so that only one move can be true
    private void setMovement(string move)
    {
        if (String.Equals(move, "walk")) walking = true; else walking = false;
        if (String.Equals(move, "bwalk")) bwalking = true; else bwalking = false;
        if (String.Equals(move, "dash")) dashing = true; else dashing = false;
        if (String.Equals(move, "bdash")) bdashing = true; else bdashing = false;
        if (String.Equals(move, "crouch")) crouching = true; else crouching = false;
        if (String.Equals(move, "jump")) jumping = true; else jumping = false;
        if (String.Equals(move, "adash")) airdash = true; else airdash = false;
        if (String.Equals(move, "badash")) bairdash = true; else bairdash = false;
    }
}