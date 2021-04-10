using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worm
{

    public float pitch;
    public float yaw;

    public Vector3 pos;

    public Worm (float pitch, float yaw, Vector3 initPos) {
        this.pitch = pitch;
        this.yaw = yaw;
        this.pos = initPos;
    }


    public void Advance (float wormSpeed) {
        pos += Quaternion.Euler(pitch, yaw, 0) * Vector3.down * wormSpeed;
    }
}
