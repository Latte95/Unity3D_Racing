using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateMeshCollider : MonoBehaviour
{
    private SkinnedMeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    void Start()
    {
        meshRenderer = GetComponent<SkinnedMeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    private void FixedUpdate()
    {
        UpdateCollider();        
    }

    public void UpdateCollider()
    {
        Mesh colliderMesh = new Mesh();
        meshRenderer.BakeMesh(colliderMesh);
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = colliderMesh;
    }
}
