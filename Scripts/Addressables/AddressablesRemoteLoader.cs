using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AddressablesRemoteLoader : MonoBehaviour
{
    [SerializeField] private string addressablesStorageRemotePath = "http://data.virtualbrainlab.org/AddressablesStorage";
    private string addressablesStorageTargetPath;

    private AsyncOperationHandle catalogLoadHandle;

    // Start is called before the first frame update
    void Start()
    {
        BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
        addressablesStorageTargetPath = addressablesStorageRemotePath + "/" + buildTarget;
        
        AsyncStart(addressablesStorageRemotePath + buildTarget);
    }

    public string GetAddressablesPath()
    {
        return addressablesStorageTargetPath;
    }

    public async void AsyncStart(string resourcePath)
    {
        //Load a catalog and automatically release the operation handle.
        Debug.Log("Loading content catalog from: " + resourcePath);

        AsyncOperationHandle<IResourceLocator> handle
            = Addressables.LoadContentCatalogAsync(resourcePath, true);
        await handle.Task;

        LoadCCFMesh("8.obj", mesh => { });
    }

    public async void LoadCCFMesh(string objPath, Action<Mesh> callback)
    {        
        // Catalog is loaded, test loading a single mesh file
        string path = GetAddressablesPath() + "/" + objPath;
        AsyncOperationHandle<IList<IResourceLocation>> pathHandle = Addressables.LoadResourceLocationsAsync(path);

        await pathHandle.Task;

        AsyncOperationHandle loadHandle = Addressables.LoadAssetAsync<Mesh>(path);
        loadHandle.Completed += meshHandle =>
        {
            Mesh temp = (Mesh)meshHandle.Result;
            Debug.Log("Loaded: " + path);
            callback(temp);
        };

        //await loadHandle.Task;
    }
}
