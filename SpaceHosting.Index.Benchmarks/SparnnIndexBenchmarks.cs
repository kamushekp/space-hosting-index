using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using MoreLinq;
using SpaceHosting.Index.Sparnn;
using Vostok.Logging.Abstractions;

namespace SpaceHosting.Index.Benchmarks
{
    [Config(typeof(JobsConfig))]
    [MemoryDiagnoser, ThreadingDiagnoser]
    [AllStatisticsColumn]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnassignedField.Global")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    public class SparnnIndexBenchmarks
    {
        [Params("Cosine", "JaccardBinary")]
        public string IndexMetric = null!;

        [Params(1, 10, 100)]
        public int QueryVectorsCount;

        [Params(10, 100, 500)]
        public int K;

        private readonly Random deterministicRandom = new Random(Seed: 42);
        private readonly IndexStoreBuilder indexStoreBuilder = new IndexStoreBuilder(new SilentLog());

        private List<double?[]> vectors = null!;
        private IIndexStore<int, object, SparseVector> indexStore = null!;
        private IndexQueryDataPoint<SparseVector>[] queryDataPoints = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            vectors = VectorFile.ReadVectors(
                @"c:\temp\sparse-vectors-df.csv",
                VectorsFileFormat.PandasDataFrameCsv);
            Console.Out.WriteLine($"vectors.Count: {vectors.Count}");

            indexStore = indexStoreBuilder.BuildIndexStore(
                $"{Algorithms.SparnnIndex}.{IndexMetric}",
                vectors,
                VectorConversions.ToSparseVector);
            Console.Out.WriteLine($"indexStore.Count: {indexStore.Count}");
        }

        [IterationSetup]
        public void IterationSetup()
        {
            var sampleVectors = vectors.RandomSubset(QueryVectorsCount, deterministicRandom).ToList();
            var sampleVectorsReordered = sampleVectors.Shuffle(deterministicRandom).ToList();
            var vectorsToSearch = sampleVectors.Zip(sampleVectorsReordered).Select(t => MidPoint(t.First.ToSparseVector(), t.Second.ToSparseVector())).ToArray();

            queryDataPoints = vectorsToSearch.Select(x => new IndexQueryDataPoint<SparseVector> {Vector = x}).ToArray();
        }

        [Benchmark]
        public void Search()
        {
            var results = indexStore.FindNearest(queryDataPoints, limitPerQuery: K);

            if (results.Count != QueryVectorsCount)
                throw new InvalidOperationException("results.Count != QueryVectorsCount");
        }

        public static SparseVector MidPoint(SparseVector v1, SparseVector v2)
        {
            if (v1.Dimension != v2.Dimension)
                throw new InvalidOperationException("v1.Dimension != v2.Dimension");

            var midPoint = ((v1.ToIndexVector() + v2.ToIndexVector()) / 2).ToModelVector();

            if (midPoint.Dimension != v1.Dimension)
                throw new InvalidOperationException("midPoint.Dimension != v1.Dimension");

            return midPoint;
        }

        private class JobsConfig : ManualConfig
        {
            private readonly RunMode runMode = new RunMode
            {
                RunStrategy = RunStrategy.Monitoring,
                LaunchCount = 1,
                WarmupCount = 1,
                IterationCount = 50,
                InvocationCount = 10, //ops per iteration
                UnrollFactor = 1
            };

            public JobsConfig()
            {
                AddJob(baseline: true);
                AddJob(baseline: false);
            }

            private void AddJob(bool baseline)
            {
                var job = Job
                    .Dry
                    .WithBaseline(baseline)
                    .WithId(baseline ? "baseline" : "current")
                    .WithCustomBuildConfiguration(baseline ? "Baseline" : "Release")
                    .Apply(runMode);

                AddJob(job);
            }
        }
    }
}