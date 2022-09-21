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
    
    // Annotations
    private byte[] datasetIndexes_bytes;
    private ushort[] annotationIndexes_shorts;
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

        Task<byte[]> dataTask = AddressablesRemoteLoader.LoadVolumeIndexes();
        dataLoaders.Add(dataTask);


        Task<List<byte[]>> annotationTask = AddressablesRemoteLoader.LoadAnnotationIndexMap();
        dataLoaders.Add(annotationTask);

        await Task.WhenAll(dataLoaders);

        // When all loaded, copy the data locally using Buffer.BlockCopy()
        datasetIndexes_bytes = dataTask.Result;

        annotationIndexes_shorts = new ushort[annotationTask.Result[0].Length / 2];
        Buffer.BlockCopy(annotationTask.Result[0], 0, annotationIndexes_shorts, 0, annotationTask.Result[0].Length);

        annotationMap_ints = new uint[annotationTask.Result[1].Length / 4];
        Buffer.BlockCopy(annotationTask.Result[1], 0, annotationMap_ints, 0, annotationTask.Result[1].Length);

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
}
