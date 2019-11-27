//Easy Collider Editor created by Patrick Murphy.
//Please contact pmurph.software@gmail.com for any questions, comments, and support. 
//Please check include docuementation .pdf in the EasyColliderEditor folder for common problems users have encountered.
//If you have any ideas for improvements, or have specific use cases that you wish implemented, just e-mail me and I will see what I can do.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshListBuilder
{
    /// <summary>
    /// Generates the mesh list from the base game object, searches for meshes/skinned meshes on children if include child meshes set to true.
    /// </summary>
    /// <param name="obj"></param>
    public List<MeshTransform> GetMeshList(GameObject baseGameObject, bool IncludeChildMeshes)
    {
        List<MeshTransform> meshTransformList = new List<MeshTransform>();
        if (baseGameObject != null)
        {
            MeshFilter meshFilter = baseGameObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                if (meshFilter.sharedMesh != null)
                {
                    MeshTransform meshTransform = new MeshTransform(meshFilter.sharedMesh, baseGameObject.transform);
                    meshTransformList.Add(meshTransform);
                }
            }
            else
            {
                SkinnedMeshRenderer skinnedMeshRenderer = baseGameObject.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer != null)
                {
                    if (skinnedMeshRenderer.sharedMesh != null)
                    {
                        MeshTransform meshTransform = new MeshTransform(skinnedMeshRenderer.sharedMesh,
                            baseGameObject.transform);
                        meshTransformList.Add(meshTransform);
                    }
                }
            }
            if (IncludeChildMeshes)
            {
                meshTransformList = GetAndAddChildMeshesToList(meshTransformList, baseGameObject.transform);
            }
        }
        if (meshTransformList.Count == 0)
        {
            Debug.LogWarning("No meshes found! Make sure that the object you are selecting has a mesh filter, and a mesh renderer OR a skinned mesh renderer. " +
                "Or set include child meshes to true if selecting root empty/base object.", baseGameObject.transform);
        }
        return meshTransformList;
    }


    /// <summary>
    /// Goes through the list of all the children and the childrens children to find meshes (skinned or regular) to add to the mesh list.
    /// </summary>
    /// <param name="meshList"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    private List<MeshTransform> GetAndAddChildMeshesToList(List<MeshTransform> meshTransformList, Transform parent)
    {
        var childCount = parent.childCount;

        for (var i = 0; i < childCount; i++)
        {
            var t = parent.GetChild(i);
            var meshFilter = t.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                if (meshFilter.sharedMesh != null)
                {
                    MeshTransform meshTransform = new MeshTransform(meshFilter.sharedMesh, t);
                    meshTransformList.Add(meshTransform);
                }
            }
            else
            {
                var skinnedMeshRender = t.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRender != null)
                {
                    if (skinnedMeshRender.sharedMesh != null)
                    {
                        MeshTransform meshTransform = new MeshTransform(skinnedMeshRender.sharedMesh, t);
                        meshTransformList.Add(meshTransform);
                    }
                }
            }
            GetAndAddChildMeshesToList(meshTransformList, t);
        }
        return meshTransformList;
    }
}
