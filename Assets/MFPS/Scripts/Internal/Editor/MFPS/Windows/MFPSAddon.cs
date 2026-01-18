using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPSEditor.Addons
{
    [CreateAssetMenu(fileName = "MFPS Addon", menuName = "MFPS/Addon Info", order = 300)]
    public class MFPSAddon : ScriptableObject
    {
        public string Name;
        public string Version;
        public string MinMFPSVersion = "1.6";

        [TextArea(4, 10)]
        public string Instructions;
        public string TutorialScript = "";
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MFPSAddon))]
    public class MFPSAddonsEditor : Editor
    {
        MFPSAddon script;
        private GUIStyle TextStyle = null;
        private GUIStyle TextStyleFlat = null;
        private GUIStyle m_LinkStyle;
        private GUIStyle m_btnStyle;
        private GUIStyle m_btn2Style;
        private bool editMode = false;
        public TutorialWizardText contentText;
        const float k_Space = 16f;
        private bool m_Initialized;
        private GUISkin m_Skin;

        private void OnEnable()
        {
            script = (MFPSAddon)target;
            contentText = new TutorialWizardText();

            if (MFPSAddonsData.Instance != null)
            {
                int i = MFPSAddonsData.Instance.Addons.FindIndex(x => x.NiceName == script.Name);
                if (i >= 0 && MFPSAddonsData.Instance.Addons[i].Info == null)
                {
                    MFPSAddonsData.Instance.Addons[i].Info = script;
                    EditorUtility.SetDirty(MFPSAddonsData.Instance);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    LovattoStats.SetStat($"aa-{script.Name}", 1);
                }
            }
        }

        protected override void OnHeaderGUI()
        {
            InitGUI();

            var hr = EditorGUILayout.BeginHorizontal("In BigTitle");
            {
                if (!EditorGUIUtility.isProSkin)
                {
                    TutorialWizard.Style.DrawGlowRect(hr, MFPSEditorStyles.LovattoEditorPalette.GetMainColor(false), Color.white);
                }
                hr.width = 2;
                EditorGUI.DrawRect(hr, MFPSEditorStyles.LovattoEditorPalette.GetHighlightColor(true));
                GUILayout.Space(k_Space);
                GUILayout.BeginVertical();
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField($"<size=30>{script.Name.ToUpper()}</size>", TextStyle);
                    EditorGUILayout.LabelField(string.Format("<size=14><b>{0}</b></size>", script.Version), TextStyleFlat);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndVertical();
                GUILayout.Space(k_Space);
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                {
                    GUILayout.FlexibleSpace();
                    if (!string.IsNullOrEmpty(script.TutorialScript))
                    {
                        GUILayout.Space(5);
                        if (GUILayout.Button(new GUIContent("Documentation"), m_btnStyle, GUILayout.Width(140)))
                        {
                            EditorWindow.GetWindow(System.Type.GetType(string.Format("{0}, Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", script.TutorialScript)));
                        }
                        GUILayout.Space(5);
                        if (GUILayout.Button(new GUIContent("Addons Manager"), m_btn2Style, GUILayout.Width(140)))
                        {
                            EditorWindow.GetWindow<MFPSAddonsWindow>().OpenAddonPage(script.Name);
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndVertical();
                GUILayout.Space(k_Space);

            }
            EditorGUILayout.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
            Rect rect;
            Rect r = EditorGUILayout.BeginVertical();
            {
                var ap = new Rect(0, 0, EditorGUIUtility.currentViewWidth, r.height + 5);
                TutorialWizard.Style.DrawGlowRect(ap, MFPSEditorStyles.LovattoEditorPalette.GetMainColor(true), Color.white);
                rect = r;
                if (!editMode && !string.IsNullOrEmpty(script.Name))
                {
                    GUILayout.Space(10);
                    contentText ??= new TutorialWizardText();
                    if (script == null) script = (MFPSAddon)target;

                    if (script != null && !string.IsNullOrEmpty(script.Instructions) && contentText != null)
                        contentText.DrawText(script.Instructions, TextStyleFlat);
                }
                else
                {
                    EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
                    GUILayout.FlexibleSpace();
                    GUILayout.FlexibleSpace();

                    EditorGUILayout.EndHorizontal();
                    DrawDefaultInspector();
                }
                GUILayout.Space(5);
            }
            EditorGUILayout.EndVertical();

            rect.x += rect.width - 15;
            rect.y += 5;
            rect.width = 10; rect.height = 25;

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) editMode = !editMode;
        }

        void InitGUI()
        {
            if (m_Initialized)
                return;

            m_Skin = Resources.Load<GUISkin>("content/MFPSEditorSkin");
            TextStyle = m_Skin.customStyles[3];
            m_btnStyle = m_Skin.customStyles[11];
            m_btn2Style = m_Skin.customStyles[12];

            TextStyleFlat = new(EditorStyles.label)
            {
                richText = true,
                wordWrap = true,
                fontSize = 12,
            };
            TextStyleFlat.normal.textColor = TextStyleFlat.hover.textColor = new Color(0.7f, 0.7f, 0.7f, 1f);

            m_LinkStyle = new(TextStyleFlat);

            m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
            m_LinkStyle.stretchWidth = false;

            m_Initialized = true;
        }

        bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
        {
            var position = GUILayoutUtility.GetRect(label, m_LinkStyle, options);

            Handles.BeginGUI();
            Handles.color = m_LinkStyle.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();

            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

            return GUI.Button(position, label, m_LinkStyle);
        }
    }

    public class MFPSAddonPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets,
                                           string[] deletedAssets,
                                           string[] movedAssets,
                                           string[] movedFromAssetPaths)
        {
            if (importedAssets != null && importedAssets.Length > 1)
                foreach (string assetPath in importedAssets)
                {
                    // Check if the asset is of type MFPSAddon
                    if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(MFPSAddon))
                    {
                        Object addonAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                        Selection.activeObject = addonAsset;

                        break;
                    }
                }
        }
    }
#endif
}