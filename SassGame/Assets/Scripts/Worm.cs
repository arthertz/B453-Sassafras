using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worm
{

    public float pitch;
    public float yaw;

    public Vector3 pos;

    static float dt = .1f;

    public Worm (float pitch, float yaw, Vector3 initPos) {
        this.pitch = pitch;
        this.yaw = yaw;
        this.pos = initPos;
    }
}
