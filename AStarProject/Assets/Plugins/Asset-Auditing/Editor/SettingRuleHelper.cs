using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

public class AssetSettingRuleHelper
{
    private static AssetSettingRuleHelper _instance;
    /// <summary>
    /// 单例
    /// </summary>
    public static AssetSettingRuleHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AssetSettingRuleHelper();
            }
            return _instance;
        }
    }
}


/// <summary>
/// 资源设置信息
/// </summary>
public class AssetSettingInfo
{
    /// <summary>
    /// 规则文件路径
    /// </summary>
    public string path { get; set; }

    /// <summary>
    /// filter信息
    /// </summary>
    public FilterInfo filter { get; set; }

    /// <summary>
    /// Setting Info
    /// </summary>
    public AssetRuleSettingInfo settings { get; set; }

    public void InjectAssetSettingInfo(string path, AssetImportSettingRule rule)
    {
        this.path = path;
        this.filter = new FilterInfo();
        this.filter.InjectFilterInfo(rule.filter);
        this.settings = new AssetRuleSettingInfo();
        this.settings.InjectAssetRuleSettingInfo(rule.settings);
    }
}

/// <summary>
/// 设置文件类型信息
/// </summary>
public class FilterInfo
{
    public AssetFilterType type { get; set; }

    public string path { get; set; }

    public void InjectFilterInfo(AssetFilter filter)
    {
        this.type = filter.type;
        this.path = filter.path;
    }
}

/// <summary>
/// 设置文件详细信息
/// </summary>
public class AssetRuleSettingInfo
{
    public void InjectAssetRuleSettingInfo(AssetRuleImportSettings setting)
    {
        this.applyPlatformSetting = setting.ApplyPlatFormSetting;
        this.applyPOT = setting.ApplyPOT;
        this.applyMipMap = setting.ApplyTextureMipMap;
        this.textureSettings = new TextureImportSettingInfo();
        this.textureSettings.InjectTextureImportSettingInfo(setting.textureSettings);
        this.meshSettings = new MeshInportSettingInfo();
        this.meshSettings.InjectMeshInportSettingInfo(setting.meshSettings);

        this.textureCompressDefault = new PlatformSettingInfo();
        this.textureCompressDefault.InjectPlatformSettingInfo(setting.textureCompressDefault);
        this.textureCompressIOS = new PlatformSettingInfo();
        this.textureCompressIOS.InjectPlatformSettingInfo(setting.textureCompressIOS);
        this.textureCompressAndroid = new PlatformSettingInfo();
        this.textureCompressAndroid.InjectPlatformSettingInfo(setting.textureCompressAndroid);
    }

    public bool applyPlatformSetting { get; set; }

    public bool applyPOT { get; set; }

    public bool applyMipMap { get; set; }

    /// <summary>
    /// 纹理导入设置
    /// </summary>
    public TextureImportSettingInfo textureSettings { get; set; }

    /// <summary>
    /// 网格导入设置
    /// </summary>
    public MeshInportSettingInfo meshSettings { get; set; }

    /// <summary>
    ///纹理默认压缩设置
    /// </summary>
    public PlatformSettingInfo textureCompressDefault { get; set; }

    /// <summary>
    ///纹理IOS压缩设置
    /// </summary>
    public PlatformSettingInfo textureCompressIOS { get; set; }

    /// <summary>
    ///纹理Android压缩设置
    /// </summary>
    public PlatformSettingInfo textureCompressAndroid { get; set; }
}

/// <summary>
/// 纹理设置信息
/// </summary>
public class TextureImportSettingInfo
{
    public void InjectTextureImportSettingInfo(TextureImporterSettings txSetting)
    {
        this.mipmapEnabled = txSetting.mipmapEnabled;
        this.readable = txSetting.readable;
        this.npotScale = txSetting.npotScale;
    }

    public bool mipmapEnabled { get; set; }

    public bool readable { get; set; }

    public TextureImporterNPOTScale npotScale { get; set; }
}

/// <summary>
/// 网格设置信息
/// </summary>
public class MeshInportSettingInfo
{
    public void InjectMeshInportSettingInfo(MeshImporterSettings setting)
    {
        this.applyMeshReadWrite = setting.ApplyMeshReadWrite;
        this.applyAnimationType = setting.applyAnimationType;
        this.readWriteEnabled = setting.readWriteEnabled;
        this.optimiseMesh = setting.optimiseMesh;
        this.importBlendShapes = setting.ImportBlendShapes;
        this.importAnimaton = setting.importAnimaton;
        this.animationType = setting.animationType;
        this.importMaterials = setting.importMaterials;
    }

    public bool applyMeshReadWrite { get; set; }

    public bool applyAnimationType { get; set; }

    public bool readWriteEnabled { get; set; }

    public bool optimiseMesh { get; set; }

    public bool importBlendShapes { get; set; }

    public bool importAnimaton { get; set; }

    public ModelImporterAnimationType animationType { get; set; }

    public bool importMaterials { get; set; }
}

/// <summary>
/// 纹理平台压缩设置
/// </summary>
public class PlatformSettingInfo
{
    public void InjectPlatformSettingInfo(TextureImporterPlatformSettings setting)
    {
        this.name = setting.name;
        this.overridden = setting.overridden;
        this.textureCompression = setting.textureCompression;
        this.compressionQuality = setting.compressionQuality;
        this.format = setting.format;
    }

    public string name { get; set; }

    public bool overridden { get; set; }

    public TextureImporterCompression textureCompression { get; set; }

    public int compressionQuality { get; set; }

    public TextureImporterFormat format { get; set; }
}

