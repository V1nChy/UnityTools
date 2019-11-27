using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class EasyColliderEditorWindow : EditorWindow
{
    enum AttachmentEnum
    {
        SelectedGameObject,
        ColliderHolders,
    }
    AttachmentEnum attachmentStyle;

    private GameObject _easyColliderGameObject;
    private EasyColliderEditor _easyColliderEditor;

    public GameObject SelectedGameObject;
    public Transform TransformToAttachCollidersTo;

    public List<MeshTransform> MeshTransformList;

    public List<Component> AddedComponents;
    public List<Component> DisabledComponents; 

    public bool UpdateSceneDisplay;
    public bool DisplayMeshVertexHandles;

    public bool AttachToBaseObject;
    public bool AttachToColliderHolders;

    private bool _includeMeshesFromChildren = true;
    private bool _displayPreferences = false;
    public bool VertexSelectEnabled;
    public bool ColliderSelectEnabled;
    public bool IncludeMeshesFromChildren = true;
    public bool IsTrigger = false;

    private EasyColliderPreferences _easyColliderPreferences;

    [MenuItem("GameObject/ColliderEditor", false, 11)]
    static void Open()
    {
        EasyColliderEditorWindow ecew = GetWindow<EasyColliderEditorWindow>("Collider编辑器");
        if(Selection.activeObject != null)
        {
            ecew.VertexSelectEnabled = true;
            ecew.DisplayMeshVertexHandles = true;

            ecew.SelectedGameObject = Selection.activeObject as GameObject;
            if(ecew.MeshTransformList != null)
            {
                ecew.MeshTransformList.Clear();
                ecew.MeshTransformList = null;
            }
        }
    }

    void OnEnable()
    {
        if (_easyColliderPreferences == null)
        {
            _easyColliderPreferences = AssetDatabase.LoadAssetAtPath(EasyColliderPreferences.PreferencesPath, typeof(EasyColliderPreferences)) as EasyColliderPreferences;
            if (_easyColliderPreferences == null)
            {
                _easyColliderPreferences = CreateInstance<EasyColliderPreferences>();
                _easyColliderPreferences.SetDefaultValues();
                AssetDatabase.CreateAsset(_easyColliderPreferences, EasyColliderPreferences.PreferencesPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        if (_easyColliderGameObject == null)
        {
            _easyColliderGameObject = new GameObject("EasyColliderEditor");
            Undo.RegisterCreatedObjectUndo(_easyColliderGameObject, "Created EasyColliderEditorObject");
            _easyColliderEditor = Undo.AddComponent<EasyColliderEditor>(_easyColliderGameObject);
            _easyColliderEditor.editorWindow = this;
        }
    }

    void OnDisable()
    {
        RestoreComponents();
        if (_easyColliderGameObject != null)
        {
            Undo.DestroyObjectImmediate(_easyColliderGameObject);
        }
    }

    void OnGUI()
    {
        if (_easyColliderEditor == null)
        {
            if (SelectedGameObject!=null)
            {
                OnDisable();
                SelectedGameObject = null;
            }
            OnEnable();
        }

        EditorGUI.BeginChangeCheck();
        SelectedGameObject =(GameObject)EditorGUILayout.ObjectField("编辑对象:", SelectedGameObject, typeof (GameObject), true);
        EditorGUI.EndChangeCheck();
        if (GUI.changed)
        {
            if (SelectedGameObject != null)
            {
                if (!EditorUtility.IsPersistent(SelectedGameObject))
                {
                    GameObjectSelectedHasChanged();
                }
                else
                {
                    SelectedGameObject = null;
                }
            }
            else
            {
                GameObjectSelectedHasChanged(); //取消了编辑对象
            }
        }
        if (GameObjectIsActiveAndFromScene(ref SelectedGameObject))
        {
            if (MeshTransformList == null)
            {
                GameObjectSelectedHasChanged();
            }
            EditorGUI.BeginChangeCheck();
            VertexSelectEnabled = EditorGUILayout.ToggleLeft(new GUIContent("开启顶点选择","开启射线选择顶点视图"), VertexSelectEnabled);
            EditorGUI.EndChangeCheck();
            _easyColliderEditor.SelectVertByRaycast = VertexSelectEnabled;
            if (VertexSelectEnabled && GUI.changed)
            {
                _easyColliderEditor.SelectedVertices.Clear();
                if (EditorWindow.focusedWindow != SceneView.currentDrawingSceneView)
                {
                    if (SceneView.currentDrawingSceneView != null)
                    {
                        SceneView.currentDrawingSceneView.Focus();
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            ColliderSelectEnabled = EditorGUILayout.ToggleLeft(new GUIContent("开启Collider选择", "开启Collider选择视图"), ColliderSelectEnabled);
            EditorGUI.EndChangeCheck();
            _easyColliderEditor.SelectColliderByRaycast = ColliderSelectEnabled;
            if (ColliderSelectEnabled && GUI.changed)
            {
                _easyColliderEditor.SelectedCollider = null;
                if (EditorWindow.focusedWindow != SceneView.currentDrawingSceneView)
                {
                    if (SceneView.currentDrawingSceneView != null)
                    {
                        SceneView.currentDrawingSceneView.Focus();
                    }
                }
            }
            if (_easyColliderEditor.SelectedCollider!=null)
            {
                if (GUILayout.Button(new GUIContent("移除碰Collider", "移除当前所选中的Collider")))
                {
                    Undo.DestroyObjectImmediate(_easyColliderEditor.SelectedCollider);
                    _easyColliderEditor.SelectedCollider = null;
                }
            }

            //Which object to attach created colliders to. Changed to enum popup to improve UI/confusion by having multiple checkboxes where one has to be checked, enum works better for this.
            attachmentStyle = (AttachmentEnum)EditorGUILayout.EnumPopup(new GUIContent("增加到:",
                "用于增加Collider的方法，可以增加到选定的游戏对象（如果可能），也可以增加到创建的子Collider父对象上"), attachmentStyle);
            if (attachmentStyle==AttachmentEnum.ColliderHolders)
            {
                AttachToBaseObject = false;
                AttachToColliderHolders = true;
                TransformToAttachCollidersTo = null;  
            }
            else if (attachmentStyle == AttachmentEnum.SelectedGameObject)
            {
                AttachToBaseObject = true;
                AttachToColliderHolders = false;
                TransformToAttachCollidersTo = SelectedGameObject.transform;
            }

            //collider creation buttons on editor window show up only after vertices are selected.
            if (_easyColliderEditor.SelectedVertices.Count > 0)
            {
                if (GUILayout.Button(new GUIContent("Create Box Collider",
                         "Creates a Box Collider that contains the currently selected vertices. See documentation for vertex selection guide.")))
                {
                    if (_easyColliderEditor.SelectedVertices.Count <= 1)
                    {
                        Debug.LogWarning("To create a box collider correctly at least vertices should be selected. See documentation for for more info.");
                    }
                    CreateOrSetObjectToAttachColliderTo();
                        _easyColliderEditor.CreateBoxCollider(TransformToAttachCollidersTo);
                }
                if (GUILayout.Button(new GUIContent("Create Sphere Collider",
                        "Creates a Sphere Collider that contains the currently selected vertices. See documentation for vertex selection guide.")))
                {
                    if (_easyColliderEditor.SelectedVertices.Count <=1)
                    {
                        Debug.LogWarning("To create a sphere collider at least 2 vertices should be selected. See documentation for more info.");
                    }
                    CreateOrSetObjectToAttachColliderTo();
                    _easyColliderEditor.CreateSphereCollider(TransformToAttachCollidersTo);
                }
                if (GUILayout.Button(new GUIContent("Create Capsule Collider",
                         "Creates a Capsule Collider that contains the currently selected vertices. See documentation for vertex selection guide.")))
                {
                    if (!(_easyColliderEditor.SelectedVertices.Count==3))
                    {
                        Debug.LogWarning("To create a capsule collider correctly 3 vertices should be selected. See documentation for more info.");
                    }
                    CreateOrSetObjectToAttachColliderTo();
                    _easyColliderEditor.CreateCapsuleCollider(TransformToAttachCollidersTo);
                }
                if (GUILayout.Button(new GUIContent("Create Capsule Collider Alternate",
                         "Creates a Capsule Collider that contains the currently selected vertices. See documentation for vertex selection guide.")))
                {
                    if (!(_easyColliderEditor.SelectedVertices.Count >= 3))
                    {
                        Debug.LogWarning("To create a capsule collider correctly using this method at least 3 vertices should be selected. See documentation for more info.");
                    }
                    CreateOrSetObjectToAttachColliderTo();
                    _easyColliderEditor.CreateCapsuleColliderAlternate(TransformToAttachCollidersTo);
                }
                    if (GUILayout.Button(new GUIContent("Create Rotated Box Collider",
                        "Tries to create a Rotated Box Collider that contains the currently selected vertices. See documentation for vertex selection guide.")))
                {
                    if (!(_easyColliderEditor.SelectedVertices.Count == 4))
                    {
                        Debug.LogWarning("To create a rotated box collider correctly 4 vertices should be selected. See documentation for more info.");
                    }
                    //Rotated Colliders create their own object to attach to.
                    _easyColliderEditor.CreateRotatedBoxCollider(SelectedGameObject.transform);
                }
            }
            EditorGUILayout.LabelField("-Additional Toggles");
            //Preferences and Extras
            DisplayMeshVertexHandles = EditorGUILayout.ToggleLeft(new GUIContent("显示网格顶点", "在所有当前可选的顶点上绘制Gizmo"), DisplayMeshVertexHandles);
            _easyColliderEditor.DrawVertices = DisplayMeshVertexHandles;
            IncludeMeshesFromChildren = EditorGUILayout.ToggleLeft(new GUIContent("包括子网格", "包括子网格作为可能的顶点选择。"), IncludeMeshesFromChildren);
            if (IncludeMeshesFromChildren!=_includeMeshesFromChildren)
            {
                _includeMeshesFromChildren = IncludeMeshesFromChildren;
                GameObjectSelectedHasChanged(); //allows regeneration of mesh lists to include/uninclude children.
            }
            EditorGUI.BeginChangeCheck();
            IsTrigger = EditorGUILayout.ToggleLeft(new GUIContent("Collider作为Trigger",
                "设置创建的Collider'Is Trigger'为TRUE"), IsTrigger);
            EditorGUI.EndChangeCheck();
            if (GUI.changed && _easyColliderEditor!=null)
            {
                _easyColliderEditor.IsTrigger = IsTrigger;
            }
            if (GUILayout.Button(new GUIContent("移除对象所有Collider",
                "删除选定游戏对象上的所有Collider，包括在编辑之前存在的Collider。 如果启用了包括子网格物体，则删除所有子网格物体上的Collider")))
            {
                RemoveAllCollidersOnSelectedGameObject(_includeMeshesFromChildren);
            }
            if (_easyColliderPreferences != null)
            {
                _displayPreferences = EditorGUILayout.ToggleLeft(new GUIContent("Edit Preferences","Enables editing of preferences"), _displayPreferences);
                if (_displayPreferences)
                {
                    _easyColliderPreferences.DisplayVerticesColour = EditorGUILayout.ColorField("Vertex Display Colour:",
               _easyColliderPreferences.DisplayVerticesColour);
                    _easyColliderPreferences.DisplayVerticesScaling = EditorGUILayout.FloatField("Vertex Display Scaling:",
                        _easyColliderPreferences.DisplayVerticesScaling);

                    _easyColliderPreferences.HoverVertColour = EditorGUILayout.ColorField("Hover Vertex Colour:",
                        _easyColliderPreferences.HoverVertColour);
                    _easyColliderPreferences.HoverVertScaling = EditorGUILayout.FloatField("Hover Vert Scaling:",
                        _easyColliderPreferences.HoverVertScaling);

                    _easyColliderPreferences.SelectedVertexColour = EditorGUILayout.ColorField("Selected Vertex Colour:",
                        _easyColliderPreferences.SelectedVertexColour);
                    _easyColliderPreferences.SelectedVertScaling = EditorGUILayout.FloatField("Selected Vertex Scaling:",
                        _easyColliderPreferences.SelectedVertScaling);

                    _easyColliderPreferences.OverlapSelectedVertColour =
                        EditorGUILayout.ColorField("Overlap Selected Vert Colour:",
                            _easyColliderPreferences.OverlapSelectedVertColour);
                    _easyColliderPreferences.OverlapSelectedVertScale =
                        EditorGUILayout.FloatField("Overlap Selected Vert Scale:",
                            _easyColliderPreferences.OverlapSelectedVertScale);

                    _easyColliderPreferences.VertSelectKeyCode = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Vert Select KeyCode:", "Shortcut to use for selecting vertices."),
                            _easyColliderPreferences.VertSelectKeyCode);
                    _easyColliderPreferences.SelectedColliderColour = EditorGUILayout.ColorField("Selected Collider Colour:",
                        _easyColliderPreferences.SelectedColliderColour);


                    if (GUILayout.Button("Reset Preferences to Defaults"))
                    {
                        _easyColliderPreferences.SetDefaultValues();
                    }
                }
            }
            if (GUILayout.Button(new GUIContent("Finish Current GameObject",
                "Removes any required components that were added, Restores any disabled components, and resets for new gameobject selection.")))
            {
                OnDisable();
                SelectedGameObject = null;
            }

        }


        EditorGUILayout.LabelField("Quickstart:");
        EditorGUILayout.LabelField("1. Select object with mesh from scene hierarchy.");
        string key = "V"; //default 
        if (_easyColliderPreferences != null)
        {
            key = _easyColliderPreferences.VertSelectKeyCode.ToString();
        }
        EditorGUILayout.LabelField("2. Enable vertex selection with the toggle beside it.");
        EditorGUILayout.LabelField("3. Select vertices by hovering over mesh in scene and pressing " + key + ".");
        EditorGUILayout.LabelField("4. Click buttons that appear after selection to create colliders.");
        EditorGUILayout.LabelField("5. Click Finish Current GameObject button when done creating colliders on an object.");
        EditorGUILayout.LabelField("5. For info on proper vertex selection see documentation .pdf in Assets/EasyColliderEditor", GUILayout.ExpandWidth(true));

        if (GUI.changed)
        {
           SceneView.RepaintAll();
        }
    }

    /// <summary>
    /// removes all colliders on selected gameobject and collider holders, also includes children if include child meshes is enabled.
    /// </summary>
    void RemoveAllCollidersOnSelectedGameObject(bool includeChildren)
    {
        if (SelectedGameObject!=null)
        {
            if (includeChildren)
            {
                RemoveCollidersOnTransformAndAllChildren(SelectedGameObject.transform);
            }

            else
            {
                foreach (MeshTransform mt in MeshTransformList)
                {
                    Collider[] collider = mt.transform.GetComponents<Collider>();
                    if (collider != null)
                    {
                        for (int i = 0; i < collider.Length; i++)
                        {
                            if (!(collider[i] is MeshCollider))
                            {
                                Undo.DestroyObjectImmediate(collider[i]);
                            }
                        }
                    }
                    int childrenCount = mt.transform.childCount;
                    for (int k = 0; k < childrenCount; k++)
                    {
                        Transform t = mt.transform.GetChild(k);
                        if (t.name == "Collider Holder" || t.name == "RotatedCollider")
                        {
                            Collider[] cols = t.GetComponents<Collider>();
                            if (cols!=null)
                            {
                                for (int p = 0; p < cols.Length; p++)
                                {
                                    Undo.DestroyObjectImmediate(cols[p]);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Removes all colliders on the transform, and all of it's children, and all of their children, etc.
    /// </summary>
    /// <param name="t"></param>
    void RemoveCollidersOnTransformAndAllChildren(Transform t)
    {
        Collider[] colliders = t.GetComponents<Collider>();
        if (colliders!=null)
        {
            for (int j = 0; j<colliders.Length;j++)
            {
                if (!(colliders[j]  is MeshCollider))
                {
                    Undo.DestroyObjectImmediate(colliders[j]);
                }
            }
        }
        int children = t.childCount;
        for (int i = 0; i < children; i++)
        {
            RemoveCollidersOnTransformAndAllChildren(t.GetChild(i));
        }
    }


    /// <summary>
    /// Sets the transform the created collider should be attached to, or creates a new collider holder if none found.
    /// </summary>
    void CreateOrSetObjectToAttachColliderTo()
    {
        if (AttachToBaseObject)
        {
            TransformToAttachCollidersTo = SelectedGameObject.transform;
        }
        else if (TransformToAttachCollidersTo == null)
        {
            TransformToAttachCollidersTo = SelectedGameObject.transform.Find("Collider Holder");
            if (TransformToAttachCollidersTo==null)
            {
                TransformToAttachCollidersTo = new GameObject("Collider Holder").transform;
                Undo.RegisterCreatedObjectUndo(TransformToAttachCollidersTo.gameObject, "Created Collider Holder"); //Registers created holder for undo.
                TransformToAttachCollidersTo.parent = SelectedGameObject.transform;
                TransformToAttachCollidersTo.position = SelectedGameObject.transform.position;
                TransformToAttachCollidersTo.rotation = SelectedGameObject.transform.rotation;
                TransformToAttachCollidersTo.localScale = SelectedGameObject.transform.localScale;
            }
        }
    }

    /// <summary>
    /// Removes any added components by EasyColliderEditor, and restores any disabled components (colliders & rigidbody settings are modified)
    /// </summary>
    void RestoreComponents()
    {
        if (AddedComponents != null)
        {
            foreach (Component component in AddedComponents)
            {
                if (component != null)
                {
                    Undo.DestroyObjectImmediate(component);
                }
            }
            AddedComponents.Clear();
        }
        if (DisabledComponents != null)
        {
            foreach (Component component in DisabledComponents)
            {
                if (component != null)
                {
                    if (component is Rigidbody)
                    {
                        Rigidbody rb = component as Rigidbody;
                        rb.isKinematic = false; //only rb's whos setting was false originally were added to disabled components.
                    }
                    else if (component is Collider)
                    {
                        Collider col = component as Collider;
                        col.enabled = true;
                    }
                }
            }
            DisabledComponents.Clear();
        }
    }



    /// <summary>
    /// Restores previously removed components, and removes added components,
    /// then creates a new mesh transform list.
    /// then ensures required components are added.
    /// </summary>
    void GameObjectSelectedHasChanged()
    {
        RestoreComponents();
        CreateNewMeshTransformList();
        foreach (MeshTransform mt in MeshTransformList)
        {
            EnsureRequiredComponents(mt);
        }
    }



    /// <summary>
    /// Clears the current list of mesh transforms and creates a new one
    /// that includes the selected gameobject and it's children if include meshes from children is selected.
    /// </summary>
    private void CreateNewMeshTransformList()
    {
        if (MeshTransformList != null)
        {
            MeshTransformList.Clear();
        }
        var meshListBuilder = new MeshListBuilder();
        MeshTransformList = meshListBuilder.GetMeshList(SelectedGameObject, IncludeMeshesFromChildren);
        _easyColliderEditor.MTList = MeshTransformList;
        _easyColliderEditor.SelectedGameObject = SelectedGameObject;
    }

   
    /// <summary>
    /// Ensures required components are attached, like mesh colliders.
    /// Also ensures components that cause errors in functionality are disabled like rigidbodies, and other colliders.
    /// </summary>
    /// <param name="mt"></param>
    private void EnsureRequiredComponents(MeshTransform mt)
    {
        if (AddedComponents == null)
        {
            AddedComponents = new List<Component>();
        }
        if (DisabledComponents == null)
        {
            DisabledComponents = new List<Component>();
        }
        Collider[] colliders = mt.transform.GetComponents<Collider>();
        foreach (Collider item in colliders) {
            MeshCollider testCast = item as MeshCollider; //if already has a mesh collider doesn't disable it.
            if (testCast == null)
            {
                Debug.LogWarning("Collider already on " + mt.transform.name + ". Disabling while creating colliders.");
                DisabledComponents.Add(item);
                //item.enabled = false;
            }
        }
        MeshCollider meshCollider = mt.transform.GetComponent<MeshCollider>();
        if (meshCollider == null )
        {
            meshCollider = Undo.AddComponent<MeshCollider>(mt.transform.gameObject);
            if (meshCollider.sharedMesh == null)
            {
                meshCollider.sharedMesh = mt.mesh;
            }
            AddedComponents.Add(meshCollider);
        }
        Rigidbody rb = mt.transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (!rb.isKinematic) //checks to see if rb is kinematic, non-kinematic rigidbodies do not allow for vertex selection.
            {
                Debug.LogWarning("Rigidbody attached to " + mt.transform.name + ". Setting to kinematic temporarily to enable functionality");
                DisabledComponents.Add(rb); //only rigibodies with kinematic not set are added to disabled components.
                rb.isKinematic = true;
            }
        }

    }

    /// <summary>
    /// 检查是否为场景对象
    /// </summary>
    bool GameObjectIsActiveAndFromScene(ref GameObject obj)
    {
        if (obj != null)
        {
            if (!obj.activeInHierarchy)
            {
                EditorUtility.DisplayDialog("Collider编辑器", "请选择正确的编辑对象！编辑的对象必须为场景中实例化的对象！", "确定");
                obj = null;
                return false;
            }
            else
                return true;
        }
        return false;
    }
}
