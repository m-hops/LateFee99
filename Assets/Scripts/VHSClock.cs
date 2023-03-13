using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nie;

public class VHSClock : MonoBehaviour, IStateObserver
{
    public string StateToObserve;

    public TMPro.TextMeshPro Text;
    float m_LastTime = -1;
    VHS m_VHS;

    void IStateObserver.OnBegin(Owner owner, EventParameters parameters)
    {

        if (parameters.Current.TriggerObject == null)
            Debug.LogWarning($"VHS triggerObject is null");
        else if (parameters.Current.TriggerObject.TryGetComponent<VHS>(out var vhs))
        {
            m_VHS = vhs;
        }
        else
        {
            Debug.LogError($"No Found VHS : {parameters.Current.TriggerObject.name}");
        }
    }

    void IStateObserver.OnEnd(Owner owner, EventParameters parameters)
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
        var rsm = GetComponent<ReactionStateMachine>();
        if(rsm.TryGetState(StateToObserve, out var state))
        {
            state.StateObservers.Add(this);
        }
        else
            Debug.LogError($"No state '{state}' found");
    }

    void OnDestroy()
    {

        var rsm = GetComponent<ReactionStateMachine>();
        if (rsm.TryGetState(StateToObserve, out var state))
        {
            state.StateObservers.Remove(this);
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
