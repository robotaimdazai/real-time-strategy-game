using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceValue
{
    public InGameResource code;
    public int amount;

    public ResourceValue(InGameResource code, int amount)
    {
        this.code = code;
        this.amount = amount;
    }
}
