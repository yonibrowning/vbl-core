using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddressablesRemoteTest : MonoBehaviour
{
    [SerializeField] AddressablesRemoteLoader remoteLoader;

    // Start is called before the first frame update
    void Start()
    {
        remoteLoader.LoadCCFMesh("8.obj", mesh => { });
    }
}
