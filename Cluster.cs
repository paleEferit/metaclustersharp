using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clusterising
{
    public class Cluster
    {
        private int dimensions;
        private List<int> indexes;
        private List<double[]> data;

        //TODO: методы добавления/удаления элементов по локальным и глобальным индекса

        private Cluster()
        { 
        
        }

        public Cluster(int[] indexes, ICollection<double[]> dataSecondarySpace)
        {
            if (indexes.Length != dataSecondarySpace.Count)
            {
                throw new ArgumentException("Набор индексов и набор данных не совпадают по размеру");
            }
            /*if (indexes.Length == 0)
            {
                throw new ArgumentException("Кластер не может быть пустым");
            }*/
            int c = indexes.Length;
            int d = dataSecondarySpace.ElementAt(0).Length;
            for (int i = 0; i < c; i++)
            {
                if (d != dataSecondarySpace.ElementAt(i).Length)
                {
                    throw new ArgumentException("Нестабильная размерность вторичного пространства признаков");
                }
            }
            this.dimensions = d;
            this.data = new List<double[]>(dataSecondarySpace);
            this.indexes = new List<int>(indexes);
        }

        public Cluster(int dimensions)
        {
            this.indexes = new List<int>();
            this.data = new List<double[]>();
            this.dimensions = dimensions;
        }

        /// <summary>
        /// прикрепляет содержимое другого кластера к текущему
        /// </summary>
        /// <param name="c">добавляемый кластер</param>
        public void Append(Cluster c)
        {
            if (this.Dimensions != c.Dimensions)
            {
                throw new ArgumentException("несовпадение размерности");
            }
            for (int i = 0; i < c.Size; i++)
            {
                this.AddElement(c.GetElementByLocalIndex(i), c.GetGlobalIndexByLocal(i));
            }
        }

        /// <summary>
        /// набор глобальных индексов кластера
        /// </summary>
        public int[] Indexes
        {
            get 
            { 
                return this.indexes.ToArray();
            }
        }

        public void AddElement(double[] vals, int globalIndex)
        {
            if (vals.Length != this.Dimensions)
            {
                throw new ArgumentException("несовпадение в количестве метрик");
            }
            this.data.Add(vals);
            this.indexes.Add(globalIndex);
        }

        public void DeleteElementByLocalIndex(int localIndex)
        {
            this.indexes.RemoveAt(localIndex);
            this.data.RemoveAt(localIndex);
        }

        public void DeleteElementByGlobalIndex(int globalIndex)
        {
            int localIndex = this.FindLocalIndexByGlobal(globalIndex);
            this.indexes.RemoveAt(localIndex);
            this.data.RemoveAt(localIndex);
        }

        public int Size
        {
            get
            {
                return this.indexes.Count;
            }
        }

        public int Dimensions
        {
            get
            {
                return this.dimensions;
            }
        }

        public double[] GetElementByLocalIndex(int localIndex)
        {
            return this.data[localIndex];
        }

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

        public double[] GetElementByGlobalIndex(int globalIndex)
        {
            int ind = this.FindLocalIndexByGlobal(globalIndex);
            return GetElementByLocalIndex(ind);
        }

        public int GetGlobalIndexByLocal(int localIndex)
        {
            return this.indexes[localIndex];
        }
    }
}
