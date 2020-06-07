using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clusterising
{
    /// <summary>
    /// Cluster of data. Each element of cluster is represented as array of double values. 
    /// All elements have same dimension size. 
    /// All elements have global indexes i.e. local indexes for cluster zero (input data).
    /// Local index is element index in data field.
    /// </summary>
    public class Cluster
    {
        private int dimensions;
        private List<int> indexes;
        private List<double[]> data;


        private Cluster()
        { 
        
        }

        /// <summary>
        /// Creates a cluster by array of global element indexes and corresponding list of elements as data. Can be used for creating subclusters
        /// </summary>
        /// <param name="indexes">a list of global indexes</param>
        /// <param name="dataSecondarySpace"></param>
        public Cluster(int[] indexes, ICollection<double[]> dataSecondarySpace)
        {
            if (indexes.Length != dataSecondarySpace.Count)
            {
                throw new ArgumentException("Size of index array does not match size of elelment collection");
            }
            int c = indexes.Length;
            int d = dataSecondarySpace.ElementAt(0).Length;
            for (int i = 0; i < c; i++)
            {
                if (d != dataSecondarySpace.ElementAt(i).Length)
                {
                    throw new ArgumentException("Data dimensions are not unified");
                }
            }
            this.dimensions = d;
            this.data = new List<double[]>(dataSecondarySpace);
            this.indexes = new List<int>(indexes);
        }

        /// <summary>
        /// Creates an empty cluster with required element dimensions
        /// </summary>
        /// <param name="dimensions">element dimension count</param>
        public Cluster(int dimensions)
        {
            this.indexes = new List<int>();
            this.data = new List<double[]>();
            this.dimensions = dimensions;
        }

        /// <summary>
        /// adding contents of input cluster to current one
        /// </summary>
        /// <param name="c">appended cluster</param>
        public void Append(Cluster c)
        {
            if (this.Dimensions != c.Dimensions)
            {
                throw new ArgumentException("dimension mismatch");
            }
            for (int i = 0; i < c.Size; i++)
            {
                this.AddElement(c.GetElementByLocalIndex(i), c.GetGlobalIndexByLocal(i));
            }
        }

        /// <summary>
        /// Getting global indexes for this cluster elements
        /// </summary>
        public int[] Indexes
        {
            get 
            { 
                return this.indexes.ToArray();
            }
        }

        /// <summary>
        /// Adding element to cluster
        /// </summary>
        /// <param name="vals">element data as array of double values</param>
        /// <param name="globalIndex">global index of added element</param>
        public void AddElement(double[] vals, int globalIndex)
        {
            if (vals.Length != this.Dimensions)
            {
                throw new ArgumentException("dimension mismatch");
            }
            this.data.Add(vals);
            this.indexes.Add(globalIndex);
        }

        /// <summary>
        /// Deletes cluster element by local index in current cluster
        /// </summary>
        /// <param name="localIndex">index in current cluster</param>
        public void DeleteElementByLocalIndex(int localIndex)
        {
            this.indexes.RemoveAt(localIndex);
            this.data.RemoveAt(localIndex);
        }

        /// <summary>
        ///  Deletes cluster element by global index
        /// </summary>
        /// <param name="globalIndex">global element index</param>
        public void DeleteElementByGlobalIndex(int globalIndex)
        {
            int localIndex = this.FindLocalIndexByGlobal(globalIndex);
            this.indexes.RemoveAt(localIndex);
            this.data.RemoveAt(localIndex);
        }

        /// <summary>
        /// Getting size of cluster (element count)
        /// </summary>
        public int Size
        {
            get
            {
                return this.indexes.Count;
            }
        }

        /// <summary>
        /// Getting dimension count
        /// </summary>
        public int Dimensions
        {
            get
            {
                return this.dimensions;
            }
        }

        /// <summary>
        /// Gets element by local index in current cluster
        /// </summary>
        /// <param name="localIndex">local element index</param>
        /// <returns></returns>
        public double[] GetElementByLocalIndex(int localIndex)
        {
            return this.data[localIndex];
        }

        /// <summary>
        /// Getting local index in current cluster by global element index 
        /// </summary>
        /// <param name="globalIndex">global index</param>
        /// <returns></returns>
        private int FindLocalIndexByGlobal(int globalIndex)
        {
            int res = -1;
            int c = indexes.Count;
            for (int i = 0; i < c; i++)
            {
                if (this.indexes[i] == globalIndex)
                {
                    res = i;
                    break;
                }
            }
            return res;
        }

        /// <summary>
        /// Getting element by global element index
        /// </summary>
        /// <param name="globalIndex"></param>
        /// <returns></returns>
        public double[] GetElementByGlobalIndex(int globalIndex)
        {
            int ind = this.FindLocalIndexByGlobal(globalIndex);
            return GetElementByLocalIndex(ind);
        }

        /// <summary>
        /// Getting global index by local index in current cluster
        /// </summary>
        /// <param name="localIndex">local element index</param>
        /// <returns></returns>
        public int GetGlobalIndexByLocal(int localIndex)
        {
            return this.indexes[localIndex];
        }
    }
}
