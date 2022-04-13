using Akka.Actor;
using Akka.Configuration;
using System;
using System.Linq;
using DotNetty;
using System.IO;
using Akka.Routing;
using Serilog;
using System.Globalization; 
using System.Reflection; 
using System.Runtime.InteropServices;
using RaftWithActorModel.Nodes;
using Akka.Cluster;
using RaftWithActorModel.Leader;
using RaftWithActorModel.Followers;
using RaftWithActorModel.Candidates;
using RaftWithActorModel.Elections;
using RaftWithActorModel.Logs;
using RaftWithActorModel.Heartbeats;

var assm = Assembly.GetEntryAssembly();

string path = AppContext.BaseDirectory;

Log.Logger = new LoggerConfiguration()
       .MinimumLevel.Debug()
       .WriteTo.LiterateConsole()
       .WriteTo.RollingFile(path + "\\logs\\{Date}.log")
       .CreateLogger();

string basePath = AppContext.BaseDirectory;
string hocon = File.ReadAllText(Path.Combine(basePath, "akka.hocon"));

var config = ConfigurationFactory.ParseString(hocon);

using (var system = ActorSystem.Create("raftsystem", config))
{ 
    var cluster = Cluster.Get(system);
    int uid = cluster.SelfUniqueAddress.Uid;
    var node = new RaftNode(uid);
    NodeManager.SetLeader(system.ActorOf<LeaderActor>("leader"));
    NodeManager.SetFollower(system.ActorOf<FollowerActor>("follower"));
    NodeManager.SetCandidate(system.ActorOf<CandidateActor>("candidate"));
    NodeManager.SetElection(system.ActorOf<ElectionCycleActor>("electionCycle"));
    NodeManager.SetHeartbeat(system.ActorOf<HeartbeatActor>("heartbeat"));
    NodeManager.SetStatusBroadcast(system.ActorOf<StatusBroadcastActor>("status"));
    Log.Information("Enter 'quit' to exit Actor"); 

    string exitcommand = "";
    do
    {
        exitcommand = Console.ReadLine();
    } while (exitcommand.ToLower() != "quit"); 
    node.Stop(TimeSpan.FromSeconds(20));
}

