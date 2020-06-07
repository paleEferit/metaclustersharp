using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaclustersharp
{
    //delegates for the delegate god!
    /// <summary>
    /// distance function for the multidimensional space, suitable for both raw data and space-converted data
    /// </summary>
    /// <param name="x1">first point in multidimensional space</param>
    /// <param name="x2">second point in multidimensional space</param>
    /// <returns>a nonnegative double value that represents distance between points</returns>
    public delegate double distanceFunction(double[] x1, double[] x2); 
    
    /// <summary>
    /// a function to find cluster center
    /// </summary>
    /// <param name="c">cluster</param>
    /// <param name="df">distance funtion</param>
    /// <param name="pars">additional params as array of double values</param>
    /// <returns>a set of double values matching cluster dimensions that represent cluster center as virtual (or not so virtual) cluster element</returns>
    public delegate double[] clusterCenterFunction(Cluster c, distanceFunction df, double[] pars);

    /// <summary>
    /// a function to convert n-dminsional space to k-dimensional space, might use distance function
    /// It is used in raw data preprocessing
    /// </summary>
    /// <param name="xDist">distance function for input data space</param>
    /// <param name="xSpc">input data as cluster</param>
    /// <param name="pars">additional conversion params</param>
    /// <returns>data translated to another multidimensional space</returns>
    public delegate Cluster spaceConversionFunction(distanceFunction xDist, Cluster xSpc, double[] pars); 

    /// <summary>
    /// a nearest neihbor based function that represents whether two clusters have elements that can be shared
    /// </summary>
    /// <param name="y1">cluster one</param>
    /// <param name="y2">cluster two</param>
    /// <param name="yDist">distance function for cluster multidimensional space</param>
    /// <param name="cnf">cluster center function</param>
    /// <param name="cnfPars">cluster center function params</param>
    /// <returns>a degree of intersection between two clusters on nearest neihbor terms, in [0; 1] range </returns>
    public delegate float sameCluster(Cluster y1, Cluster y2, distanceFunction yDist, clusterCenterFunction cnf, double[] cnfPars);
    
    /// <summary>
    /// a fuction for determining whether clusters should be united, defines the degree of similarity
    /// </summary>
    /// <param name="k1">cluster one</param>
    /// <param name="k2">cluster two</param>
    /// <param name="yDist">distance function for input cluster space</param>
    /// <param name="cnf">cluster center function</param>
    /// <param name="cnfPars">cluster center funcion params</param>
    /// <returns>a degree of similarity in [0; 1] range</returns>
    public delegate float similarClusters(Cluster k1, Cluster k2, distanceFunction yDist, clusterCenterFunction cnf, double[] cnfPars);

    /// <summary>
    /// a cluster division function divides cluster in two least similar ones
    /// </summary>
    /// <param name="k1">input cluster</param>
    /// <param name="yDist">distance function for cluster space</param>
    /// <param name="cnf">cluser center function</param>
    /// <param name="cnfPars">cluster center function params</param>
    /// <param name="splitPars">cluster split params</param>
    /// <returns>a collection of subclusters to current cluster as a result of split</returns>
    public delegate ICollection<Cluster> splitCluster(Cluster k1, distanceFunction yDist, clusterCenterFunction cnf, double[] cnfPars, double[] splitPars);
    /* 0*/
    /// <summary>
    /// a function to caompare and sort clusters
    /// </summary>
    /// <param name="k1">cluster one</param>
    /// <param name="k2">cluster two</param>
    /// <param name="yDist">distance function for cluster space</param>
    /// <param name="cnf">cluster center function</param>
    /// <param name="cnfPars">cluster center function params</param>
    /// <returns> a float value that above 0 if k1>k2, below 0 if k1<k2, and 0 if k1=k2</returns>
    public delegate float comapreClusters(Cluster k1, Cluster k2, distanceFunction yDist, clusterCenterFunction cnf, double[] cnfPars);

    /// <summary>
    /// Cluster algorithm that can be inited with functions matching the delegates above
    /// </summary>
    public class ClusterAlgorithm
    {
        /// <summary>
        /// cluster split trivial flag
        /// </summary>
        private bool isSplitingTrivial;
        private clusterCenterFunction cntFunc;
        private distanceFunction xDist, yDist;
        private spaceConversionFunction xyConv;
        private sameCluster comparePointFunction;
        private similarClusters clusterFusionCriteria;
        private splitCluster clusterDivider;
        private comapreClusters clusterSortCriteria;

        private ClusterAlgorithm()
        { 
            //никаких заведомо неверных состояний
        }

        /// <summary>
        /// конструктор алгоритма
        /// </summary>
        /// <param name="xDist">distance function for input data space</param>
        /// <param name="yDist">distance function for preprocessed data space</param>
        /// <param name="xyConv">space conversion function (in some cases could be similar to feature mapping)</param>
        /// <param name="comparePointFunction">nearest neihbor function for clusters, shoud return constant -1 to be trivial</param>
        /// <param name="clusterFusionCriteria">similarity function to define cluster fusion, should return constant -1 to be trivial</param>
        /// <param name="clusterDivider">a function for spliting cluster in two, should return empty collection of clusters to be trivial</param>
        /// <param name="clusterSortCriteria">a function for cluster comparing</param>
        /// <param name="cnt">cluster center function</param>
        public ClusterAlgorithm(distanceFunction xDist, distanceFunction yDist, spaceConversionFunction xyConv, sameCluster comparePointFunction,
            similarClusters clusterFusionCriteria, splitCluster clusterDivider, comapreClusters clusterSortCriteria, clusterCenterFunction cnt, bool isSplitTrivial = false)
        {
            this.xDist = xDist;
            this.yDist = yDist;
            this.xyConv = xyConv;
            this.comparePointFunction = comparePointFunction;
            this.clusterFusionCriteria = clusterFusionCriteria;
            this.clusterDivider = clusterDivider;
            this.clusterSortCriteria = clusterSortCriteria;
            this.cntFunc = cnt;
            this.isSplitingTrivial = isSplitTrivial;
        }

        private double GetMaxCenterToMemberDist(ICollection<Cluster> clusts, double[] centerClusterParams)
        {
            double maxDist = -1;
            foreach (var cluster in clusts)
            {
                double[] c = this.cntFunc(cluster, this.yDist, centerClusterParams);
                for (int i = 0; i < cluster.Size; i++)
                {
                    double dist = this.yDist(c, cluster.GetElementByLocalIndex(i));
                    if (dist > maxDist)
                    {
                        maxDist = dist;
                    }
                }
            }
            return maxDist;
        }

        //TODO: check all trivialized options in combinations
        //TODO: make threading solution
        /// <summary>
        /// metacluster algorithm
        /// </summary>
        /// <param name="data">input data as cluster zero</param>
        /// <param name="convesionParams">input data space conversion params</param>
        /// <param name="centerClusterParams">cluster center function params</param>
        /// <param name="uniteThresholdRatio">a ratio representing  distance for current step compared to previous, exceeding it triggers cluster fusion stop</param>
        /// <param name="initialWorkSectorRatio">a fraction ratio data to make first cluster set, in range of (0;1), i.e. above zero and below 1</param>
        /// <param name="targetClusterCount">target cluster count, -1 if undefined</param>
        /// <returns>a collection of clusters as a result</returns>
        public ICollection<Cluster> Process(Cluster data, double[] convesionParams, double[] centerClusterParams, float uniteThresholdRatio, float initialWorkSectorRatio, int targetClusterCount, double[] splitPars)
        {
            if (initialWorkSectorRatio < 0 || initialWorkSectorRatio > 1)
            {
                throw new ArgumentException("Некорректное соотношение начальной выборки");
            }
            if (data.Size <= 1)
            {
                throw new ArgumentException("Недостаточно данных для кластеризации");
            }
            List<Cluster> res = new List<Cluster>();
            /*space conversion*/
            Cluster realData = this.xyConv(this.xDist, data, convesionParams);
            /*sampling initial work sector*/
            List<int> remainingIndexes = new List<int>(realData.Indexes), sectorIndexes = new List<int>();
            int countStartPartition = (int)Math.Round(remainingIndexes.Count * initialWorkSectorRatio);
            Random r = new Random(DateTime.Now.Millisecond);
            int tmpIndVal, tmpLstInd;
            //index filtering
            while (countStartPartition > 0)
            {
                tmpLstInd = r.Next(remainingIndexes.Count);
                tmpIndVal = remainingIndexes[tmpLstInd];
                sectorIndexes.Add(tmpIndVal);
                remainingIndexes.RemoveAt(tmpLstInd);
                countStartPartition--;
            }
            //filling
            double[][] selArr = new double[sectorIndexes.Count][];
            List<int> unusedLocalInds = new List<int>();
            for(int i = 0; i<selArr.Length; i++)
            {
                selArr[i]=realData.GetElementByGlobalIndex(sectorIndexes[i]);
                unusedLocalInds.Add(i);
            }
            Cluster selectedData = new Cluster(sectorIndexes.ToArray(), selArr);
            /*sinitial work sector selected*/
            //creating paired clusters
            for (int i = 0; i < selectedData.Size-selectedData.Size%2; i++)
            {
                if (unusedLocalInds.Contains(i))
                { 
                    int minInd = i==unusedLocalInds[0]?unusedLocalInds[unusedLocalInds.Count-1]:unusedLocalInds[0];
                    double minVal = i==unusedLocalInds[0]?yDist(selectedData.GetElementByLocalIndex(i), selectedData.GetElementByLocalIndex(unusedLocalInds[unusedLocalInds.Count-1])):
                        yDist(selectedData.GetElementByLocalIndex(i), selectedData.GetElementByLocalIndex(unusedLocalInds[0]));
                    for (int j = 0; j < unusedLocalInds.Count; j++)
                    {
                        if (i != unusedLocalInds[j])
                        {
                            double dist = yDist(selectedData.GetElementByLocalIndex(i), selectedData.GetElementByLocalIndex(unusedLocalInds[j]));
                            if (dist < minVal)
                            {
                                minVal = dist;
                                minInd = unusedLocalInds[j];
                            }
                        }
                    }
                    unusedLocalInds.Remove(i);
                    unusedLocalInds.Remove(minInd);
                    res.Add(new Cluster(new int[] { selectedData.GetGlobalIndexByLocal(i), selectedData.GetGlobalIndexByLocal(minInd) }, 
                                        new double[][] { selectedData.GetElementByLocalIndex(i), selectedData.GetElementByLocalIndex(minInd) }));
                }
                
            }
            if (unusedLocalInds.Count == 1)
            {
                int minInd = 0;
                double minVal = yDist(selectedData.GetElementByLocalIndex(unusedLocalInds[0]), cntFunc(res[0], yDist, centerClusterParams));
                for (int i = 0; i < res.Count; i++)
                {
                    double dist = yDist(selectedData.GetElementByLocalIndex(unusedLocalInds[0]), cntFunc(res[i], yDist, centerClusterParams));
                    if (dist < minVal)
                    {
                        minInd = i;
                        minVal = dist;
                    }
                }
                res[minInd].AddElement(selectedData.GetElementByLocalIndex(unusedLocalInds[0]), selectedData.GetGlobalIndexByLocal(unusedLocalInds[0]));
            }
            //first center-to-center distance-wise iteration
            for (int i = 0; i < res.Count; i++)
            {
                float tmp, max = -1;
                int ind = -1;
                int j;
                for (j = 0; j < res.Count; j++)
                {
                    if (targetClusterCount != -1 && res.Count <= targetClusterCount)
                    {
                        break;
                    }
                    if (i != j)
                    {
                        tmp = this.clusterFusionCriteria(res[i], res[j], this.yDist, this.cntFunc, centerClusterParams);
                        if(tmp > max)
                        {
                            max = tmp;
                            ind = j;
                        }
                    }
                }
                if (max > 0)
                {
                    res[i].Append(res[ind]);
                    res.RemoveAt(ind);
                    // index shift
                    j--;
                    if (j < i)
                    {
                        i--;
                    }
                }
            }
            //first neihbor-wise iteration
            for (int i = 0; i < res.Count; i++)
            {
                float tmp, max = -1;
                int ind = -1;
                int j;
                for (j = 0; j < res.Count; j++)
                {
                    if (targetClusterCount != -1 && res.Count <= targetClusterCount)
                    {
                        break;
                    }
                    if (i != j)
                    {
                        tmp = this.comparePointFunction(res[i], res[j], this.yDist, this.cntFunc, centerClusterParams);
                        if (tmp > max)
                        {
                            max = tmp;
                            ind = j;
                        }
                    }
                }
                if (max > 0)
                {
                    res[i].Append(res[ind]);
                    res.RemoveAt(ind);
                    // index shift
                    j--;
                    if (j < i)
                    {
                        i--;
                    }
                }
            }
            /*adding remaining elements to nearest clusters*/
            for (int i = 0; i < remainingIndexes.Count; i++)
            {
                double minDist = this.yDist(realData.GetElementByLocalIndex(remainingIndexes[i]), this.cntFunc(res[0], this.yDist, centerClusterParams));
                int minInd = 0;
                for (int j = 0; j < res.Count; j++)
                {
                    double dst = this.yDist(realData.GetElementByLocalIndex(remainingIndexes[i]), this.cntFunc(res[j], this.yDist, centerClusterParams));
                    if (dst < minDist)
                    {
                        minDist = dst;
                        minInd = j;
                    }
                }
                res[minInd].AddElement(realData.GetElementByLocalIndex(remainingIndexes[i]), realData.GetGlobalIndexByLocal(remainingIndexes[i]));
            }

            //finding highest cluster radius in current iteration, will be used for comparing
            double maxDist = GetMaxCenterToMemberDist(res, centerClusterParams);
            //refreshing current max distance
            double nMaxDist = maxDist;
            /*unite until the end*/
            int oldClusterCount = selectedData.Size, newClusterCount = res.Count;
            while ((((targetClusterCount != -1) && (res.Count > targetClusterCount)) || (nMaxDist/maxDist<uniteThresholdRatio)) && (newClusterCount!=oldClusterCount))
            {
                oldClusterCount = newClusterCount;
                //saving old value
                maxDist = nMaxDist;
                //going by center to center distance
                for (int i = 0; i < res.Count; i++)
                {
                    float tmp, max = -1;
                    int ind = -1;
                    int j;
                    for (j = 0; j < res.Count; j++)
                    {
                        if (targetClusterCount != -1 && res.Count <= targetClusterCount)
                        {
                            break;
                        }
                        if (i != j)
                        {
                            tmp = this.comparePointFunction(res[i], res[j], this.yDist, this.cntFunc, centerClusterParams);
                            if (tmp > max)
                            {
                                max = tmp;
                                ind = j;
                            }
                        }
                    }
                    if (max > 0)
                    {
                        res[i].Append(res[ind]);
                        res.RemoveAt(ind);
                        // index shift
                        j--;
                        if (j < i)
                        {
                            i--;
                        }
                    }
                }
                //by neihboors
                for (int i = 0; i < res.Count; i++)
                {
                    float tmp, max = -1;
                    int ind = -1;
                    int j;
                    for (j = 0; j < res.Count; j++)
                    {
                        if (targetClusterCount != -1 && res.Count <= targetClusterCount)
                        {
                            break;
                        }
                        if (i != j)
                        {
                            tmp = this.comparePointFunction(res[i], res[j], this.yDist, this.cntFunc, centerClusterParams);
                            if (tmp > max)
                            {
                                max = tmp;
                                ind = j;
                            }
                        }
                    }
                    if (max > 0)
                    {
                        res[i].Append(res[ind]);
                        res.RemoveAt(ind);
                        // index shift
                        j--;
                        if (j < i)
                        {
                            i--;
                        }
                    }
                }
                //refreshing maximum cluster radius
                nMaxDist = GetMaxCenterToMemberDist(res, centerClusterParams);
                newClusterCount = res.Count;
            }
            maxDist = nMaxDist;
            oldClusterCount = newClusterCount-1;
            /*division until the end*/
            while ((newClusterCount!=oldClusterCount) && !this.isSplitingTrivial && (((targetClusterCount == -1) || (res.Count < targetClusterCount)) || (maxDist/nMaxDist > uniteThresholdRatio)))
            {
                oldClusterCount = newClusterCount;
                for (int i = 0; i < res.Count; i++)
                {
                    int cnt = 0;
                    ICollection<Cluster> cls = this.clusterDivider(res[i], this.yDist, this.cntFunc, centerClusterParams, splitPars);
                    cnt = cls.Count;
                    if (cnt > 0)
                    {
                        res.RemoveAt(i);
                        res.InsertRange(i, cls);
                    }
                    if (cnt > 1)
                    {
                        i+=cnt-1;
                    }
                }
                newClusterCount = res.Count;
            }
            //final conversion from pre-processed space to input data space using global indexes
            List<Cluster> realRes = new List<Cluster>();
            for (int i = 0; i < res.Count; i++)
            {
                Cluster tmp = new Cluster(data.Dimensions);
                for (int j = 0; j < res[i].Size; j++)
                {
                    int gInd = res[i].GetGlobalIndexByLocal(j);
                    double[] gEl = data.GetElementByGlobalIndex(gInd);
                    tmp.AddElement(gEl, gInd);
                }
                realRes.Add(tmp);
            }
            return realRes;
        }
    }
}
