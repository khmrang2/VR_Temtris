namespace ColliderVisualizer.Editor
{
    using UnityEditor;
    using UnityEngine;
    
    /// <summary>
    /// Rendering the <see cref="ColliderVisualizer"/> component in the Inspector.
    /// </summary>
    [CustomEditor(typeof(ColliderVisualizer))]
    public class ColliderVisualizerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Reinitialize"))
            {
                ((ColliderVisualizer)target).Initialize();
            }
        }
    }
}