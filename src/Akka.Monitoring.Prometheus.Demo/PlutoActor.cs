using System;
using System.Diagnostics;
using Akka.Actor;
using Akka.Dispatch.SysMsg;
using Akka.Event;

namespace Akka.Monitoring.Prometheus.Demo
{
    public class PlutoActor : ReceiveActor
    {
        private Stopwatch _stopwatch;

        public class YoureMyMoon { }
        public PlutoActor()
        {
            Context.IncrementActorCreated();

            var random = new Random();

            Receive<CharonActor.ImYourMoon>(_ =>
            {
                Context.IncrementMessagesReceived();
                Sender.Tell(new YoureMyMoon());
            });

            Receive<CharonActor.WeGoRoundAndRound>(_ =>
            {
                Context.IncrementMessagesReceived();
                _stopwatch = Stopwatch.StartNew();

                Context.GetLogger().Info("Start \"revolving\"");
                var cancelable = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.Zero, TimeSpan.FromSeconds(1), Self, "revolve", Self);

                Context.GetLogger().Info("Stop \"revolving\"");
                Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(5), Self, cancelable, Self);
            });

            Receive<string>(_ =>
            {
                Context.IncrementCounter("revolutions");
                Context.GetLogger().Debug("Whee!");
                Context.Gauge("revolutions.enjoyment", random.Next(1, 11));
            });

            Receive<ICancelable>(cancelable =>
            {
                cancelable.Cancel();
                Context.Timing("revolutions.elapsed", _stopwatch.ElapsedMilliseconds);
                Context.GetLogger().Warning("... the rest of the world seems so small");
                Self.Tell(PoisonPill.Instance);
            });

            Receive<Stop>(_ =>
            {
                Context.IncrementActorStopped();
            });
        }
    }
}