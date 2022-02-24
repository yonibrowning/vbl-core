using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CCFModelControl : MonoBehaviour
{
    public CCFTree tree;

    public GameObject ontologyContentPanel;
    public GameObject ontologyTogglePrefab;

    public int heightPerToggle = 20;

    //private int lowQualityDepth = 5;
    //private int[] lowQualityDefaults = new int[] { 315, 343, 698, 1089, 512 };
    private int highQualityDepth = 6;
    private int[] highQualityDefaults = new int[] {184, 500, 453, 1057, 677, 247, 669, 31, 972, 44, 714, 95, 254, 22, 541, 922, 698, 895, 1089, 703, 623, 343, 512};
    //private int berylDepth;
    //private int[] berylDefaults = new int[] { };

    private int[] defaultNodes;
    private int defaultDepth;

    private Dictionary<int, Color> ccfAreaColors;
    private Dictionary<int, Color> ccfAreaColorsMinDepth;
    private Dictionary<int, string> ccfAreaAcronyms;
    private Dictionary<int, string> ccfAreaNames;

    // beryl remapping
    private Dictionary<int, int> berylRemap;
    private Dictionary<int, int> cosmosRemap;
    private bool useBerylRemap;
    private bool useCosmosRemap;

    [SerializeField] private bool overrideNetwork;

    [SerializeField] List<Material> brainRegionMaterials;
    [SerializeField] List<string> brainRegionMaterialNames;

    [SerializeField] private float modelScale;

    [SerializeField] private string addressableAssetPath;
    private static string _addressableAssetPath;
    [SerializeField] private AssetReference ontologyStructureAsset;

    private Material defaultBrainRegionMaterial;

    private List<CCFTreeNode> defaultLoadedNodes;

    private void Awake()
    {
        _addressableAssetPath = addressableAssetPath;

        // Initialize
        ccfAreaColors = new Dictionary<int, Color>();
        ccfAreaColorsMinDepth = new Dictionary<int, Color>();
        ccfAreaAcronyms = new Dictionary<int, string>();
        ccfAreaNames = new Dictionary<int, string>();
        berylRemap = new Dictionary<int, int>();
        cosmosRemap = new Dictionary<int, int>();

        defaultBrainRegionMaterial = brainRegionMaterials[brainRegionMaterialNames.IndexOf("default")];

        defaultNodes = highQualityDefaults;
        //defaultNodes = lowQualityDefaults;
        //if (QualitySettings.GetQualityLevel() >= 5) { defaultNodes = highQualityDefaults; }
        defaultDepth = highQualityDepth;

        defaultLoadedNodes = new List<CCFTreeNode>();

        useBerylRemap = false;
    }

    public static string GetAddressablePath()
    {
        return _addressableAssetPath;
    }

    public void LateStart(bool loadDefaults)
    {
        Debug.Log("Ontology start called");
        if (!overrideNetwork) return;

        Debug.LogWarning("On MLAPI compatible systems we shouldn't load the annotation dataset on the client!!");

        LoadCSVData(loadDefaults);
    }

    public void SetBeryl(bool state)
    {
        useBerylRemap = state;
    }

    public bool InDefaults(int ID)
    {
        return defaultNodes.Contains(ID);
    }

    /// <summary>
    /// Load the ontology CSV file (Resources/AllenCCF/ontology_structure_minimal)
    /// Also loads the current default set of areas, set by defaultNodes
    /// </summary>
    void LoadCSVData(bool loadDefaults)
    {
        Debug.Log("(CCFMC) Starting remote load of ontology structure file");
        ontologyStructureAsset.LoadAssetAsync<TextAsset>().Completed += handle =>
        {
            Debug.Log("(CCFMC) Ontology structure file loaded");
            TextAsset text = handle.Result;

            List<Dictionary<string, object>> data = CSVReader.ParseText(text.text);
            Addressables.Release(handle);

            for (var i = 0; i < data.Count; i++)
            {
                // get the values in the CSV file and add to the tree
                int id = (int)data[i]["id"];
                int atlas_id = (int)data[i]["atlas_id"];
                int depth = (int)data[i]["depth"];
                int parent = (int)data[i]["parent_structure_id"];
                int beryl = (int)data[i]["beryl_id"];
                int cosmos = (int)data[i]["cosmos_id"];
                string name = (string)data[i]["name"];
                string shortName = (string)data[i]["acronym"];
                string hexColorString = data[i]["color_hex_code"].ToString();
                Color color = ParseHexColor(hexColorString);

                if (name.Equals("root"))
                {
                    tree = new CCFTree(id, atlas_id, "root", modelScale, color, defaultBrainRegionMaterial);
                }
                else
                {
                    // not the root, so add this node 
                    CCFTreeNode node = tree.addNode(parent, id, atlas_id, depth, name, shortName, color);

                    // If this node should be visible by default, load it now
                    if (loadDefaults && defaultNodes.Contains(id))
                    {
                        // Note: it's fine not to await this asynchronous call, we don't need to use the node model for anything in this function
                        #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        node.loadNodeModel(false);
                        defaultLoadedNodes.Add(node);
                        #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }

                    // Keep track of the colors of areas in the dictionary, these are used to color e.g. neurons in different areas with different colors
                    if (!ccfAreaColors.ContainsKey(id))
                        ccfAreaColors.Add(id, color);
                    // Now pop up the tree until we find the parent at the defaultDepth
                    // then add that color to the min depth list
                    CCFTreeNode minDepthParent = node;
                    while (minDepthParent.Depth > defaultDepth)
                        minDepthParent = minDepthParent.Parent();
                    if (!ccfAreaColorsMinDepth.ContainsKey(id))
                        ccfAreaColorsMinDepth.Add(id, ccfAreaColors[minDepthParent.ID]);

                    // Keep track of the acronyms as well
                    if (!ccfAreaAcronyms.ContainsKey(id))
                        ccfAreaAcronyms.Add(id, shortName);

                    if (!ccfAreaNames.ContainsKey(id))
                        ccfAreaNames.Add(id, name);

                    if (!berylRemap.ContainsKey(id))
                        berylRemap.Add(id, beryl);

                    if (!cosmosRemap.ContainsKey(id))
                        cosmosRemap.Add(id, cosmos);
                }
            }
        };
    }

    public List<CCFTreeNode> DefaultLoadedNodes()
    {
        return defaultLoadedNodes;
    }

    private Color ParseHexColor(string hexString)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hexString, out color);
        return color;
    }

    public Color GetCCFAreaColor(int ID)
    {
        ID = GetCurrentID(ID);
        if (ccfAreaColors.ContainsKey(ID))
            return ccfAreaColors[ID];
        else
            return Color.black;
    }

    public Color GetCCFAreaColorMinDepth(int ID)
    {
        ID = GetCurrentID(ID);
        if (ccfAreaColorsMinDepth.ContainsKey(ID))
            return ccfAreaColorsMinDepth[ID];
        else
            return Color.black;
    }

    public string GetCCFAreaAcronym(int ID)
    {
        ID = GetCurrentID(ID);
        if (ccfAreaAcronyms.ContainsKey(ID))
            return ccfAreaAcronyms[ID];
        else
            return "-";
    }

    public int Acronym2ID(string acronym)
    {
        if (ccfAreaAcronyms.ContainsValue(acronym))
            foreach (KeyValuePair<int, string> pair in ccfAreaAcronyms)
                if (pair.Value == acronym)
                    return pair.Key;
        return -1;
    }

    public bool IsAcronym(string acronym)
    {
        return ccfAreaAcronyms.ContainsValue(acronym);
    }

    public string GetCCFAreaName(int ID)
    {
        ID = GetCurrentID(ID);
        if (ccfAreaNames.ContainsKey(ID))
            return ccfAreaNames[ID];
        else
            return "-";
    }

    public List<int> AreasMatchingAcronym(string match)
    {
        match = match.ToLower();
        List<int> ret = new List<int>();
        foreach (KeyValuePair<int, string> kvp in ccfAreaAcronyms)
            if (kvp.Value.ToLower().Contains(match))
            {
                int currentID = GetCurrentID(kvp.Key);
                if (!ret.Contains(currentID))
                    ret.Add(currentID);
            }
        return ret;
    }

    public List<int> AreasMatchingName(string match)
    {
        match = match.ToLower();
        List<int> ret = new List<int>();
        foreach (KeyValuePair<int, string> kvp in ccfAreaNames)
            if (kvp.Value.ToLower().Contains(match))
            {
                int currentID = GetCurrentID(kvp.Key);
                if (!ret.Contains(currentID))
                    ret.Add(currentID);
            }
        return ret;
    }

    public int GetCurrentID(int ID)
    {
        // cosmos remapping strictly supercedes beryl remapping
        if (useCosmosRemap)
            return GetCosmosID(ID);
        else if (useBerylRemap)
            return GetBerylID(ID);
        else
            return ID;
    }

    public void ChangeMaterial(int ID, string materialName)
    {
        CCFTreeNode node = tree.findNode(ID);
        if (node != null)
            ChangeMaterial(node, materialName);
    }
    public void ChangeMaterial(CCFTreeNode node, string materialName)
    {
        if (brainRegionMaterialNames.Contains(materialName))
            node.SetMaterial(brainRegionMaterials[brainRegionMaterialNames.IndexOf(materialName)]);
        else
            Debug.LogWarning("Material name [" + materialName + "] missing from material options");
    }

    public int GetBerylID(int ID)
    {
        if (berylRemap.ContainsKey(ID))
            return berylRemap[ID];
        else
            return -1;
    }
    public int GetCosmosID(int ID)
    {
        if (cosmosRemap.ContainsKey(ID))
            return cosmosRemap[ID];
        else
            return -1;
    }
}
