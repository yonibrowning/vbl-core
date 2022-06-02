using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AddressablesRemoteLoader : MonoBehaviour
{
    [SerializeField] private string addressablesStorageRemotePath = "http://data.virtualbrainlab.org/AddressablesStorage";
    [SerializeField] private string buildVersion = "0.1.0";

    private string fileEnding = ".json";
    private string addressablesStorageTargetPath;

    private Task<bool> catalogLoadedTask;

    // Start is called before the first frame update
    void Awake()
    {
        RuntimePlatform platform = Application.platform;
        if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
            addressablesStorageTargetPath = addressablesStorageRemotePath + "/" + "StandaloneWindows64/catalog_" + buildVersion + fileEnding;
        else if (platform == RuntimePlatform.WebGLPlayer)
            addressablesStorageTargetPath = addressablesStorageRemotePath + "/" + "WebGL/catalog_" + buildVersion + fileEnding;
        else
            Debug.LogError("Running on a platform that does NOT have a built Addressables Storage bundle");

        catalogLoadedTask = AsyncStart();
    }

    public string GetAddressablesPath()
    {
        return addressablesStorageTargetPath;
    }

    /// <summary>
    /// Load the remote catalog
    /// </summary>
    public async Task<bool> AsyncStart()
    {
        bool finished = false;
        //Load a catalog and automatically release the operation handle.
        Debug.Log("Loading content catalog from: " + GetAddressablesPath());

        //catalogLoadHandle
        //    = Addressables.LoadContentCatalogAsync(GetAddressablesPath(), true);

        AsyncOperationHandle<IResourceLocator> catalogLoadHandle
            = Addressables.LoadContentCatalogAsync("http://data.virtualbrainlab.org/AddressablesStorage/StandaloneWindows64/", true);

        await catalogLoadHandle.Task;

        Debug.Log("Content catalog loaded");

        finished = true;

        return finished;
    }

    public async void LoadCCFMesh(string objPath, Action<Mesh> callback)
    {
        // Wait for the catalog to load if this hasn't already happened
        await catalogLoadedTask;

        // Catalog is loaded, test loading a single mesh file
        string path = "Assets/AddressableAssets/AllenCCF/" + objPath;
        AsyncOperationHandle<IList<IResourceLocation>> pathHandle = Addressables.LoadResourceLocationsAsync(path);

        await pathHandle.Task;

        AsyncOperationHandle loadHandle = Addressables.LoadAssetAsync<Mesh>(path);
        loadHandle.Completed += meshHandle =>
        {
            Mesh temp = (Mesh)meshHandle.Result;
            Debug.Log("Loaded: " + path);
            callback(temp);
        };
    }
}
