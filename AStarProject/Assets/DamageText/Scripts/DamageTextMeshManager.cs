using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DamageTextMeshManager
{
    private static DamageTextMeshManager m_Instance;
    public static DamageTextMeshManager Instance
    {
        get
        {
            if(m_Instance == null)
            {
                m_Instance = new DamageTextMeshManager();
            }
            return m_Instance;
        }
    }
    private Camera m_MainCamera;
    private Camera MainCamera
    {
        get
        {
            if (m_MainCamera == null)
            {
                m_MainCamera = Camera.main;
            }
            return m_MainCamera;
        }
    }
    private Camera m_RenderCamera;
    private Camera RenderCamera
    {
        get
        {
            if(m_RenderCamera == null)
            {
                m_RenderCamera = Camera.main;
            }
            return m_RenderCamera;
        }
    }

    private DamageTextMesh[] m_DamageTextMeshes = null;
    private int m_MinSortingOrder;

    private DamageTextMeshManager() { }
    public void Init(Camera renderCam, int size, int minSortingOrder)
    {
        m_RenderCamera = renderCam;
        m_DamageTextMeshes = new DamageTextMesh[size];
        m_MinSortingOrder = minSortingOrder;
    }
    public DamageTextMesh RequestDamageTextMesh(int fontType, Font font)
    {
        DamageTextMesh dtMesh = m_DamageTextMeshes[fontType];
        if(dtMesh == null)
        {
            dtMesh = new DamageTextMesh();
            int sortingOrder = m_DamageTextMeshes.Length - fontType + m_MinSortingOrder;
            dtMesh.Init(fontType, font, RenderCamera.transform.position, sortingOrder);
            m_DamageTextMeshes[fontType] = dtMesh;
        }
        return dtMesh;
    }

    public void Destroy()
    {
        int length = m_DamageTextMeshes.Length;
        for(int i = 0; i < length; ++i)
        {
            DamageTextMesh dtMesh = m_DamageTextMeshes[i];
            if(dtMesh != null)
            {
                dtMesh.Destroy();
            }
            m_DamageTextMeshes[i] = null;
        }
    }

    public void CalWorldPosition(DamageTextObject dtObject, DamageTextAnimation setting)
    {
        if(!setting.screenSpace)
        {
            Vector3 viewPoint = MainCamera.WorldToViewportPoint(dtObject.transform.position);
            dtObject.worldPosition = RenderCamera.ViewportToWorldPoint(viewPoint);
        }
        else
        {
            float offsetX = Screen.width * setting.offsetX;
            float offsetY = Screen.height * setting.offsetY;
            Vector3 screenPos = new Vector3(offsetX, offsetY, 0f);
            dtObject.worldPosition = RenderCamera.ScreenToWorldPoint(screenPos);
            dtObject.worldPosition.z += 100;
        }
    }

    public void Update()
    {
        UpdateMesh();
    }

    private void UpdateMesh()
    {
        int length = m_DamageTextMeshes.Length;
        for (int i = 0; i < length; ++i)
        {
            DamageTextMesh dtMesh = m_DamageTextMeshes[i];
            if(dtMesh != null && dtMesh.MeshDirty)
            {
                dtMesh.ReBuildVerts();
            }
        }
    }
}
