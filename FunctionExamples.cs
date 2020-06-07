using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaclustersharp
{
    public class FunctionExamples
    {
        private FunctionExamples()
        { 
        
        }

        public static double EuclidianDistance(double[] a, double[] b)
        {
            if (a.Length != b.Length)
            {
                throw new ArgumentException("Dimensional mismatch");
            }
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                sum += (a[i] - b[i]) * (a[i] - b[i]);
            }
            return Math.Sqrt(sum);
        }

        public static double[] AbsClusterCenter(Cluster c, distanceFunction df, double[] pars)
        {
            //ищем среднее
            double[] avg = new double[c.Dimensions];
            for (int i = 0; i < avg.Length; i++)
            {
                avg[i] = 0;
            }
            for (int i = 0; i < c.Size; i++)
            {
                for (int j = 0; j < avg.Length; j++)
                {
                    avg[j] += c.GetElementByLocalIndex(i)[j];
                }
            }
            for (int i = 0; i < avg.Length; i++)
            {
                avg[i] /= c.Size;
            }
            //ищем ближайшее к среднему
            double minDist = df(avg, c.GetElementByLocalIndex(0)), tmpDist;
            int ind = 0;
            for (int i = 0; i < c.Size; i++)
            {
                tmpDist = df(avg, c.GetElementByLocalIndex(i));
                if (tmpDist < minDist)
                {
                    minDist = tmpDist;
                    ind = i;
                }
            }
            return c.GetElementByLocalIndex(ind);
        }

        /// <summary>
        /// Building a graph on cluster
        /// </summary>
        /// <param name="c">input cluster</param>
        /// <param name="df">distance function</param>
        /// <returns>a graph as collection of Edges</returns>
        public static ICollection<Edge> GetEdgesForCluster(Cluster c, distanceFunction df)
        {
            List<Edge> edgesRes = new List<Edge>();
            for (int i = 0; i < c.Size; i++)
            {
                int tmpV = i == 0 ? c.Size - 1 : 0;
                double minDist = df(c.GetElementByLocalIndex(i), c.GetElementByLocalIndex(tmpV));
                for (int j = 0; j < c.Size; j++)
                {
                    if (i != j)
                    {
                        double tmpDist = df(c.GetElementByLocalIndex(i), c.GetElementByLocalIndex(j));
                        if (tmpDist < minDist)
                        {
                            minDist = tmpDist;
                            tmpV = j;
                        }
                    }
                }
                Edge mEdge = new Edge(minDist, i, tmpV);
                if (!edgesRes.Contains(mEdge))
                {
                    edgesRes.Add(mEdge);
                }
            }
            return edgesRes;
        }

        /// <summary>
        /// Searching linked components
        /// </summary>
        /// <param name="edges">graph as a list of edges</param>
        /// <param name="indexes">a collection of global indexes</param>
        /// <returns>a collection of linked components represented as collections of global indexes</returns>
        public static ICollection<ICollection<int>> GetLinkedComponents(ICollection<Edge> edges, ICollection<int> indexes)
        {
            List<ICollection<int>> components = new List<ICollection<int>>();
            List<int> remainingIndexes = new List<int>(indexes);
            List<Edge> remainingEdges = new List<Edge>(edges);
            while (remainingIndexes.Count > 0)
            {
                int startInd = remainingIndexes[0];
                List<int> tmpInd = DeepGraphSearch(remainingEdges, remainingIndexes, startInd);
                components.Add(tmpInd);
            }
            return components;
        }

        /// <summary>
        /// Depth graph search for all linked vertexes
        /// </summary>
        /// <param name="freeEdges">unused edges</param>
        /// <param name="freeIndexes">unsued indexes</param>
        /// <param name="startInd">an index to start search</param>
        /// <returns>found indexes</returns>
        public static List<int> DeepGraphSearch(List<Edge> freeEdges, List<int> freeIndexes, int startInd)
        {
            ICollection<Edge> tmp;
            List<int> res = new List<int>(GetAllLinkedInds(freeEdges, startInd, out tmp));
            freeIndexes.Remove(startInd);
            for (int i = 0; i < res.Count; i++)
            {
                freeIndexes.Remove(res[i]);
            }
            foreach (Edge e in tmp)
            {
                freeEdges.Remove(e);
            }
            List<List<int>> tmpR = new List<List<int>>();
            for (int i = 0; i < res.Count; i++)
            {
                tmpR.Add(DeepGraphSearch(freeEdges, freeIndexes, res[i]));
            }
            res.Add(startInd);
            for (int i = 0; i < tmpR.Count; i++)
            {
                for (int j = 0; j < tmpR[i].Count; j++)
                {
                    if (!res.Contains(tmpR[i][j]))
                    {
                        res.Add(tmpR[i][j]);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Getting all vertex indexes that are connected to vertex with inputed index
        /// </summary>
        /// <param name="edges">a list of all unused edges</param>
        /// <param name="index">start vertex index</param>
        /// <param name="usedEdges">output index of edges used in search</param>
        /// <returns>a collection of connected vertex indexes</returns>
        public static ICollection<int> GetAllLinkedInds(ICollection<Edge> edges, int index, out ICollection<Edge> usedEdges)
        {
            List<int> res = new List<int>();
            List<Edge> used = new List<Edge>();
            foreach (var e in edges)
            {
                if (e.ContainsV(index))
                {
                    if (!res.Contains(e.OtherSide(index)))
                    {
                        res.Add(e.OtherSide(index));
                    }
                    used.Add(e);
                }
            }
            usedEdges = used;
            return res;
        }

        public static Cluster TrivialConversionFunction(distanceFunction xDist, Cluster xSpc, double[] pars)
        {
            return xSpc;
        }

        /// <summary>
        /// Finding k closest elements to the element with known local index
        /// </summary>
        /// <param name="localPointInd">Local index of cluster element</param>
        /// <param name="xDist">distance function for </param>
        /// <param name="xSpc">input cluster</param>
        /// <param name="k">a number of closest element to search</param>
        /// <param name="dists">out distances</param>
        /// <returns>a collection of k closest elements as arrays of double values</returns>
        private static ICollection<double[]> FindClosest(int localPointInd, distanceFunction xDist, Cluster xSpc, int k, out double[] dists)
        {
            List<double[]> res = new List<double[]>();
            List<double> dist = new List<double>();
            double[] pt = xSpc.GetElementByLocalIndex(localPointInd);
            for (int i = 0; i < xSpc.Size; i++)
            {
                if (i != localPointInd)
                {
                    double[] tmp = xSpc.GetElementByLocalIndex(i);
                    double tmpDist = xDist(tmp, pt);
                    if (res.Count == 0)
                    {
                        res.Add(tmp);
                        dist.Add(tmpDist);
                    }
                    else
                    {
                        int j = 0;
                        for (j = dist.Count - 1; j >= 0; j--)
                        {
                            if (tmpDist > dist[j] || j==0)
                            {
                                break;
                            }
                        }
                        dist.Insert(j, tmpDist);
                        res.Insert(j, tmp);
                    }
                    if (res.Count > k)
                    {
                        res.RemoveAt(res.Count - 1);
                        dist.RemoveAt(dist.Count - 1);
                    }
                }
            }
            dists = dist.ToArray();
            return res;
        }

        public static Cluster DensityConversionFunction(distanceFunction xDist, Cluster xSpc, double[] pars)
        {
            List<double[]> resData = new List<double[]>();
            List<int> resInds = new List<int>();
            int k = (int)pars[0];
            double alpha = pars[1];
            for (int i = 0; i < xSpc.Size; i++)
            { 
                double[] dst;
                List<int> checkInds = new List<int>();
                List<double[]> pts = new List<double[]>(FindClosest(i, xDist, xSpc, k, out dst));
                //фильтруем
                for (int j = 0; j < dst.Length; j++)
                {
                    for (int l = 0; l < dst.Length; l++)
                    {
                        double mx = Math.Max(dst[l], dst[j]), mn = Math.Min(dst[l], dst[j]);
                        if ((l != j) && (mx/mn>alpha))
                        { 
                            double distLJ = xDist(pts[j], pts[l]);
                            double mxL = Math.Max(dst[l], distLJ), mnL = Math.Min(dst[l], distLJ);
                            double mxJ = Math.Max(dst[j], distLJ), mnJ = Math.Min(dst[j], distLJ);
                            if ((mxL / mnL > alpha) || (mxJ / mnJ > alpha))
                            {
                                if (dst[j] > dst[l])
                                {
                                    checkInds.Add(j);
                                }
                                else
                                {
                                    checkInds.Add(l);
                                }
                            }
                        }
                    }
                }
                //итог фильтрации
                List<double> resDstI = new List<double>();
                for (int j = 0; j < dst.Length; j++)
                { 
                    if(!checkInds.Contains(j))
                    {
                        resDstI.Add(dst[j]);
                    }
                }
                //подводим статистику
                double avg = AvgVal(resDstI);
                double meanSq = MeanSq(avg, resDstI);
                double median = Median(resDstI);
                double disp = Dispersion(resDstI);
                //заполняем
                resData.Add(new double[] { avg, meanSq, median, disp });
                resInds.Add(xSpc.GetGlobalIndexByLocal(i));
            }
            Cluster res = new Cluster(resInds.ToArray(), resData);
            return res;
        }

        public static double DensityDistance(double[] a, double[] b)
        {
            if (a.Length != 4 || b.Length != 4)
            {
                throw new ArgumentException("Неправильная размерность аргументов");
            }
            double avgD = Math.Abs(a[0] - b[0]);
            double res=0;
            if (avgD > a[1])
            {
                res += Math.Abs(avgD - a[1]);
            }
            if (avgD > b[1])
            {
                res += Math.Abs(avgD - b[1]);
            }
            res /= 2;
            res += Math.Abs(a[2] - b[2]);
            res += Math.Sqrt(Math.Abs(a[3] - b[3]));
            return res;
        }

        private static double AvgVal(ICollection<double> data)
        {
            double res = 0;
            foreach (var d in data)
            {
                res += d;
            }
            res /= data.Count;
            return res;
        }

        private static double MeanSq(double avg, ICollection<double> data)
        {
            int n = data.Count;
            double s = 0;
            foreach (var d in data)
            {
                s += (d - avg) * (d - avg);
            }
            s /= n;
            return Math.Sqrt(s);
        }

        private static double Median(ICollection<double> data)
        {
            double res;
            List<double> tmp = new List<double>(data);
            tmp.Sort();
            if (tmp.Count == 1)
            {
                res = tmp[0];
            }
            else
            {
                if (tmp.Count % 2 == 1)
                {
                    res = tmp[tmp.Count / 2];
                }
                else
                {
                    res = (tmp[tmp.Count / 2] + tmp[tmp.Count / 2 - 1]) / 2;
                }
            }
            return res;
        }

        private static double Dispersion(ICollection<double> data)
        {
            double avg = 0, avgSq=0;
            foreach (var d in data)
            {
                avg += d;
                avgSq += d * d;
            }
            avg /= data.Count;
            avgSq /= data.Count;
            return avgSq - avg * avg;
        }

        public static float SameClusterBrute(Cluster y1, Cluster y2, distanceFunction yDist, clusterCenterFunction cnf, double[] cnfPars)
        {
            double[] center1 = cnf(y1, yDist, cnfPars), center2 = cnf(y2, yDist, cnfPars);
            double max1 = yDist(center1, y1.GetElementByLocalIndex(0)), max2 = yDist(center2, y2.GetElementByLocalIndex(0));
            double tmpDist;
            for (int i = 0; i < y1.Size; i++)
            {
                tmpDist = yDist(y1.GetElementByLocalIndex(i), center1);
                if (tmpDist > max1)
                {
                    max1 = tmpDist;
                }
            }

            for (int i = 0; i < y2.Size; i++)
            {
                tmpDist = yDist(y2.GetElementByLocalIndex(i), center2);
                if (tmpDist > max2)
                {
                    max2 = tmpDist;
                }
            }
            int counter1 = 0, counter2 = 0;
            for (int i = 0; i < y1.Size; i++)
            {
                tmpDist = yDist(y1.GetElementByLocalIndex(i), center2);
                if (tmpDist < max1)
                {
                    counter1++;
                }
            }
            for (int i = 0; i < y2.Size; i++)
            {
                tmpDist = yDist(y2.GetElementByLocalIndex(i), center1);
                if (tmpDist < max2)
                {
                    counter2++;
                }
            }
            //подсчёт
            float pre1 = ((float)counter1) / y1.Size, pre2 = ((float)counter2) / y2.Size;
            return pre1 * pre2;
        }

        public static float SimilarClusterBrute(Cluster y1, Cluster y2, distanceFunction yDist, clusterCenterFunction cnf, double[] cnfPars)
        {
            double[] center1 = cnf(y1, yDist, cnfPars), center2 = cnf(y2, yDist, cnfPars);
            double max1 = yDist(center1, y1.GetElementByLocalIndex(0)), max2 = yDist(center2, y2.GetElementByLocalIndex(0));
            double tmpDist;
            for (int i = 0; i < y1.Size; i++)
            {
                tmpDist = yDist(y1.GetElementByLocalIndex(i), center1);
                if (tmpDist > max1)
                {
                    max1 = tmpDist;
                }
            }

            for (int i = 0; i < y2.Size; i++)
            {
                tmpDist = yDist(y2.GetElementByLocalIndex(i), center2);
                if (tmpDist > max2)
                {
                    max2 = tmpDist;
                }
            }
            double cDist = yDist(center1, center2);
            double maxAvg = (max1 + max2) / 2;
            if (cDist < maxAvg)
            {
                return 1.0f;
            }
            else
            {
                return (float)maxAvg / (float)cDist;
            }
        }

        public static ICollection<Cluster> SplitClusterTrivial(Cluster k1, distanceFunction yDist, clusterCenterFunction cnf, double[] cnfPars, double[] splitPars)
        {
            return new Cluster[] { k1 };
        }

        public static ICollection<Cluster> SplitClusterGraph(Cluster k1, distanceFunction yDist, clusterCenterFunction cnf, double[] cnfPars, double[] splitPars)
        {
            List<Edge> edges = new List<Edge>(GetEdgesForCluster(k1, yDist));
            List<int> inds = new List<int>();
            for (int i = 0; i < k1.Size; i++)
            {
                inds.Add(i);
            }
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i].Distance > splitPars[0])
                {
                    edges.RemoveAt(i);
                    i--;
                }
            }
            List<ICollection<int>> components = new List<ICollection<int>>(GetLinkedComponents(edges, inds));
            List<Cluster> res = new List<Cluster>();
            for (int i = 0; i < components.Count; i++)
            {
                List<double[]> data = new List<double[]>();
                List<int> gInds = new List<int>();
                for (int j = 0; j < components[i].Count; j++)
                {
                    data.Add(k1.GetElementByLocalIndex(components[i].ElementAt(j)));
                    gInds.Add(k1.GetGlobalIndexByLocal(components[i].ElementAt(j)));
                }
                res.Add(new Cluster(gInds.ToArray(), data));
            }
            return res;
        }

        public static float ComapreClustersBySize(Cluster k1, Cluster k2, distanceFunction yDist, clusterCenterFunction cnf, double[] cnfPars)
        {
            return k1.Size - k2.Size;
        }
    }
}
