using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Akka.Actor;
using Akka.Monitoring.Impl;
using DestructureExtensions;
using Prometheus;

namespace Akka.Monitoring.Prometheus
{
    public class ActorPrometheusMonitor : AbstractActorMonitoringClient
    {
        private readonly Random _random = new Random();
        private string _name;


        public ActorPrometheusMonitor(ActorSystem system)
        {
            _name = system.Name;
        }

        private bool IsSpecificMetric(string metricName)
        {
            return metricName.StartsWith($"{_name}.");
        }

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

        private static (string metric, Dictionary<string, string> labels) Metric(string metricName)
        {
            var (system, actor, metricWithLabels, _) = metricName.Split(new[] {'.'}, 3);
            var (metric, labelString, _) = metricWithLabels.Split(new[] {'{'}, 2);
            var labels = (labelString ?? "")
                .Trim('\'', '"', '}')
                .Split(',')
                .Where(pair => !string.IsNullOrWhiteSpace(pair))
                .Select(pair =>
                {
                    var (label, value, _) = pair.Split(new[] {'='}, 2);
                    return new
                    {
                        Label = label.Trim('\'', '"', '`'),
                        Value = value.Trim('\'', '"', '`'),
                    };
                })
                .Concat(new[]
                {
                    new
                    {
                        Label = nameof(system),
                        Value = system,
                    },
                    new
                    {
                        Label = nameof(actor),
                        Value = actor,
                    },
                })
                .ToDictionary(x => x.Label, x => x.Value);
            return (metric, labels);
        }

        public override void UpdateCounter(string metricName, int delta, double sampleRate)
        {
            if (!IsSpecificMetric(metricName)) return;
            if (!ShouldSample(sampleRate)) return;
            var (metric, labels) = Metric(metricName);
            var counter = Metrics.CreateCounter(StripInvalidChars(metric), metric, labels.Keys.ToArray());
            counter.Labels(labels.Values.ToArray()).Inc(delta);
        }

        public override void UpdateTiming(string metricName, long time, double sampleRate)
        {
            if (!IsSpecificMetric(metricName)) return;
            if (!ShouldSample(sampleRate)) return;
            var (metric, labels) = Metric(metricName);
            var timing = Metrics.CreateSummary(StripInvalidChars(metric), metric, labels.Keys.ToArray());
            timing.Labels(labels.Values.ToArray()).Observe(time);
        }

        public override void UpdateGauge(string metricName, int value, double sampleRate)
        {
            if (!IsSpecificMetric(metricName)) return;
            if (!ShouldSample(sampleRate)) return;
            var (metric, labels) = Metric(metricName);
            var timing = Metrics.CreateGauge(StripInvalidChars(metric), metric, labels.Keys.ToArray());
            timing.Labels(labels.Values.ToArray()).Set(value);
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
