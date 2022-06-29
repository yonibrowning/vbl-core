using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class CCFTree
{
    public CCFTreeNode root;
    private Material brainRegionMaterial;
    private float scale;
    private Dictionary<int, CCFTreeNode> fastSearchDictionary;
    private Transform brainModelParent;

    public CCFTree(int rootID, int atlasID, string rootName, float scale, Color color, Material material)
    {
        brainModelParent = GameObject.Find("BrainAreas").transform;

        this.scale = scale;
        root = new CCFTreeNode(rootID, atlasID, 0, scale, null, rootName, "", color, material, brainModelParent);
        brainRegionMaterial = material;

        fastSearchDictionary = new Dictionary<int, CCFTreeNode>();
        fastSearchDictionary.Add(rootID, root);

    }

    public CCFTreeNode addNode(int parentID, int id, int atlasID, int depth, string name, string acronym, Color color)
    {
        // find the parent ID node
        CCFTreeNode parentNode = findNode(parentID);

        // return if you fail to find it
        if (parentNode==null) {Debug.Log("Can't add new node: parent not found");return null;}

        // add the node if you succeeded
        CCFTreeNode newNode = new CCFTreeNode(id, atlasID, depth, scale, parentNode, name, acronym, color, brainRegionMaterial, brainModelParent);
        parentNode.appendNode(newNode);

        fastSearchDictionary.Add(id, newNode);

        return newNode;
    }

    public int nodeCount()
    {
        return root.nodeCount();
    }

    public CCFTreeNode findNode(int ID)
    {
        if (fastSearchDictionary.ContainsKey(ID))
            return fastSearchDictionary[ID];
        return null;
    }

    [Obsolete("Deprecated in favor of findNode with dictionary")]
    public CCFTreeNode findNodeRecursive(int ID)
    {
        return root.findNode(ID);
    }
}

public class CCFTreeNode
{
    private CCFTreeNode parent;
    private List<CCFTreeNode> childNodes;
    public int ID { get;}
    public int atlasID { get; }
    public string Name { get; }
    public string ShortName { get; }
    public int Depth { get; }
    private Color defaultColor;
    private Color color;
    private float scale;

    private bool singleModel;
    private GameObject nodeModelGO;

    private GameObject nodeModelLeftGO;
    private GameObject nodeModelRightGO;

    private Transform brainModelParent;
    private Material material;


    private Vector3 explodeScale = new Vector3(1f, 1f, 1f);

    private bool loaded;

    // Mesh properties
    // each mesh has a left and right half, we want to separate these
    Mesh localMesh;

    public CCFTreeNode(int ID, int atlasID, int depth, float scale, CCFTreeNode parent, string Name, string ShortName, Color color, Material material, Transform brainModelParent)
    {
        this.ID = ID;
        this.atlasID = atlasID;
        this.Name = Name;
        this.parent = parent;
        this.Depth = depth;
        this.scale = scale;
        this.ShortName = ShortName;
        color.a = 1.0f;
        this.color = color;
        defaultColor = new Color(color.r, color.g, color.b, color.a);
        this.material = material;
        this.brainModelParent = brainModelParent;
        childNodes = new List<CCFTreeNode>();

        loaded = false;
    }

    public bool IsLoaded()
    {
        return loaded;
    }

    public async Task<CCFTreeNode> loadNodeModel(bool loadSeparatedModels)
    {
        singleModel = !loadSeparatedModels;

        nodeModelGO = new GameObject(Name);
        nodeModelGO.transform.parent = brainModelParent;

        string path = (loadSeparatedModels) ? this.ID + "L.obj" : this.ID + ".obj";

        Task<Mesh> meshTask = AddressablesRemoteLoader.LoadCCFMesh(path);
        await meshTask;

        LoadNodeModelCompleted(meshTask.Result);

        return this;
    }
    public async Task<CCFTreeNode> loadNodeModel(bool loadSeparatedModels, Action<AsyncOperationHandle> callback)
    {
        singleModel = !loadSeparatedModels;

        nodeModelGO = new GameObject(Name);
        nodeModelGO.transform.SetParent(brainModelParent);

        string path = (loadSeparatedModels) ? this.ID + "L.obj" : this.ID + ".obj";

        Task<Mesh> meshTask = AddressablesRemoteLoader.LoadCCFMesh(path);
        await meshTask;

        LoadNodeModelCompleted(meshTask.Result);

        return this;
    }

    private void LoadNodeModelCompleted(Mesh fullMesh)
    {
        // Copy the mesh so that we can modify it without modifying the original
        localMesh = new Mesh();
        localMesh.vertices = fullMesh.vertices;
        localMesh.triangles = fullMesh.triangles;
        //localMesh.uv = fullMesh.uv;
        localMesh.normals = fullMesh.normals;
        //localMesh.colors = fullMesh.colors;
        localMesh.tangents = fullMesh.tangents;

        // Check if the mesh has extra vertices near 0,0,0

        if (!singleModel)
        {
            // Create the left/right meshes
            nodeModelLeftGO = new GameObject(Name + "_L");
            nodeModelLeftGO.transform.SetParent(nodeModelGO.transform);
            nodeModelLeftGO.transform.localScale = new Vector3(scale, scale, scale);
            nodeModelLeftGO.AddComponent<MeshFilter>();
            nodeModelLeftGO.AddComponent<MeshRenderer>();
            nodeModelLeftGO.layer = 13;
            nodeModelLeftGO.tag = "BrainRegion";
            Renderer leftRend = nodeModelLeftGO.GetComponent<Renderer>();
            leftRend.material = material;
            leftRend.material.SetColor("_Color", color);
            leftRend.receiveShadows = false;
            leftRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            nodeModelLeftGO.GetComponent<MeshFilter>().mesh = localMesh;
            nodeModelLeftGO.AddComponent<MeshCollider>();
            nodeModelLeftGO.SetActive(false);

            // Create the right meshes
            nodeModelRightGO = new GameObject(Name + "_R");
            nodeModelRightGO.transform.SetParent(nodeModelGO.transform);
            nodeModelRightGO.transform.localScale = new Vector3(scale, scale, -scale);
            nodeModelRightGO.AddComponent<MeshFilter>();
            nodeModelRightGO.AddComponent<MeshRenderer>();
            nodeModelRightGO.layer = 13;
            nodeModelRightGO.tag = "BrainRegion";
            Renderer rightRend = nodeModelRightGO.GetComponent<Renderer>();
            rightRend.material = material;
            rightRend.material.SetColor("_Color", color);
            rightRend.receiveShadows = false;
            rightRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            nodeModelRightGO.GetComponent<MeshFilter>().mesh = localMesh;
            nodeModelRightGO.SetActive(false);
        }
        else
        {
            nodeModelGO.transform.localScale = new Vector3(scale, scale, scale);
            nodeModelGO.AddComponent<MeshFilter>();
            nodeModelGO.AddComponent<MeshRenderer>();
            nodeModelGO.layer = 13;
            nodeModelGO.tag = "BrainRegion";
            Renderer rend = nodeModelGO.GetComponent<Renderer>();
            rend.material = material;
            rend.material.SetColor("_Color", color);
            rend.receiveShadows = false;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            nodeModelGO.GetComponent<MeshFilter>().mesh = localMesh;

            nodeModelGO.SetActive(false);
        }

        loaded = true;
    }

    public Color GetColor()
    {
        return color;
    }

    public Color GetDefaultColor()
    {
        return defaultColor;
    }

    public List<CCFTreeNode> GetChildren()
    {
        return childNodes;
    }

    public void ResetColor()
    {
        SetColor(defaultColor, true);
    }

    public void SetColor(Color newColor, bool saveColor)
    {
        if (!loaded)
        {
            Debug.LogError("Node model needs to be loaded before color can be set");
            return;
        }

        if (saveColor)
            color = newColor;

        if (singleModel)
            nodeModelGO.GetComponent<Renderer>().material.SetColor("_Color", color);
        else
        {
            nodeModelLeftGO.GetComponent<Renderer>().material.SetColor("_Color", color);
            nodeModelRightGO.GetComponent<Renderer>().material.SetColor("_Color", color);
        }
    }

    public void SetColorOneSided(Color newColor, bool leftSide, bool saveColor)
    {
        if (!loaded)
        {
            Debug.LogError("Node model needs to be loaded before color can be set");
            return;
        }
        if (singleModel)
        {
            Debug.LogError("Can't set one-sided colors when loading single models.");
            return;
        }

        if (saveColor)
            color = newColor;

        if (leftSide)
        {
            nodeModelLeftGO.GetComponent<Renderer>().material.SetColor("_Color", newColor);
        }
        else
        {
            nodeModelRightGO.GetComponent<Renderer>().material.SetColor("_Color", newColor);
        }
    }

    public void SetMaterial(Material newMaterial)
    {
        if (!loaded)
        {
            Debug.LogError("Node model needs to be loaded before material can be set");
            return;
        }
        this.material = newMaterial;
        if (singleModel)
            nodeModelGO.GetComponent<Renderer>().material = newMaterial;
        else
        {
            nodeModelLeftGO.GetComponent<Renderer>().material = newMaterial;
            nodeModelRightGO.GetComponent<Renderer>().material = newMaterial;
        }

        SetColor(color, false);
    }
    public void SetMaterialOneSided(Material newMaterial, bool leftSide)
    {
        if (!loaded)
        {
            Debug.LogError("Node model needs to be loaded before material can be set");
            return;
        }
        this.material = newMaterial;
        if (singleModel)
            Debug.LogError("Can't set sided material in singleModel mode");
        else if (leftSide)
            nodeModelLeftGO.GetComponent<Renderer>().material = newMaterial;
        else
            nodeModelRightGO.GetComponent<Renderer>().material = newMaterial;

        SetColorOneSided(color, leftSide, false);
    }

    public void SetShaderProperty(string property, Vector4 value)
    {
        if (!loaded)
        {
            Debug.LogError("Node model needs to be loaded before material properties can be set");
            return;
        }
        if (singleModel)
            nodeModelGO.GetComponent<Renderer>().material.SetVector(property, value);
        else
        {
            nodeModelLeftGO.GetComponent<Renderer>().material.SetVector(property, value);
            nodeModelRightGO.GetComponent<Renderer>().material.SetVector(property, value);
        }
    }

    public void SetShaderPropertyOneSided(string property, Vector4 value, bool leftSide)
    {
        if (!loaded)
        {
            Debug.LogError("Node model needs to be loaded before material properties can be set");
            return;
        }
        if (singleModel)
            Debug.LogError("Can't set sided properties in single mode");
        else if (leftSide)
            nodeModelLeftGO.GetComponent<Renderer>().material.SetVector(property, value);
        else
            nodeModelRightGO.GetComponent<Renderer>().material.SetVector(property, value);
    }

    public void SetShaderProperty(string property, float value)
    {
        if (!loaded)
        {
            Debug.LogError("Node model needs to be loaded before material properties can be set");
            return;
        }
        if (singleModel)
            nodeModelGO.GetComponent<Renderer>().material.SetFloat(property, value);
        else
        {
            nodeModelLeftGO.GetComponent<Renderer>().material.SetFloat(property, value);
            nodeModelRightGO.GetComponent<Renderer>().material.SetFloat(property, value);
        }
    }
    public void SetShaderPropertyOneSided(string property, float value, bool leftSide)
    {
        if (!loaded)
        {
            Debug.LogError("Node model needs to be loaded before material properties can be set");
            return;
        }
        if (singleModel)
            Debug.LogError("Can't set sided properties in single mode");
        else if (leftSide)
            nodeModelLeftGO.GetComponent<Renderer>().material.SetFloat(property, value);
        else
            nodeModelRightGO.GetComponent<Renderer>().material.SetFloat(property, value);
    }


    public void SetNodeModelVisibility(bool visible)
    {
        if (singleModel)
        {
            nodeModelGO.SetActive(visible);
        }
        else
        {
            nodeModelLeftGO.SetActive(visible);
            nodeModelRightGO.SetActive(visible);
        }
    }

    public void SetNodeModelVisibilityLeft(bool leftVisible)
    {
        if (singleModel)
        {
            Debug.LogWarning("Node model visibility cannot be set separately when running in single model mode.");
        }
        else
        {
            nodeModelLeftGO.SetActive(leftVisible);
        }
    }

    public void SetNodeModelVisibilityRight(bool rightVisible)
    {
        if (singleModel)
        {
            Debug.LogWarning("Node model visibility cannot be set separately when running in single model mode.");
        }
        else
        {
            nodeModelLeftGO.SetActive(rightVisible);
        }
    }

    public void SetNodeModelVisibility(bool leftVisible, bool rightVisible)
    {
        if (singleModel)
        {
            Debug.LogWarning("Node model visibility cannot be set separately when running in single model mode.");
        }
        else
        {
            nodeModelLeftGO.SetActive(leftVisible);
            nodeModelRightGO.SetActive(rightVisible);
        }
    }

    public int nodeCount()
    {
        int count = childNodes.Count;
        foreach (CCFTreeNode node in childNodes)
        {
            count += node.nodeCount();
        }
        return count;
    }

    public CCFTreeNode findNode(int ID)
    {
        if (this.ID == ID) { return this; }
        foreach (CCFTreeNode node in childNodes)
        {
            CCFTreeNode found = node.findNode(ID);
            if (found != null) { return found; }
        }
        return null;
    }

    public CCFTreeNode Parent()
    {
        return parent;
    }

    public List<CCFTreeNode> Nodes()
    {
        return childNodes;
    }

    public void appendNode(CCFTreeNode newNode)
    {
        childNodes.Add(newNode);
    }

    public void DebugPrint()
    {
        Debug.Log(this.ID);
        Debug.Log(this.color);
        Debug.Log(this.Name);
    }

    public Transform GetNodeTransform()
    {
        return nodeModelGO.transform;
    }

    public Vector3 GetMeshCenter(bool leftSide = false, bool rightSide = false)
    {
        if (singleModel)
            return nodeModelGO.GetComponent<Renderer>().bounds.center;
        else if (leftSide)
            return nodeModelLeftGO.GetComponent<Renderer>().bounds.center;
        else if (rightSide)
            return nodeModelRightGO.GetComponent<Renderer>().bounds.center;
        else
            return nodeModelLeftGO.GetComponent<Renderer>().bounds.center + nodeModelRightGO.GetComponent<Renderer>().bounds.center;
    }

    public GameObject MainGameObject()
    {
        return nodeModelGO;
    }
    public GameObject LeftGameObject()
    {
        return nodeModelLeftGO;
    }
    public GameObject RightGameObject()
    {
        return nodeModelRightGO;
    }
}