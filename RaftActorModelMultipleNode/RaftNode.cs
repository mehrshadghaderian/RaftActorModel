using Serilog;
using System.Diagnostics;

public class RaftNode
{
    public enum Roles
    {
        Follower = 0,
        Candidate = 1,
        Leader = 2
    }

    public static int Term { get; private set; }
    public static int ClusterUid { get; private set; }
    public static int CurrentLeaderId { get; private set; }
    public static int Selection_ExpiredTime { get; set; }
    public static int SelectionDuration { get; set; }
    public static List<LogEntry> LogEntries { get; private set; }
    public static int LogIndex { get { return LogEntries.Count; } }
    public static NodeRequest? CurrentRequet { get; set; }
    static Roles _role;
    public static Roles Role
    {
        get { return _role; }
        set
        {
            Log.Information("{0}", $"node state is now {_role.ToString()}");
            _role = value;
        }
    }
    private int _votedForTerm;

    public static int Votes { get; private set; }
    public static int Majority { get; private set; }
    public static int ProcessId { get; private set; }
    public RaftNode(int clusterUniqueId)
    {
        ProcessId = Process.GetCurrentProcess().Id;
        ClusterUid = clusterUniqueId;
        Log.Information("{0}", $"node ClusterUid is {ClusterUid}");
        LogEntries = new List<LogEntry>();

        RaftEvents.SelectionDurationChangedEvent = dur => SelectionDuration = dur;
        RaftEvents.SelectionExpiredTimeEvent = el => Selection_ExpiredTime = el;
        RaftEvents.SelectionTimeoutEvent = () => {
            //step up as candidate
            //Request for vote
            if (Role == Roles.Follower)
            {
                Role = Roles.Candidate;
                Term++;
                Votes = 0; 
                Log.Information("{0}", $"Starting selection for term {Term}");
                NodeManager.StopSelectionTimer();
                NodeManager.RequestForVote  (Term);
                NodeManager.StartWaitForVote();
            }
        };

        RaftEvents.GotVoteEvent = (uid, term) =>
        {
            //if got more than majority vote, then becomes leader           
            //start sending heartbeat
            if (term == Term)
            {
                Votes++;
            }

            Log.Information("{0}", $"Got {Votes}/{Majority} votes for term {term} from {uid}");
            if (Votes >= Majority)
            {
                Log.Information("{0}", "Selected!");
                Role = Roles.Leader;
                CurrentLeaderId = ClusterUid;
                NodeManager.StopWaitForVote();
                NodeManager.StartHeartbeat();
            }
        };

         RaftEvents.HeartbeatEvent = (senderpath, hb) => {
            //resets selection time
            //otherwise becomes candidate and send request for votes         
            NodeManager.ResetSelectionTimer();

            //if heartbeat has term equal or bigger than self, then step down
            if (hb.Term >= Term)
            {
                //need to roll back changes and take match leader's log entries
                if (Role != Roles.Follower)
                {
                    Log.Information("{0}", "Stepping down");
                    Role = Roles.Follower;
                    NodeManager.StopHeartbeat();
                    NodeManager.StartSelectionTimer();
                }

                if (hb.Term != Term)
                {
                    Log.Information("{0}", $"Changing from term {Term} to {hb.Term}");
                    Term = hb.Term;
                }
            }

            CurrentLeaderId = hb.SenderId;
         
             if (Role == Roles.Leader)
             {
                 Log.Error("{0}", "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@  " + CurrentLeaderId.ToString());
             }

                 if (Role == Roles.Follower)
            {
                NodeManager.SendHeartbeatResponse(hb.Id, hb.SenderId, senderpath, hb.Term, hb.LogIndex,CurrentRequet);
                 CurrentRequet = null;
            } 
        };

        RaftEvents.JoinedClusterEvent = () => {
            if (Role != Roles.Leader)
            {
                Log.Information("{0}", "Starting selection timer");
                NodeManager.StartSelectionTimer();
            }
        };

        RaftEvents.VoteRequestEvent = vr =>
        {
            //if not voted this term and is not a leader, then vote for the candidate
            if (_votedForTerm < vr.Term && Role != Roles.Leader)
            {
                if (vr.SenderId != ClusterUid)
                {
                    Log.Information("{0}", $"Vote request from candidate {vr.SenderId} for term {vr.Term}");
                }
                else
                {
                    Log.Information("{0}", $"Vote request from self in term {vr.Term}");
                }

                _votedForTerm = vr.Term;

                return true;
            }
            else
            {
                Log.Information("{0}", $"Not voting. {vr.SenderId} asking for term {vr.Term}, last voted for {_votedForTerm} and state is {Role.ToString()}");
                return false;
            }
        };

        RaftEvents.WaitForVoteTimeoutEvent = () =>
        {
            //restart selection time                
            Role = Roles.Follower;
            NodeManager.ResetSelectionTimer();
            NodeManager.StartSelectionTimer();
        };

        RaftEvents.NodeChangedEvent = count => {
            //update majority
            Majority = (count + 1) / 2;
            Log.Information("{0}", $"Majority is now {Majority}");
        };
           RaftEvents.NodeRequestResponseEvent = (boo) => {
               if (Role != Roles.Leader)
               {
                   //Log.Information("{0}", "Starting selection timer");
                   //NodeManager.StartSelectionTimer();
               }
           };

    }

    public void SendRequest(NodeRequest nodeRequest)
    {
        if (Role == Roles.Leader)
        {

            NodeManager.SendRequest(nodeRequest.Number, nodeRequest.RequestDateTime);
            CurrentRequet = null;
        }
        else
        {
            CurrentRequet = nodeRequest;
        }

    }

    public int GetProcessId()
    {
        return ProcessId;
    }
    
    public void OnKill()
    {
        NodeManager.SendTerminateSignal();
    }
    public Roles getRoles()
    {
        return _role;
    }

    public void Exit(TimeSpan timeSpan)
    {
        NodeManager.Exit(timeSpan);
    }
}