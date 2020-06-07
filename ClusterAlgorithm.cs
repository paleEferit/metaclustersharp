using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clusterising
{
    //больше делегатов богу делегатов!
    /// <summary>
    /// функция расстояния для пространства с сырыми метриками и для пространства с обработанными метриками раздельно
    /// </summary>
    /// <param name="x1">первая точка пространства</param>
    /// <param name="x2">вторая точка пространства</param>
    /// <returns>вещественное число, выражающее расстояние между точками</returns>
    public delegate double distanceFunction(double[] x1, double[] x2); 
    
    /// <summary>
    /// функция ищет центр кластера
    /// </summary>
    /// <param name="c">кластер</param>
    /// <param name="df">функция расстояния (может и не понадобиться)</param>
    /// <param name="pars">дополнительные параметры</param>
    /// <returns>напор переменных из поля размерности кластера, отражающие его центр</returns>
    public delegate double[] clusterCenterFunction(Cluster c, distanceFunction df, double[] pars);

    /// <summary>
    /// функция преобразования n-мерного исходного пространства метрик в k-мерное пространство признаков, может использовать функцию расстояния
    /// </summary>
    /// <param name="xDist">функция расстояния для исходного пространства</param>
    /// <param name="xSpc">исходное пространство</param>
    /// <param name="pars">дополнительные параметры преобразования</param>
    /// <returns>новое пространство</returns>
    public delegate Cluster spaceConversionFunction(distanceFunction xDist, Cluster xSpc, double[] pars); 

    /// <summary>
    /// функция принадлежности по ближайшему соседу, формально нечёткая, зависит от функции расстояния для пространства признаков
    /// </summary>
    /// <param name="y1">первая точка нового пространства</param>
    /// <param name="y2">вторая точка нового пространства</param>
    /// <param name="yDist">функция расстояния на новом пространстве</param>
    /// <param name="cnf">функция поиска центра кластера</param>
    /// <param name="cnfPars">параметры функции поиска центра кластера</param>
    /// <returns>степень сходства от 0 до 1 по входящим объектам</returns>
    public delegate float sameCluster(Cluster y1, Cluster y2, distanceFunction yDist, clusterCenterFunction cnf, double[] cnfPars);
    
    /// <summary>
    /// функция для объединения кластеров, формально нечёткая (степень сходства кластеров), зависит от функции расстояния для пространства признаков
    /// </summary>
    /// <param name="k1">первый кластер</param>
    /// <param name="k2">второй кластер</param>
    /// <param name="yDist">функция расстояния в новом пространстве</param>
    /// <param name="cnf">функция поиска центра кластера</param>
    /// <param name="cnfPars">параметры функции поиска центра кластера</param>
    /// <returns>степень сходства от 0 до 1 по расстоянию</returns>
    public delegate float similarClusters(Cluster k1, Cluster k2, distanceFunction yDist, clusterCenterFunction cnf, double[] cnfPars);

    /// <summary>
    /// функция разделения кластера на два наименее схожих между собой, зависит от функции расстояния для пространства признаков
    /// </summary>
    /// <param name="k1">исходный кластер</param>
    /// <param name="yDist">функция расстояния на вторичном пространстве</param>
    /// <param name="cnf">функция поиска центра кластера</param>
    /// <param name="cnfPars">параметры функции поиска центра кластера</param>
    /// <param name="splitPars">параметры функции разделения</param>
    /// <returns>набор кластеров- результатов разделения</returns>
    public delegate ICollection<Cluster> splitCluster(Cluster k1, distanceFunction yDist, clusterCenterFunction cnf, double[] cnfPars, double[] splitPars);
    /* 0*/
    /// <summary>
    /// функция для сравнения/сортировки кластеров, 
    /// </summary>
    /// <param name="k1">первый кластер</param>
    /// <param name="k2">второй кластер</param>
    /// <param name="yDist">функция расстояния на вторичном пространстве</param>
    /// <param name="cnf">функция поиска центра кластера</param>
    /// <param name="cnfPars">параметры функции поиска центра кластера</param>
    /// <returns>число больше 0, если k1>k2 и меньше 0 в противном случае, при полном совпадении должна возвращать 0</returns>
    public delegate float comapreClusters(Cluster k1, Cluster k2, distanceFunction yDist, clusterCenterFunction cnf, double[] cnfPars);

    /// <summary>
    /// Абстракция над алгоритмами кластеризации.
    /// </summary>
    public class ClusterAlgorithm
    {
        /// <summary>
        /// тривиальна ли функция дробления кластеров
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
        /// <param name="xDist">функция расстояния на исходном пространстве</param>
        /// <param name="yDist">функция расстояния на преобразованном пространстве</param>
        /// <param name="xyConv">функция преобразования пространства (выделение признаков)</param>
        /// <param name="comparePointFunction">функция сопоставления кластеров по ближайшим взаимно входящим точкам (константно -1 чтобы тривиализовать)</param>
        /// <param name="clusterFusionCriteria">функция сопоставления клстеров по расстояниям от центров (константно -1 чтобы тривиализовать)</param>
        /// <param name="clusterDivider">функция разделения кластера на два наиболее различных (константно входной и пустой на выход чтобы тривиализовать)</param>
        /// <param name="clusterSortCriteria">функция сравнения кластеров для их сортировки (наименьшие присоединяют, наибольшие делят)</param>
        /// <param name="cnt">функция поиска центра кластера</param>
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

        //TODO: обобщённый алгоритм кластеризации, добавить учёт тривиализаций
        /// <summary>
        /// метаалгоритм кластеризации
        /// </summary>
        /// <param name="data">входные даннные</param>
        /// <param name="convesionParams">параметры преобразования входных данных</param>
        /// <param name="centerClusterParams">параметры поиска центра кластера</param>
        /// <param name="uniteThresholdRatio">соотношение расстояний между текущим шагом и предыдущим, превышение которого означает остановку объединения</param>
        /// <param name="initialWorkSectorRatio">доля точек пространства, подлежащих кластеризации на первом этапе (от 0 до 1)</param>
        /// <param name="targetClusterCount">количество целевых кластеров. Не определено при значении -1</param>
        /// <returns></returns>
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
            /*преобразование пространств*/
            Cluster realData = this.xyConv(this.xDist, data, convesionParams);
            /*делаем выборку*/
            List<int> remainingIndexes = new List<int>(realData.Indexes), sectorIndexes = new List<int>();
            int countStartPartition = (int)Math.Round(remainingIndexes.Count * initialWorkSectorRatio);
            Random r = new Random(DateTime.Now.Millisecond);
            int tmpIndVal, tmpLstInd;
            //просеиваем индексы
            while (countStartPartition > 0)
            {
                tmpLstInd = r.Next(remainingIndexes.Count);
                tmpIndVal = remainingIndexes[tmpLstInd];
                sectorIndexes.Add(tmpIndVal);
                remainingIndexes.RemoveAt(tmpLstInd);
                countStartPartition--;
            }
            //заполняем
            double[][] selArr = new double[sectorIndexes.Count][];
            List<int> unusedLocalInds = new List<int>();
            for(int i = 0; i<selArr.Length; i++)
            {
                selArr[i]=realData.GetElementByGlobalIndex(sectorIndexes[i]);
                unusedLocalInds.Add(i);
            }
            Cluster selectedData = new Cluster(sectorIndexes.ToArray(), selArr);
            /*выборка завершена, время разделять/объединять кластеры*/
            //создаём парные микрокластеры
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
            //первый проход по расстояниям
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
                    // смещение индексов
                    j--;
                    if (j < i)
                    {
                        i--;
                    }
                }
            }
            //первый проход по соседям
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
                    // смещение индексов
                    j--;
                    if (j < i)
                    {
                        i--;
                    }
                }
            }
            /*добавление не попавших в подвыборку*/
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

            //поискать в цикле по новым кластерам самое большое расстояние от центра до входящего, будет сравниваться по коэффициенту
            double maxDist = GetMaxCenterToMemberDist(res, centerClusterParams);
            //обновляемое для соотношения
            double nMaxDist = maxDist;
            /*объединение до упора*/
            int oldClusterCount = selectedData.Size, newClusterCount = res.Count;
            while ((((targetClusterCount != -1) && (res.Count > targetClusterCount)) || (nMaxDist/maxDist<uniteThresholdRatio)) && (newClusterCount!=oldClusterCount))
            {
                oldClusterCount = newClusterCount;
                //сохранение старого значения
                maxDist = nMaxDist;
                //по центральным расстояниям
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
                        // смещение индексов
                        j--;
                        if (j < i)
                        {
                            i--;
                        }
                    }
                }
                //по соседям
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
                        // смещение индексов
                        j--;
                        if (j < i)
                        {
                            i--;
                        }
                    }
                }
                //поиск максимума в новых кластерах
                nMaxDist = GetMaxCenterToMemberDist(res, centerClusterParams);
                newClusterCount = res.Count;
            }
            maxDist = nMaxDist;
            oldClusterCount = newClusterCount-1;
            /*разбиение до упора*/
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
            //финальная конверсия
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
