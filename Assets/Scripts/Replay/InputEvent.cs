using System;
using UnityEngine;

[Serializable]
public abstract class InputEvent
{
    public float ts; //timeStamp
}

//Movement Event
public class ME : InputEvent
{
    public float x, y, z; //position

    public ME(float ts, Vector3 p)
    {
        this.ts = ts;
        x = p.x;
        y = p.y;
        z = p.z;
    }
}

//Rotation Event
public class RE : InputEvent
{
    public float ry; //rotationY

    public RE(float ts, float ry)
    {
        this.ts = ts;
        this.ry = ry;
    }
}

//Shoot Event
public class SE : InputEvent
{
    public SE(float ts)
    {
        this.ts = ts;
    }
}

//Interact Event
public class IE : InputEvent
{
    public string i; //Name of object we interact with. Can be null/String.empty

    public IE(float ts, string i)
    {
        this.ts = ts;
        this.i = i;
    }
}

//Switch Hand
public class SH : InputEvent
{
    public bool ge; //gunEquipped
    public SH(float ts, bool ge)
    {
        this.ts = ts;
        this.ge = ge;
    }
}


//Konami Event
public class KE : InputEvent
{
    public KE(float ts)
    {
        this.ts = ts;
    }
}
