using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaclustersharp
{
    /// <summary>
    /// Unoriented graph edge
    /// </summary>
    public class Edge
    {
        private int ind1, ind2;
        private double dist;

        private Edge()
        {

        }

        /// <summary>
        /// Edge constructor
        /// </summary>
        /// <param name="d">edge length</param>
        /// <param name="v1">first vertex index</param>
        /// <param name="v2">second vertex index</param>
        public Edge(double d, int v1, int v2)
        {
            this.dist = d;
            this.ind1 = v1;
            this.ind2 = v2;
        }

        /// <summary>
        /// Getting edge length
        /// </summary>
        public double Distance
        {
            get
            {
                return this.dist;
            }
        }

        /// <summary>
        /// Gettig first vertex index
        /// </summary>
        public int V1
        {
            get
            {
                return ind1;
            }
        }

        /// <summary>
        /// Getting second vertex index
        /// </summary>
        public int V2
        {
            get
            {
                return ind2;
            }
        }

        /// <summary>
        /// Check if edge has vertex
        /// </summary>
        /// <param name="ind">vertex index to check</param>
        /// <returns></returns>
        public bool ContainsV(int ind)
        {
            return this.ind2 == ind || this.ind1 == ind;
        }

        /// <summary>
        /// Getting opposite vertex to the input
        /// </summary>
        /// <param name="v">Index of vertex for the opposite side</param>
        /// <returns></returns>
        public int OtherSide(int v)
        {
            if (!this.ContainsV(v))
            {
                throw new ArgumentException("No such vertex");
            }
            if (v == this.ind1)
            {
                return ind2;
            }
            else
            {
                return ind1;
            }
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash += hash * 20 + 8 * (this.ind1 + this.ind2);
            hash += hash * 99 + (int)(this.dist * 1001);
            return hash;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || !(obj is Edge) || (this.GetHashCode() != obj.GetHashCode()))
            {
                return false;
            }
            Edge e = (Edge)obj;
            return (this.ContainsV(e.V1)) && (this.ContainsV(e.V2));
        }
    }
}
