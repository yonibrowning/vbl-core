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

    // Server setup task
    private TaskCompletionSource<bool> catalogTargetSetSource;
    private Task<bool> catalogTargetSetTask;

    // Catalog load task
    private static Task catalogLoadedTask;

    // Delaying the load allows you to set the catalog address
    [SerializeField] private bool delayCatalogLoad = false;

    // Start is called before the first frame update
    void Awake()
    {
        catalogTargetSetSource = new TaskCompletionSource<bool>();
        catalogTargetSetTask = catalogTargetSetSource.Task;

        if (!delayCatalogLoad) {
            LoadCatalog();
        }

        // Warning: I think there must be a better way to do this, but because we only initialize this Task
        // in Awake() we **CANNOT** call any of the Load() functions from another classes Awake() function.
        // Technically this is consistent with Unity's Awake/Start architecture, but it's still a little annoying.
        catalogLoadedTask = AsyncLink2Catalog();
    }

    public void ChangeCatalogServer(string newAddressablesStorageRemotePath) {
        this.addressablesStorageRemotePath = newAddressablesStorageRemotePath;
    }

    public void LoadCatalog() {
        RuntimePlatform platform = Application.platform;
        if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
            addressablesStorageTargetPath = addressablesStorageRemotePath + "/" + "StandaloneWindows64/catalog_" + buildVersion + fileEnding;
        else if (platform == RuntimePlatform.WebGLPlayer)
            addressablesStorageTargetPath = addressablesStorageRemotePath + "/" + "WebGL/catalog_" + buildVersion + fileEnding;
        else if (platform == RuntimePlatform.OSXEditor)
            addressablesStorageTargetPath = addressablesStorageRemotePath + "/" + "StandaloneOSX/catalog_" + buildVersion + fileEnding;
        else {
            Debug.LogError(string.Format("Running on {0} we do NOT have a built Addressables Storage bundle",platform));
        }
        catalogTargetSetSource.SetResult(true);
    }

    public Task GetCatalogLoadedTask() {
        return catalogLoadedTask;
    }

    public string GetAddressablesPath()
    {
        return addressablesStorageTargetPath;
    }

    /// <summary>
    /// Load the remote catalog
    /// </summary>
    public async Task<bool> AsyncLink2Catalog()
    {
        await catalogTargetSetTask;

        Debug.Log("(AddressablesStorage) Loading catalog v" + buildVersion);
        bool finished = true;
        //Load a catalog and automatically release the operation handle.
        Debug.Log("Loading content catalog from: " + GetAddressablesPath());

        AsyncOperationHandle<IResourceLocator> catalogLoadHandle
            = Addressables.LoadContentCatalogAsync(GetAddressablesPath(), true);

        await catalogLoadHandle.Task;

        Debug.Log("Content catalog loaded");
        return finished;
    }

    public static async Task<Mesh> LoadCCFMesh(string objPath)
    {
        Debug.Log("Loading mesh file: " + objPath);
        // Wait for the catalog to load if this hasn't already happened
        await catalogLoadedTask;


        // Catalog is loaded, load specified mesh file
        string path = "Assets/AddressableAssets/AllenCCF/" + objPath;
        // Not sure why this extra path check is here, I think maybe some objects don't exist and so this hangs indefinitely for those?
        AsyncOperationHandle<IList<IResourceLocation>> pathHandle = Addressables.LoadResourceLocationsAsync(path);
        await pathHandle.Task;

        AsyncOperationHandle loadHandle = Addressables.LoadAssetAsync<Mesh>(path);
        await loadHandle.Task;

        Mesh returnMesh = (Mesh)loadHandle.Result;
        Addressables.Release(pathHandle);
        Addressables.Release(loadHandle);

        return returnMesh;
    }

    public static async Task<TextAsset> LoadAllenCCFOntology()
    {
        Debug.Log("Loading Allen CCF");

        await catalogLoadedTask;

        string path = "Assets/AddressableAssets/AllenCCF/ontology_structure_minimal.csv";

        AsyncOperationHandle loadHandle = Addressables.LoadAssetAsync<TextAsset>(path);
        await loadHandle.Task;

        TextAsset returnText = (TextAsset)loadHandle.Result;
        Addressables.Release(loadHandle);

        return returnText;
    }

    public static async Task<Texture3D> LoadAnnotationTexture()
    {
        Debug.Log("Loading Allen CCF annotation texture");

        // Wait for the catalog to load if this hasn't already happened
        await catalogLoadedTask;

        // Catalog is loaded, load the Texture3D object
        string path = "Assets/AddressableAssets/Textures/AnnotationDatasetTexture3DAlpha.asset";

        AsyncOperationHandle loadHandle = Addressables.LoadAssetAsync<Texture3D>(path);
        await loadHandle.Task;

        Texture3D returnTexture = (Texture3D)loadHandle.Result;
        //Addressables.Release(loadHandle);

        return returnTexture;
    }

    public static async Task<TextAsset> LoadVolumeIndexes()
    {
        Debug.Log("Loading volume indexes");

        // Wait for the catalog to load if this hasn't already happened
        await catalogLoadedTask;

        string volumePath = "Assets/AddressableAssets/Datasets/volume_indexes.bytes";
        
        AsyncOperationHandle loadHandle = Addressables.LoadAssetAsync<TextAsset>(volumePath);
        await loadHandle.Task;

        TextAsset resultText = (TextAsset)loadHandle.Result;
        Addressables.Release(loadHandle);

        return resultText;
    }

    /// <summary>
    /// Loads the annotation data to be reconstructed by the VolumeDatasetManager
    /// </summary>
    /// <returns>List of TextAssets where [0] is the index and [1] is the map</returns>
    public static async Task<List<TextAsset>> LoadAnnotationIndexMap()
    {
        Debug.Log("Loading annotation index mapping");

        // Wait for the catalog to load if this hasn't already happened
        await catalogLoadedTask;

        string annIndexPath = "Assets/AddressableAssets/Datasets/ann/annotation_indexes.bytes";
        string annMapPath = "Assets/AddressableAssets/Datasets/ann/annotation_map.bytes";

        AsyncOperationHandle indexHandle = Addressables.LoadAssetAsync<TextAsset>(annIndexPath);
        AsyncOperationHandle mapHandle = Addressables.LoadAssetAsync<TextAsset>(annMapPath);

        // Build a list of tasks and await them
        List<Task> dataLoaders = new List<Task>();
        dataLoaders.Add(indexHandle.Task);
        dataLoaders.Add(mapHandle.Task);
        await Task.WhenAll(dataLoaders);

        List<TextAsset> returnList = new List<TextAsset>() { (TextAsset)indexHandle.Result, (TextAsset)mapHandle.Result };
        Addressables.Release(indexHandle);
        Addressables.Release(mapHandle);

        return returnList;
    } 
}
