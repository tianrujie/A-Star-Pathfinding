using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DamageTextAnimationItem
{
    [SerializeField]
    public string key;

    [SerializeField]
    public GameObject setting;
}

public class DamageTextAnimationSetting : MonoBehaviour
{
    [HideInInspector]
    public DamageTextAnimationItem[] settings;
    public Camera renderCamera;
    public int minSortingOrder;
    void Start()
    {
        DamageTextMeshManager.Instance.Init(renderCamera, settings.Length, minSortingOrder);
        DamageTextManager.Instance.Init(settings);
        DamageTextSpriteManager.Instance.Init(settings.Length);
    }

    private void Update()
    {
        DamageTextManager.Instance.Update();
        DamageTextMeshManager.Instance.Update();
    }

    private void OnDestroy()
    {
        DamageTextManager.Instance.Destroy();
        DamageTextMeshManager.Instance.Destroy();
    }
}
