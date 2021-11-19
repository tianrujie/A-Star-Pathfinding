//#define ENABLE_LOG

using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;

[System.Serializable]
public enum AssetFilterType
{
	kAny,
	kTexture,
	kMesh
}

[System.Serializable]
public class AssetImportSettingRule : ScriptableObject
{
    /// <summary>
    /// 资源设置的全局开关，默认为关闭
    /// </summary>
	public static bool globalRuleActive = true;
	
	public AssetFilter filter;
	public AssetRuleImportSettings settings;

    public static AssetImportSettingRule CreateAssetRule()
    {
        var assetRule = AssetImportSettingRule.CreateInstance<AssetImportSettingRule>();

        assetRule.ApplyDefaults();

        return assetRule;
    }

    public static AssetImportSettingRule CreateAssetRule(AssetSettingInfo data)
    {
        var assetRule = AssetImportSettingRule.CreateInstance<AssetImportSettingRule>();

        assetRule.ApplyData(data);

        return assetRule;
    }

    public void ApplyDefaults()
    {
        filter.ApplyDefaults();
        settings.ApplyDefaults();
    }

    public void ApplyData(AssetSettingInfo data)
    {
        filter.ApplyData(data.filter);
        settings.ApplyData(data.settings);
    }

    public bool IsMatch(AssetImporter importer)
	{
		return filter.IsMatch(importer);
	}

	public bool AreSettingsCorrect(AssetImporter importer)
	{
		return settings.AreSettingsCorrect(importer);
	}

    public bool AreCustomSettingsCorrect(AssetImporter importer)
    {
        return settings.AreCustomSettingsCorrect(importer);
    }

    public void ApplySettings(AssetImporter importer)
	{
		if(!globalRuleActive)
			return;
			
		settings.Apply(importer);
	}
}

[System.Serializable]
public struct AssetFilter
{
	public AssetFilterType type;
	public string path;

	public bool IsMatch(AssetImporter importer)
	{
	    if (importer == null) return false;

		AssetFilterType filterType = GetAssetFilterType(importer);
		return IsMatch(filterType, importer.assetPath);
	}

	public bool IsMatch(string path)
	{
	    if (string.IsNullOrEmpty(this.path)) return true;
		if(string.IsNullOrEmpty(path)) { return string.IsNullOrEmpty(this.path); }

		string fullPath = Path.Combine(Application.dataPath, path);

		string[] files = Directory.GetFiles(Application.dataPath, this.path);
		if(files == null)
			return false;

		for(int i = 0; i < files.Length; i++)
		{
			if(fullPath.Equals(files[i]))
				return true;
		}

		return false;
	}

	public bool IsMatch(AssetFilterType type, string path)
	{
		return (this.type == AssetFilterType.kAny || type == this.type) &&
			IsMatch(path);
	}

	public static AssetFilterType GetAssetFilterType(AssetImporter importer)
	{
		if(importer is TextureImporter)
			return AssetFilterType.kTexture;
		else if(importer is ModelImporter)
			return AssetFilterType.kMesh;

		return AssetFilterType.kAny;
	}

    public void ApplyDefaults()
    {
        type = AssetFilterType.kAny;
        path = "";
    }

    public void ApplyData(FilterInfo data)
    {
        if (data == null)
        {
            ApplyDefaults();
            return;
        }

        this.type = data.type;
        this.path = data.path;
    }
}


[System.Serializable]
public struct AssetRuleImportSettings
{
	public TextureImporterSettings textureSettings;
	public MeshImporterSettings meshSettings;
	public TextureImporterPlatformSettings textureCompressDefault;
	public TextureImporterPlatformSettings textureCompressIOS;
	public TextureImporterPlatformSettings textureCompressAndroid;
	public bool ApplyPlatFormSetting;
	public bool ApplyPOT;
	public bool ApplyTextureMipMap;

	public bool AreSettingsCorrect(AssetImporter importer)
	{
		if(importer is TextureImporter)
			return AreSettingsCorrectTexture((TextureImporter)importer);
		else if(importer is ModelImporter)
			return AreSettingsCorrectModel((ModelImporter)importer);

		return true;
	}

    public bool AreCustomSettingsCorrect(AssetImporter importer)
    {
        if (importer is TextureImporter)
            return AreCustomSettingCorrectTexture((TextureImporter)importer);
        else if (importer is ModelImporter)
            return AreCustomSettingsCorrectModel((ModelImporter)importer);

        return true;
    }

    bool AreSettingsCorrectTexture(TextureImporter importer)
	{
		TextureImporterSettings currentSettings = new TextureImporterSettings();
		importer.ReadTextureSettings(currentSettings);

		return TextureImporterSettings.Equal(currentSettings, this.textureSettings);
	}

    bool AreSettingsCorrectModel(ModelImporter importer)
	{
		var currentSettings = MeshImporterSettings.Extract(importer);
		return MeshImporterSettings.Equal(currentSettings, this.meshSettings);
	}

    bool AreCustomSettingCorrectTexture(TextureImporter importer)
    {
        bool dirty = false;
        TextureImporterSettings tis = new TextureImporterSettings();
        importer.ReadTextureSettings(tis);
        if (!tis.mipmapEnabled == textureSettings.mipmapEnabled && ApplyTextureMipMap)
            dirty = true;
        if (!tis.readable == textureSettings.readable)
            dirty = true;

        if (tis.npotScale != textureSettings.npotScale && ApplyPOT)
            dirty = true;

        if (ApplyPlatFormSetting)
        {
            if (textureCompressDefault != null && !CompareTextureCompress(textureCompressDefault, importer.GetDefaultPlatformTextureSettings()))
                dirty = true;

            if (textureCompressIOS != null && !textureCompressIOS.name.Equals("DefaultTexturePlatform")
                && !CompareTextureCompress(textureCompressIOS, importer.GetPlatformTextureSettings("iPhone")))
                dirty = true;

            if (textureCompressAndroid != null && !textureCompressAndroid.name.Equals("DefaultTexturePlatform")
                && !CompareTextureCompress(textureCompressAndroid, importer.GetPlatformTextureSettings("Android")))
                dirty = true;
        }

        return !dirty;
    }

    bool AreCustomSettingsCorrectModel(ModelImporter importer)
    {
        bool dirty = false;
        if (importer.isReadable != meshSettings.readWriteEnabled && meshSettings.ApplyMeshReadWrite)
            dirty = true;

        if (importer.optimizeMesh != meshSettings.optimiseMesh)
            dirty = true;

        if (importer.importBlendShapes != meshSettings.ImportBlendShapes)
            dirty = true;

        if (importer.animationType != ModelImporterAnimationType.None && importer.animationType != meshSettings.animationType && meshSettings.applyAnimationType)
            dirty = true;

        if (importer.importAnimation != meshSettings.importAnimaton)
            dirty = true;

        if (importer.importAnimation != meshSettings.importAnimaton)
            dirty = true;

        if (importer.importMaterials != meshSettings.importMaterials)
            dirty = true;

        return !dirty;
    }

    public void Apply(AssetImporter importer)
	{
		if(importer is TextureImporter)
			ApplyTextureSettings((TextureImporter)importer);
		else if(importer is ModelImporter)
			ApplyMeshSettings((ModelImporter)importer);
	}

	public TextureImporterPlatformSettings GetTextureCompressSetting(string platform)
	{
		if (textureCompressDefault != null && textureCompressDefault.name.Equals (platform))
			return textureCompressDefault;

		if (textureCompressIOS != null && textureCompressIOS.name.Equals (platform))
			return textureCompressIOS;

		if (textureCompressAndroid != null && textureCompressAndroid.name.Equals (platform))
			return textureCompressAndroid;

		return null;
	}
		
	//自定义压缩比较
	bool CompareTextureCompress(TextureImporterPlatformSettings A,TextureImporterPlatformSettings B)
	{
// 		if (A.name.Equals ("DefaultTexturePlatform") || B.name.Equals ("DefaultTexturePlatform"))
//			return true;

		if (!A.name.Equals (B.name))
			return false;

		if (A.overridden != B.overridden)
			return false;
		
		if (A.textureCompression != B.textureCompression)
			return false;

		if (!TextureFormatCompare(A.format,B.format))
			return false;

		return true;
	}

	bool TextureFormatCompare(TextureImporterFormat A,TextureImporterFormat B)
	{
		if (A == B)
			return true;

		if ((AssetRuleInspector.CorrectionTextureFormat (A, true) == AssetRuleInspector.CorrectionTextureFormat (B, true))&&
			(AssetRuleInspector.CorrectionTextureFormat (A, false) == AssetRuleInspector.CorrectionTextureFormat (B, false)))
			return true;

		return false;
	}

	void ApplyTextureSettings(TextureImporter importer)
	{
	    bool dirty = false;
		TextureImporterSettings tis = new TextureImporterSettings();
	    importer.ReadTextureSettings(tis);
		if (!tis.mipmapEnabled == textureSettings.mipmapEnabled && ApplyTextureMipMap)
	    {
	        tis.mipmapEnabled = textureSettings.mipmapEnabled;    
	        dirty = true;
	    }
	    if (!tis.readable == textureSettings.readable)
	    {
	        tis.readable = textureSettings.readable;
	        dirty = true;
	    }

        //as maxTextureSize attribute is obsolete, dont control it
        //if (tis.maxTextureSize > textureSettings.maxTextureSize)
        //{
        //    tis.maxTextureSize = textureSettings.maxTextureSize;
        //    dirty = true;
        //}
        if (tis.npotScale != textureSettings.npotScale && ApplyPOT) 
		{
			tis.npotScale = textureSettings.npotScale;
			dirty = true;
		}

        // add settings as needed

		if (ApplyPlatFormSetting) {
			if (textureCompressDefault != null && !CompareTextureCompress(textureCompressDefault,importer.GetDefaultPlatformTextureSettings())) {
				importer.SetPlatformTextureSettings (textureCompressDefault);
				dirty = true;
			}

			if (textureCompressIOS != null && !textureCompressIOS.name.Equals("DefaultTexturePlatform")
				&& !CompareTextureCompress(textureCompressIOS,importer.GetPlatformTextureSettings("iPhone"))) {
				importer.SetPlatformTextureSettings (textureCompressIOS);
				dirty = true;
			}

			if (textureCompressAndroid != null && !textureCompressAndroid.name.Equals("DefaultTexturePlatform")
				&& !CompareTextureCompress(textureCompressAndroid,importer.GetPlatformTextureSettings("Android"))) {
				importer.SetPlatformTextureSettings (textureCompressAndroid);
				dirty = true;
			}

			//AdjustForAlpha (importer);
		}

	    if (dirty)
	    {
            Debug.Log("Modifying texture settings");
            importer.SetTextureSettings(tis);
	        importer.SaveAndReimport();
	    }
	    else
	    {
            Debug.Log("Texture Import Settings are Ok");	  
	    }
	}

	void ApplyMeshSettings(ModelImporter importer)
	{
		bool dirty = false;
		if(importer.isReadable != meshSettings.readWriteEnabled && meshSettings.ApplyMeshReadWrite)
		{
			importer.isReadable = meshSettings.readWriteEnabled;
			dirty = true;
		}

	    if (importer.optimizeMesh != meshSettings.optimiseMesh)
	    {
	        importer.optimizeMesh = meshSettings.optimiseMesh;
	        dirty = true;
	    }

	    if (importer.importBlendShapes != meshSettings.ImportBlendShapes)
	    {
	        importer.importBlendShapes = meshSettings.ImportBlendShapes;
	        dirty = true;
	    }

        if(importer.animationType != ModelImporterAnimationType.None && importer.animationType != meshSettings.animationType && meshSettings.applyAnimationType)
        {
            importer.animationType = meshSettings.animationType;
            dirty = true;
        }
		
		if (importer.importAnimation != meshSettings.importAnimaton)
		{
			importer.importAnimation = meshSettings.importAnimaton;
			dirty = true;
		}

        if (importer.importAnimation != meshSettings.importAnimaton)
        {
            importer.importAnimation = meshSettings.importAnimaton;
            dirty = true;
        }
        
        if (importer.importMaterials != meshSettings.importMaterials)
        {
            importer.importMaterials = meshSettings.importMaterials;
            dirty = true;
        }

        // Add more settings in here that you might need

        if (dirty)
	    {
#if ENABLE_LOG
            Debug.Log("Modifying Model Import Settings, An Import will now occur and the settings will be checked to be OK again during that import");
#endif
			importer.SaveAndReimport();
	    }
	    else
	    {
#if ENABLE_LOG
	        Debug.Log("Model Import Settings OK");
#endif
		}
	}

    public void ApplyDefaults()
    {
        meshSettings.ApplyDefaults();
        ApplyTextureSettingDefaults();
		ApplyTexturePlatFormCompressSettingDefault();
    }

    public void ApplyData(AssetRuleSettingInfo data)
    {
        if (data == null)
        {
            ApplyDefaults();
            Debug.LogError("AssetRuleImportSettings:ApplyData error data is null, apply default instead!");
            return;
        }
        meshSettings.ApplyData(data.meshSettings);
        ApplyTextureSettingData(data);
        ApplyTexturePlatFormCompressSettingData(data);
    }

    private void ApplyTextureSettingDefaults()
    {
        textureSettings = new TextureImporterSettings();
#if ENABLE_LOG
        Debug.Log("texture setting defaults");
#endif
        //textureSettings.maxTextureSize = 2048;
        textureSettings.mipmapEnabled = true;
		textureSettings.npotScale = TextureImporterNPOTScale.None;
		ApplyPlatFormSetting = false;
		ApplyPOT = false;
		ApplyTextureMipMap = true;
    }

    private void ApplyTextureSettingData(AssetRuleSettingInfo data)
    {
        textureSettings = new TextureImporterSettings();
        textureSettings.mipmapEnabled = data.textureSettings.mipmapEnabled;
        textureSettings.npotScale = data.textureSettings.npotScale;
        textureSettings.readable = data.textureSettings.readable;
        ApplyPlatFormSetting = data.applyPlatformSetting;
        ApplyPOT = data.applyPOT;
        ApplyTextureMipMap = data.applyMipMap;
    }

    private void ApplyTexturePlatFormCompressSettingDefault()
	{
		textureCompressDefault = new TextureImporterPlatformSettings ();
		textureCompressDefault.overridden = false;
		textureCompressDefault.name = "DefaultTexturePlatform";
		textureCompressDefault.textureCompression = TextureImporterCompression.Compressed;
        textureCompressDefault.compressionQuality = (int)UnityEngine.TextureCompressionQuality.Best;

        textureCompressIOS = new TextureImporterPlatformSettings ();
		textureCompressIOS.name = "iPhone";
		textureCompressIOS.overridden = false;
		textureCompressIOS.textureCompression = TextureImporterCompression.Compressed;
		textureCompressIOS.format = TextureImporterFormat.ASTC_RGBA_6x6;
        textureCompressIOS.compressionQuality = (int)UnityEngine.TextureCompressionQuality.Best;

        textureCompressAndroid = new TextureImporterPlatformSettings ();
		textureCompressAndroid.name = "Android";
		textureCompressAndroid.overridden = false;
		textureCompressAndroid.textureCompression = TextureImporterCompression.Compressed;
		textureCompressAndroid.format = TextureImporterFormat.ETC2_RGBA8;
        textureCompressAndroid.compressionQuality = (int)UnityEngine.TextureCompressionQuality.Best;
    }

    private void ApplyTexturePlatFormCompressSettingData(AssetRuleSettingInfo data)
    {
        textureCompressDefault = new TextureImporterPlatformSettings();
        textureCompressDefault.overridden = data.textureCompressDefault.overridden;
        textureCompressDefault.name = data.textureCompressDefault.name;
        textureCompressDefault.textureCompression = data.textureCompressDefault.textureCompression;
        textureCompressDefault.compressionQuality = data.textureCompressDefault.compressionQuality;

        textureCompressIOS = new TextureImporterPlatformSettings();
        textureCompressIOS.name = data.textureCompressIOS.name;
        textureCompressIOS.overridden = data.textureCompressIOS.overridden;
        textureCompressIOS.textureCompression = data.textureCompressIOS.textureCompression;
        textureCompressIOS.format = data.textureCompressIOS.format;
        textureCompressIOS.compressionQuality = data.textureCompressIOS.compressionQuality;

        textureCompressAndroid = new TextureImporterPlatformSettings();
        textureCompressAndroid.name = data.textureCompressAndroid.name;
        textureCompressAndroid.overridden = data.textureCompressAndroid.overridden;
        textureCompressAndroid.textureCompression = data.textureCompressAndroid.textureCompression;
        textureCompressAndroid.format = data.textureCompressAndroid.format;
        textureCompressAndroid.compressionQuality = data.textureCompressAndroid.compressionQuality;
    }

    void AdjustForAlpha(TextureImporter importer)
	{
		//ios
		TextureImporterPlatformSettings iosSet = importer.GetPlatformTextureSettings("iPhone");
		if (iosSet.overridden)
		{
			iosSet.format = AssetRuleInspector.CorrectionTextureFormat (iosSet.format,importer.DoesSourceTextureHaveAlpha());
			importer.SetPlatformTextureSettings (iosSet);
		}

		//android
		TextureImporterPlatformSettings androidSet = importer.GetPlatformTextureSettings("Android");
		if (androidSet.overridden)
		{

		}
	}
}
