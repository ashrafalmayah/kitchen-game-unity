using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextMeshProMaterialApplier : MonoBehaviour
{
    [SerializeField]private Material textMaterial;

    private void Start() {
        GetComponent<MeshRenderer>().material = textMaterial;
    }
}
