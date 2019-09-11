using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MPX.Tools
{
    public class DebugGizmosWindow : EditorWindow
    {
        private GameObject _gizmoDrawer = null;
        private SearchField _searchField = null;
        private string _searchString = "";
        Vector2 _scrollPos = Vector2.zero;
        HashSet<string> _categories = new HashSet<string>();
        
        class CategorySetupNode
        {
            private static string[] _modes = new string[] {DebugGizmos.DrawMode.Disabled.ToString(), DebugGizmos.DrawMode.Selected.ToString(), DebugGizmos.DrawMode.Allways.ToString()};
            private bool _foldout = false;

            public bool DrawMode = true;
            public string Path = null;
            public string Name = "";
            public List<CategorySetupNode> Childs = null;
            public bool Selected = false;
            
            private DebugGizmos.DrawMode _mode = DebugGizmos.DrawMode.Allways;
            public DebugGizmos.DrawMode Mode
            {
                get => _mode;
                set
                {
                    if (_mode != value)
                    {
                        if (!string.IsNullOrEmpty(Path))
                        {
                            DebugGizmos.SetCategoryMode(Path, value);
                        }
                    }

                    _mode = value;
                }
            }

            public void SetModeRecursive(DebugGizmos.DrawMode mode)
            {
                Mode = mode;
                Childs?.ForEach(c => c.SetModeRecursive(mode));
            }
            
            public void SetModeRecursiveSelected(DebugGizmos.DrawMode mode)
            {
                if (Selected)
                    Mode = mode;
                Childs?.ForEach(c => c.SetModeRecursiveSelected(mode));
            }

            public bool SelectRecursive(string[] searchWords)
            {
                Selected = false;

                if (Childs == null)
                {
                    if (searchWords != null && searchWords.Length > 0)
                    {
                        Selected = true;
                        
                        for (var i = 0; i < searchWords.Length; i++)
                        {
                            if (Path == null || Path.IndexOf(searchWords[i], StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                Selected = false;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var child in Childs)
                    {
                        Selected |= child.SelectRecursive(searchWords);   
                    }
                }

                return Selected;
            }

            public void AddChilds(string[] path, int idx)
            {
                if (idx >= path.Length) return;

                if (Childs == null) Childs = new List<CategorySetupNode>();

                var child = Childs.Find(n => n.Name == path[idx]);
                if (child == null)
                {
                    child = new CategorySetupNode()
                    {
                        Name = path[idx],
                        Path = string.Join("/", path, 0, idx + 1)
                    };

                    Childs.Add(child);
                }

                child.AddChilds(path, idx + 1);
            }

            public void Draw(bool selected)
            {
                if (selected && !Selected) return;
                
                EditorGUILayout.BeginHorizontal();

                if (Childs != null)
                    _foldout = EditorGUILayout.Foldout(_foldout, Name);
                else
                    GUILayout.Label(Name);
                
                if (DrawMode)
                    Mode = (DebugGizmos.DrawMode) GUILayout.Toolbar((int)Mode, _modes);

                if (Childs != null)
                {
                    GUILayout.Space(10);
                    
                    if (GUILayout.Button(new GUIContent("+", "Enable all child"), GUILayout.MaxWidth(40)))
                    {
                        _foldout = true;
                        foreach (var child in Childs)
                        {
                            if (selected && !child.Selected) continue;
                            child.Mode = DebugGizmos.DrawMode.Allways;
                        }
                    }
                    
                    if (GUILayout.Button(new GUIContent("X", "Disable all child"), GUILayout.MaxWidth(40)))
                    {
                        _foldout = true;
                        foreach (var child in Childs)
                        {
                            if (selected && !child.Selected) continue;
                            child.Mode = DebugGizmos.DrawMode.Disabled;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (_foldout  || (selected && Selected))
                {
                    if (Childs != null)
                    {
                        GUILayout.Space(5);
                        
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(16);

                        EditorGUILayout.BeginVertical();
                        {
                            foreach (var node in Childs)
                            {
                                node.Draw(selected);
                            }
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        private CategorySetupNode _categoriesRoot = new CategorySetupNode {Name = "All Categories", DrawMode = false};


        [MenuItem("Window/Analysis/Debug Gizmos")]
        private static void Open()
        {
            GetWindow<DebugGizmosWindow>("Debug Gizmos", true);
        }

        private void OnGUI()
        {
            if (_searchField == null)
            {
                _searchField = new SearchField();
            }
            
            GUILayout.Space(10);
            var search = _searchField.OnGUI(_searchString);
            if (!EditorApplication.isPlaying) _searchString = "";
            
            if (EditorApplication.isPlaying && search != _searchString)
            {
                _searchString = search;
                var searchWords = _searchString.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                _categoriesRoot.SelectRecursive(searchWords);
                _categoriesRoot.Selected = true;
            }

            if (GUILayout.Button("Enable all"))
            {
                _categoriesRoot.SetModeRecursive(DebugGizmos.DrawMode.Allways);
            }
            
            if (GUILayout.Button("Disable all"))
            {
                _categoriesRoot.SetModeRecursive(DebugGizmos.DrawMode.Disabled);
            }

            if (!string.IsNullOrEmpty(_searchString))
            {
                if (GUILayout.Button("Only selected"))
                {
                    _categoriesRoot.SetModeRecursive(DebugGizmos.DrawMode.Disabled);
                    _categoriesRoot.SetModeRecursiveSelected(DebugGizmos.DrawMode.Allways);
                }
            }

            GUILayout.Space(10);
            bool enabled = GUILayout.Toggle(DebugGizmos.Enabled, "Enabled") && EditorApplication.isPlaying;

            if (DebugGizmos.Enabled != enabled)
            {
                DebugGizmos.Enabled = enabled;
                OnSelectionChange();

                if (enabled)
                {
                    CreateGizmoDrawer();
                    UpdateCategories();
                }
                else
                {
                    DestroyImmediate(_gizmoDrawer);
                    _categoriesRoot.Childs = null;
                }
            }

            if (DebugGizmos.Enabled)
            {
                UpdateCategories();
            }

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            _categoriesRoot.Draw(!string.IsNullOrEmpty(_searchString));
            GUILayout.EndScrollView();
        }

        void CreateGizmoDrawer()
        {
            _gizmoDrawer = new GameObject("<GizmoDrawer>");
            _gizmoDrawer.hideFlags = HideFlags.DontSave;
            _gizmoDrawer.AddComponent<GizmosDrawer>();
        }

        private void Awake()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            if (mode == PlayModeStateChange.EnteredEditMode)
            {
                DebugGizmos.Enabled = false;
                DebugGizmos.SelectedIds.Clear();
                _categories.Clear();
                _searchString = "";
            }
        }

        
        private void OnSelectionChange()
        {
            if (!DebugGizmos.Enabled) return;
            
            DebugGizmos.SelectedIds.Clear();
            
            Array.ForEach(Selection.objects,
                o =>
                {
                    var ids = (o as GameObject)?.GetComponentsInChildren<GizmoSelect>();
                    if (ids != null)
                    {
                        foreach (var gizmoSelect in ids)
                        {
                            DebugGizmos.SelectedIds.Add(gizmoSelect.SelectionId);
                        }
                    }
                });
        }


        void UpdateCategories()
        {
            if (!EditorApplication.isPlaying) return;
            
            foreach (var key in DebugGizmos.CategoriesSetup.Keys)
            {
                if (_categories.Contains(key)) continue;

                _categories.Add(key);
                
                var subCategories = key.Split('/');
                _categoriesRoot.AddChilds(subCategories, 0);

                if (string.IsNullOrEmpty(_searchString))
                {
                    _categoriesRoot.SelectRecursive(_searchString.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries));
                }
            }
        }
    }
}