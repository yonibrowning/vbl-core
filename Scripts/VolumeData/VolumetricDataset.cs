using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumetricDataset
{
    protected int[] size;
    protected int[,,] data;

    public VolumetricDataset(int[] size, byte[] volumeIndexes, uint[] map, ushort[] dataIndexes)
    {
        this.size = size;
        ConstructorHelper(volumeIndexes, map, dataIndexes);
    }
    public VolumetricDataset(int[] size, byte[] volumeIndexes, uint[] map, byte[] dataIndexes)
    {
        this.size = size;
        ushort[] dataIndexesCopy = new ushort[dataIndexes.Length];
        dataIndexes.CopyTo(dataIndexesCopy, 0);
        ConstructorHelper(volumeIndexes, map, dataIndexesCopy);
    }

    private void ConstructorHelper(byte[] volumeIndexes, uint[] map, ushort[] dataIndexes)
    {
        data = new int[size[0], size[1], size[2]];

        int ccfi = 0;
        int i = 0;

        // Datasets are stored in column order, so go through in reverse
        for (int ap = 0; ap < size[0]; ap++)
        {
            for (int dv = 0; dv < size[1]; dv++)
            {
                for (int lr = 0; lr < size[2]; lr++)
                {
                    if (volumeIndexes[ccfi] == 1)
                    {
                        data[ap, dv, lr] = (int)map[dataIndexes[i]];
                        i++;
                    }
                    ccfi++;
                }
            }
        }
    }

    public int ValueAtIndex(int ap, int dv, int lr)
    {
        if ((ap >= 0 && ap < size[0]) && (dv >= 0 && dv < size[1]) && (lr >= 0 && lr < size[2]))
            return data[ap, dv, lr];
        else
            return int.MinValue;
    }

    public int ValueAtIndex(Vector3 apdvlr)
    {
        return ValueAtIndex(Mathf.RoundToInt(apdvlr.x), Mathf.RoundToInt(apdvlr.y), Mathf.RoundToInt(apdvlr.z));
    }
}
