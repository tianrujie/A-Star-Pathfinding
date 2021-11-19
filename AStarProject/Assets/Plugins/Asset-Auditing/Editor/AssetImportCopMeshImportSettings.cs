using System;
using UnityEditor;

[System.Serializable]
public struct MeshImporterSettings
{
    /// <summary>
    /// 读写设置开关
    /// </summary>
    public bool ApplyMeshReadWrite;

    /// <summary>
    /// 动作类型设置开关
    /// </summary>
    public bool applyAnimationType;

    /// <summary>
    /// 是否开启读写
    /// </summary>
    public bool readWriteEnabled;

    /// <summary>
    /// 网格优化
    /// </summary>
    public bool optimiseMesh;

    /// <summary>
    /// 是否导入顶点动画(Morph)
    /// </summary>
    public bool ImportBlendShapes;

    /// <summary>
    /// 是否导入动画
    /// </summary>
    public bool importAnimaton;

    /// <summary>
    /// 动画类型
    /// </summary>
    public ModelImporterAnimationType animationType;

    /// <summary>
    /// 是否导入材质
    /// </summary>
    public bool importMaterials;

    /// <summary>
    /// 获取MeshSetting数据
    /// </summary>
    /// <param name="importer"></param>
    /// <returns></returns>
    public static MeshImporterSettings Extract(ModelImporter importer)
    {
        if (importer == null)
            throw new ArgumentException();

        MeshImporterSettings settings = new MeshImporterSettings();
        settings.readWriteEnabled = importer.isReadable;
        settings.optimiseMesh = importer.optimizeMesh;
        settings.ImportBlendShapes = importer.importBlendShapes;
        settings.animationType = importer.animationType;
        settings.importAnimaton = importer.importAnimation;
        settings.importMaterials = importer.importMaterials;
        return settings;
    }

    public static bool Equal(MeshImporterSettings a, MeshImporterSettings b)
    {
        return (a.readWriteEnabled == b.readWriteEnabled) 
            && (a.optimiseMesh == b.optimiseMesh) 
            && (a.ImportBlendShapes == b.ImportBlendShapes)
            && (a.animationType == b.animationType)
            && (a.importAnimaton == b.importAnimaton)
            && (a.importMaterials == b.importMaterials);
    }

    public void ApplyDefaults()
    {
        readWriteEnabled = false;
        optimiseMesh = true;
        ImportBlendShapes = false;
        importAnimaton = false;
        importMaterials = false;
        animationType = ModelImporterAnimationType.Generic;
        ApplyMeshReadWrite = false;
        applyAnimationType = false;
    }

    public void ApplyData(MeshInportSettingInfo data)
    {
        if (data == null)
        {
            ApplyDefaults();
            UnityEngine.Debug.LogError("MeshImportSetting:ApplyData error data is null, apply default instead!");
            return;
        }

        readWriteEnabled = data.readWriteEnabled;
        optimiseMesh = data.optimiseMesh;
        ImportBlendShapes = data.importBlendShapes;
        ApplyMeshReadWrite = data.applyMeshReadWrite;
        applyAnimationType = data.applyAnimationType;
        importAnimaton = data.importAnimaton;
        importMaterials = data.importMaterials;
        animationType = data.animationType;
    }
}