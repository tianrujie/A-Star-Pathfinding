using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTextManager
{
    static DamageTextManager m_Instance = null;
    public static DamageTextManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new DamageTextManager();
            }
            return m_Instance;
        }
    }
    private DamageTextManager() { }

    private DamageTextObject m_FreeList;
    private DamageTextObject m_UsingList;
    private int m_FreeEntryCount = 0;
    private int m_UsingEntryCount = 0;

    private DamageTextAnimation[] m_DamageTextAnimation = null;
    private Dictionary<string, int> m_DamageTextKeyToInt = new Dictionary<string, int>();
    private bool m_Hide = false;
    
    DamageTextObject RequestHudNumber()
    {
        DamageTextObject dtObject = m_FreeList;
        if (dtObject != null)
        {
            m_FreeList = dtObject.next;
            m_FreeEntryCount--;
        }
        if (dtObject == null)
        {
            dtObject = new DamageTextObject();
        }

        m_UsingEntryCount++;
        dtObject.next = m_UsingList;
        m_UsingList = dtObject;
        dtObject.Reset();

        return dtObject;
    }

    public void Init(DamageTextAnimationItem[] animations)
    {
        m_DamageTextKeyToInt.Clear();
        m_DamageTextAnimation = new DamageTextAnimation[animations.Length];
        for(int i = 0; i < animations.Length; ++i)
        {
            m_DamageTextAnimation[i] = animations[i].setting.GetComponent<DamageTextAnimation>();
            m_DamageTextKeyToInt.Add(animations[i].key, i);
        }
    }

    private bool DoAnimation(DamageTextObject dtObject, float now, bool firstTime)
    {
        float elapsedTime = (now - dtObject.startTime);
        float duration = dtObject.duration;
        float percent = elapsedTime / duration;
        byte alpha = (byte)(dtObject.animationCurve.alpha.Evaluate(percent) * 255f + 0.5f);
        float scale = dtObject.animationCurve.scale.Evaluate(percent);
        float movementX = dtObject.animationCurve.movementX.Evaluate(percent);
        float movementY = dtObject.animationCurve.movementY.Evaluate(percent);

        byte oldAlpha = dtObject.alpha;
        float oldScale = dtObject.scale;
        float oldMovementX = dtObject.movement.x;
        float oldMovementY = dtObject.movement.y;

        dtObject.alpha = alpha;
        dtObject.scale = scale;
        dtObject.movement.x = movementX;
        dtObject.movement.y = movementY;

        bool alphaDirty = false;
        bool vertDirty = false;
        if (!firstTime)
        {
            if (oldAlpha != dtObject.alpha)
            {
                alphaDirty = true;
            }
            if (oldScale != dtObject.scale)
            {
                vertDirty = true;
            }
            if (oldMovementX != dtObject.movement.x)
            {
                vertDirty = true;
            }
            if (oldMovementY != dtObject.movement.y)
            {
                vertDirty = true;
            }
        }        

        if (alphaDirty || vertDirty || firstTime)
        {
            dtObject.ApplyAnimation(vertDirty, alphaDirty, firstTime);
        }

        return elapsedTime >= duration;
    }

    private void ReleaseDamageTextObject(DamageTextObject dtObject)
    {
        dtObject.ReleaseSprites();
        dtObject.next = m_FreeList;
        m_FreeList = dtObject;
        m_FreeEntryCount++;
        m_UsingEntryCount--;
    }

    private void ReleaseAllDamageTextObject()
    {
        DamageTextObject head = m_UsingList;
        while (head != null)
        {
            DamageTextObject temp = head;
            head = head.next;
            ReleaseDamageTextObject(temp);
        }
        m_UsingList = null;
    }

    public void Update()
    {
        if(m_UsingList == null)
        {
            return;
        }
        DamageTextObject head = m_UsingList;
        DamageTextObject prev = m_UsingList;
        float now = Time.time;
        while (head != null)
        {
            if(DoAnimation(head, now, false))
            {
                DamageTextObject temp = head;
                if (head == m_UsingList)
                {
                    m_UsingList = m_UsingList.next;
                    prev = m_UsingList;
                }
                else
                {
                    prev.next = head.next;
                }
                head = head.next;
                ReleaseDamageTextObject(temp);
                continue;
            }
            prev = head;
            head = head.next;
        }

    }

    public void Destroy()
    {
        m_UsingList = m_FreeList = null;
    }

    public void ShowDamageText(string key, string content, Transform tf)
    {
        int fontType = 0;
        if(!m_DamageTextKeyToInt.TryGetValue(key, out fontType))
        {
            Debug.LogErrorFormat("[DamageTextManager](ShowDamageText)Invalid key:[{0}]", key);
            return;
        }
        ShowDamageText(fontType, content, tf);
    }

    public void ShowDamageText(int fontType, string content, Transform tf)
    {
        if(m_Hide)
        {
            return;
        }
        DamageTextAnimation setting = m_DamageTextAnimation[fontType];
        float now = Time.time;
        DamageTextObject dtObject = RequestHudNumber();
        dtObject.startTime = now;
        dtObject.transform = tf;
        dtObject.fontType = fontType;
        setting.InitDamageTextAnimation(dtObject, now);
        DamageTextMeshManager.Instance.CalWorldPosition(dtObject, setting);
        dtObject.InitSprites(setting.font, content);
        DoAnimation(dtObject, now, true);
    }

    public void HideAllDamageText(bool hide)
    {
        m_Hide = hide;
        if(hide)
        {
            ReleaseAllDamageTextObject();
        }
    }
}
