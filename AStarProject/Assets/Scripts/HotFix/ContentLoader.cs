using System;
using System.Collections;
using Boo.Lang;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ContentLoader : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(ConnectDirectorServer());
    }

    private IEnumerator ConnectDirectorServer()
    {
        var platform = "Android";
        var version = "v0.0.1";
        var directorVersionConfUrl = "http://10.7.18.41/ContentDeliveryFolder/" + platform + "/" + version+ ".txt";
        
        WWWForm wwwForm = new WWWForm();
        wwwForm.AddField(version,version);
        UnityWebRequest unityWebRequest = UnityWebRequest.Post(directorVersionConfUrl,wwwForm);
        yield return unityWebRequest.SendWebRequest();
        
        if (unityWebRequest.isHttpError || unityWebRequest.isNetworkError)
        {
            Debug.LogError($"Connect to {directorVersionConfUrl} Error:" + unityWebRequest.error);
        }
        else
        {
            Debug.Log($" Connect to {directorVersionConfUrl} sucess, get version ${unityWebRequest.downloadHandler.text}");
            StartCoroutine(InitAddressable());
        }
    }
    
    private IEnumerator InitAddressable()
    {
        Debug.Log("Begin InitAddressable System");
        var platform = "Android";
        var version = "v0.0.1";
        HotFix.HotFixURL = "http://10.7.18.41/ContentDeliveryFolder/" + platform + "/" + version;
        AsyncOperationHandle<IResourceLocator> initHandle = Addressables.InitializeAsync();
        yield return initHandle;
        
        var asyncOperationCheckHandle = Addressables.CheckForCatalogUpdates(false);
        yield return asyncOperationCheckHandle;

        if (asyncOperationCheckHandle.Status == AsyncOperationStatus.Succeeded)
        {
            var catalogs = asyncOperationCheckHandle.Result;
            Debug.Log("CataLogs Count " + catalogs.Count);
            if (catalogs != null && catalogs.Count > 0)
            {
                Debug.Log("Begin Update Catalog here");
                //??????????????????
                var asyncOperationUpdateHandle = Addressables.UpdateCatalogs(catalogs, false);
                yield return asyncOperationUpdateHandle;
                
                //?????????
                float allsize = 0.0f;
                float downloadsize = 0.0f;
                
                //?????????????????????
                var resourceLocators = asyncOperationUpdateHandle.Result;
                if (resourceLocators.Count > 0)
                {
                    Debug.Log("We have some resourceLocator to Update ,Count:" + resourceLocators.Count);
                }
                //??????????????????
                foreach (var v in resourceLocators)
                {
                    List<object> keys = new List<object>();
                    keys.AddRange(v.Keys);
                    
                    var asyncOperationDownloadHandle = Addressables.GetDownloadSizeAsync(keys);
                    yield return asyncOperationDownloadHandle;
                    
                    long size = asyncOperationDownloadHandle.Result;
                    if (size > 0)
                    {
                        float mb = size / 1024.0f / 1024.0f;
                        allsize += mb;
                        float lastloadmb = 0.0f;
                        //????????????
                        var downloadHandle = Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union,false);
                        while (!downloadHandle.IsDone)
                        {
                            //????????????
                            downloadsize += (mb * downloadHandle.PercentComplete - lastloadmb);
                            lastloadmb = mb * downloadHandle.PercentComplete;
                            yield return null;
                        }
                        //??????????????????
                        downloadsize += (mb - lastloadmb);
                        //??????
                        Addressables.Release(downloadHandle);
                    }
                }
                Addressables.Release(asyncOperationUpdateHandle);
                Debug.Log($"Down Load Finish, total Size {allsize} MB!");
            }
            else
            {
                Debug.Log("We dont have any changed assets need to update!");
            }
        }
        else
        {
            Debug.LogError("Check Update Catalog Failed!");
        }
        
        Addressables.Release(asyncOperationCheckHandle);
        
        //load??????
        MapDirector.Instance.AddressableLoadTest();
    }
}