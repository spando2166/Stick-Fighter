using System;
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


    // placeholder variables
    Vector2 speed;

    // cache variables
    Rigidbody2D gameBody;
    InputController inputController;
    [SerializeField] Animator animator;

    // action function delegate
    public delegate void ActionDelegate();
    ActionDelegate act;
    float timer;

    // input and state data
    InputData inputs;
    StateData state;

    // Start is called before the first frame update
    void Start()
    {
        gameBody = GetComponent<Rigidbody2D>();
        inputController = FindObjectOfType<InputController>();
    }

    // Update is called once per frame
    void Update()
    {
        speed = gameBody.velocity;
        inputs = inputController.GetControllerInputs();

        if (timer > 0)
        {
            timer--;
            act();
        }
        else
        {
            if (state.airborne)
            {
                if (inputs.doubleR && !state.airdash)         airDashRight();
                else if (inputs.doubleL && !state.bairdash)   airDashLeft();
            }
            else
            {
                if (inputs.hdup) jump();
                else if (inputs.direction == "down" ||
                        inputs.direction == "downright" ||
                        inputs.direction == "downleft")
                {
                    setMovement("crouch");
                    halt();
                }
                else if (inputs.doubleR)
                {
                    if (!state.dashing)
                    {
                        lock_movement(5f, dashRight);
                        setMovement("dash");
                    }
                    dashRight();
                }
                else if (inputs.direction == "right") walkRight();
                else if (inputs.doubleL)
                {
                    if (!state.dashing)
                    {
                        lock_movement(5f, dashLeft);
                        setMovement("bdash");
                    }
                    dashLeft();
                }
                else if (inputs.direction == "left") walkLeft();
                else
                {
                    setMovement("none");
                    halt();
                }
            }
        }

        // handle the animator
        RenderCurrentFrame();
    }

    //----------------------------
    // COLLISION
    //----------------------------

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            setMovement("none");
            state.airborne = false;
        }
    }

    //----------------------------
    // MOVEMENT
    //----------------------------

    private void jump()
    {
        setMovement("jump");
        if (inputs.direction == "up")
        {
            speed.x = 0f;
            speed.y = 10f;
        }
        else if (inputs.direction == "upright")
        {
            if (orientation() == 'L') speed.x = 3f;
            speed.y = 10f;
        }
        else if (inputs.direction == "upleft")
        {
            if (orientation() == 'R') speed.x = -3f;
            speed.y = 10f;
        }
        gameBody.velocity = speed;
        state.airborne = true;
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

    private void RenderCurrentFrame()
    {
        if (state.walking) animator.SetBool("isWalking", true); else animator.SetBool("isWalking", false);
        if (state.bwalking) animator.SetBool("isBackWalk", true); else animator.SetBool("isBackWalk", false);
        if (state.dashing) animator.SetBool("isDashing", true); else animator.SetBool("isDashing", false);
        if (state.bdashing) animator.SetBool("isBackDash", true); else animator.SetBool("isBackDash", false);
        if (state.crouching) animator.SetBool("isCrouching", true); else animator.SetBool("isCrouching", false);
        if (state.jumping) animator.SetBool("isJumping", true); else animator.SetBool("isJumping", false);
        if (state.airdash) animator.SetBool("isAirDash", true); else animator.SetBool("isAirDash", false);
        if (state.bairdash) animator.SetBool("isBairDash", true); else animator.SetBool("isBairDash", false);
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
        if (String.Equals(move, "walk")) state.walking = true; else state.walking = false;
        if (String.Equals(move, "bwalk")) state.bwalking = true; else state.bwalking = false;
        if (String.Equals(move, "dash")) state.dashing = true; else state.dashing = false;
        if (String.Equals(move, "bdash")) state.bdashing = true; else state.bdashing = false;
        if (String.Equals(move, "crouch")) state.crouching = true; else state.crouching = false;
        if (String.Equals(move, "jump")) state.jumping = true; else state.jumping = false;
        if (String.Equals(move, "adash")) state.airdash = true; else state.airdash = false;
        if (String.Equals(move, "badash")) state.bairdash = true; else state.bairdash = false;
    }
}