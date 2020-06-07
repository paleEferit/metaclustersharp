using System;
using System.Collections.Generic;
using System.Text;

namespace Metaclustersharp
{
    /// <summary>
    /// Just some cluster algorithm samples
    /// </summary>
    public class AlgorithmExamples
    {
        public static ClusterAlgorithm GetKmeans
        {
            get
            {
                ClusterAlgorithm kMeansAlg = new ClusterAlgorithm(FunctionExamples.EuclidianDistance, FunctionExamples.EuclidianDistance, FunctionExamples.TrivialConversionFunction, FunctionExamples.SameClusterBrute, FunctionExamples.SimilarClusterBrute, FunctionExamples.SplitClusterTrivial, FunctionExamples.ComapreClustersBySize, FunctionExamples.AbsClusterCenter, true);
                return kMeansAlg;
            }
        }

        public static ClusterAlgorithm DensityAlgorithm
        {
            get
            {
                ClusterAlgorithm densityAlg = new ClusterAlgorithm(FunctionExamples.EuclidianDistance, FunctionExamples.DensityDistance, FunctionExamples.DensityConversionFunction, FunctionExamples.SameClusterBrute, FunctionExamples.SimilarClusterBrute, FunctionExamples.SplitClusterGraph, FunctionExamples.ComapreClustersBySize, FunctionExamples.AbsClusterCenter, false);
                return densityAlg;
            }
        }

        public static ClusterAlgorithm HierarchyAlgorithm
        {
            get
            {
                ClusterAlgorithm hierarchyAlg = new ClusterAlgorithm(FunctionExamples.EuclidianDistance, FunctionExamples.EuclidianDistance, FunctionExamples.TrivialConversionFunction, FunctionExamples.SameClusterBrute, FunctionExamples.SimilarClusterBrute, FunctionExamples.SplitClusterGraph, FunctionExamples.ComapreClustersBySize, FunctionExamples.AbsClusterCenter, false);
                return hierarchyAlg;
            }
        }
    }
}
