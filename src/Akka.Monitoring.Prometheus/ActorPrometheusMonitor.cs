using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Akka.Monitoring.Impl;
using Prometheus;

namespace Akka.Monitoring.Prometheus
{
    public class ActorPrometheusMonitor : AbstractActorMonitoringClient
    {
        private readonly Random _random = new Random();
        private readonly ConcurrentDictionary<string, Counter> _countersByMetricName = new ConcurrentDictionary<string, Counter>();
        private readonly ConcurrentDictionary<string, Summary> _summariesByMetricName = new ConcurrentDictionary<string, Summary>();
        private readonly ConcurrentDictionary<string, Gauge> _gaugesByMetricName = new ConcurrentDictionary<string, Gauge>();

        private bool ShouldSample(double sampleRate)
        {
            return _random.NextDouble() < sampleRate;
        }

        private static string StripInvalidChars(string metricName)
        {
            var metricNameChars = new char[metricName.Length];
            for (var i = 0; i < metricName.Length; i++)
            {
                var c = metricName[i];
                if ('a' <= c && c <= 'z'
                    || 'A' <= c && c <= 'Z'
                    || c == '_'
                    || c == ':'
                    || i > 0 && '0' <= c && c <= '9')
                {
                    metricNameChars[i] = c;
                }
                else
                {
                    metricNameChars[i] = '_';
                }
            }

            var mn = new string(metricNameChars);
            return mn;
        }

        public override void UpdateCounter(string metricName, int delta, double sampleRate)
        {
            var counter = _countersByMetricName.GetOrAdd(StripInvalidChars(metricName), key => Metrics.CreateCounter(key, key));

            if (!ShouldSample(sampleRate)) return;

            counter.Inc(delta);
        }

        public override void UpdateTiming(string metricName, long time, double sampleRate)
        {
            var summary = _summariesByMetricName.GetOrAdd(StripInvalidChars(metricName), key => Metrics.CreateSummary(key, key));

            if (!ShouldSample(sampleRate)) return;

            summary.Observe(time);
        }

        public override void UpdateGauge(string metricName, int value, double sampleRate)
        {
            var gauge = _gaugesByMetricName.GetOrAdd(StripInvalidChars(metricName), key => Metrics.CreateGauge(key, key));

            if (!ShouldSample(sampleRate)) return;

            gauge.Set(value);
        }

        //Unique name used to enforce a single instance of this client
        private static readonly int UniqueMonitoringClientId = new Guid("2385352d-f0a7-4919-8d7f-97a6e17c8122").GetHashCode();

        public override int MonitoringClientId => UniqueMonitoringClientId;

        public override void DisposeInternal()
        {
            // NOOP
        }
    }
}
