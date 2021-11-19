using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DamageTextMesh
{
    public const int DEFAULT_VERT_COUNT = 4096;
    public const int DEFAULT_TRIANGLE_COUNT = DEFAULT_VERT_COUNT / 4 * 6;
    public const int DEFAULT_SPRITE_COUNT = DEFAULT_VERT_COUNT / 4;
    public const string HUD_SPRITE_SHADER_NAME = "Hidden/DamageText";

    private Vector3[] m_VertList = new Vector3[DEFAULT_VERT_COUNT];
    private Vector2[] m_UVList = new Vector2[DEFAULT_VERT_COUNT];
    private Color32[] m_ColorList = new Color32[DEFAULT_VERT_COUNT];
    private int[] m_Triangle = new int[DEFAULT_TRIANGLE_COUNT];
    private DamageTextSprite[] m_Sprites = new DamageTextSprite[DEFAULT_SPRITE_COUNT];

    private MeshFilter m_MeshFilter = null;
    private MeshRenderer m_MeshRenderer = null;
    public bool MeshDirty
    {
        get { return m_Dirty; }
    }
    public int FontType
    {
        get { return m_FontType; }
    }
    public int SpriteNumb
    {
        get { return m_CurrentSpriteNumb; }
    }

    private Mesh mesh;
    private Material material;
    private int m_FontType = 0;
    private bool m_Dirty = false;
    private int m_CurrentMaxSpriteNumb = DEFAULT_SPRITE_COUNT;
    private int m_CurrentSpriteNumb = 0;
    private int m_CurrentVertIndex = 0;

    public void Init(int fontType, Font font, Vector3 position, int sortingOrder)
    {
        mesh = new Mesh();
        mesh.hideFlags = HideFlags.DontSave;
        mesh.name = "DamageText";
        mesh.MarkDynamic();

        material = new Material(Shader.Find(HUD_SPRITE_SHADER_NAME));

        for (int i = 0; i < DEFAULT_VERT_COUNT; i += 4)
        {
            int nTriangleIndex = (i >> 2) * 6;
            m_Triangle[nTriangleIndex++] = i;
            m_Triangle[nTriangleIndex++] = i + 1;
            m_Triangle[nTriangleIndex++] = i + 2;
            m_Triangle[nTriangleIndex++] = i + 2;
            m_Triangle[nTriangleIndex++] = i + 3;
            m_Triangle[nTriangleIndex++] = i;
        }

        mesh.vertices = m_VertList;
        mesh.triangles = m_Triangle;

        m_FontType = fontType;
        material.mainTexture = font.material.mainTexture;

        GameObject go = new GameObject("DamageTextMesh_" + fontType.ToString());
        go.transform.position = new Vector3(position.x, position.y, position.z + 10);
        go.layer = LayerMask.NameToLayer("UI");

        m_MeshFilter = go.AddComponent<MeshFilter>();
        m_MeshFilter.sharedMesh = mesh;
        m_MeshRenderer = go.AddComponent<MeshRenderer>();
        m_MeshRenderer.sharedMaterial = material;
        m_MeshRenderer.sortingOrder = sortingOrder;
        m_MeshRenderer.lightProbeUsage = LightProbeUsage.Off;
        m_MeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        m_MeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        m_MeshRenderer.receiveShadows = false;
        m_MeshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;        
    }

    public void Destroy()
    {
        Clear();
        if (mesh != null)
        {
            GameObject.Destroy(mesh);
            mesh = null;
        }
        if (material != null)
        {
            GameObject.Destroy(material);
            material = null;
        }
        if(m_MeshFilter != null)
        {
            GameObject.Destroy(m_MeshFilter.gameObject);
            m_MeshFilter = null;
            m_MeshRenderer = null;
        }
    }

    public void Clear()
    {
        m_Dirty = true;
        Array.Clear(m_VertList, 0, m_VertList.Length);
        Array.Clear(m_Sprites, 0, m_Sprites.Length);
        m_CurrentVertIndex = 0;
        m_CurrentSpriteNumb = 0;
    }

    public void RemoveSpriteFromMesh(DamageTextSprite v)
    {
        m_Dirty = true;
        int index = v.meshStartIndex;
        m_VertList[index++] = DamageTextUtil.EMPTY_VECTOR3;
        m_VertList[index++] = DamageTextUtil.EMPTY_VECTOR3;
        m_VertList[index++] = DamageTextUtil.EMPTY_VECTOR3;
        m_VertList[index] = DamageTextUtil.EMPTY_VECTOR3;
        m_Sprites[v.meshStartIndex >> 2] = null;
        m_CurrentSpriteNumb--;
    }

    public void AddSpriteToMesh(DamageTextSprite sprite)
    {
        m_Dirty = true;
        if (m_CurrentSpriteNumb >= m_CurrentMaxSpriteNumb)
        {
            AllocateMore();
        }
        m_CurrentVertIndex = GetNextValidIndex();

        sprite.meshStartIndex = m_CurrentVertIndex;
        int index = m_CurrentVertIndex;
        m_VertList[index++] = sprite.verts[0];
        m_VertList[index++] = sprite.verts[1];
        m_VertList[index++] = sprite.verts[2];
        m_VertList[index] = sprite.verts[3];
        int currentVertIndex = index;

        index = m_CurrentVertIndex;
        m_UVList[index++] = sprite.uvs[0];
        m_UVList[index++] = sprite.uvs[1];
        m_UVList[index++] = sprite.uvs[2];
        m_UVList[index] = sprite.uvs[3];

        index = m_CurrentVertIndex;
        m_ColorList[index++] = sprite.colors[0];
        m_ColorList[index++] = sprite.colors[1];
        m_ColorList[index++] = sprite.colors[2];
        m_ColorList[index] = sprite.colors[3];

        m_Sprites[m_CurrentVertIndex >> 2] = sprite;

        m_CurrentVertIndex += 4;
        m_CurrentSpriteNumb++;
    }

    public void ModifySpriteVerts(DamageTextSprite sprite, bool verts, bool color, bool firstTime)
    {
        if (verts || firstTime)
        {
            int index = sprite.meshStartIndex;
            m_VertList[index++] = sprite.verts[0];
            m_VertList[index++] = sprite.verts[1];
            m_VertList[index++] = sprite.verts[2];
            m_VertList[index] = sprite.verts[3];
        }
        if (color || firstTime)
        {
            int index = sprite.meshStartIndex;
            m_ColorList[index++] = sprite.colors[0];
            m_ColorList[index++] = sprite.colors[1];
            m_ColorList[index++] = sprite.colors[2];
            m_ColorList[index] = sprite.colors[3];
        }
        m_Dirty = true;
    }

    public void ReBuildVerts()
    {
        m_Dirty = false;
        if(m_CurrentSpriteNumb == 0)
        {
            m_MeshRenderer.enabled = false;
        }
        else if(m_CurrentSpriteNumb > 0 && !m_MeshRenderer.enabled)
        {
            m_MeshRenderer.enabled = true;
        }
        mesh.vertices = m_VertList;
        mesh.uv = m_UVList;
        mesh.colors32 = m_ColorList;
    }

    #region PrivateFunction
    private int GetNextValidIndex()
    {
        for(int i = m_CurrentVertIndex >> 2; i < m_CurrentMaxSpriteNumb; ++i)
        {
            if(m_Sprites[i] == null)
            {
                return i << 2;
            }
        }

        for (int i = 0; i < m_CurrentMaxSpriteNumb; ++i)
        {
            if (m_Sprites[i] == null)
            {
                return i << 2;
            }
        }

        return 0;
    }

    private void AllocateMore()
    {
        Vector3[] newVertArray = new Vector3[m_VertList.Length << 1];
        m_VertList.CopyTo(newVertArray, 0);

        Vector2[] newUVArray = new Vector2[m_UVList.Length << 1];
        m_UVList.CopyTo(newUVArray, 0);

        Color32[] newColorArray = new Color32[m_ColorList.Length << 1];
        m_ColorList.CopyTo(newColorArray, 0);

        int[] newTriangleArray = new int[(newVertArray.Length >> 2) * 6];
        m_Triangle.CopyTo(newTriangleArray, 0);
        for (int i = m_VertList.Length; i < newVertArray.Length; i += 4)
        {
            int triangleIndex = (i >> 2) * 6;
            newTriangleArray[triangleIndex++] = i;
            newTriangleArray[triangleIndex++] = i + 1;
            newTriangleArray[triangleIndex++] = i + 2;
            newTriangleArray[triangleIndex++] = i + 2;
            newTriangleArray[triangleIndex++] = i + 3;
            newTriangleArray[triangleIndex++] = i;
        }

        DamageTextSprite[] newSpriteArray = new DamageTextSprite[newVertArray.Length >> 2];
        m_Sprites.CopyTo(newSpriteArray, 0);

        m_VertList = newVertArray;
        m_UVList = newUVArray;
        m_ColorList = newColorArray;
        m_Triangle = newTriangleArray;
        m_Sprites = newSpriteArray;

        m_CurrentMaxSpriteNumb = m_VertList.Length >> 2;

        if(mesh != null)
        {
            mesh.Clear();
            mesh.vertices = m_VertList;
            mesh.triangles = m_Triangle;
        }
    }
    #endregion
}
