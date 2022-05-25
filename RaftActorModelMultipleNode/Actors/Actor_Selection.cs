using Akka.Actor; 
using Serilog; 
using System.Security.Cryptography;
public class Actor_Selection : ReceiveActor
{
    class SelectionExpiredTime { }

    private const int timeStepMillisecond = 50;
    private const int minmunPeriodTimeMillisecond = 5000;
    private const int maximumPeriodTimeMillisecond = 10000; 
    private ICancelable _timerTask;
    private int _selectionDuration = maximumPeriodTimeMillisecond;
    private int _expiredTime = 0;
    private bool _selectionStarted = false;

    public int SelectionDuration { get => _selectionDuration; set => _selectionDuration = value; }

    public Actor_Selection()
    {
        randomTimeout();
        Log.Information("{0}", $"time period is {(float)_selectionDuration / 1000}");

        Receive<SelectionExpiredTime>(t => {

            _expiredTime += timeStepMillisecond;

            RaftEvents.SelectionExpiredTimeEvent?.Invoke(_expiredTime);
            //todo mehrshad
            //Console.Write($"({(float)_expiredTime / 1000})");
            if (_expiredTime >= _selectionDuration)
            {
                randomTimeout();
                RaftEvents.SelectionTimeoutEvent?.Invoke();
            }
        });

        Receive<ResetSelection>(t => {
            _expiredTime = 0;
        });

        Receive<RunSelectionTime>(s => {
            if (s.Start)
            {
                if (!_selectionStarted)
                {
                    _selectionStarted = true;
                    _timerTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(20),
                    TimeSpan.FromMilliseconds(timeStepMillisecond), Context.Self, new SelectionExpiredTime(), ActorRefs.NoSender);
                }
            }
            else
            {
                if (_selectionStarted)
                {
                    _selectionStarted = false;
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
        // todo mehrshad
        //Log.Information("{0}", $"selection is now {_selectionDuration}ms");
        _selectionDuration = (int)(rand * (maximumPeriodTimeMillisecond - minmunPeriodTimeMillisecond) + minmunPeriodTimeMillisecond);
        RaftEvents.SelectionDurationChangedEvent?.Invoke(_selectionDuration);
    }

    protected override void PreStart()
    {
        Self.Tell(new RunSelectionTime(true));
    }
}