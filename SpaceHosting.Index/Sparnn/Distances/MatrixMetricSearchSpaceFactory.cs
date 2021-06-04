using System;
using System.Collections.Generic;

namespace SpaceHosting.Index.Sparnn.Distances
{
    internal class MatrixMetricSearchSpaceFactory
    {
        private readonly MatrixMetricSearchSpaceAlgorithm searchSpaceAlgorithm;

        public MatrixMetricSearchSpaceFactory(MatrixMetricSearchSpaceAlgorithm searchSpaceAlgorithm)
        {
            this.searchSpaceAlgorithm = searchSpaceAlgorithm;
        }

        public MatrixMetricSearchSpace<TElement> Create<TElement>(IList<MathNet.Numerics.LinearAlgebra.Double.SparseVector> featureVectors, TElement[] elements, int searchBatchSize)
        {
            return searchSpaceAlgorithm switch
            {
                MatrixMetricSearchSpaceAlgorithm.Cosine => new CosineDistanceSpace<TElement>(featureVectors, elements, searchBatchSize),
                MatrixMetricSearchSpaceAlgorithm.JaccardBinary => new JaccardBinaryDistanceSpace<TElement>(featureVectors, elements, searchBatchSize),
                _ => throw new InvalidOperationException($"Invalid {nameof(searchSpaceAlgorithm)}: {searchSpaceAlgorithm}")
            };
        }
    }
}