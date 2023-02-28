using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VHS : MonoBehaviour
{
    public string Title;
    public float Duration;
    public float HeadTime;

    public Transform DebugTitle;
    public Transform DebugTime;

    public string CurrentVHSTimeString
    {
        get
        {
            //float second = HeadTime % 60;
            //var time = HeadTime - second;
            //float minute = (HeadTime - second) % 60;
            //float hour = HeadTime % 60;
            
            return new System.TimeSpan(0, (int)(HeadTime % 86400) / 3600, (int)(HeadTime % 3600) / 60, (int)HeadTime % 60, 0).ToString("c");
        }
    }

#if UNITY_EDITOR
    TextMesh DebugLabelTitle = null;
    TextMesh DebugLabelTime = null;
    
    void CheckDebugLabel(Transform pos, ref TextMesh label, System.Func<string> text)
    {

        if (Nie.EditorMenu.DrawStatesLabel && pos != null && label == null)
        {
            var obj = GameObject.Instantiate(Nie.EditorMenu.DebugLabelAsset, pos);
            label = obj.GetComponent<TextMesh>();
            obj.hideFlags = HideFlags.HideAndDontSave;
            label.transform.localPosition = Vector3.zero;
            label.transform.localRotation = Quaternion.identity;
            var parentScale = pos.lossyScale;
            var scale = Nie.EditorMenu.DebugLabelAsset.transform.localScale;
            label.transform.localScale = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, scale.z / parentScale.z);
            label.text = text();
        }
        else if ((!Nie.EditorMenu.DrawStatesLabel || pos == null) && label != null)
        {
            if (UnityEditor.EditorApplication.isPlaying)
                Destroy(label.gameObject);
            else
                DestroyImmediate(label.gameObject);
            label = null;
        }
        else
        {
            label.text = text();
        }
    }
    private void Update()
    {
        
        CheckDebugLabel(DebugTitle, ref DebugLabelTitle, ()=>Title);
        CheckDebugLabel(DebugTime, ref DebugLabelTime, () => CurrentVHSTimeString);
    }
#endif

}

