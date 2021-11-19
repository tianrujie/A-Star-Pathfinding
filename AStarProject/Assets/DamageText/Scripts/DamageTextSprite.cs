using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DamageTextSpriteBaseInfo
{
    public int width;
    public int height;
    public Vector2 uvBottomLeft;
    public Vector2 uvBottomRight;
    public Vector2 uvTopLeft;
    public Vector2 uvTopRight;
}

public class DamageTextSpriteManager
{
    private static DamageTextSpriteManager m_Instance;
    public static DamageTextSpriteManager Instance
    {
        get
        {
            if(m_Instance == null)
            {
                m_Instance = new DamageTextSpriteManager();
            }
            return m_Instance;
        }
    }

    private DamageTextSprite m_FreeSpriteInfoList = null;
    private CharacterInfo characterInfo;
    private Dictionary<char, DamageTextSpriteBaseInfo>[] m_CachedSpriteBaseInfo = null;
    private DamageTextSpriteManager() { }
    public void Init(int cachedSize)
    {
        m_CachedSpriteBaseInfo = new Dictionary<char, DamageTextSpriteBaseInfo>[cachedSize];
    }
    public DamageTextSprite RequestSpriteInfo()
    {
        DamageTextSprite p = m_FreeSpriteInfoList;
        if (p != null)
        {
            m_FreeSpriteInfoList = p.next;
            p.next = null;
        }
        if (p == null)
        {
            p = new DamageTextSprite();
        }
        return p;
    }

    public void ReleaseSprite(DamageTextSprite p)
    {
        if (p != null)
        {
            p.next = m_FreeSpriteInfoList;
            m_FreeSpriteInfoList = p;
        }
    }

    public DamageTextSpriteBaseInfo GetSpriteInfo(int fontType, Font font, char ch)
    {
        Dictionary<char, DamageTextSpriteBaseInfo> dict = m_CachedSpriteBaseInfo[fontType];
        if(dict == null)
        {
            dict = new Dictionary<char, DamageTextSpriteBaseInfo>();
            m_CachedSpriteBaseInfo[fontType] = dict;
        }
        DamageTextSpriteBaseInfo info;
        if(dict.TryGetValue(ch, out info))
        {
            return info;
        }

        font.GetCharacterInfo(ch, out characterInfo);
        info = new DamageTextSpriteBaseInfo();
        info.width = characterInfo.glyphWidth;
        info.height = characterInfo.glyphHeight;
        info.uvBottomLeft = characterInfo.uvBottomLeft;
        info.uvBottomRight = characterInfo.uvBottomRight;
        info.uvTopLeft = characterInfo.uvTopLeft;
        info.uvTopRight = characterInfo.uvTopRight;
        dict.Add(ch, info);

        return info;
    }
}

public class DamageTextSprite
{
    public DamageTextSprite next;

    public Vector3[] verts = new Vector3[4];
    public Vector2[] uvs = new Vector2[4];
    public Color32[] colors = new Color32[4];
    public Vector2 offset;

    public int width;
    public int height;
    public int meshStartIndex = 0;

    public DamageTextSprite()
    {
        colors[0] = colors[1] = colors[2] = colors[3] = DamageTextUtil.COLOR_WHITE;
    }

    public void InitChar(int fontType, Font font, char ch)
    {
        DamageTextSpriteBaseInfo baseInfo = DamageTextSpriteManager.Instance.GetSpriteInfo(fontType, font, ch);

        width = baseInfo.width;
        height = baseInfo.height;
        uvs[0] = baseInfo.uvBottomRight;
        uvs[1] = baseInfo.uvTopRight;
        uvs[2] = baseInfo.uvTopLeft;
        uvs[3] = baseInfo.uvBottomLeft;
    }
}

