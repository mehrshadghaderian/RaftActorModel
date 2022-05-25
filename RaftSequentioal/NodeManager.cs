using Akka.Actor;  
internal class NodeManager
{
    static IActorRef _heartbeat;
    static IActorRef _selectionCycle;
    static IActorRef _candidate;
    static IActorRef _follower;
    static IActorRef _leader;
    static IActorRef _statusBroadcast;
    static List<IActorRef> _actorList=new List<IActorRef>();

    public static void CreateActor(IActorRef actor)
    {
        NodeManager._actorList.Add(actor);
    }
    public static List<IActorRef> GetActorList()
    {
        return _actorList;
    }
    
    public static void SendHeartbeatResponse(double heartbeatId, int senderId, string senderPath, int term, int logIndex,NodeRequest? CurrentRequet)
    {
        _heartbeat.Tell(new SendHeartbeatResponse(heartbeatId, senderId, senderPath, term, logIndex,CurrentRequet));
    }

    public static void Exit(TimeSpan timeout)
    { 
        _selectionCycle?.GracefulStop(timeout);
        _selectionCycle = null;
        _heartbeat?.GracefulStop(timeout);
        _heartbeat = null;
        _candidate?.GracefulStop(timeout);
        _candidate = null;
        _follower?.GracefulStop(timeout);
        _follower = null;
    }
}