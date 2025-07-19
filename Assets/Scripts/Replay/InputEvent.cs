using System;
using UnityEngine;

[Serializable]
public abstract class InputEvent
{
    public float ts; //timeStamp
}

public class ME : InputEvent
{
    public float x,y,z; //position

    public ME(float ts, Vector3 p)
    {
        this.ts = ts;
        x = p.x;
        y = p.y;
        z = p.z;
    }
}

public class RE : InputEvent 
{
    public float ry; //rotationY

    public RE(float ts, float ry)
    {
        this.ts = ts;
        this.ry = ry;
    }
}

public class CE : InputEvent
{
    public bool lc;
    public bool rc;

    public CE(float ts, bool lc, bool rc)
    {
        this.ts = ts;
        this.lc = lc;
        this.rc = rc;
    }
}

public class KE : InputEvent
{
    public KE(float ts)
    {
        this.ts = ts;
    }
}

public class IE : InputEvent
{
    public string i;

    public IE(float ts, string i)
    {
        this.ts = ts;
        this.i = i;
    }
}