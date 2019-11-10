using System;
using UnityEngine;

public class Fighter : MonoBehaviour
{
    // state variables
    // [SerializeField] int hitPoints = 100;
    [SerializeField] float f_walkSpeed = 3f;
    [SerializeField] float f_runSpeed = 8f;
    [SerializeField] float f_jumpSpeed = 10f;
    [SerializeField] float f_airDashSpeed = 5f;
    [SerializeField] float f_accelRate = 20f;
    [SerializeField] float f_decelRate = 10f;
    [SerializeField] Fix64 border_l = (Fix64)0.75f;
    [SerializeField] Fix64 border_r = (Fix64)15.75f;
    [SerializeField] Fix64 border_b = (Fix64)1.5f;
    [SerializeField] Fix64 border_t = (Fix64)10.5f;

    Fix64 time_delta = (Fix64)(1f/60f);
    Fix64 walkSpeed;
    Fix64 runSpeed;
    Fix64 jumpSpeed;
    Fix64 airDashSpeed;
    Fix64 accelRate;
    Fix64 decelRate;

    // placeholder variables
    FixVector2D pos;
    FixVector2D pos_delta = new FixVector2D(Fix64.Zero, Fix64.Zero);
    FixVector2D velocity = new FixVector2D(Fix64.Zero, Fix64.Zero);
    Fix64 max_accel = (Fix64)10000f;

    // cache variables
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
        walkSpeed =       (Fix64)f_walkSpeed;
        runSpeed =        (Fix64)f_runSpeed;
        jumpSpeed =       (Fix64)f_jumpSpeed;
        airDashSpeed =    (Fix64)f_airDashSpeed;
        accelRate =       (Fix64)f_accelRate;
        decelRate =       (Fix64)f_decelRate;

        inputController = FindObjectOfType<InputController>();
    }

    // Update is called once per frame
    void Update()
    {
        StorePosition();
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
                Fall(decelRate);
                //if (inputs.doubleR && !state.airdash) airDashRight();
                //else if (inputs.doubleL && !state.bairdash) airDashLeft();
            }
            else
            {
                if (inputs.hdup)
                {
                    Jump();
                }
                else if (inputs.direction == "down" ||
                        inputs.direction == "downright" ||
                        inputs.direction == "downleft")
                {
                    SetMovement("crouch");
                    Halt();
                }
                else if (inputs.doubleR)
                {
                    if (!state.dashing)
                    {
                        LockMovement(5f, DashRight);
                        SetMovement("dash");
                    }
                    DashRight();
                }
                else if (inputs.direction == "right") WalkRight();
                else if (inputs.doubleL)
                {
                    if (!state.dashing)
                    {
                        LockMovement(5f, DashLeft);
                        SetMovement("bdash");
                    }
                    DashLeft();
                }
                else if (inputs.direction == "left") WalkLeft();
                else
                {
                    SetMovement("none");
                    Halt();
                }
            }
        }

        // get the change in our positions
        SetDelta();

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
            SetMovement("none");
            state.airborne = false;
        }
    }

    private void StorePosition()
    {
        pos.X = (Fix64)transform.position.x;
        pos.Y = (Fix64)transform.position.y;
    }

    private void SetDelta()
    {
        pos_delta.X = (Fix64)transform.position.x - pos.X;
        pos_delta.Y = (Fix64)transform.position.y - pos.Y;
    }

    //----------------------------
    // MOVEMENT
    //----------------------------
    
    private void Jump()
    {
        SetMovement("jump");

        // if this is the first time, give the intial velocities
        velocity.Y = jumpSpeed;

        if (inputs.direction == "upright" && !AdvancingRight())
            velocity.X = (Fix64)3f;
        else if (inputs.direction == "upleft" && AdvancingRight())
            velocity.X = (Fix64)(-3f);

        state.airborne = true;
    }

    private void Fall(Fix64 decel)
    {
        // get the velocity after deceleration
        if (velocity.Y - (decel * time_delta) > -jumpSpeed)
            velocity.Y -= decel * time_delta;
        else
            velocity.Y = -jumpSpeed;

        float y_distance = Mathf.Clamp((float)(velocity.Y * time_delta), (float)(border_b - pos.Y), (float)(border_t - pos.Y));
        float x_distance = Mathf.Clamp((float)(velocity.X * time_delta), (float)(border_l - pos.X), (float)(border_r - pos.X));

        if ((Fix64)y_distance == border_b - pos.Y)
            state.airborne = false;

        transform.position += (Vector3.right * x_distance) + (Vector3.up * y_distance);
    }

    /*
private void setXSpeed(float val)
{
   pos.x = val;
   gameBody.velocity = pos;
}

private void addXSpeed(float val)
{
   pos.x += val;
   gameBody.velocity = pos;
}

private void setYSpeed(float val)
{
   pos.y = val;
   gameBody.velocity = pos;
}

private void addYSpeed(float val)
{
   pos.y += val;
   gameBody.velocity = pos;
}
*/
    private void WalkRight()
    {
        AccelerateRight(walkSpeed, max_accel);
        SetMovement("walk");
    }

    private void WalkLeft()
    {
        AccelerateLeft(walkSpeed, max_accel);
        SetMovement("bwalk");
    }

    private void DashRight()
    {
        AccelerateRight(runSpeed, accelRate);
    }

    private void DashLeft()
    {
        AccelerateLeft(runSpeed, accelRate);
    }
    /*
    private void airDashRight()
    {
        if (pos.x >= airDashSpeed)    addXSpeed(airDashSpeed);
        else                            setXSpeed(runSpeed);
        setYSpeed(1f);
        setMovement("adash");
    }

    private void airDashLeft()
    {
        if (pos.x <= -airDashSpeed)    addXSpeed(-airDashSpeed);
        else                            setXSpeed(-runSpeed);
        setYSpeed(1f);
        setMovement("badash");
    }
    */

    private void Halt()
    {
        if (AdvancingRight())   DecelerateRight(decelRate);
        else                    DecelerateLeft(decelRate);
    }

    private void AccelerateRight(Fix64 fvelocity, Fix64 accel)
    {
        // get the new velocity
        Fix64 new_velocity = velocity.X + (accel * time_delta);

        // get the velocity after acceleration (cannot exceed fvelocity)
        if (new_velocity < Fix64.Zero)
            velocity.X = Fix64.Zero;
        else if (new_velocity >= fvelocity)
            velocity.X = fvelocity;
        else
            velocity.X = new_velocity;

        float distance = Mathf.Clamp((float)(velocity.X * time_delta), (float)(border_l - pos.X), (float)(border_r - pos.X));
        transform.position += Vector3.right * distance;
    }

    private void AccelerateLeft(Fix64 fvelocity, Fix64 accel)
    {
        // get the new velocity
        Fix64 new_velocity = velocity.X - (accel * time_delta);

        // get the velocity after acceleration (cannot exceed -fvelocity)
        if (new_velocity > Fix64.Zero)
            velocity.X = Fix64.Zero;
        else if (new_velocity <= -fvelocity)
            velocity.X = -fvelocity;
        else
            velocity.X = new_velocity;

        float distance = Mathf.Clamp((float)(velocity.X * time_delta), (float)(border_l - pos.X), (float)(border_r - pos.X));
        transform.position += Vector3.right * distance;
    }

    private void DecelerateRight(Fix64 decel)
    {
        // get the velocity after deceleration
        if (velocity.X > (decel * time_delta))
            velocity.X -= decel * time_delta;
        else
            velocity.X = Fix64.Zero;

        float distance = Mathf.Clamp((float)(velocity.X * time_delta), (float)(border_l - pos.X), (float)(border_r - pos.X));
        transform.position += Vector3.right * distance;
    }

    private void DecelerateLeft(Fix64 decel)
    {
        // get the velocity after deceleration
        if (velocity.X < (-decel * time_delta))
            velocity.X += decel * time_delta;
        else
            velocity.X = Fix64.Zero;

        float distance = Mathf.Clamp((float)(velocity.X * time_delta), (float)(border_l - pos.X), (float)(border_r - pos.X));
        transform.position += Vector3.right * distance;
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
    public void LockMovement(float amt, ActionDelegate tmp)
    {
        timer = amt;
        act = tmp;
    }

    // tells if object is currently moving left or right
    private bool AdvancingRight()
    {
        if (pos_delta.X >= (Fix64)0)
            return true;
        return false;
    }

    // we can set our act delegate to do no action
    private void Nothing()
    {
        return;
    }

    // we set our movement so that only one move can be true
    private void SetMovement(string move)
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