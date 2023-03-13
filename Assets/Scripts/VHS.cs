using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VHS : MonoBehaviour
{
    public string Title;
    public string Genre;
    public float Duration;
    public float HeadTime;
    public enum QualityEnum
    {
        Good,
        Bad,
    }
    public QualityEnum Quality;

    public Transform DebugTitle;
    public Transform DebugTime;

    public string CurrentVHSTimeString
        => new System.TimeSpan(0, (int)(HeadTime % 86400) / 3600, (int)(HeadTime % 3600) / 60, (int)HeadTime % 60, 0).ToString("c");

//#if UNITY_EDITOR
//    TextMesh DebugLabelTitle = null;
//    TextMesh DebugLabelTime = null;
    
//    void CheckDebugLabel(Transform pos, ref TextMesh label, System.Func<string> text)
//    {

//        if (Nie.EditorMenu.DrawStatesLabel && pos != null && label == null)
//        {
//            var obj = GameObject.Instantiate(Nie.EditorMenu.DebugLabelAsset, pos);
//            label = obj.GetComponent<TextMesh>();
//            obj.hideFlags = HideFlags.HideAndDontSave;
//            label.transform.localPosition = Vector3.zero;
//            label.transform.localRotation = Quaternion.identity;
//            var parentScale = pos.lossyScale;
//            var scale = Nie.EditorMenu.DebugLabelAsset.transform.localScale;
//            label.transform.localScale = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, scale.z / parentScale.z);
//            label.text = text();
//        }
//        else if ((!Nie.EditorMenu.DrawStatesLabel || pos == null) && label != null)
//        {
//            if (UnityEditor.EditorApplication.isPlaying)
//                Destroy(label.gameObject);
//            else
//                DestroyImmediate(label.gameObject);
//            label = null;
//        }
//        else if(label != null)
//        {
//            label.text = text();
//        }
//    }
//    private void Update()
//    {
        
//        CheckDebugLabel(DebugTitle, ref DebugLabelTitle, ()=>Title);
//        CheckDebugLabel(DebugTime, ref DebugLabelTime, () => CurrentVHSTimeString);
//    }
//#endif

}

