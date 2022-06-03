namespace CoreCraft.Networking
{
    public class NetworkingDisconnectReason
    {
        public EConnectStatus Reason { get; private set; } = EConnectStatus.Undefined;

        public void SetDisconnectReason(EConnectStatus reason)
        {
            Reason = reason;
        }

        public void Clear()
        {
            Reason = EConnectStatus.Undefined;
        }

        public bool HasTransitionReason => Reason != EConnectStatus.Undefined;
    }
}