using UnityEngine;

namespace CoreCraft.Programming.Events
{
    [CreateAssetMenu (fileName = "New Void Event", menuName = "Events/Game Events/Void Event")]
    public class VoidEvent : BaseGameEvent<Void>
    {
        public void RaiseEvent() => RaiseEvent(new Void());
    }
}