//Easy Collider Editor created by Patrick Murphy.
//Please contact pmurph.software@gmail.com for any questions, comments, and support. 
//Please check include docuementation .pdf in the EasyColliderEditor folder for common problems users have encountered.
//If you have any ideas for improvements, or have specific use cases that you wish implemented, just e-mail me and I will see what I can do.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Contains both the mesh, and the transform associated with the mesh.
/// Used while generating colliders between multiple meshes, allows for easy world-space conversion of vertex positions from the transforms.
/// Also allows for easy updating of the location of vertices to be updated as the mesh moves.
/// </summary>
public class MeshTransform
{
    public Mesh mesh { get; private set; }
    public Transform transform { get; private set; }

    public List<Vector3> WorldSpaceVertices { get; private set; }

    public Vector3 listCreationPosition;// { get; private set; }
    public Quaternion listCreationRotation;// { get; private set; }

    //need both scales for updating vertex positions as occasionally lossy scales can be skewed and will not update positions properly when entering a value for scale instead of mousedrag/incremental scaling.
    public Vector3 listCreationScale;// { get; private set; }
    public Vector3 listCreationLossyScale;// { get; private set; }

    /// <summary>
    /// Creates a new MeshTransform, keeps the current position, rotation, and scale on creation. Generates the WorldSpace Vertex list.
    /// </summary>
    /// <param name="m"></param>
    /// <param name="t"></param>
    public MeshTransform(Mesh m, Transform t)
    {
        mesh = m;
        transform = t;
        WorldSpaceVertices = new List<Vector3>();
        listCreationPosition = t.position;
        listCreationRotation = t.rotation;
        listCreationScale = t.localScale;
        listCreationLossyScale = t.lossyScale;
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            WorldSpaceVertices.Add(transform.TransformPoint(mesh.vertices[i]));
        }
    }

    /// <summary>
    /// Checks if the mesh transform has moved and updates the worldspace vertex list of the mesh if it has.
    /// returns true if there was movement of transform, false if not.
    /// </summary>
    public bool CheckForMovement()
    {
        if (transform != null)
        {
            if (transform.position != listCreationPosition || transform.rotation != listCreationRotation || transform.lossyScale != listCreationLossyScale || transform.localScale != listCreationScale)
            {
                WorldSpaceVertices.Clear();
                listCreationPosition = transform.position;
                listCreationRotation = transform.rotation;
                listCreationScale = transform.localScale;
                listCreationLossyScale = transform.lossyScale;
                for (int i = 0; i < mesh.vertexCount; i++)
                {
                    WorldSpaceVertices.Add(transform.TransformPoint(mesh.vertices[i]));
                }
                return true;
            }
        }
        return false;
    }
}