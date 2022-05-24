using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.Programming.Events
{
    public abstract class BaseGameEvent<T> : ScriptableObject
    {
        private readonly List<IGameEventListener<T>> _gameEventListeners = new List<IGameEventListener<T>>();

        public void RaiseEvent(T item)
        {
            for (int i = _gameEventListeners.Count - 1; i >= 0; i--)
            {
                _gameEventListeners[i].OnEventRaised(item);
            }
        }

        public void RegisterListener(IGameEventListener<T> listener)
        {
            if (!_gameEventListeners.Contains(listener))
            {
                _gameEventListeners.Add(listener);
            }
        }

        public void UnregisterListener(IGameEventListener<T> listener)
        {
            if (_gameEventListeners.Contains(listener))
            {
                _gameEventListeners.Remove(listener);
            }
        }
    }
}
