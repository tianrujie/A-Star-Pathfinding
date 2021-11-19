using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DamageTextAnimationCurve : System.Object
{
    public AnimationCurve movementX;
    public AnimationCurve movementY;
    public AnimationCurve alpha;
    public AnimationCurve scale;
}

public class DamageTextAnimation : MonoBehaviour
{
    [Range(0.01f, 100f)]
    public float duration = 0.01f;
    public int spriteGap;
    public bool screenSpace = false;
    [ConditionHide("screenSpace")]
    public float offsetX = 0f;
    [ConditionHide("screenSpace")]
    //[Range(0f, 1f)]
    public float offsetY = 0f;
    public Font font;
    public DamageTextAnimationCurve[] curves;

    public void InitDamageTextAnimation(DamageTextObject dtObject, float now)
    {
        if(curves.Length <= 0)
        {
            return;
        }

        dtObject.spriteGap = spriteGap;
        dtObject.animationCurve = curves[UnityEngine.Random.Range(0, curves.Length)];
        dtObject.duration = duration;
    }
}