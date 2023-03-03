using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{

    float m_LastTime;
    public GameSession GameSession;
    public TMPro.TextMeshPro Text;
    // Start is called before the first frame update
    void Start()
    {
        m_LastTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameSession.CurrentTime != m_LastTime)
        {
            m_LastTime = GameSession.CurrentTime;
            Text.text = GameSession.CurrentVHSTimeString;
        }
        
    }
}
