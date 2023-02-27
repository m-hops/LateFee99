using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Nie
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    public static class EditorMenu
    {

        private const string DrawStatesGizmosName = "Tools/NiEngine/Draw States/Gizmos";
        private const string DrawStatesLabelName = "Tools/NiEngine/Draw States/Label";
        public static bool DrawStatesGizmos;
        public static bool DrawStatesLabel;

        static EditorMenu()
        {
            DrawStatesGizmos = UnityEditor.EditorPrefs.GetBool(DrawStatesGizmosName, true);
            DrawStatesLabel = UnityEditor.EditorPrefs.GetBool(DrawStatesLabelName, false);
            UnityEditor.EditorApplication.delayCall += () => SetDrawStates(DrawStatesGizmos, DrawStatesLabel);
        }


        [UnityEditor.MenuItem("Tools/NiEngine/Draw States/Off")]
        private static void SetOff() => SetDrawStates(false, false);

        [UnityEditor.MenuItem(DrawStatesGizmosName)]
        private static void SetGizmo() => SetDrawStates(!DrawStatesGizmos, DrawStatesLabel);

        [UnityEditor.MenuItem(DrawStatesLabelName)]
        private static void SetLabel() => SetDrawStates(DrawStatesGizmos, !DrawStatesLabel);
        public static void SetDrawStates(bool gizmos, bool label)
        {
            DrawStatesGizmos = gizmos;
            DrawStatesLabel = label;
            UnityEditor.Menu.SetChecked(DrawStatesGizmosName, DrawStatesGizmos);
            UnityEditor.Menu.SetChecked(DrawStatesLabelName, DrawStatesLabel);
            UnityEditor.EditorPrefs.SetBool(DrawStatesGizmosName, DrawStatesGizmos);
            UnityEditor.EditorPrefs.SetBool(DrawStatesLabelName, DrawStatesLabel);


        }

        public static GameObject DebugLabelAsset = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Packages/NiEngine/src/Editor/Assets/label.prefab", typeof(GameObject));
    }

#endif
}