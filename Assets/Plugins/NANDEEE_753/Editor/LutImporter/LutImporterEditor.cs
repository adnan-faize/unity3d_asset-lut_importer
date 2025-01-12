using UnityEditor;
using UnityEditor.AssetImporters;

namespace NANDEEE_753.Editor.LutImporter
{
    [CustomEditor(typeof(LutImporter))]
    public class LutImporterEditor : ScriptedImporterEditor
    {
        private SerializedProperty _use3DProperty;
        private SerializedProperty _sizeProperty;

        public override void OnEnable()
        {
            base.OnEnable();
            
            _use3DProperty = serializedObject.FindProperty("use3D");
            _sizeProperty = serializedObject.FindProperty("size");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _use3DProperty.boolValue = EditorGUILayout.Toggle("Use 3D Texture", _use3DProperty.boolValue);
            _sizeProperty.intValue = EditorGUILayout.IntSlider("LUT texture size", _sizeProperty.intValue, 16, 64);
            serializedObject.ApplyModifiedProperties();
            
            EditorGUILayout.Space();
            
            ApplyRevertGUI();
        }
    }
}