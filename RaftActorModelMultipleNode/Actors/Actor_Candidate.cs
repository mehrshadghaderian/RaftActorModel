using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Serilog;

public class Actor_Candidate : ReceiveActor
{
    public class StopTimeout { }

    private bool _timeStarted;
    private ICancelable _timerTask;
    public Actor_Candidate()
    {
        var mediator = DistributedPubSub.Get(Context.System).Mediator;

        Receive<RequestForVote  >(a => {
            Log.Information("{0}", "Receive Request for votes Message");
            mediator.Tell(new Publish("voterequest", new VoteRequest(a.Term, RaftNode.ClusterUid)));
        });

        Receive<Vote>(v => {
            Log.Information("{0}", "Receive Vote Message, than Reset Wait timeout");
            //reset
            stopWait();
            startWait();
            RaftEvents.GotVoteEvent?.Invoke(v.SenderId, v.Term);

        });

        Receive<StopTimeout>(v =>
        {
            if (_timeStarted)
            {
                Log.Information("{0}", "Wait timeout");
                RaftEvents.WaitForVoteTimeoutEvent?.Invoke();
            }
        });

        Receive<StartWaitForVote>(w => {
            if (w.Start)
            {
                Log.Information("{0}", "Waiting for recive vote");
                startWait();
            }
            else
            {
                Log.Information("{0}", "Stopped waiting for recive vote");
                stopWait();
            }
        });

    }

    private void startWait()
    {
        if (!_timeStarted)
        {
            _timeStarted = true;
            _timerTask = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromSeconds(3),
                Context.Self, new StopTimeout(), ActorRefs.NoSender);
        }
    }

    private void stopWait()
    {
        if (_timeStarted)
        {
            _timeStarted = false;
            _timerTask?.Cancel();
        }
    }
}