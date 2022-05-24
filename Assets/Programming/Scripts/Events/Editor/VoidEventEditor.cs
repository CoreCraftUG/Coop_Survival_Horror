using UnityEditor;
using UnityEngine;

namespace CoreCraft.Programming.Events.Editor
{
    [CustomEditor(typeof(BaseGameEvent<Void>), editorForChildClasses: true)]
    public class VoidEventEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUI.enabled = Application.isPlaying;

            BaseGameEvent<Void> e = target as BaseGameEvent<Void>;
            if (GUILayout.Button("Raise Event"))
                e.RaiseEvent(new Void());
        }
    }
}