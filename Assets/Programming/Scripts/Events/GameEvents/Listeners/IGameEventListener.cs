namespace CoreCraft.Programming.Events
{
    public interface IGameEventListener<T>
    {
        void OnEventRaised(T item);
    }
}