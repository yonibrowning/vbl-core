using UnityEngine;

public class AnnotationDataset : VolumetricDataset
{
    private bool[,,] areaBorders;

    public AnnotationDataset(int[] size, ushort[] data, uint[] map, byte[] ccfIndexMap) : base(size, ccfIndexMap, map, data)
    {
        // pass
        //ComputeBorders();
    }

    public void ComputeBorders()
    {
        areaBorders = new bool[size[0], size[1], size[2]];

        for (int ap = 0; ap < size[0]; ap++)
        {
            // We go through coronal slices, going down each DV depth, anytime the *next* annotation point changes, we mark this as a border
            for (int lr = 0; lr < (size[2]-1); lr++)
            {
                for (int dv = 0; dv < (size[1]-1); dv ++)
                {
                    if ((data[ap, dv, lr] != data[ap, dv + 1, lr]) || data[ap,dv,lr] != data[ap, dv, lr+1])
                        areaBorders[ap, dv, lr] = true;
                }
            }
        }

    }

    public bool BorderAtIndex(int ap, int dv, int lr)
    {
        if ((ap >= 0 && ap < size[0]) && (dv >= 0 && dv < size[1]) && (lr >= 0 && lr < size[2]))
            return areaBorders[ap, dv, lr];
        else
            return false;
    }

    public bool BorderAtIndex(Vector3 apdvlr)
    {
        return BorderAtIndex(Mathf.RoundToInt(apdvlr.x), Mathf.RoundToInt(apdvlr.y), Mathf.RoundToInt(apdvlr.z));
    }

}
