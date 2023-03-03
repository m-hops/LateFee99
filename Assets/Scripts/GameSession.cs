using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{

    public List<GameObject> VhsToSpawn;
    public GameObject WhereToSpawn;
    public int NextIndexToSpawn;
    public float CurrentTime;
    public float HourTimeInSecond = 60 * 5;
    public string CurrentVHSTimeString
        => new System.TimeSpan(0, (int)(CurrentTime % 86400) / 3600, (int)(CurrentTime % 3600) / 60, (int)CurrentTime % 60, 0).ToString("c");
    public void SpawnNextVHS()
    {
        if(VhsToSpawn.Count > NextIndexToSpawn)
        {
            var spawned = Instantiate(VhsToSpawn[NextIndexToSpawn], WhereToSpawn.transform.position, WhereToSpawn.transform.rotation);
            ++NextIndexToSpawn;
        }
    }
    public void AdvanceTime1H()
    {
        CurrentTime += 3600;

    }



    void Start()
    {
        
    }

    
    void Update()
    {
        var speed = 3600 / HourTimeInSecond;
        CurrentTime += Time.deltaTime * speed;
    }
}
