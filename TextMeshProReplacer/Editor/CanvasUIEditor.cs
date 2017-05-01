using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TextMeshProReplacer
{ 
//note: since it's impossible to extends text editor now, use canvas renderer instead
//it was textmesh pro replacer but i extended it to store text properties
    [CustomEditor(typeof(CanvasRenderer))]
    class CanvasUIEditor :Editor
    {
        private CanvasRenderer canvasRenderer;
        private void OnEnable()
        {
            canvasRenderer = (CanvasRenderer)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            if(canvasRenderer.GetComponent<Text>()!=null &&
               GUILayout.Button("Replace with TextMeshPro"))
            {
                TextReplacer.ReplaceUnityText(canvasRenderer.GetComponent<Text>());
                GUIUtility.ExitGUI();


            }
            GUILayout.EndHorizontal();
        }
    }
    
}