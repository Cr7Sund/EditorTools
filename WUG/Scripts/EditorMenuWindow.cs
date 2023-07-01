namespace Cr7Sund
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.UIElements;

    using UnityEngine;
    using UnityEngine.UIElements;

    public class EditorMenuWindow : EditorWindow
    {
        #region Fields
        #region Data
        private readonly List<BaseMenuItem> m_ItemDatabase = new();
        private BaseMenuItem m_activeItem;
        private BaseMenuItem m_rootItem;

        #endregion

        private VisualTreeAsset m_ItemRowTemplate;
        #region  visualViews
        private ScrollView m_DetailSection;
        private TreeView m_ItemTreeView;
        private VisualElement m_LargeDisplayIcon;

        protected List<TreeViewItemData<BaseMenuItem>> m_treeRoots;

        private Sprite m_DefaultItemIcon;

        #endregion

        #region 
        // The default rotate and scale values for the new Label.
        Rotate defaultRotate;
        Scale defaultScale;
        #endregion

        #region Define
        private const int ITEMHEIGHT = 16;

        private const string ROOTVIRTUALITEM = "VirtualRootItem";

        #endregion

        #endregion

        #region Init

        [MenuItem("Cr7Sund/BaseMenuItem Database #x")]
        public static void Init()
        {
            EditorMenuWindow wnd = GetWindow<EditorMenuWindow>();
            wnd.titleContent = new GUIContent("Cr7Sund");

            Vector2 size = new Vector2(1000, 475);
            wnd.minSize = size;
            wnd.maxSize = size;
        }

        /// <summary>
        /// Look through all items located in Assets/Data and load them into memory
        /// </summary>
        private void LoadAllItems()
        {
            int id = 0;
            m_ItemDatabase.Clear();

            string assetPath = $"{EditorPath.DATAPATH}{ROOTVIRTUALITEM}.asset";
            m_rootItem = AssetDatabase.LoadAssetAtPath<BaseMenuItem>(assetPath);
            if (m_rootItem == null)
            {
                m_rootItem = ScriptableObjectHelper.CreateAsset<BaseMenuItem>(assetPath);
            }

            var childMenuSet = new HashSet<BaseMenuItem>();
            PreorderMenuTreeRecursive(childMenuSet, m_rootItem, ref id);
        }

        private void PreorderMenuTreeRecursive(HashSet<BaseMenuItem> childMenuSet, BaseMenuItem item, ref int id)
        {
            m_ItemDatabase.Add(item);


            // Auto fix
            for (int i = item.childMenuItems.Count - 1; i >= 0; i--)
            {
                if (item.childMenuItems[i] == null)
                {
                    Log.Error($"{item.name} contain empty child: index of {i}", item);
                    // item.childMenuItems.RemoveAt(i);
                }
            }


            for (int i = 0; i < item.childMenuItems.Count; i++)
            {
                BaseMenuItem childMenuItem = item.childMenuItems[i];

                if (!childMenuSet.Contains(childMenuItem))
                {
                    childMenuSet.Add(childMenuItem);
                }
                else
                {
                    Log.Error($"{item.name} contain cyclic child {childMenuItem.name}", item);
                    continue;
                }

                PreorderMenuTreeRecursive(childMenuSet, childMenuItem, ref id);
            }
        }

        private void InitTreeViewData()
        {
            int id = 0;

            m_treeRoots = PreorderTreeViewDataRecursive(m_rootItem, ref id);
        }

        private List<TreeViewItemData<BaseMenuItem>> PreorderTreeViewDataRecursive(BaseMenuItem menuItem, ref int id)
        {
            List<BaseMenuItem> childMenuItems = menuItem.childMenuItems;

            var treeViewSubItemsData = new List<TreeViewItemData<BaseMenuItem>>(childMenuItems.Count);
            foreach (var childMenuItem in childMenuItems)
            {
                int itemIndex = id++;
                var viewDataList = PreorderTreeViewDataRecursive(childMenuItem, ref id);

                TreeViewItemData<BaseMenuItem>
                       treeViewItemData = new TreeViewItemData<BaseMenuItem>(itemIndex, childMenuItem, viewDataList);

                treeViewSubItemsData.Add(treeViewItemData);
            }
            return treeViewSubItemsData;
        }

        #endregion

        #region GUI
        public void CreateGUI()
        {
            // Import the UXML Window
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{EditorPath.UXMLPATH}EditorMenuWindow.uxml");
            VisualElement rootFromUXML = visualTree.Instantiate();
            rootVisualElement.Add(rootFromUXML);

            // Import the stylesheet
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{EditorPath.CSSPATH}EditorMenuWindow.uss");
            rootVisualElement.styleSheets.Add(styleSheet);

            //Import the ListView BaseMenuItem Template
            m_ItemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{EditorPath.UXMLPATH}MenuRowTemplate.uxml");
            m_DefaultItemIcon = (Sprite)AssetDatabase.LoadAssetAtPath($"{EditorPath.SPRITEPATH}baseline_bug_report_black_24pt_1x.png", typeof(Sprite));

            ShowTreeView();
        }

        private void ShowTreeView()
        {
            //Load all existing item assets 

            LoadAllItems();
            InitTreeViewData();
            m_activeItem = m_rootItem;

            //Populate the treeview
            GenerateTreeView();

            RegisterHandler();
        }

        private void RefreshTreeView()
        {
            //Load all existing item assets 
            InitTreeViewData();
            m_activeItem = m_rootItem;

            //Populate the treeview
            GenerateTreeView();

            m_ItemTreeView.Rebuild();
        }

        /// <summary>
        /// Create the list view based on the asset data
        /// </summary>
        private void GenerateTreeView()
        {
            //Defining what each item will visually look like. In this case, the makeItem function is creating a clone of the ItemRowTemplate.
            Func<VisualElement> makeItem = () => m_ItemRowTemplate.CloneTree();
            // Func<VisualElement> makeItem = () => new Label("west");

            //Define the binding of each individual BaseMenuItem that is created. Specifically, 
            //it binds the Icon visual element to the scriptable object�s Icon property and the 
            //Name label to the FriendlyName property.
            Action<VisualElement, int> bindItem = (e, i) =>
            {
                VisualElement iconElement = e.Q<VisualElement>("Icon");

                var baseMenuItem = m_ItemTreeView.GetItemDataForIndex<BaseMenuItem>(i);
                iconElement.style.backgroundImage = (baseMenuItem == null || baseMenuItem.icon == null) ? m_DefaultItemIcon.texture : baseMenuItem.icon.texture;
                e.Q<Label>("Name").text = baseMenuItem.menuName;
            };

            //Create the listview and set various properties
            m_ItemTreeView = rootVisualElement.Q<TreeView>("ItemTreView");
            m_ItemTreeView.SetRootItems(m_treeRoots);
            m_ItemTreeView.selectionType = SelectionType.Single;
            m_ItemTreeView.style.height = m_ItemDatabase.Count * ITEMHEIGHT * 10 + 5;

            m_ItemTreeView.bindItem = bindItem;
            m_ItemTreeView.makeItem = makeItem;
            m_ItemTreeView.Rebuild();

            // Callback invoked when the user double clicks an item
            //m_ItemListView.itemsChosen += ListView_onSelectionChange;
            m_ItemTreeView.selectionChanged += ListView_onSelectionChange;

            if (m_treeRoots.Count > 0)
                m_ItemTreeView.SetSelection(0);
        }

        private void GenerateDetailSection(VisualTreeAsset m_BaseMenuItemTemplate)
        {
            //Get references for later
            var baseMenuItemTemplate = m_BaseMenuItemTemplate.CloneTree();

            m_DetailSection = rootVisualElement.Q<ScrollView>("ScrollView_Details");
            //Make sure the detail section is visible. This can turn off when you delete an item
            m_DetailSection.style.visibility = Visibility.Visible;

            m_DetailSection.Clear();
            m_DetailSection.Add(baseMenuItemTemplate);

            m_LargeDisplayIcon = m_DetailSection.Q<VisualElement>("Icon");

            //Register Value Changed Callbacks for new items added to the ListView
            m_DetailSection.Q<TextField>("MenuName").RegisterValueChangedCallback(evt =>
            {
                m_activeItem.menuName = evt.newValue;
                m_ItemTreeView.Rebuild();
            });
            m_DetailSection.Q<TextField>("MenuName").RegisterCallback<PointerLeaveEvent>(evt =>
            {
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(m_activeItem), m_activeItem.menuName);
            });


            m_DetailSection.Q<ObjectField>("IconPicker").RegisterValueChangedCallback(evt =>
            {
                Sprite newSprite = evt.newValue as Sprite;
                m_activeItem.icon = newSprite == null ? m_DefaultItemIcon : newSprite;
                m_LargeDisplayIcon.style.backgroundImage = newSprite == null ? m_DefaultItemIcon.texture : newSprite.texture;

                m_ItemTreeView.Rebuild();
            });
        }

        private void GenPublicFields(BaseMenuItem baseMenuItem)
        {
            var fieldContainer = m_DetailSection.Q<VisualElement>("FieldContainer");

            var type = baseMenuItem.GetType();
            var so = new SerializedObject(baseMenuItem);
            var iterator = so.GetIterator();

            //1 .Firstly support the uxml sytle
            m_DetailSection.Bind(so);

            // 2. Automatically generating the other bindfields
            iterator.NextVisible(true);
            // skip m_Script
            iterator.NextVisible(false);

            do
            {

                string fieldName = iterator.name;
                if (fieldContainer.Q<VisualElement>(fieldName) != null) continue;

                var fieldInfo = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                // if (fieldInfo == null) continue;

                var attributeDict = new Dictionary<Type, Attribute>();
                foreach (var attribute in fieldInfo.GetCustomAttributes())
                {
                    attributeDict.Add(attribute.GetType(), attribute);
                }

                if (attributeDict.ContainsKey(typeof(HideInEditorAttribute))) continue;

                var bindableElement = VisualElementHelper.GetMatchBindableElement(iterator, attributeDict);
                if (bindableElement == null) continue;
                bindableElement.BindProperty(iterator);

                fieldContainer.Add(bindableElement);
            } while (iterator.NextVisible(false));
        }

        private void GenBtnFields(BaseMenuItem baseMenuItem)
        {
            var btnContainer = m_DetailSection.Q<VisualElement>("BtnContainer");

            var type = baseMenuItem.GetType();
            var btnAttributes = type.GetCustomAttributes<ButtonAttribute>();
            var methodInfos = type.GetMethods();
            foreach (var methodInfo in methodInfos)
            {
                string btnName = string.Empty;
                var btnAttribute = methodInfo.GetCustomAttribute<ButtonAttribute>();
                if (btnAttribute != null)
                {
                    btnName = btnAttribute.labelName;

                    if (string.IsNullOrEmpty(btnName))
                    {
                        btnName = methodInfo.Name;
                    }

                    var btnElement = new Button(() => { methodInfo.Invoke(baseMenuItem, null); });
                    btnElement.name = btnName;
                    btnElement.text = btnName;

                    btnContainer.Add(btnElement);
                }
            }
        }

        #endregion

        #region UIEvents
        private void RegisterHandler()
        {
            rootVisualElement.Q<Button>("Btn_AddItem").RegisterCallback<ClickEvent>(OnAddItemClick);
            rootVisualElement.RegisterCallback<KeyUpEvent>(OnKeyUp, TrickleDown.TrickleDown);
        }

        private void OnKeyUp(KeyUpEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Delete:
                    OnDelItemClick(evt);
                    break;
                default: break;
            }
        }

        private void OnDelItemClick(EventBase evt)
        {
            BaseMenuItem deleteItem = null;
            BaseMenuItem parentItem = null;

            var queue = new Queue<BaseMenuItem>();
            queue.Enqueue(m_rootItem);
            while (queue.Count > 0)
            {
                var topItem = queue.Dequeue();
                foreach (var childItem in topItem.childMenuItems)
                {
                    if (childItem.Equals(m_activeItem))
                    {
                        deleteItem = childItem;
                        parentItem = topItem;
                        break;
                    }
                    queue.Enqueue(childItem);
                }

                if (deleteItem != null)
                {
                    break;
                }
            }

            if (deleteItem == null) return;

            parentItem.childMenuItems.Remove(deleteItem);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(deleteItem));

            RefreshTreeView();
        }

        private void OnAddItemClick(ClickEvent evt)
        {
            string assetName = GUID.Generate().ToString();
            string assetPath = $"{EditorPath.DATAPATH}{assetName}.asset";
            var newItem = ScriptableObjectHelper.CreateAsset<BaseMenuItem>(assetPath);
            newItem.menuName = assetName;
            m_activeItem.childMenuItems.Add(newItem);

            // EditorUtility.SetDirty(m_activeItem);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            RefreshTreeView();
        }

        private void ListView_onSelectionChange(IEnumerable<object> selectedItems)
        {
            //Get the first item in the selectedItems list. 
            //There will only ever be one because SelectionType is set to Single
            m_activeItem = (BaseMenuItem)selectedItems.First();

            string typeName = m_activeItem.GetType().ToString();
            typeName = typeName.Substring(typeName.LastIndexOf('.') + 1);
            if (m_activeItem.uxmlFile == null)
            {
                string assetPath = $"{EditorPath.UXMLPATH}{typeName}.uxml";
                m_activeItem.uxmlFile = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
            }

            //Create a new SerializedObject and bind the Details VE to it. 
            //This cascades the binding to the children
            GenerateDetailSection(m_activeItem.uxmlFile);

            //Set the icon if it exists
            if (m_activeItem.icon != null)
            {
                m_LargeDisplayIcon.style.backgroundImage = m_activeItem.icon.texture;
            }

            GenBtnFields(m_activeItem);
            GenPublicFields(m_activeItem);

            Button deleteBtn = rootVisualElement.Q<Button>("Btn_DeleteItem");
            // Create transition on the new Label.
            // deleteBtn.style.transitionDuration = new List<TimeValue> { new TimeValue(3) };

            // Record default rotate and scale values.
            defaultRotate = deleteBtn.resolvedStyle.rotate;
            defaultScale = deleteBtn.resolvedStyle.scale;

            deleteBtn.RegisterCallback<ClickEvent>(OnDelItemClick);
            deleteBtn.RegisterCallback<PointerOverEvent>(OnDelPointerOver);
            deleteBtn.RegisterCallback<PointerOutEvent>(OnDelPointerOut);
        }


        #endregion

        #region  Transition

        private void OnDelPointerOut(PointerOutEvent evt)
        {
            SetHover(evt.currentTarget as VisualElement, false);
        }

        private void OnDelPointerOver(PointerOverEvent evt)
        {
            SetHover(evt.currentTarget as VisualElement, true);
        }


        // When the user enters or exits the Label, set the rotate and scale.
        void SetHover(VisualElement label, bool hover)
        {
            label.style.rotate = hover ? new(Angle.Degrees(10)) : defaultRotate;
            label.style.scale = hover ? new Vector2(1.1f, 1) : defaultScale;
        }

        #endregion
    }

}
