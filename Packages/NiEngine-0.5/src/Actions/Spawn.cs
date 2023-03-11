using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nie.Actions
{

    [Serializable, ClassPickerName("Spawn")]
    public class Spawn : Action
    {
        public GameObjectReference ObjectToSpawn;
        public PositionReference SpawnPosition;
        public override void Act(Owner owner, EventParameters parameters)
        {
            var obj = ObjectToSpawn.GetTargetGameObject(parameters);
            if (obj)
            {
                var spawned = GameObject.Instantiate(obj);
                spawned.transform.position = parameters.Current.TriggerPosition;
            }
        }
    }

}