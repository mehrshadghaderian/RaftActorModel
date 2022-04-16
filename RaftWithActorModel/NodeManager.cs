﻿using Akka.Actor;  
internal class NodeManager
{
    static IActorRef _heartbeat;
    static IActorRef _electionCycle;
    static IActorRef _candidate;
    static IActorRef _follower;
    static IActorRef _leader;
    static IActorRef _statusBroadcast;

    public static void SetStatusBroadcast(IActorRef statusBroadcast)
    {
        _statusBroadcast = statusBroadcast;
    }

    public static void SetHeartbeat(IActorRef heartbeat)
    {
        _heartbeat = heartbeat;
    }

    public static void SetElection(IActorRef electionCycle)
    {
        _electionCycle = electionCycle;
    }

    public static void SetCandidate(IActorRef candidate)
    {
        _candidate = candidate;
    }

    public static void SetLeader(IActorRef leader)
    {
        _leader = leader;
    }

    public static void SetFollower(IActorRef follower)
    {
        _follower = follower;
    }

    public static void StartHeartbeat()
    {
        _heartbeat?.Tell(new RunHeartbeat(true));
    }

    public static void StopHeartbeat()
    {
        _heartbeat?.Tell(new RunHeartbeat(false));
    }

    public static void StartElectionTimer()
    {
        _electionCycle?.Tell(new RunElectionTime(true));
    }

    public static void StopElectionTimer()
    {
        _electionCycle?.Tell(new RunElectionTime(false));
    }

    public static void ResetElectionTimer()
    {
        _electionCycle?.Tell(new ResetElection());
    }

    public static void AskForVote(int term)
    {
        _candidate?.Tell(new AskForVote(term));
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
    public static void SendHeartbeatResponse(double heartbeatId, int senderId, string senderPath, int term, int logIndex)
    {
        _heartbeat.Tell(new SendHeartbeatResponse(heartbeatId, senderId, senderPath, term, logIndex));
    }

    public static void Stop(TimeSpan timeout)
    {
         SendTerminateSignal();
        _electionCycle?.GracefulStop(timeout);
        _electionCycle = null;
        _heartbeat?.GracefulStop(timeout);
        _heartbeat = null;
        _candidate?.GracefulStop(timeout);
        _candidate = null;
        _follower?.GracefulStop(timeout);
        _follower = null;
    }
}