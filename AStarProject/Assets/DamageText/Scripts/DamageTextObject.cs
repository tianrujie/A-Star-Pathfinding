using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTextObject
{
    public const float OFFSET_SCALE = 0.01f;

    public Transform transform;
    public DamageTextObject next;
    public int fontType;
    public Vector3 worldPosition;
    public Vector2 movement;
    public float scale;
    public byte alpha;
    public DamageTextMesh m_Mesh = null;
    public int width = 0;
    public int height = 0;
    public int spriteGap = 0;
    public float startTime = 0.0f;
    public float duration = 0.0f;
    public DamageTextAnimationCurve animationCurve;

    private DamageTextSprite m_SpriteHead = null;
    private DamageTextSprite m_SpriteTail = null;
    public void Reset()
    {
        transform = null;
        worldPosition = DamageTextUtil.EMPTY_VECTOR3;
        movement = DamageTextUtil.EMPTY_VECTOR2;
        scale = 1.0f;
        alpha = 255;
        width = 0;
        height = 0;
        spriteGap = 0;
    }
    public void ReleaseSprites()
    {
        if (m_SpriteHead == null)
        {
            return;
        }
        DamageTextSprite head = m_SpriteHead;
        while(head != null)
        {
            m_Mesh.RemoveSpriteFromMesh(head);
            DamageTextSprite current = head;
            head = head.next;
            DamageTextSpriteManager.Instance.ReleaseSprite(current);
            
        }
        m_Mesh = null;
        m_SpriteHead = m_SpriteTail = null;
        
    }

    public void InitSprites(Font font, string content)
    {
        DamageTextMesh hudMesh = DamageTextMeshManager.Instance.RequestDamageTextMesh(fontType, font);
        m_Mesh = hudMesh;

        for (int i = 0; i < content.Length; ++i)
        {
            DamageTextSprite sprite = DamageTextSpriteManager.Instance.RequestSpriteInfo();
            sprite.InitChar(fontType, font, content[i]);
            sprite.offset.x = width;
            sprite.offset.y = 0;
            width += sprite.width + spriteGap;
            if (height < sprite.height)
            {
                height = sprite.height;
            }

            if(m_SpriteHead == null)
            {
                m_SpriteHead = m_SpriteTail = sprite;
            }
            else
            {
                m_SpriteTail.next = sprite;
                m_SpriteTail = sprite;
            }
            hudMesh.AddSpriteToMesh(sprite);
        }

        AlignCenter();
    }

    public void ApplyAnimation(bool vertDirty, bool alphaDirty, bool firstTime)
    {
        DamageTextSprite sprite = m_SpriteHead;
        while (sprite != null)
        {
            if(vertDirty || firstTime)
            {
                sprite.verts[0].x = worldPosition.x + ((sprite.width + sprite.offset.x) * scale + movement.x) * OFFSET_SCALE;
                sprite.verts[0].y = worldPosition.y + (sprite.offset.y * scale + movement.y) * OFFSET_SCALE;
                sprite.verts[0].z = worldPosition.z;
                sprite.verts[1].x = worldPosition.x + ((sprite.width + sprite.offset.x) * scale + movement.x) * OFFSET_SCALE;
                sprite.verts[1].y = worldPosition.y + ((sprite.height + sprite.offset.y) * scale + movement.y) * OFFSET_SCALE;
                sprite.verts[1].z = worldPosition.z;
                sprite.verts[2].x = worldPosition.x + (sprite.offset.x * scale + movement.x) * OFFSET_SCALE;
                sprite.verts[2].y = worldPosition.y + ((sprite.height + sprite.offset.y) * scale + movement.y) * OFFSET_SCALE;
                sprite.verts[2].z = worldPosition.z;
                sprite.verts[3].x = worldPosition.x + (sprite.offset.x * scale + movement.x) * OFFSET_SCALE;
                sprite.verts[3].y = worldPosition.y + (sprite.offset.y * scale + movement.y) * OFFSET_SCALE;
                sprite.verts[3].z = worldPosition.z;
            }
            if(alphaDirty || firstTime)
            {
                sprite.colors[0].a = alpha;
                sprite.colors[1].a = alpha;
                sprite.colors[2].a = alpha;
                sprite.colors[3].a = alpha;
            }
            m_Mesh.ModifySpriteVerts(sprite, vertDirty, alphaDirty, firstTime);
            sprite = sprite.next;
        }
    }

    private void AlignCenter()
    {
        float halfWidth = width * 0.5f;
        DamageTextSprite head = m_SpriteHead;
        while(head != null)
        {
            head.offset.x -= halfWidth;
            head = head.next;
        }
    }
};
