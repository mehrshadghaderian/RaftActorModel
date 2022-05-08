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



for (int i = 0; i < 1; i++)
{
    var system = ActorSystem.Create("raftActorSystem", hocanConfig);
 
        var cluster = Cluster.Get(system);
        int uid = cluster.SelfUniqueAddress.Uid;
        var node = new RaftNode(uid);
        NodeManager.CreateActorLeader(system.ActorOf<Actor_Leader>("leader" ));
        NodeManager.CreateActorFollower(system.ActorOf<Actor_Follower>("follower"  ));
        NodeManager.CreateActorCandidate(system.ActorOf<Actor_Candidate>("candidate"  ));
        NodeManager.CreateActorSelection(system.ActorOf<Actor_Selection>("selectionTerm"  ));
        NodeManager.CreateActorHeartbeat(system.ActorOf<Actor_Heartbeat>("heartbeat"  ));
        Log.Information("Enter 'quit' to exit Actor");
     
        //string exitcommand = "";
        //do
        //{
        //    exitcommand = Console.ReadLine();
        //} while (exitcommand.ToLower() != "quit");
        //node.Exit(TimeSpan.FromSeconds(20));
  
}
Console.ReadLine();

