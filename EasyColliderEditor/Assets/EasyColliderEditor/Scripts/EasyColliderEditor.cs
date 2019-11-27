using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Debug = UnityEngine.Debug;
using UnityEditorInternal;

[ExecuteInEditMode]
public class EasyColliderEditor : MonoBehaviour
{
    //编辑器窗口与MonoBehaviour耦合，以在未聚焦时重新绘制编辑器窗口。
    //允许按钮在未聚焦时在顶点选择期间显示/消失。
    public EditorWindow editorWindow;
    
    public GameObject SelectedGameObject;
    public List<MeshTransform> MTList;

    public bool SelectVertByRaycast;
    public bool SelectColliderByRaycast;
    public bool IsTrigger;

    public Collider SelectedCollider;
    public Collider EditCollider;

    public List<Vector3> SelectedVertices { get; private set; }

    public bool DrawVertices;
    private bool DrawSelectedVerts;
    private bool DrawHoverVertGizmo;
    private Vector3 DrawHoverVertGizmoPos;

    private EasyColliderPreferences _easyColliderPreferences;

    void OnEnable()
    {
        SelectedVertices = new List<Vector3>();
        SceneView.onSceneGUIDelegate += OnSceneGUI;
        if (_easyColliderPreferences == null)
        {
            _easyColliderPreferences = AssetDatabase.LoadAssetAtPath(EasyColliderPreferences.PreferencesPath, typeof(EasyColliderPreferences)) as EasyColliderPreferences;
        }
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }


    void OnSceneGUI(SceneView sceneView)
    {
        if (EventType.KeyDown == Event.current.type)
        {
            if(Event.current.control)
            {
                Vector3 euler = sceneView.rotation.eulerAngles;
                euler.z = 0;
                switch (Event.current.keyCode)
                {
                    case KeyCode.LeftArrow:
                        if (!sceneView.orthographic)
                            sceneView.orthographic = true;
                        euler.y = euler.y < 0 ? euler.y + 180 : euler.y;
                        euler.y = Mathf.CeilToInt(euler.y);
                        euler.y = Mathf.FloorToInt((euler.y - 90) / 90) * 90;
                        sceneView.LookAt(sceneView.pivot, Quaternion.Euler(euler));
                        sceneView.orthographic = true;
                        break;
                    case KeyCode.RightArrow:
                        if (!sceneView.orthographic)
                            sceneView.orthographic = true;
                        euler.y = euler.y < 0 ? euler.y + 180 : euler.y;
                        euler.y = Mathf.CeilToInt(euler.y);
                        euler.y = Mathf.FloorToInt((euler.y + 90) / 90) * 90;
                        sceneView.LookAt(sceneView.pivot, Quaternion.Euler(euler));
                        break;
                    case KeyCode.UpArrow:
                        if (!sceneView.orthographic)
                            sceneView.orthographic = true;
                        euler.x = euler.x < 0 ? euler.x + 180 : euler.x;
                        euler.x = Mathf.CeilToInt(euler.x);
                        euler.x = Mathf.FloorToInt((euler.x + 90) / 90) * 90;
                        sceneView.LookAt(sceneView.pivot, Quaternion.Euler(euler));
                        break;
                    case KeyCode.DownArrow:
                        if (!sceneView.orthographic)
                            sceneView.orthographic = true;
                        euler.x = euler.x < 0 ? euler.x + 180 : euler.x;
                        euler.x = Mathf.CeilToInt(euler.x);
                        euler.x = Mathf.FloorToInt((euler.x - 90) / 90) * 90;
                        sceneView.LookAt(sceneView.pivot, Quaternion.Euler(euler));
                        break;
                }
            }
        }

        if (_easyColliderPreferences != null)
        {
            if (SelectedGameObject != null && MTList != null)
            {
                if (SelectVertByRaycast)
                {
                    SelectVerticesByRaycast();
                    if (Event.current.type == EventType.KeyUp && Event.current.isKey && Event.current.keyCode == _easyColliderPreferences.VertSelectKeyCode)
                    {
                        if (!SelectedVertices.Contains(DrawHoverVertGizmoPos))
                        {
                            SelectedVertices.Add(DrawHoverVertGizmoPos);
                        }
                        else
                        {
                            SelectedVertices.Remove(DrawHoverVertGizmoPos);
                        }
                        SceneView.RepaintAll();
                    }

                }
                if (SelectColliderByRaycast)
                {
                    SelectCollidersByRaycast();
                    if (SelectedCollider!=null)
                    {
                        if (Event.current.type == EventType.KeyUp && Event.current.isKey && Event.current.keyCode == _easyColliderPreferences.ColliderSelectKeyCode)
                        {
                            SetColliderEdit(SelectedCollider);
                        }
                        DrawSelectedCollider();
                    }
                }
                else if (SelectedCollider!=null)
                {
                    SelectedCollider = null;
                    SetColliderEdit(null);
                }
                
            }
            if (editorWindow != null)
            {//重新绘制编辑器窗口，以便出现可以使用选定顶点创建的碰撞器的按钮。
                editorWindow.Repaint();
            }
        }
        else
        {
            _easyColliderPreferences = AssetDatabase.LoadAssetAtPath(EasyColliderPreferences.PreferencesPath, typeof(EasyColliderPreferences)) as EasyColliderPreferences;
        }

        if (_easyColliderPreferences==null)
        {
            Debug.LogWarning("Cannot find collider editor prefences, EasyColliderPreferences.asset it should be located in " + EasyColliderPreferences.PreferencesPath);
        }
        if (editorWindow==null)
        {
            OnDisable();
            Undo.DestroyObjectImmediate(this.gameObject);
        }
    }

    void SelectCollidersByRaycast()
    {
        if (Camera.current!=null)
        {
            Vector2 mousePosition = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Collider HitCollider = hit.collider;
                if (HitCollider != null)
                {
                    if (!(HitCollider is MeshCollider))
                    {
                        bool isInMTListOrColliderHolder = false;
                        if (HitCollider.transform.parent == SelectedGameObject.transform)
                        {
                            if (HitCollider.transform.gameObject.name == "Collider Holder")
                            {
                                isInMTListOrColliderHolder = true;
                            }
                        }
                        if (HitCollider.transform.gameObject.name == "RotatedCollider")
                        {
                            isInMTListOrColliderHolder = true;
                        }
                        if (!isInMTListOrColliderHolder)
                        {
                            foreach (MeshTransform mt in MTList)
                            {
                                if (mt.transform == HitCollider.transform)
                                {
                                    isInMTListOrColliderHolder = true;
                                }
                            }
                        }
                        if (isInMTListOrColliderHolder)
                        {
                            SelectedCollider = HitCollider;
                        }
                    }
                }
            }
        }
    }

    void SetColliderEdit(Collider collider)
    {
        if ((EditCollider == null && collider != null) || (EditCollider != null && EditCollider != collider))
        {
            EditCollider = collider;
            ActiveEditorTracker editorTracker = ActiveEditorTracker.sharedTracker;
            Editor[] editors = editorTracker.activeEditors;
            for (int i = 1; i < editors.Length; i++)
            {
                Editor et = editors[i];
                if (EditCollider == (et.target as Collider))
                {
                    if (et is CustomBoxColliderEditor)
                    {
                        CustomBoxColliderEditor ced = et as CustomBoxColliderEditor;
                        ced.SetEditMode(true);
                    }
                    else if (et is CustomSphereCollider)
                    {
                        CustomSphereCollider ced = et as CustomSphereCollider;
                        ced.SetEditMode(true);
                    }
                    else if (et is CustomCapsuleColliderEditor)
                    {
                        CustomCapsuleColliderEditor ced = et as CustomCapsuleColliderEditor;
                        ced.SetEditMode(true);
                    }
                    break;
                }
            }
            return;
        }

        if ((EditCollider != null && EditCollider == collider) || (EditCollider != null && collider == null))
        {
            ActiveEditorTracker editorTracker = ActiveEditorTracker.sharedTracker;
            Editor[] editors = editorTracker.activeEditors;
            for (int i = 1; i < editors.Length; i++)
            {
                Editor et = editors[i];
                if (EditCollider == (et.target as Collider))
                {
                    if (et is CustomBoxColliderEditor)
                    {
                        CustomBoxColliderEditor ced = et as CustomBoxColliderEditor;
                        ced.SetEditMode(false);
                    }
                    else if (et is CustomSphereCollider)
                    {
                        CustomSphereCollider ced = et as CustomSphereCollider;
                        ced.SetEditMode(true);
                    }
                    else if (et is CustomCapsuleColliderEditor)
                    {
                        CustomCapsuleColliderEditor ced = et as CustomCapsuleColliderEditor;
                        ced.SetEditMode(true);
                    }
                    break;
                }
            }
            EditCollider = null;
        }
    }

    void SelectVerticesByRaycast()
    {
        if (Camera.current != null && SelectedGameObject != null && Event.current!=null)
        {
            if (EditorWindow.focusedWindow != SceneView.currentDrawingSceneView)
            {//Focuses the editor scene window if it is not currently focused.
                if (SceneView.currentDrawingSceneView != null)
                {
                   SceneView.currentDrawingSceneView.Focus();
                }
            }
            Vector2 mousePos = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Vector3 closestVertex = GetClosestVertexPosToRaycastHit(hit.point);
                DrawHoverVertGizmo = true;
                DrawHoverVertGizmoPos = closestVertex;
                SceneView.RepaintAll(); //updates scene view so there is no delay between moving cursor and displaying the closest vertex.
            }
            else
            {
                DrawHoverVertGizmo = false;
            }

        }
        //some helpful warnings to be logged if something goes wrong, can only happen in rare circumstances.
        else if (Camera.current == null)
        {
            Debug.LogWarning("Current camera cannot be found, try reloading scene");
        }
        else if (SelectedGameObject == null)
        {
            Debug.LogWarning("Cannot find selected game object, try reselecting game object.");
        }
    }

    Vector3 GetClosestVertexPosToRaycastHit(Vector3 hitPosition)
    {
        Vector3 vertexPos = new Vector3();
        float minDistance = Mathf.Infinity;
        foreach (MeshTransform mt in MTList)
        {
                foreach (Vector3 vertex in mt.WorldSpaceVertices)
                {
                    float distance = Vector3.Distance(hitPosition, vertex);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        vertexPos = vertex;
                    }
                }
        }
        return vertexPos;
    }

    private void DrawSelectedCollider()
    {
        if(EditCollider != null)
        {
            DrawCollider(EditCollider, _easyColliderPreferences.SelectedColliderColour);
        }

        if (SelectedCollider != null && SelectedCollider != EditCollider)
        {
            DrawCollider(SelectedCollider, _easyColliderPreferences.HoverColliderColour);
        }
    }
    private void DrawCollider(Collider collider, Color color)
    {
        if (collider is BoxCollider)
        {
            BoxCollider col = (BoxCollider)collider;
            DrawBoxCollider(col, color);
        }
        else if (collider is SphereCollider)
        {
            SphereCollider col = (SphereCollider)collider;
            DrawSphereCollider(col, color);
        }
        else if (collider is CapsuleCollider)
        {
            CapsuleCollider col = (CapsuleCollider)collider;
            DrawCapsuleCollider(col, color);
        }
    }
    private void DrawSphereCollider(SphereCollider collider, Color color)
    {
        Handles.color = color;
        Vector3 center = collider.transform.TransformPoint(collider.center);
        float radius = collider.radius;
        Handles.DrawWireDisc(center, Vector3.up, radius);
        Handles.DrawWireDisc(center, Vector3.left, radius);
        Handles.DrawWireDisc(center, Vector3.forward, radius);
    }
    private void DrawBoxCollider(BoxCollider collider, Color color)
    {
        Handles.color = color;
        Vector3 center = collider.center;
        Vector3 size = collider.size/2;
        Vector3 p1 = collider.transform.TransformPoint(center + size);
        Vector3 p2 = collider.transform.TransformPoint(center - size);
        Vector3 p3 = collider.transform.TransformPoint(center + new Vector3(size.x, size.y, -size.z));
        Vector3 p4 = collider.transform.TransformPoint(center + new Vector3(size.x, -size.y, size.z));
        Vector3 p5 = collider.transform.TransformPoint(center + new Vector3(size.x, -size.y, -size.z));
        Vector3 p6 = collider.transform.TransformPoint(center + new Vector3(-size.x, size.y, size.z));
        Vector3 p7 = collider.transform.TransformPoint(center + new Vector3(-size.x, -size.y, size.z));
        Vector3 p8 = collider.transform.TransformPoint(center + new Vector3(-size.x, size.y, -size.z));
        Handles.DrawLine(p1, p3);
        Handles.DrawLine(p1, p4);
        Handles.DrawLine(p1, p6);
        Handles.DrawLine(p8, p3);
        Handles.DrawLine(p8, p6);
        Handles.DrawLine(p8, p2);
        Handles.DrawLine(p7, p6);
        Handles.DrawLine(p7, p2);
        Handles.DrawLine(p7, p4);
        Handles.DrawLine(p5, p4);
        Handles.DrawLine(p5, p2);
        Handles.DrawLine(p5, p3);
    }
    private void DrawCapsuleCollider(CapsuleCollider collider, Color color)
    {
        Handles.color = color;
        Vector3 center = collider.center;
        float radius = collider.radius;
        float height = collider.height;
        int direction = collider.direction;
        float length = (height / 2 - radius);
        Vector3 p1 = Vector3.zero, p2, p3, p4, p5, p6, p7, p8;// = Vector3.zero;
        p1 = p2 = p3 = p4 = p5 = p6 = p7 = p8 = Vector3.zero;
        Vector3 centerTop = Vector3.zero;
        Vector3 centerBottom = Vector3.zero;
        if (direction == 0)
        {
            p1 = new Vector3(center.x - length, center.y + radius, center.z);
            p2 = new Vector3(center.x + length, center.y + radius, center.z);
            p3 = new Vector3(center.x + length, center.y, center.z + radius);
            p4 = new Vector3(center.x - length, center.y, center.z + radius);
            p5 = new Vector3(center.x + length, center.y, center.z - radius);
            p6 = new Vector3(center.x - length, center.y, center.z - radius);
            p7 = new Vector3(center.x - length, center.y - radius, center.z);
            p8 = new Vector3(center.x + length, center.y - radius, center.z);
            centerTop = new Vector3(center.x + length, center.y, center.z);
            centerBottom = new Vector3(center.x - length, center.y, center.z);
        }
        if (direction == 1)
        {
            p1 = new Vector3(center.x + radius, center.y - length, center.z);
            p2 = new Vector3(center.x + radius, center.y + length, center.z);
            p3 = new Vector3(center.x, center.y - length, center.z + radius);
            p4 = new Vector3(center.x, center.y + length, center.z + radius);
            p5 = new Vector3(center.x, center.y - length, center.z - radius);
            p6 = new Vector3(center.x, center.y + length, center.z - radius);
            p7 = new Vector3(center.x - radius, center.y - length, center.z);
            p8 = new Vector3(center.x - radius, center.y + length, center.z);
            centerTop = new Vector3(center.x, center.y + length, center.z);
            centerBottom = new Vector3(center.x, center.y - length, center.z);
        }
        if (direction == 2)
        {
            p1 = new Vector3(center.x + radius, center.y, center.z + length);
            p2 = new Vector3(center.x + radius, center.y, center.z - length);
            p3 = new Vector3(center.x, center.y + radius, center.z + length);
            p4 = new Vector3(center.x, center.y + radius, center.z - length);
            p5 = new Vector3(center.x, center.y - radius, center.z + length);
            p6 = new Vector3(center.x, center.y - radius, center.z - length);
            p7 = new Vector3(center.x - radius, center.y, center.z + length);
            p8 = new Vector3(center.x - radius, center.y, center.z - length);
            centerTop = new Vector3(center.x, center.y, center.z + length);
            centerBottom = new Vector3(center.x, center.y, center.z - length);
        } //
        p1 = collider.transform.TransformPoint(p1);
        p2 = collider.transform.TransformPoint(p2);
        p3 = collider.transform.TransformPoint(p3);
        p4 = collider.transform.TransformPoint(p4);
        p5 = collider.transform.TransformPoint(p5);
        p6 = collider.transform.TransformPoint(p6);
        p7 = collider.transform.TransformPoint(p7);
        p8 = collider.transform.TransformPoint(p8);
        Handles.DrawLine(p1, p2);
        Handles.DrawLine(p3, p4);
        Handles.DrawLine(p5, p6);
        Handles.DrawLine(p7, p8);
        centerTop = collider.transform.TransformPoint(centerTop);
        centerBottom = collider.transform.TransformPoint(centerBottom);
        Handles.DrawWireDisc(centerTop, Vector3.left, radius);
        Handles.DrawWireDisc(centerTop, Vector3.up, radius);
        Handles.DrawWireDisc(centerTop, Vector3.back, radius);
        Handles.DrawWireDisc(centerBottom, Vector3.left, radius);
        Handles.DrawWireDisc(centerBottom, Vector3.up, radius);
        Handles.DrawWireDisc(centerBottom, Vector3.back, radius);
    }

    void OnDrawGizmos()
    {
        if (SelectedGameObject != null && _easyColliderPreferences != null)
        {
            if (MTList != null)
            {
                foreach (MeshTransform mt in MTList)
                {
                    bool ObjectHasMoved = mt.CheckForMovement();
                    if (ObjectHasMoved)
                    {
                        SelectedVertices.Clear();
                    }
                }
                if (DrawVertices)
                {
                    DrawVertexDisplayGizmos(_easyColliderPreferences.DisplayVerticesColour, new Vector3(_easyColliderPreferences.DisplayVerticesScaling,
                        _easyColliderPreferences.DisplayVerticesScaling, _easyColliderPreferences.DisplayVerticesScaling));
                }
            }
            if (DrawHoverVertGizmo)
            {
                Gizmos.color = _easyColliderPreferences.HoverVertColour;
                Gizmos.DrawCube(DrawHoverVertGizmoPos, new Vector3(_easyColliderPreferences.HoverVertScaling,
                    _easyColliderPreferences.HoverVertScaling, _easyColliderPreferences.HoverVertScaling));
            }
            if (SelectedVertices.Count > 0)
            {
                foreach (Vector3 vertex in SelectedVertices)
                {
                    Gizmos.color = _easyColliderPreferences.SelectedVertexColour;
                    Gizmos.DrawCube(vertex, new Vector3(_easyColliderPreferences.SelectedVertScaling,
                        _easyColliderPreferences.SelectedVertScaling, _easyColliderPreferences.SelectedVertScaling));
                }
                if (SelectedVertices.Contains(DrawHoverVertGizmoPos))
                {
                    Gizmos.color = _easyColliderPreferences.OverlapSelectedVertColour;
                    Gizmos.DrawCube(DrawHoverVertGizmoPos, new Vector3(_easyColliderPreferences.OverlapSelectedVertScale,
                        _easyColliderPreferences.OverlapSelectedVertScale, _easyColliderPreferences.OverlapSelectedVertScale));
                }
            }
        }
    }

    private void DrawVertexDisplayGizmos(Color displayColor, Vector3 scale)
    {
        Gizmos.color = displayColor;
        foreach (var mt in MTList)
        {
            foreach (Vector3 vertex in mt.WorldSpaceVertices)
            {
                Gizmos.DrawCube(vertex, scale);
            }
        }
    }

    public void CreateRotatedBoxCollider(Transform attachTo)
    {
        Vector3 center = new Vector3();
        Vector3 size = new Vector3();
        Vector3 cornerVector = new Vector3();
        List<Vector3> localVerts = new List<Vector3>();
        foreach (Vector3 point in SelectedVertices)
        {
            localVerts.Add(this.transform.InverseTransformPoint(point)); //transforms to local vertexs
        }
        float minDistance = Mathf.Infinity;
        foreach (Vector3 vectorA in SelectedVertices) //finds corner vertex by calculating max distance. corner will have least distance.
        {
            float distance = 0;
            foreach (Vector3 vectorB in SelectedVertices)
            {
                if (SelectedVertices.IndexOf(vectorA) != SelectedVertices.IndexOf(vectorB))
                {
                    distance += Vector3.Distance(vectorA, vectorB);
                }
            }
            if (distance < minDistance)
            {
                cornerVector = vectorA;
                minDistance = distance;
            }
        }
        float maxDistance = 0;
        Vector3 forwardVector = new Vector3();
        Vector3 forwardVertex = new Vector3();
        foreach (Vector3 vector in SelectedVertices) //calculates forward vector for quaternion creation.
        {
            if (SelectedVertices.IndexOf(cornerVector) != SelectedVertices.IndexOf(vector))
            {

                float distance = Vector3.Distance(cornerVector, vector);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    forwardVector = vector - cornerVector;
                    forwardVertex = vector;
                }
            }
        }
        Vector3 otherVector = new Vector3();
        bool isOtherVectorAlreadyOrtho = false;
        float dotproduct = 100000000F;
        foreach (Vector3 vector in SelectedVertices) //calculates the other vector used to get CrossProduct for othogonal vector for quaternion creation.
        {
            if (SelectedVertices.IndexOf(vector) != SelectedVertices.IndexOf(forwardVertex) &&
                SelectedVertices.IndexOf(vector) != SelectedVertices.IndexOf(cornerVector))
            {
                Vector3 otherVector2 = vector - cornerVector;
                if (Vector3.Dot(forwardVector, otherVector2) == 0F) //ie is already orthogonal
                {
                    isOtherVectorAlreadyOrtho = true;
                    otherVector = otherVector2;
                    break;
                }
                else //checks to see which is closest to orthogonal already to use that factor in ortho calculation for improved accuracy.
                {
                    float dot = Vector3.Dot(Vector3.forward, otherVector2);
                    if (dot < dotproduct)
                    {
                        otherVector = otherVector2;
                        dotproduct = dot;
                    }
                }
            }
        }
        Vector3 orthoVector = new Vector3();
        if (isOtherVectorAlreadyOrtho)
        {
            orthoVector = otherVector;
        }
        else
        {
            orthoVector = Vector3.Cross(forwardVector, otherVector);
        }
        GameObject rotatedCollider = new GameObject("RotatedCollider");
        Undo.RegisterCreatedObjectUndo(rotatedCollider, "Created rotated collider");
        Undo.SetTransformParent(rotatedCollider.transform, attachTo, "RotatedCollider");
        //    rotatedCollider.transform.SetParent(attachTo);
        rotatedCollider.transform.localPosition = Vector3.zero;
        Quaternion q = new Quaternion();
        q.SetLookRotation(forwardVector, orthoVector);
        rotatedCollider.transform.rotation = q;
        rotatedCollider.layer = 30; //for use in selection by raycast.
        Undo.AddComponent<BoxCollider>(rotatedCollider);
        BoxCollider box = rotatedCollider.GetComponent<BoxCollider>();
        int cornerIndex = SelectedVertices.IndexOf(cornerVector);
        List<Vector3> localBoxVerts = new List<Vector3>();
        foreach (Vector3 vert in SelectedVertices)
        {
            localBoxVerts.Add(rotatedCollider.transform.InverseTransformPoint(vert));
        }
        foreach (Vector3 vertex in SelectedVertices)
        {
            size += rotatedCollider.transform.InverseTransformPoint(vertex) - rotatedCollider.transform.InverseTransformPoint(SelectedVertices[cornerIndex]);
            if (SelectedVertices.IndexOf(vertex) != cornerIndex)
            {
                center += (vertex - SelectedVertices[cornerIndex]) / 2;
            }
        }
        center += SelectedVertices[cornerIndex];
        box.size = size;
        box.center = rotatedCollider.transform.InverseTransformPoint(center);
        SelectedVertices.Clear();
        Selection.activeGameObject = attachTo.gameObject;
    }

    /// <summary>
    /// calculates height vert, and corner vert is based on height vert, then radius.
    /// </summary>
    /// <param name="attachTo"></param>
    public void CreateCapsuleColliderAlternate(Transform attachTo)
    {
        if (SelectedVertices.Count > 0)
        {
            for (int i = 0; i < SelectedVertices.Count; i++)
            {//converts selected vertices to local positions of object to attach collider to.
                SelectedVertices[i] = attachTo.InverseTransformPoint(SelectedVertices[i]);
            }
            Vector3 cornerVert = new Vector3();
            Vector3 radiusVert = new Vector3();
            Vector3 heightVert = new Vector3();
            float maxDistance = 0;
            int cornerIndex = 0;
            for (int i = 0; i < SelectedVertices.Count; i++)
            { //Calculates height vertex as vertex furest from all the others. and corner vertex and vertex closest to the height vertex.
               
                float distance = 0;
                float minDistance = Mathf.Infinity;
                for (int j = 0; j < SelectedVertices.Count; j++)
                {
                    if (i != j)
                    {
                        float vecDistance = Vector3.Distance(SelectedVertices[i], SelectedVertices[j]);
                        distance += vecDistance;
                        if (vecDistance < minDistance)
                        {
                            minDistance = vecDistance;
                            cornerIndex = j;
                        }
                    }
                }
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    heightVert = SelectedVertices[i];
                    cornerVert = SelectedVertices[cornerIndex];
                }
            }
            float radius = 0;
            for (int i = 0; i < SelectedVertices.Count; i++)
            { //calculates radius as closest vert distance that isn't height
                if (SelectedVertices[i] != cornerVert && SelectedVertices[i] != heightVert)
                {
                    float distance = Vector3.Distance(SelectedVertices[i], cornerVert);
                    if (distance > radius)
                    {
                        radius = distance;
                        radiusVert = SelectedVertices[i];
                    }
                }
            }
            int direction;
            float dX = Mathf.Abs(heightVert.x - cornerVert.x);
            float dY = Mathf.Abs(heightVert.y - cornerVert.y);
            if (dX < 0.001F)
            {
                if (dY < 0.001F)
                {
                    direction = 2;
                }
                else
                {
                    direction = 1;
                }
            }
            else
            {
                direction = 0;
            }
            float maxHeight = Vector3.Distance(cornerVert, heightVert);
            Vector3 center = (heightVert + radiusVert) / 2;
            CapsuleCollider capsule = Undo.AddComponent<CapsuleCollider>(attachTo.gameObject);
            capsule.radius = Vector3.Distance(cornerVert, radiusVert) / 2;
            capsule.height = maxHeight + (capsule.radius * 2);
            capsule.center = center;
            capsule.direction = direction;
            capsule.isTrigger = IsTrigger;
            SelectedVertices.Clear();
            Selection.activeGameObject = attachTo.gameObject;
        }
    }

    /// <summary>
    /// calculates corner first, then height & radius.
    /// </summary>
    /// <param name="attachTo"></param>
    public void CreateCapsuleCollider(Transform attachTo)
    {
        if (SelectedVertices.Count > 0)
        {
            for (int i = 0; i < SelectedVertices.Count; i++)
            {//converts selected vertices to local positions of object to attach collider to.
                SelectedVertices[i] = attachTo.InverseTransformPoint(SelectedVertices[i]);
            }
            Vector3 cornerVert = new Vector3();
            Vector3 radiusVert = new Vector3();
            Vector3 heightVert = new Vector3();
            float minDistance = Mathf.Infinity;
            for (int i = 0; i < SelectedVertices.Count; i++)
            { //Calculates corner vertex.
                float cornerDist = 0F;
                for (int j = 0; j < SelectedVertices.Count; j++)
                {
                    if (i != j)
                    {
                        cornerDist += Vector3.Distance(SelectedVertices[i], SelectedVertices[j]);
                    }
                }
                if (cornerDist < minDistance)
                {
                    cornerVert = SelectedVertices[i];
                    minDistance = cornerDist;
                }
            }
            float radius = Mathf.Infinity;
            float height = 0;
            for (int i = 0; i < SelectedVertices.Count; i++)
            { //calculates height and radius vertex / distances.
                if (SelectedVertices[i] != cornerVert)
                {
                    float distance = Vector3.Distance(SelectedVertices[i], cornerVert);
                    if (distance < radius)
                    {
                        radius = distance;
                        radiusVert = SelectedVertices[i];
                    }
                    if (distance > height)
                    {
                        height = distance;
                        heightVert = SelectedVertices[i];
                    }
                }
            }
            int direction;
            float dX = Mathf.Abs(heightVert.x - cornerVert.x);
            float dY = Mathf.Abs(heightVert.y - cornerVert.y);
            if (dX < 0.001F)
            {
                if (dY < 0.001F)
                {
                    direction = 2;
                }
                else
                {
                    direction = 1;
                }
            }
            else
            {
                direction = 0;
            }
            float maxHeight = Vector3.Distance(cornerVert, heightVert);
            Vector3 center = (heightVert + radiusVert) / 2;
            CapsuleCollider capsule = Undo.AddComponent<CapsuleCollider>(attachTo.gameObject);
            capsule.radius = Vector3.Distance(cornerVert, radiusVert) / 2;
            capsule.height = maxHeight + (capsule.radius*2);
            capsule.center = center;
            capsule.direction = direction;
            capsule.isTrigger = IsTrigger;
            SelectedVertices.Clear();
            Selection.activeGameObject = attachTo.gameObject;
        }
    }

    /// <summary>
    /// Creates a sphere collider by simplying calculating the two points furthest from eachother and using those to calculate radius and center.
    /// </summary>
    /// <param name="attachTo"></param>
    public void CreateSphereCollider(Transform attachTo)
    {
        if (SelectedVertices.Count > 0)
        {
            for (int i = 0; i < SelectedVertices.Count; i++)
            {//converts selected vertices to local positions of object to attach collider to.
                SelectedVertices[i] = attachTo.InverseTransformPoint(SelectedVertices[i]);
            }
            Vector3 distVert1 = new Vector3();
            Vector3 distVert2 = new Vector3();
            float maxDistance = 0;
            for (int i = 0; i < SelectedVertices.Count; i++)
            {
                for (int j = i + 1; j < SelectedVertices.Count; j++)
                {
                    float distance = Vector3.Distance(SelectedVertices[i], SelectedVertices[j]);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        distVert1 = SelectedVertices[i];
                        distVert2 = SelectedVertices[j];
                    }
                }
            }
            Vector3 center = (distVert1 + distVert2) / 2;
            SphereCollider sphereCollider = Undo.AddComponent<SphereCollider>(attachTo.gameObject);
            sphereCollider.radius = maxDistance / 2;
            sphereCollider.center = center;
            sphereCollider.isTrigger = IsTrigger;
            SelectedVertices.Clear();
            Selection.activeGameObject = attachTo.gameObject;
        }
    }

    /// <summary>
    /// Creates box collider by finding the min and max x,y,z then creating the box collider's size and center based on those values.
    /// </summary>
    /// <param name="attachTo"></param>
    public void CreateBoxCollider(Transform attachTo)
    {
        if (SelectedVertices.Count > 0)
        {
            for (int i = 0; i < SelectedVertices.Count; i++)
            {//converts selected vertices to local positions of object to attach collider to.
                SelectedVertices[i] = attachTo.InverseTransformPoint(SelectedVertices[i]);
            }
            float xMin, xMax = xMin = SelectedVertices[0].x;
            float yMin, yMax = yMin = SelectedVertices[0].y;
            float zMin, zMax = zMin = SelectedVertices[0].z;

            foreach (Vector3 vector in SelectedVertices)
            { //gets min and max for vectors for box creation.
                xMin = (vector.x < xMin) ? vector.x : xMin;
                xMax = (vector.x > xMax) ? vector.x : xMax;
                yMin = (vector.y < yMin) ? vector.y : yMin;
                yMax = (vector.y > yMax) ? vector.y : yMax;
                zMin = (vector.z < zMin) ? vector.z : zMin;
                zMax = (vector.z > zMax) ? vector.z : zMax;
            }
            Vector3 maxVector = new Vector3(xMax, yMax, zMax);
            Vector3 minVector = new Vector3(xMin, yMin, zMin);
            Vector3 size = maxVector - minVector;
            Vector3 center = (maxVector + minVector)/2;
            BoxCollider boxCollider = Undo.AddComponent<BoxCollider>(attachTo.gameObject);
            boxCollider.size = size;
            boxCollider.center = center;
            boxCollider.isTrigger = IsTrigger;
            SelectedVertices.Clear();
            Selection.activeGameObject = attachTo.gameObject;
        }
    }
}
