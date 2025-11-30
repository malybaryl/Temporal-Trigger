using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class GameTime
{
    public static float timescale { private set; get; } = 1f;

    public static void SetTimeScale(float newScale)
    {
        timescale = Mathf.Clamp(newScale, 0.1f, 1f);
    }
}
