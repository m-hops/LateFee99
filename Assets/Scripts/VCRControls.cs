using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VCRControls : MonoBehaviour
{
    public float RewindDurationPerHour = 15;
    VHS m_CurrentVHS;

    void FindVHS()
    {
        m_CurrentVHS = gameObject.GetComponentInChildren<VHS>();
    }
    public void Rewind()
    {
        FindVHS();
        if (m_CurrentVHS == null)
            Debug.LogError("VCRControls could not find VHS", gameObject);
        State = StateEnum.Rewind;
    }
    public void Play()
    {
        FindVHS();

        if (m_CurrentVHS == null)
            Debug.LogError("VCRControls could not find VHS", gameObject);
        State = StateEnum.Play;
    }
    public void Stop()
    {
        FindVHS();
        State = StateEnum.Idle;
    }
    public enum StateEnum
    {
        Idle,
        Play,
        Rewind,
    }
    public StateEnum State;


    public Nie.ActionSet OnPlayOver;
    public Nie.ActionSet OnRewindOver;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (State)
        {
            case StateEnum.Play:
                if (m_CurrentVHS)
                {
                    m_CurrentVHS.HeadTime += Time.deltaTime;
                    
                    if (m_CurrentVHS.HeadTime >= m_CurrentVHS.Duration)
                    {
                        m_CurrentVHS.HeadTime = m_CurrentVHS.Duration;
                        State = StateEnum.Idle;
                        OnPlayOver.Act(new(this), Nie.EventParameters.Trigger(gameObject, gameObject, m_CurrentVHS.gameObject));
                    }
                }
                break;
            case StateEnum.Rewind:
                if (m_CurrentVHS)
                {
                    m_CurrentVHS.HeadTime -= Time.deltaTime * 3600 / RewindDurationPerHour;
                    if(m_CurrentVHS.HeadTime <= 0)
                    {
                        m_CurrentVHS.HeadTime = 0;
                        State = StateEnum.Idle;
                        OnRewindOver.Act(new(this), Nie.EventParameters.Trigger(gameObject, gameObject, m_CurrentVHS.gameObject));
                    }
                }
                break;

        }
    }
    
}
