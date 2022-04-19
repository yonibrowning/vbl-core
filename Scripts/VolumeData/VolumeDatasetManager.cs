using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class VolumeDatasetManager : MonoBehaviour
{
    private AnnotationDataset annotationDataset;

    private VolumetricDataset coverageDataset;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // Annotations
    [SerializeField] private AssetReference dataIndexes;
    private byte[] datasetIndexes_bytes;
    [SerializeField] private AssetReference annotationIndexes;
    private ushort[] annotationIndexes_shorts;
    [SerializeField] private AssetReference annotationMap;
    private uint[] annotationMap_ints;

    /// <summary>
    /// Loads the annotation dataset files from their Addressable AssetReference objects
    /// 
    /// Asynchronous dependencies: inPlaneSlice, localPrefs
    /// </summary>
    public async Task<bool> LoadAnnotationDataset(List<Action> callbackList)
    {
        bool finished = true;

        Debug.Log("(VDManager) Annotation dataset loading");
        List<Task> dataLoaders = new List<Task>();

        AsyncOperationHandle<TextAsset> dataLoader = dataIndexes.LoadAssetAsync<TextAsset>();
        dataLoaders.Add(dataLoader.Task);
        AsyncOperationHandle<TextAsset> annIndexLoader = annotationIndexes.LoadAssetAsync<TextAsset>();
        dataLoaders.Add(annIndexLoader.Task);
        AsyncOperationHandle<TextAsset> annMapLoader = annotationMap.LoadAssetAsync<TextAsset>();
        dataLoaders.Add(annMapLoader.Task);

        await Task.WhenAll(dataLoaders);

        // When all loaded, copy the data locally using Buffer.BlockCopy()
        datasetIndexes_bytes = dataLoader.Result.bytes;
        Addressables.Release(dataLoader);

        annotationIndexes_shorts = new ushort[annIndexLoader.Result.bytes.Length / 2];
        Buffer.BlockCopy(annIndexLoader.Result.bytes, 0, annotationIndexes_shorts, 0, annIndexLoader.Result.bytes.Length);
        Addressables.Release(annIndexLoader);

        annotationMap_ints = new uint[annMapLoader.Result.bytes.Length / 4];
        Buffer.BlockCopy(annMapLoader.Result.bytes, 0, annotationMap_ints, 0, annMapLoader.Result.bytes.Length);
        Addressables.Release(annMapLoader);

        Debug.Log("(VDManager) Annotation dataset files loaded, building dataset");

        annotationDataset = new AnnotationDataset(new int[] { 528, 320, 456 }, annotationIndexes_shorts, annotationMap_ints, datasetIndexes_bytes);
        annotationIndexes_shorts = null;
        annotationMap_ints = null;

        // Run any callbacks
        foreach (Action callback in callbackList)
            callback();

        return finished;
    }

    public AnnotationDataset GetAnnotationDataset()
    {
        return annotationDataset;
    }

    // IBL Coverage map
    [SerializeField] private AssetReference coverageIndexes;
    private ushort[] coverageIndexes_shorts;
    private uint[] coverageMap = new uint[] { 0, 1, 2 };

    public async Task<bool> LoadIBLCoverage()
    {
        bool finished = true;

        Debug.Log("(VDManager) Coverage map loading");

        AsyncOperationHandle<TextAsset> coverageLoader = coverageIndexes.LoadAssetAsync<TextAsset>();

        await coverageLoader.Task;

        // When all loaded, copy the data locally using Buffer.BlockCopy()
        coverageIndexes_shorts = new ushort[coverageLoader.Result.bytes.Length / 2];
        Buffer.BlockCopy(coverageLoader.Result.bytes, 0, coverageIndexes_shorts, 0, coverageLoader.Result.bytes.Length);
        Addressables.Release(coverageLoader);


        Debug.Log("(VDManager) Coverage map files loaded, building dataset");

        coverageDataset = new VolumetricDataset(new int[] { 528, 320, 456 }, datasetIndexes_bytes, coverageMap, coverageIndexes_shorts);
        coverageIndexes_shorts = null;

        return finished;
    }

    public VolumetricDataset GetIBLCoverageDataset()
    {
        return coverageDataset;
    }
}
