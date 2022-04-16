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
//var system = ActorSystem.Create("raftActorSystem", hocanConfig);

//// creating Actors
//NodeManager.SetLeader(system.ActorOf<Actor_Leader>("leader"));
//NodeManager.SetFollower(system.ActorOf<Actor_Follower>("follower"));
//NodeManager.SetCandidate(system.ActorOf<Actor_Candidate>("candidate"));
//NodeManager.SetElection(system.ActorOf<Actor_Election>("electionCycle"));
//NodeManager.SetHeartbeat(system.ActorOf<Actor_Heartbeat>("heartbeat")); 

//Log.Information("Enter 'quit' to exit Actor");

//string exitcommand = "";
//do
//{
//    exitcommand = Console.ReadLine();
//} while (exitcommand.ToLower() != "quit");
//new RaftNode(Cluster.Get(system).SelfUniqueAddress.Uid).Stop(TimeSpan.FromSeconds(20));


using (var system = ActorSystem.Create("raftActorSystem", hocanConfig))
{
    var cluster = Cluster.Get(system);
    int uid = cluster.SelfUniqueAddress.Uid;
    var node = new RaftNode(uid);
    NodeManager.SetLeader(system.ActorOf<Actor_Leader>("leader"));
    NodeManager.SetFollower(system.ActorOf<Actor_Follower>("follower"));
    NodeManager.SetCandidate(system.ActorOf<Actor_Candidate>("candidate"));
    NodeManager.SetElection(system.ActorOf<Actor_Election>("electionCycle"));
    NodeManager.SetHeartbeat(system.ActorOf<Actor_Heartbeat>("heartbeat"));
    //NodeManager.SetStatusBroadcast(system.ActorOf<StatusBroadcastActor>("status"));
    Log.Information("Enter 'quit' to exit Actor");

    string exitcommand = "";
    do
    {
        exitcommand = Console.ReadLine();
    } while (exitcommand.ToLower() != "quit");
    node.Stop(TimeSpan.FromSeconds(20));
}