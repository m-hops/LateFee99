using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{
    public static class GameObjectExt
    {
        public static IEnumerable<ReactionStateMachine> AllReactionStateMachine(this GameObject @this)
            => @this.GetComponents<ReactionStateMachine>().Where(x => x.enabled);
        public static string GetNameOrNull(this GameObject @this)
            => @this == null ? "<null>" : @this.name;
        public static string GetNameOrNull(this MonoBehaviour @this)
            => @this == null ? "<null>" : @this.name;
        public static GameObject GetGameObjectOrNull(this Component @this)
            => @this == null ? null : @this.gameObject;
    }
}