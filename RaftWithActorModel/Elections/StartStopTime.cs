using System;
using System.Collections.Generic;
using System.Text;

namespace RaftWithActorModel.Elections
{
    public class StartStopTime
    {
        public bool Start { get; private set; }
        public StartStopTime(bool start)
        {
            Start = start;
        }
    }
}
