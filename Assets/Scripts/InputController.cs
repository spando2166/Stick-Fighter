using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    // key-down variables
    bool left;
    bool right;
    bool up;
    bool hdup;
    bool down;

    // direction variable
    string direction = "";
    string prev = "";

    // timing variables
    float timeSinceLeft;
    float timeSinceRight;
    float dashWindow = 0.2f;
    bool doubleR;
    bool doubleL;

    private void Start()
    {
        timeSinceLeft = Time.time;
        timeSinceRight = Time.time;
    }

    void Update()
    {
        prev = direction;

        //----------------------------
        // GET DIRECTION
        //----------------------------

        if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))         direction = "downright";
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A))    direction = "downleft";
        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))    direction = "upright";
        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))    direction = "upleft";
        else if (Input.GetKey(KeyCode.A))
        {
            if (prev == "right" && Input.GetKey(KeyCode.D)) direction = "right";
            else                                            direction = "left";
        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (prev == "left" && Input.GetKey(KeyCode.A))  direction = "left";
            else                                            direction = "right";
        }
        else if (Input.GetKey(KeyCode.W))   direction = "up";
        else if (Input.GetKey(KeyCode.S))   direction = "down";
        else                                direction = "neutral";

        //----------------------------
        // GET KEY PRESS
        //----------------------------

        if (Input.GetKeyDown(KeyCode.A))
        {
            left = true;
            doubleL = (Time.time - timeSinceLeft < dashWindow && prev == "neutral");
            timeSinceLeft = Time.time;
        }
        else
        {
            left = false;
            if (direction != "left") // if we're not holding left anymore, stop dashing
                doubleL = false;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            right = true;
            doubleR = (Time.time - timeSinceRight < dashWindow && prev == "neutral");
            timeSinceRight = Time.time;
        }
        else
        {
            right = false;
            if (direction != "right") // if we're not holding right anymore, stop dashing
                doubleR = false;
        }

        if (Input.GetKeyDown(KeyCode.W))    up = true;
        else                                up = false;

        if (Input.GetKey(KeyCode.W))    hdup = true;
        else                            hdup = false;

        if (Input.GetKeyDown(KeyCode.S))    down = true;
        else                                down = false;
    }

    //----------------------------
    // PUBLIC ACCESS FUNTIONS
    //----------------------------

    public string giveDir ()        {return direction;}
    public bool isPosLeft()         {return left;}
    public bool isPosRight()        {return right;}
    public bool isPosUp()           {return up;}
    public bool isHoldUp()          {return hdup;}
    public bool isPosDown()         {return down;}
    public bool isDoubleLeft()      {return doubleL;}
    public bool isDoubleRight()     {return doubleR;}
}
