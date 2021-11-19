//#define ENABLE_LOG
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetImportCop : AssetPostprocessor
{
    /// <summary>
    ///  资源规则查找（可访问接口）
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    AssetImportSettingRule FindRuleForAsset(string path)
    {
        return SearchRecursive(path);
    }

    /// <summary>
    /// 资源规则查找
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private AssetImportSettingRule SearchRecursive(string path)
    {
        foreach (var findAsset in AssetDatabase.FindAssets("t:AssetImportSettingRule", new[] {Path.GetDirectoryName(path)}))
        {
            var p = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(findAsset));
            if (p == Path.GetDirectoryName(path))
            {
                {
                    return AssetDatabase.LoadAssetAtPath<AssetImportSettingRule>(AssetDatabase.GUIDToAssetPath(findAsset));
                }
            }
        }

		try
		{
			//no match so go up a level
			path = Directory.GetParent(path).FullName;
			path = path.Replace('\\','/');
			path = path.Remove(0, Application.dataPath.Length);
			path = path.Insert(0, "Assets");
		}
		catch (Exception e)
		{
			Debug.Log("Path parse error: " + path);
			return null;
		}

        Debug.Log("Searching: " + path);
        if (path != "Assets")
            return SearchRecursive(path);

        //no matches
        return null;
    }

    /// <summary>
    /// 全类型资源的导入回调
    /// </summary>
    /// <param name="importedAsset"></param>
    /// <param name="deletedAssets"></param>
    /// <param name="movedAssets"></param>
    /// <param name="movedFromAssetPaths"></param>
    public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string assetPath in movedAssets)
        {
            AssetImportCop local = new AssetImportCop();
            AssetImportSettingRule rule = local.FindRuleForAsset(assetPath);

            if (rule != null && rule.name == "SkipRule")
            {
                return;
            }

            if (rule == null)
            {
                return;
            }

            var tempImporter = AssetImporter.GetAtPath(assetPath);
            rule.ApplySettings(tempImporter);
        }
    }

    /// <summary>
    /// 纹理资源的导入回调
    /// </summary>
    public void OnPreprocessTexture()
    {
        AssetImportSettingRule rule = FindRuleForAsset(assetImporter.assetPath);

        if (rule != null && rule.name == "SkipRule")
        {
            return;
        }

        if (rule == null)
        {
            Debug.Log("No asset rules found for asset");
            return;
        }

        rule.ApplySettings(assetImporter);
    }

    /// <summary>
    /// 模型资源的导入回调
    /// </summary>
    public void OnPreprocessModel()
    {
        AssetImportSettingRule rule = FindRuleForAsset(assetImporter.assetPath);

		if(rule != null && rule.name == "SkipRule")
        {
            return;
        }

        if (rule == null)
        {
#if ENABLE_LOG
            Debug.Log("No asset rules found for asset");
#endif

            return;
        }
        rule.ApplySettings(assetImporter);
    }
}
