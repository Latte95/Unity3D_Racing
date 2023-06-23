using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateMeshCollider : MonoBehaviour
{
    private SkinnedMeshRenderer meshRenderer;
    private MeshCollider collider;

    void Start()
    {
        meshRenderer = GetComponent<SkinnedMeshRenderer>();
        collider = GetComponent<MeshCollider>();
    }

    private void FixedUpdate()
    {
        UpdateCollider();        
    }

    public void UpdateCollider()
    {
        Mesh colliderMesh = new Mesh();
        meshRenderer.BakeMesh(colliderMesh);
        collider.sharedMesh = null;
        collider.sharedMesh = colliderMesh;
    }
}
