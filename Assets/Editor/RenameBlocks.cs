using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RenameBlocks : MonoBehaviour {

    [SerializeField] GameObject blockArray;

    public void Start()
    {
        for (int i = 0; i < blockArray.transform.childCount; i++)
        {
            blockArray.transform.GetChild(i).gameObject.name = i.ToString();
        }
    }
}
