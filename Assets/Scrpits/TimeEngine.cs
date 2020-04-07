//#define DEBUG_TIME

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeEngine : MonoBehaviour
{
    public static int frameCounter;
    public static int fixedFrameCounter;

    public static float reciprocalFixedDeltaTime; // 1f / fixedDeltaTime

    void Start()
    {
        reciprocalFixedDeltaTime = 1f / Time.fixedDeltaTime; // Cache the reciprocal
        frameCounter = 0;
        fixedFrameCounter = 0;
    }

    void Update()
    {
        frameCounter++;

#if DEBUG_TIME
        Debug.Log("Update frame : " + frameCounter);
#endif
    }

    private void FixedUpdate()
    {
        fixedFrameCounter++;

#if DEBUG_TIME
        Debug.Log("Fixed Update frame : " + frameCounter);
#endif
    }
}
