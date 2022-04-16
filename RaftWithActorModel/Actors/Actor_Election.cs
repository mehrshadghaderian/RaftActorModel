using Akka.Actor; 
using Serilog; 
using System.Security.Cryptography;
public class Actor_Election : ReceiveActor
{
    class ElectionExpiredTime { }

    private const int TIME_STEP_MS = 50;
    private const int MIN_DURATION_MS = 5000;
    private const int MAX_DURATION_MS = 10000;
    private const int MS_PER_SEC = 1000;

    private ICancelable _timerTask;
    private int _electionDuration = MAX_DURATION_MS;
    private int _expiredTime = 0;
    private bool _electionStarted = false;

    public int ElectionDuration { get => _electionDuration; set => _electionDuration = value; }

    public Actor_Election()
    {
        randomTimeout();
        Log.Information("{0}", $"Duration is {(float)_electionDuration / MS_PER_SEC}");

        Receive<ElectionExpiredTime>(t => {

            _expiredTime += TIME_STEP_MS;

            RaftEvents.ElectionExpiredTimeEvent?.Invoke(_expiredTime);

            Console.Write($"({(float)_expiredTime / MS_PER_SEC})");
            if (_expiredTime >= _electionDuration)
            {
                randomTimeout();
                RaftEvents.ElectionTimeoutEvent?.Invoke();
            }
        });

        Receive<ResetElection>(t => {
            _expiredTime = 0;
        });

        Receive<RunElectionTime>(s => {
            if (s.Start)
            {
                if (!_electionStarted)
                {
                    _electionStarted = true;
                    _timerTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(20),
                    TimeSpan.FromMilliseconds(TIME_STEP_MS), Context.Self, new ElectionExpiredTime(), ActorRefs.NoSender);
                }
            }
            else
            {
                if (_electionStarted)
                {
                    _electionStarted = false;
                    _timerTask?.Cancel();
                }
            }
        });
    }
    private void randomTimeout()
    {
        byte[] b = new byte[2];
        RandomNumberGenerator.Create().GetBytes(b);
        double rand = Math.Abs((double)BitConverter.ToInt16(b, 0)) / 100000;
        Log.Information("{0}", $"Election is now {_electionDuration}ms");
        _electionDuration = (int)(rand * (MAX_DURATION_MS - MIN_DURATION_MS) + MIN_DURATION_MS);
        RaftEvents.ElectionDurationChangedEvent?.Invoke(_electionDuration);
    }

    protected override void PreStart()
    {
        Self.Tell(new RunElectionTime(true));
    }
}