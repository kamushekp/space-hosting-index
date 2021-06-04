using System;
using System.Collections.Generic;
using SpaceHosting.Index.Sparnn.Clusters;
using SpaceHosting.Index.Sparnn.Distances;

namespace SpaceHosting.Index.Sparnn
{
    internal static class ClusterIndexFactory
    {
        public static IClusterIndex<TRecord> Create<TRecord>(
            IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors,
            TRecord[] recordsData,
            MatrixMetricSearchSpaceFactory matrixMetricSearchSpaceFactory,
            int? desiredClusterSize,
            IClusterIndex<TRecord> invoker)
        {
            var recordsCount = recordsData.Length;
            var maxClusterSize = desiredClusterSize ?? Math.Max((int)Math.Sqrt(recordsCount), 1000);

            if (invoker is null)
                return new RootClusterIndex<TRecord>(featureVectors, recordsData, matrixMetricSearchSpaceFactory, maxClusterSize);

            var levelsCount = Math.Log(recordsCount, maxClusterSize);

            if (levelsCount > 1.4)
                return new NonTerminalClusterIndex<TRecord>(featureVectors, recordsData, matrixMetricSearchSpaceFactory, maxClusterSize);

            return new TerminalClusterIndex<TRecord>(featureVectors, recordsData, matrixMetricSearchSpaceFactory, maxClusterSize);
        }
    }
}