﻿//======================================================================
//        Copyright (C) 2015-2020 Winddy He. All rights reserved
//        Email: hgplan@126.com
//======================================================================
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using Core;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

namespace UnityEngine.AssetBundles
{    
    /// <summary>
    /// 加载资源的管理类，用作资源的加载管理
    /// 采用缓存Assetbundle对象的策略，半自动的引用计数
    /// </summary>
    public class ABLoader : MonoBehaviour
    {
        public class LoaderRequest
        {
            public Object                   Asset;
            public Scene                    Scene;
            public string                   Path;
            public string                   AssetName;
            public bool                     IsScene;
            public bool                     IsSimulate;

            public LoaderRequest(string rPath, string rAssetName, bool bIsScene, bool bIsSimulate)
            {
                this.Path                   = rPath;
                this.AssetName              = rAssetName;
                this.IsScene                = bIsScene;
                this.IsSimulate             = bIsSimulate;
            }
        }

        private static ABLoader             __instance;
        public  static ABLoader             Instance { get { return __instance; } }

        private List<string>                LoadedAssetbundles;

        private List<string>                LoadedScenebundles;

        void Awake()
        {
            if (__instance == null)
            {
                __instance = this;
    
                this.LoadedAssetbundles = new List<string>();
                this.LoadedScenebundles = new List<string>();
            }
        }
        
        public async Task<LoaderRequest> LoadAsset(string rAssetbundleName, string rAssetName, bool bIsSimulate)
        {
            if (!this.LoadedAssetbundles.Contains(rAssetbundleName))
                this.LoadedAssetbundles.Add(rAssetbundleName);

            LoaderRequest rRequest = new LoaderRequest(rAssetbundleName, rAssetName, false, bIsSimulate);
            await LoadAsset_Async(rRequest);
            return rRequest;
        }

        public async Task<LoaderRequest> LoadScene(string rAssetbundleName, string rScenePath)
        {
            if (!this.LoadedScenebundles.Contains(rAssetbundleName))
                this.LoadedScenebundles.Add(rAssetbundleName);
            
            LoaderRequest rSceneRequest = new LoaderRequest(rAssetbundleName, rScenePath, true, false);
            await LoadAsset_Async(rSceneRequest);
            return rSceneRequest;
        }
        
        public void UnloadAllLoadedAssetbundles()
        {
            for (int i = 0; i < this.LoadedAssetbundles.Count; i++)
            {
                UnloadAsset(this.LoadedAssetbundles[i]);
            }
            this.LoadedAssetbundles.Clear();
            Resources.UnloadUnusedAssets();
        }
    
        public void UnloadAsset(string rAssetbundleName)
        {
            ABLoadEntry rAssetLoadEntry = null;
            if (!ABLoaderVersion.Instance.TryGetValue(rAssetbundleName, out rAssetLoadEntry))
            {
                Debug.LogErrorFormat("Can not find assetbundle: -- {0}", rAssetbundleName);
                return;
            }
    
            // 递归遍历该资源的依赖项
            for (int i = 0; i < rAssetLoadEntry.ABDependNames.Length; i++)
            {
                UnloadAsset(rAssetLoadEntry.ABDependNames[i]);
            }
    
            // 引用计数减1
            rAssetLoadEntry.RefCount--;
            
            //确定该Info的引用计数是否为0，如果为0则删除它
            if (rAssetLoadEntry.RefCount == 0)
            {
                if (rAssetLoadEntry.CacheAsset != null)
                {
                    Debug.LogFormat("-- Real unload assetbundle: {0}", rAssetbundleName);
                    rAssetLoadEntry.CacheAsset.Unload(true);
                    rAssetLoadEntry.CacheAsset = null;
                }
                rAssetLoadEntry.IsLoadCompleted = false;
                rAssetLoadEntry.IsLoading = false;
            }
        }
    
        private async Task LoadAsset_Async(LoaderRequest rRequest)
        {
            ABLoadEntry rAssetLoadEntry = null;
            if (!ABLoaderVersion.Instance.TryGetValue(rRequest.Path, out rAssetLoadEntry))
            {
                Debug.LogErrorFormat("Can not find assetbundle: -- {0}", rRequest.Path);
                rRequest.Asset = null;
                return;
            }
            
            // 确认未加载完成并且正在被加载、一直等待其加载完成
            while (rAssetLoadEntry.IsLoading && !rAssetLoadEntry.IsLoadCompleted)
            {
                await new WaitForEndOfFrame();
            }
    
            //引用计数加1
            rAssetLoadEntry.RefCount++;
            string rAssetLoadUrl = rAssetLoadEntry.ABPath;

            // 如果该资源加载完成了
            if (!rAssetLoadEntry.IsLoading && rAssetLoadEntry.IsLoadCompleted)
            {
                if (rRequest.IsSimulate)
                {
                    Debug.Log("---Simulate Load ab: " + rAssetLoadUrl);
#if UNITY_EDITOR
                    if (!string.IsNullOrEmpty(rRequest.AssetName) && !rRequest.IsScene)
                    {
                        string[] rAssetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(rAssetLoadEntry.ABName, rRequest.AssetName);
                        if (rAssetPaths.Length == 0)
                        {
                            Debug.LogError("There is no asset with name \"" + rRequest.AssetName + "\" in " + rAssetLoadEntry.ABName);
                            return;
                        }
                        Object rTargetAsset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(rAssetPaths[0]);
                        rRequest.Asset = rTargetAsset;
                    }
#endif
                }
                else
                {
                    Debug.Log("---Load asset: " + rAssetLoadUrl);

                    // 加载Object
                    if (!string.IsNullOrEmpty(rRequest.AssetName))
                    {
                        if (!rRequest.IsScene)
                        {
                            rRequest.Asset = await rAssetLoadEntry.CacheAsset.LoadAssetAsync(rRequest.AssetName);
                        }
                        else
                        {
                            rAssetLoadEntry.CacheAsset.GetAllScenePaths();
                        }
                    }
                }
                return;
            }
            
            // 开始加载资源依赖项
            if (rAssetLoadEntry.ABDependNames != null && !rRequest.IsSimulate)
            {
                for (int i = rAssetLoadEntry.ABDependNames.Length - 1; i >= 0; i--)
                {
                    string rDependABPath = rAssetLoadEntry.ABDependNames[i];
                    string rDependABName = rDependABPath;

                    LoaderRequest rDependAssetRequest = new LoaderRequest(rDependABName, "", false, rRequest.IsSimulate);
                    await LoadAsset_Async(rDependAssetRequest);
                }
            }

            //开始加载当前的资源包
            rAssetLoadEntry.IsLoading = true;
            rAssetLoadEntry.IsLoadCompleted = false;

            if (rRequest.IsSimulate)
            {
                Debug.Log("---Simulate Load ab: " + rAssetLoadUrl);
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(rRequest.AssetName) && !rRequest.IsScene)
                {
                    string[] rAssetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(rAssetLoadEntry.ABName, rRequest.AssetName);
                    if (rAssetPaths.Length == 0)
                    {
                        Debug.LogError("There is no asset with name \"" + rRequest.AssetName + "\" in " + rAssetLoadEntry.ABName);
                        return;
                    }
                    Object rTargetAsset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(rAssetPaths[0]);
                    rRequest.Asset = rTargetAsset;
                }
#endif
            }
            else
            {
                Debug.Log("---Real Load ab: " + rAssetLoadUrl);
                // 如果是一个直接的资源，将资源的对象取出来
                rAssetLoadEntry.CacheAsset = await AssetBundle.LoadFromFileAsync(rAssetLoadUrl);
                
                // 加载Object
                if (!string.IsNullOrEmpty(rRequest.AssetName))
                {
                    if (!rRequest.IsScene)
                    {
                        rRequest.Asset = await rAssetLoadEntry.CacheAsset.LoadAssetAsync(rRequest.AssetName);
                    }
                    else
                    {
                        rAssetLoadEntry.CacheAsset.GetAllScenePaths();
                    }
                }
            }
            rAssetLoadEntry.IsLoading = false;
            rAssetLoadEntry.IsLoadCompleted = true;
        }
    }
}