using Akka.Actor;  
internal class NodeManager
{
    static IActorRef _heartbeat;
    static IActorRef _selectionCycle;
    static IActorRef _candidate;
    static IActorRef _follower;
    static IActorRef _leader;
    static IActorRef _statusBroadcast;

    public static void CreateActorStatusBroadcast(IActorRef statusBroadcast)
    {
        _statusBroadcast = statusBroadcast;
    }

    public static void CreateActorHeartbeat(IActorRef heartbeat)
    {
        _heartbeat = heartbeat;
    }

    public static void CreateActorSelection(IActorRef electionCycle)
    {
        _selectionCycle = electionCycle;
    }

    public static void CreateActorCandidate(IActorRef candidate)
    {
        _candidate = candidate;
    }

    public static void CreateActorLeader(IActorRef leader)
    {
        _leader = leader;
    }

    public static void CreateActorFollower(IActorRef follower)
    {
        _follower = follower;
    }

    public static void StartHeartbeat()
    {
        _heartbeat?.Tell(new RunHeartbeat(true));
    }
    public static void SendRequest(int number,DateTime sentdatetime)
    {
        _heartbeat?.Tell(new NodeRequest(number, sentdatetime));
    }

    public static void StopHeartbeat()
    {
        _heartbeat?.Tell(new RunHeartbeat(false));
    }

    public static void StartSelectionTimer()
    {
        _selectionCycle?.Tell(new RunSelectionTime(true));
    }

    public static void StopSelectionTimer()
    {
        _selectionCycle?.Tell(new RunSelectionTime(false));
    }

    public static void ResetSelectionTimer()
    {
        _selectionCycle?.Tell(new ResetSelection());
    }

    public static void RequestForVote  (int term)
    {
        _candidate?.Tell(new RequestForVote  (term));
    }

    public static void StartWaitForVote()
    {
        _candidate?.Tell(new StartWaitForVote(true));
    }

    public static void StopWaitForVote()
    {
        _candidate?.Tell(new StartWaitForVote(false));
    }
    public static void SendTerminateSignal()
    {
        //_statusBroadcast.Tell(new SendTerminate());
    }
    public static void SendHeartbeatResponse(double heartbeatId, int senderId, string senderPath, int term, int logIndex,NodeRequest? CurrentRequet)
    {
        _heartbeat.Tell(new SendHeartbeatResponse(heartbeatId, senderId, senderPath, term, logIndex,CurrentRequet));
    }

    public static void Exit(TimeSpan timeout)
    {
         SendTerminateSignal();
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