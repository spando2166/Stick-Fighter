using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public struct InputData
{
    public bool left;
    public bool right;
    public bool up;
    public bool hdup;
    public bool down;
    public bool doubleR;
    public bool doubleL;
    public string direction;
}

public struct StateData
{
    public bool airborne;
    public bool stunned;
    public bool blocking;
    public bool attacking;
    public bool dashing;
    public bool airdash;
    public bool bairdash;
    public bool bdashing;
    public bool walking;
    public bool bwalking;
    public bool crouching;
    public bool jumping;
}