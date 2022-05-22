
namespace MyNameSpace
{ 
    using Serilog; 
    public class MyClass
    {

        public static int nodeCount { get; set; } = 2000;


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
            raftnodes.First().LeaderElection();
            NodeManager.GetActorList().Skip(3).FirstOrDefault().Tell(new RequestForVote(1, DateTime.Now));
            Console.ReadLine();
        }
    }

}
