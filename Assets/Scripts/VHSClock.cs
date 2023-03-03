using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nie;

public class VHSClock : MonoBehaviour, ReactionState.IObserver
{
    public ReactionState Observe;
    public TMPro.TextMeshPro Text;
    float m_LastTime = -1;
    VHS m_VHS;


    void ReactionState.IObserver.OnBegin(ReactionState state, GameObject from, GameObject triggerObject, Vector3 position)
    {

        if (triggerObject == null)
            Debug.LogWarning($"VHS triggerObject is null");
        else if (triggerObject.TryGetComponent<VHS>(out var vhs))
        {
            m_VHS = vhs;
        }
        else
        {
            Debug.LogWarning($"No Found VHS : {triggerObject.name}");
        }
    }
    void ReactionState.IObserver.OnEnd(ReactionState state, GameObject from, GameObject triggerObject, GameObject previousTriggerObject, Vector3 position)
    {
        m_VHS = null;
        m_LastTime = -1;
    }
    public void ShowClock(GameObject vhsGo)
    {
        if(vhsGo == null)
            Debug.Log($"VHS gameobject is null");
        else if (vhsGo.TryGetComponent<VHS>(out var vhs))
        {
            Debug.Log($"Found VHS : {vhs.name}");
        }
        else
        {
            Debug.Log($"No Found VHS : {vhsGo.name}");
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if(Observe!= null)
        {
            Observe.AddObserver(this);
        }
    }

    void OnDestroy()
    {

        if (Observe != null)
        {
            Observe.RemoveObserver(this);
        }
    }
    // Update is called once per frame
    void Update()
    {

        if (m_VHS != null && m_VHS.HeadTime != m_LastTime)
        {
            m_LastTime = m_VHS.HeadTime;
            Text.text = m_VHS.CurrentVHSTimeString;
        }

    }
}
