using UnityEngine;
using UnityEngine.Events;

namespace CoreCraft.Programming.Events
{
    public abstract class BaseGameEventListener<T, E, UER> : MonoBehaviour, IGameEventListener<T> where E : BaseGameEvent<T> where UER : UnityEvent<T>
    {
        [SerializeField] private E _gameEvent;
        public E GameEvent { get => _gameEvent; set => _gameEvent = value; }

        [SerializeField] private UER _unityEventResponse;

        private void OnEnable()
        {
            if (_gameEvent == null)
                return;

            GameEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            if (_gameEvent == null)
                return;

            GameEvent.UnregisterListener(this);
        }

        public void OnEventRaised(T item)
        {
            if (_unityEventResponse != null)
            {
                _unityEventResponse.Invoke(item);
            }
        }
    }
}