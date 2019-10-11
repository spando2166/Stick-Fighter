using System;
using UnityEngine;

public class InputController : MonoBehaviour
{
    InputData inputs;

    // helper variable
    string prev = "";

    // timing variables
    float timeSinceLeft;
    float timeSinceRight;
    float dashWindow = 0.2f;

    private void Start()
    {
        timeSinceLeft = Time.time;
        timeSinceRight = Time.time;
    }

    void Update()
    {
        prev = inputs.direction;

        //----------------------------
        // GET DIRECTION
        //----------------------------

        if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D)) inputs.direction = "downright";
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A)) inputs.direction = "downleft";
        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D)) inputs.direction = "upright";
        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A)) inputs.direction = "upleft";
        else if (Input.GetKey(KeyCode.A))
        {
            if (prev == "right" && Input.GetKey(KeyCode.D)) inputs.direction = "right";
            else inputs.direction = "left";
        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (prev == "left" && Input.GetKey(KeyCode.A)) inputs.direction = "left";
            else inputs.direction = "right";
        }
        else if (Input.GetKey(KeyCode.W)) inputs.direction = "up";
        else if (Input.GetKey(KeyCode.S)) inputs.direction = "down";
        else inputs.direction = "neutral";

        //----------------------------
        // GET KEY PRESS
        //----------------------------

        if (Input.GetKeyDown(KeyCode.A))
        {
            inputs.left = true;
            inputs.doubleL = (Time.time - timeSinceLeft < dashWindow && prev == "neutral");
            timeSinceLeft = Time.time;
        }
        else
        {
            inputs.left = false;
            if (inputs.direction != "left") // if we're not holding left anymore, stop dashing
                inputs.doubleL = false;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            inputs.right = true;
            inputs.doubleR = (Time.time - timeSinceRight < dashWindow && prev == "neutral");
            timeSinceRight = Time.time;
        }
        else
        {
            inputs.right = false;
            if (inputs.direction != "right") // if we're not holding right anymore, stop dashing
                inputs.doubleR = false;
        }

        if (Input.GetKeyDown(KeyCode.W)) inputs.up = true;
        else inputs.up = false;

        if (Input.GetKey(KeyCode.W)) inputs.hdup = true;
        else inputs.hdup = false;

        if (Input.GetKeyDown(KeyCode.S)) inputs.down = true;
        else inputs.down = false;
    }

    //----------------------------
    // PUBLIC ACCESS FUNTIONS
    //----------------------------

    public InputData GetControllerInputs()
    {
        return inputs;
    }
}