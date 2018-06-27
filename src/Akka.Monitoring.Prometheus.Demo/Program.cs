using System;
using System.Threading;
using Akka.Actor;
using Prometheus;

namespace Akka.Monitoring.Prometheus.Demo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            var metricServer = new MetricServer(10250);
            metricServer.Start();

            Console.WriteLine("Started metric server at localhost:10250 (http://localhost:10250/metrics)");

            Console.WriteLine("Starting up actor system...");
            var system = ActorSystem.Create("akka-performance-demo");

            var didMonitorRegister = ActorMonitoringExtension.RegisterMonitor(system, new ActorPrometheusMonitor(system));
            Console.WriteLine(didMonitorRegister
                ? "Successfully registered Prometheus monitor"
                : "Failed to register Prometheus monitor");

            // Start up three sets of Pluto and Charon (reference to I'm Your Moon by Jonathan Coulton)
            for (var i = 0; i < 3; i++)
            {
                var pluto = system.ActorOf<PlutoActor>();
                var charon = system.ActorOf<CharonActor>();
                charon.Tell(pluto); 
            }

            Thread.Sleep(TimeSpan.FromSeconds(15));

            Console.WriteLine("Shutting down...");
            system.Terminate().Wait();
            Console.WriteLine("Shutdown complete");

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            metricServer.Stop();
        }
    }
}
