using Akka.Actor;
using Akka.Configuration;
using Serilog; 
using Akka.Cluster; 


Log.Logger = new LoggerConfiguration()
       .MinimumLevel.Debug()
       .WriteTo.Console()
       .WriteTo.File(AppContext.BaseDirectory + "\\logs\\{Date}.log")
       .CreateLogger();

var hocanConfig = ConfigurationFactory.ParseString(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "hocan.configfile")));


List<RaftNode> raftNodes = new List<RaftNode>();
for (int i = 0; i < 8; i++)
{
    var system = ActorSystem.Create("raftActorSystem", hocanConfig);
 
        var cluster = Cluster.Get(system);
        int uid = cluster.SelfUniqueAddress.Uid;
        RaftNode? node = new RaftNode(uid);
        NodeManager.CreateActorLeader(system.ActorOf<Actor_Leader>("leader" ));
        NodeManager.CreateActorFollower(system.ActorOf<Actor_Follower>("follower"  ));
        NodeManager.CreateActorCandidate(system.ActorOf<Actor_Candidate>("candidate"  ));
        NodeManager.CreateActorSelection(system.ActorOf<Actor_Selection>("selectionTerm"  ));
        NodeManager.CreateActorHeartbeat(system.ActorOf<Actor_Heartbeat>("heartbeat"  ));
        Log.Information("Enter 'quit' to exit Actor");
        raftNodes.Add(node);



    //string exitcommand = "";
    //do
    //{
    //    exitcommand = Console.ReadLine();
    //} while (exitcommand.ToLower() != "quit");
    //node.Exit(TimeSpan.FromSeconds(20));

}
int milliseconds = 10000;
Thread.Sleep(milliseconds);
//var leaderNode=raftNodes.Where(a => a.getRoles() == RaftNode.Roles.Leader).FirstOrDefault();
 
var leaderNode = raftNodes.FirstOrDefault();
leaderNode.SendRequest(new NodeRequest(5,DateTime.Now));
    Console.ReadLine();

