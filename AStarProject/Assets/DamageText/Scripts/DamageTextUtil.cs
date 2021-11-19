using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTextUtil
{
    public static readonly Color32 COLOR_WHITE = Color.white;
    public static readonly Vector3 EMPTY_VECTOR3 = Vector3.zero;
    public static readonly Vector2 EMPTY_VECTOR2 = Vector2.zero;

    public static int GetCharWidth(ref CharacterInfo chInfo)
    {
        int nWidth = chInfo.glyphWidth;
        if(chInfo.maxX > nWidth)
        {
            if (chInfo.minX < 0)
                return chInfo.maxX - chInfo.minX;
            else
                return chInfo.maxX + chInfo.minX;
        }
        return nWidth;
    }

    static public void ConvertToTexCoords(ref Rect rect, int width, int height)
    {
        if (width != 0f && height != 0f)
        {
            rect.xMin = rect.xMin / width;
            rect.xMax = rect.xMax / width;
            float yMin = rect.yMin;
            float yMax = rect.yMax;
            rect.yMin = 1f - yMax / height;
            rect.yMax = 1f - yMin / height;
        }
    }

};

