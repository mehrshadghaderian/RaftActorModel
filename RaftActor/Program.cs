
namespace MyNameSpace
{
    using RaftActorSimple;
    using Serilog; 
    public class MyClass
    {

        public static int nodeCount { get; set; } = 30000;
     

        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Debug()
  .WriteTo.Console()
  //.WriteTo.File(AppContext.BaseDirectory + "\\logs\\{Date}.log")
  .CreateLogger();
            List<RaftNode> raftnodes = new List<RaftNode>();
            for (int i = 1; i <= nodeCount; i++)
            {
                RaftNode raftNode =new RaftNode(i);
                raftnodes.Add(raftNode);
            }
            raftnodes.Shuffle();
            RaftNode randomRaftNode  = raftnodes.First();
            DateTime starttime = DateTime.Now;
            int electionTerm = 1;
            raftnodes.ForEach(node => {
                node.SetRaftNoedList(raftnodes);
            }); 
            randomRaftNode.LeaderElection(electionTerm); 
            Console.WriteLine((DateTime.Now-starttime).TotalSeconds);
            Console.ReadLine();
        }
    }

}
