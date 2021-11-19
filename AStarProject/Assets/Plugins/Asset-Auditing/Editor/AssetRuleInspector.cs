using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AssetImportSettingRule))]
public class AssetRuleInspector : Editor
{
    /// <summary>
    /// 创建设置模板
    /// </summary>
    [MenuItem("Assets/AssetAuditing/Create AssetRule")]
    public static void CreateAssetRule()
    {
        var newRule = CreateInstance<AssetImportSettingRule>();
        newRule.ApplyDefaults();

        string selectionpath = "Assets";
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            selectionpath = AssetDatabase.GetAssetPath(obj);
            if (File.Exists(selectionpath))
            {
                selectionpath = Path.GetDirectoryName(selectionpath);
            }
            break;
        }

        string newRuleFileName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(selectionpath, "New Asset Rule.asset"));
        newRuleFileName = newRuleFileName.Replace("\\", "/");
        AssetDatabase.CreateAsset(newRule, newRuleFileName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newRule;
    }

    /// <summary>
    /// 应用项目中所有AssetRule
    /// </summary>
    [MenuItem("Assets/AssetAuditing/Apply All AssetRule")]

    public static void ApplyAllAssetImportSetting()
    {
        ApplyAllAssetRule();
        EditorUtility.DisplayDialog("Asset Autding"," Apply All AssetRule Finished!","Confirm");
    }

    public static void ApplyAllAssetRule()
    {
        AssetImportSettingRule.globalRuleActive = true;
        string resRootPath = "Assets/Arts";
        string pathName = string.Empty;
        foreach (var findAsset in AssetDatabase.FindAssets("t:AssetImportSettingRule", new[] { Path.GetDirectoryName(resRootPath) }))
        {
            pathName = AssetDatabase.GUIDToAssetPath(findAsset);
            Debug.Log(string.Format("Find assetRule path: {0}", pathName));

            AssetImportSettingRule rule = AssetDatabase.LoadAssetAtPath<AssetImportSettingRule>(pathName);
            if (rule != null)
                Apply(rule);
            else
            {
                 Debug.LogError("Load AssetRule Failed, path: " + pathName);
            }

        }

        AssetImportSettingRule.globalRuleActive = true;
    }
    
    /// <summary>
    /// Inspector绘制
    /// </summary>
    static bool changed = false;
    public override void OnInspectorGUI()
    {
        var t = (AssetImportSettingRule)target;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Rule Type");
        t.filter.type = (AssetFilterType)EditorGUILayout.EnumPopup(t.filter.type);
        EditorGUILayout.EndHorizontal();

        if (t.filter.type == AssetFilterType.kAny || t.filter.type == AssetFilterType.kTexture)
            DrawTextureSettings(t);

        if (t.filter.type == AssetFilterType.kAny || t.filter.type == AssetFilterType.kMesh)
            DrawMeshSettings(t);

        if (EditorGUI.EndChangeCheck ()) 
        {
            changed = true;    
        }

        if(changed)
        {
            if (GUILayout.Button("Apply"))
            {
                Apply(t);
                changed = false;
            }
        }
    }

    /// <summary>
    /// 应用设置模板
    /// </summary>
    /// <param name="assetRule"></param>
    private static void Apply(AssetImportSettingRule assetRule)
    {
        AssetImportSettingRule.globalRuleActive = true;

        var newRule = ScriptableObject.CreateInstance<AssetImportSettingRule>();
        newRule.filter = assetRule.filter;
        newRule.settings = assetRule.settings;
        var path = AssetDatabase.GetAssetPath(assetRule);
        assetRule = newRule;
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(newRule, path);
        AssetDatabase.SaveAssets();

        // get the directories that we do not want to apply changes to 
        List<string> dontapply = new List<string>();
        var assetrulepath = path.Replace(assetRule.name +".asset","").TrimEnd('/');
        string projPath = Application.dataPath;
        projPath = projPath.Remove(projPath.Length - 6);
 
        string[] directories = Directory.GetDirectories( Path.GetDirectoryName(projPath + AssetDatabase.GetAssetPath(assetRule)) ,"*",  SearchOption.AllDirectories);
        foreach (var directory in directories)
        {
            var d = directory.Replace(Application.dataPath, "Assets");
            var appDirs = AssetDatabase.FindAssets("t:AssetImportSettingRule", new[] {d});
            if (appDirs.Length != 0)
            {
                d = d.TrimEnd('/');
                d = d.Replace('\\', '/');
                dontapply.Add(d);
            }
        }


        List<string> finalAssetList = new List<string>();
        foreach (var findAsset in AssetDatabase.FindAssets("", new[] {assetrulepath}))
        {
            var asset = AssetDatabase.GUIDToAssetPath(findAsset);
            if (!File.Exists(asset)) continue;
            if (dontapply.Contains(Path.GetDirectoryName(asset))) continue;
            if (!assetRule.IsMatch(AssetImporter.GetAtPath(asset))) continue;
            if (finalAssetList.Contains(asset)) continue;
            if (asset == AssetDatabase.GetAssetPath(assetRule)) continue;
            finalAssetList.Add(asset);
        }

        foreach (var asset in finalAssetList)
        {
            try
            {
                AssetImporter importer = AssetImporter.GetAtPath(asset);
                if (!assetRule.AreCustomSettingsCorrect(importer))
                {
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                }
                else
                      Debug.Log(asset + " 资源符合模板设置，跳过!");
            }
            catch (System.Exception e)
            {
                 Debug.LogError("应用导入资源设置出错，path:" + asset + e.ToString()) ;
            }
        }

        AssetImportSettingRule.globalRuleActive = true;
    }

    /// <summary>
    /// 纹理尺寸选项
    /// </summary>
    private int[] sizes = new[] {32, 64, 128, 256, 512, 1024, 2048, 4096};
    private string[] sizeStrings = new[] {"32", "64", "128", "256", "512", "1024", "2048", "4096" };
	private string[] platformString = new []{"DefaultTexturePlatform","iPhone","Android"};

	/// <summary>
    /// 纹理质量等级
    /// </summary>
	private int[] compress = new int[] { 0,1,2,3};
	private string[] compressStrings = new string[]{ "None","Normal","Hight","Low"};

    /// <summary>
    /// 纹理压缩质量
    /// </summary>
    private int[] compressQuality = new int[] { 0, 50, 100};
    private string[] compressQualityStrings = new string[] { "Fast", "Normal", "Best" };

    /// <summary>
    /// IOS纹理压缩格式选项
    /// </summary>
	private string[] IOSCompressStrings = new string[]
		{ "RGB Compressed PVRTC 4 bits","RGBA Compressed PVRTC 4 bits",
		"RGB Compressed ASTC 4x4 block","RGBA Compressed ASTC 4x4 block",
		"RGB Compressed ASTC 5x5 block","RGBA Compressed ASTC 5x5 block",
		"RGB Compressed ASTC 6x6 block","RGBA Compressed ASTC 6x6 block",
		"RGB Compressed ASTC 8x8 block","RGBA Compressed ASTC 8x8 block"};

	private int[] IOSCompress = new int[] 
		{(int)TextureImporterFormat.PVRTC_RGB4,(int)TextureImporterFormat.PVRTC_RGBA4,
		(int)TextureImporterFormat.ASTC_RGB_4x4,(int)TextureImporterFormat.ASTC_RGBA_4x4,
		(int)TextureImporterFormat.ASTC_RGB_5x5,(int)TextureImporterFormat.ASTC_RGBA_5x5,
		(int)TextureImporterFormat.ASTC_RGB_6x6,(int)TextureImporterFormat.ASTC_RGBA_6x6,
		(int)TextureImporterFormat.ASTC_RGB_8x8,(int)TextureImporterFormat.ASTC_RGBA_8x8};

    /// <summary>
    /// Android纹理压缩格式选项
    /// </summary>
    private string[] AndroidCompressStrings = new string[]{
        "RGB Compressed DXT1",
        "RGBA Compressed DXT5",
		"RGB Compressed ETC 4 bits",
        "RGB Compressed ETC2 4 bits",
        "RGBA Compressed ETC2 8 bits",
        "RGB Compressed ASTC 4x4 block","RGBA Compressed ASTC 4x4 block",
        "RGB Compressed ASTC 5x5 block","RGBA Compressed ASTC 5x5 block",
        "RGB Compressed ASTC 6x6 block","RGBA Compressed ASTC 6x6 block",
        "RGB Compressed ASTC 8x8 block","RGBA Compressed ASTC 8x8 block" };

	private int[] AndroidCompress = new int[] {
        (int)TextureImporterFormat.DXT1,
        (int)TextureImporterFormat.DXT5,
		(int)TextureImporterFormat.ETC_RGB4,
        (int)TextureImporterFormat.ETC2_RGB4,
        (int)TextureImporterFormat.ETC2_RGBA8,
        (int)TextureImporterFormat.ASTC_RGB_4x4,(int)TextureImporterFormat.ASTC_RGBA_4x4,
        (int)TextureImporterFormat.ASTC_RGB_5x5,(int)TextureImporterFormat.ASTC_RGBA_5x5,
        (int)TextureImporterFormat.ASTC_RGB_6x6,(int)TextureImporterFormat.ASTC_RGBA_6x6,
        (int)TextureImporterFormat.ASTC_RGB_8x8,(int)TextureImporterFormat.ASTC_RGBA_8x8};

    /// <summary>
    /// Power of two 设置选项
    /// </summary>
	private int[] pot = new int[] { 0,1,2,3};
	private string[] potString = new string[] {"None","ToNearest","ToLarger","ToSmaller"};

	private string platform = "DefaultTexturePlatform";

    private bool foldTexture = true;
    private void DrawTextureSettings(AssetImportSettingRule assetRule)
    {
        GUILayout.Space(10);
        foldTexture = EditorGUILayout.Foldout(foldTexture, "TEXTURE SETTINGS ");
        if (foldTexture)
        {
            //read write enabled
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Read/Write Enabled");
            assetRule.settings.textureSettings.readable = EditorGUILayout.Toggle(assetRule.settings.textureSettings.readable);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            assetRule.settings.ApplyTextureMipMap = GUILayout.Toggle(assetRule.settings.ApplyTextureMipMap, "Apply MipMap Setting", GUILayout.Height(20));
            EditorGUILayout.EndHorizontal();

            if (assetRule.settings.ApplyTextureMipMap)
            {
                // mip maps
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.LabelField("Generate Mip Maps");
                assetRule.settings.textureSettings.mipmapEnabled = EditorGUILayout.Toggle(assetRule.settings.textureSettings.mipmapEnabled);
                EditorGUILayout.EndHorizontal();
            }

            // per platform settings
            //EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField("Max Texture Size");
            //assetRule.settings.textureSettings.maxTextureSize =
            //EditorGUILayout.IntPopup(assetRule.settings.textureSettings.maxTextureSize, sizeStrings, sizes);
            //EditorGUILayout.EndHorizontal();

            // Non power of 2
            EditorGUILayout.BeginHorizontal();
            assetRule.settings.ApplyPOT = GUILayout.Toggle(assetRule.settings.ApplyPOT, "Apply Power of 2", GUILayout.Height(20));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (assetRule.settings.ApplyPOT)
            {
                GUILayout.Space(20);
                EditorGUILayout.LabelField("Non power of 2");
                assetRule.settings.textureSettings.npotScale =
                    (TextureImporterNPOTScale)EditorGUILayout.IntPopup((int)assetRule.settings.textureSettings.npotScale, potString, pot);
            }

            EditorGUILayout.EndHorizontal();

            //compression setting
            EditorGUILayout.BeginHorizontal();
            assetRule.settings.ApplyPlatFormSetting = GUILayout.Toggle(assetRule.settings.ApplyPlatFormSetting, "ApplyPlatFormSetting", GUILayout.Height(20));
            EditorGUILayout.EndHorizontal();

            if (assetRule.settings.ApplyPlatFormSetting)
            {
                EditorGUILayout.BeginHorizontal();
                TextureImporterPlatformSettings platFormSetting = null;
                TextureImporterPlatformSettings selectPlatform = null;
                for (int i = 0; i < platformString.Length; i++)
                {
                    platFormSetting = assetRule.settings.GetTextureCompressSetting(platformString[i]);
                    if (platFormSetting != null)
                    {
                        if (platform.Equals(platformString[i]))
                        {
                            selectPlatform = platFormSetting;
                            GUI.color = Color.green;
                        }

                        if (GUILayout.Button(platformString[i]))
                        {    //, GUILayout.Width (80), GUILayout.Height (20)
                            platform = platformString[i];
                        }

                        GUI.color = Color.white;
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();

                if (selectPlatform != null)
                {
                    EditorGUILayout.BeginVertical();

                    if (!platform.Equals(platformString[0]))
                        selectPlatform.overridden = GUILayout.Toggle(selectPlatform.overridden, "override", GUILayout.Height(20));

                    //Android IOS 签需要
                    if (selectPlatform.overridden && !platform.Equals(platformString[0]))
                    {
                        selectPlatform.compressionQuality =
                       (int)EditorGUILayout.IntPopup("CompressionQuality", (int)selectPlatform.compressionQuality, compressQualityStrings, compressQuality);

                        //iphone
                        if (platform.Equals(platformString[1]))
                        {

                            selectPlatform.format =
                                (TextureImporterFormat)EditorGUILayout.IntPopup("Format",(int)selectPlatform.format, IOSCompressStrings, IOSCompress);
                        }
                        //android
                        else if (platform.Equals(platformString[2]))
                        {

                            selectPlatform.format =
                                (TextureImporterFormat)EditorGUILayout.IntPopup("Format", (int)selectPlatform.format, AndroidCompressStrings, AndroidCompress);
                        }
                    }
                    else if(platform.Equals(platformString[0]))
                    {
                        //只有Default签需要
                        selectPlatform.textureCompression =
                            (TextureImporterCompression)EditorGUILayout.IntPopup("Compression", (int)selectPlatform.textureCompression, compressStrings, compress);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
        }
        
    }

    private bool foldMesh = true;
    private void DrawMeshSettings(AssetImportSettingRule assetRule)
    {
        GUILayout.Space(10);

        foldMesh = EditorGUILayout.Foldout(foldMesh, "MESH SETTINGS ");
        if (foldMesh)
        {
            // read write enabled
            EditorGUILayout.BeginHorizontal();
            assetRule.settings.meshSettings.ApplyMeshReadWrite = GUILayout.Toggle(assetRule.settings.meshSettings.ApplyMeshReadWrite, "Apply MeshReadWrite Setting", GUILayout.Height(20));
            EditorGUILayout.EndHorizontal();
            if (assetRule.settings.meshSettings.ApplyMeshReadWrite)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.LabelField("Read/Write Enabled");
                assetRule.settings.meshSettings.readWriteEnabled = EditorGUILayout.Toggle(assetRule.settings.meshSettings.readWriteEnabled);
                EditorGUILayout.EndHorizontal();
            }

            // animation type enabled
            EditorGUILayout.BeginHorizontal();
            assetRule.settings.meshSettings.applyAnimationType = GUILayout.Toggle(assetRule.settings.meshSettings.applyAnimationType, "Apply AnimationType Setting", GUILayout.Height(20));
            EditorGUILayout.EndHorizontal();
            if (assetRule.settings.meshSettings.applyAnimationType)
            {
                // legacy animation
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Animation Type");
                assetRule.settings.meshSettings.animationType = (ModelImporterAnimationType)EditorGUILayout.EnumPopup(assetRule.settings.meshSettings.
                    animationType);
                EditorGUILayout.EndHorizontal();
            }
           
            // optimise mesh
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Optimize Mesh");
            assetRule.settings.meshSettings.optimiseMesh = EditorGUILayout.Toggle(assetRule.settings.meshSettings.optimiseMesh);
            EditorGUILayout.EndHorizontal();

            // optimise mesh
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Import BlendShapes");
            assetRule.settings.meshSettings.ImportBlendShapes = EditorGUILayout.Toggle(assetRule.settings.meshSettings.ImportBlendShapes);
            EditorGUILayout.EndHorizontal();
           
            //import animation
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Import Animation");
            assetRule.settings.meshSettings.importAnimaton = EditorGUILayout.Toggle(assetRule.settings.meshSettings.importAnimaton);
            EditorGUILayout.EndHorizontal();

            //import materials
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Import Materials");
            assetRule.settings.meshSettings.importMaterials = EditorGUILayout.Toggle(assetRule.settings.meshSettings.importMaterials);
            EditorGUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// 错误格式兼容，主要处理RGBA和RGB压缩格式之间的容错
    /// </summary>
    /// <param name="srcFormat"></param>
    /// <param name="isHaveAlpha"></param>
    /// <returns></returns>
    public static TextureImporterFormat CorrectionTextureFormat(TextureImporterFormat srcFormat,bool isHaveAlpha)
	{
		switch (srcFormat)
		{
		case TextureImporterFormat.PVRTC_RGB4:
			if (isHaveAlpha)
				srcFormat = TextureImporterFormat.PVRTC_RGBA4;
				break;
		case TextureImporterFormat.PVRTC_RGBA4:
			if (!isHaveAlpha)
				srcFormat = TextureImporterFormat.PVRTC_RGB4;
			break;

		case TextureImporterFormat.ASTC_RGB_4x4:
			if (isHaveAlpha)
				srcFormat = TextureImporterFormat.ASTC_RGBA_4x4;
			break;
		case TextureImporterFormat.ASTC_RGBA_4x4:
			if (!isHaveAlpha)
				srcFormat = TextureImporterFormat.ASTC_RGB_4x4;
			break;

		case TextureImporterFormat.ASTC_RGB_5x5:
			if (isHaveAlpha)
				srcFormat = TextureImporterFormat.ASTC_RGBA_5x5;
			break;
		case TextureImporterFormat.ASTC_RGBA_5x5:
			if (!isHaveAlpha)
				srcFormat = TextureImporterFormat.ASTC_RGB_5x5;
			break;

		case TextureImporterFormat.ASTC_RGB_6x6:
			if (isHaveAlpha)
				srcFormat = TextureImporterFormat.ASTC_RGBA_6x6;
			break;
		case TextureImporterFormat.ASTC_RGBA_6x6:
			if (!isHaveAlpha)
				srcFormat = TextureImporterFormat.ASTC_RGB_6x6;
			break;

		case TextureImporterFormat.ASTC_RGB_8x8:
			if (isHaveAlpha)
				srcFormat = TextureImporterFormat.ASTC_RGBA_8x8;
			break;
		case TextureImporterFormat.ASTC_RGBA_8x8:
			if (!isHaveAlpha)
				srcFormat = TextureImporterFormat.ASTC_RGB_8x8;
			break;
		}

		return srcFormat;
	}
}