﻿public class RaftEvents
{
    public static Action ElectionTimeoutEvent { get; set; }
    public static Action<int> ElectionDurationChangedEvent { get; set; }
    public static Action<int> ElectionExpiredTimeEvent { get; set; }
    public static Action WaitForVoteTimeoutEvent { get; set; }
    public static Action<string, Heartbeat> HeartbeatEvent { get; set; }
    public static Action JoinedClusterEvent { get; set; }
    public static Action<HeartbeatResponse> HeartbeatEventResponse { get; set; }
    public static Action<int> MemberChangedEvent { get; set; }
    public static Action<int, int> GotVoteEvent { get; set; }
    public static Func<VoteRequest, bool> VoteRequestEvent { get; set; }
    public enum EventsList
    {
        None = 0,
        ElectionTimeoutEvent = 1,
        MemberChanged = 2,
        HeartbeatEvent = 3,
    }

    public EventsList Event { get; private set; }
    public object[] Args { get; private set; }
    public RaftEvents(EventsList e, params object[] args)
    {
        Event = e;
        Args = args;
    }
}