using UnityEngine;

namespace CoreCraft.Programming.Events
{
    [AddComponentMenu("CoreCraft/Events/Void Listener")]
    public class VoidListener : BaseGameEventListener<Void, VoidEvent, UnityVoidEvent> { }
}