using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Genetic
{



    public delegate double GAFunction(params double[] values);
    public delegate void OnGeneration(GeneticAlgoritm ga);

    public class GeneticAlgoritm : IGeneticAlgorithm
    {
        public GeneticAlgorithmParams GAParams { get; set; }
        public bool Stop { get; set; }
        private DateTime _lastEpoch = DateTime.Now;

        //DEFAULT constructor
        //TODO: ADD size, crpssoverrate constructors too
        public GeneticAlgoritm()
        {
            InitialValues();
            GAParams.MutationRate = 0.05;
            GAParams.CrossoverRate = 0.80;
            GAParams.PopulationSize = 100;
            GAParams.GenomeSize = 2000;
        }

        public void InitialValues()
        {
            GAParams = new GeneticAlgorithmParams();
            GAParams.Elitism = false;
        }


        //Genome execution
        public void Go(bool resume = false)
        {

            if (getFitness == null)
                throw new ArgumentNullException("Fitness not included");
            if (GAParams.GenomeSize == 0)
                throw new IndexOutOfRangeException("Genome size not set");

            Genome.MutationRate = GAParams.MutationRate;

            if (!resume)
            {
                //Create the fitness table
                GAParams.FitnessTable = new List<double>();
                GAParams.ThisGeneration = new List<Genome>(GAParams.Generations);
                GAParams.NextGeneration = new List<Genome>(GAParams.Generations);
                GAParams.TotalFitness = 0;
                GAParams.TargetFitness = 0;
                GAParams.TargetFitnessCount = 0;
                GAParams.CurrentGeneration = 0;
                Stop = false;

                CreateGenomes();
                RankPopulation();
            }

            while (GAParams.CurrentGeneration < GAParams.Generations && !Stop)
            {
                CreateNextGeneration();
                double fitness = RankPopulation();

                if (GAParams.CurrentGeneration % 100 == 0)
                {
                    Console.WriteLine("Generation " + GAParams.CurrentGeneration + ", Time: " + Math.Round((DateTime.Now - _lastEpoch).TotalSeconds, 2) + "s, Best Fitness: " + fitness);

                    if (GAParams.HistoryPath != "")
                    {
                        //Log timeline
                        File.AppendAllText(GAParams.HistoryPath, DateTime.Now.ToString() + "," + fitness + "," + GAParams.TargetFitness + "," + GAParams.CurrentGeneration + "\r\n");
                    }

                    _lastEpoch = DateTime.Now;
                }

                if (GAParams.TargetFitness > 0 && fitness >= GAParams.TargetFitness)
                {
                    if (GAParams.TargetFitnessCount++ > 500)
                        break;
                }
                else
                {
                    GAParams.TargetFitnessCount = 0;
                }

                if (OnGenerationFunction != null)
                {
                    OnGenerationFunction(this);
                }

                GAParams.CurrentGeneration++;
            }


        }

        private int SelectOne()
        {
            double randomFitness = my_rand.NextDouble() * (GAParams.FitnessTable[GAParams.FitnessTable.Count - 1] == 0 ? 1 : GAParams.FitnessTable[GAParams.FitnessTable.Count - 1]);
            int idx = -1;
            int mid;
            int first = 0;
            int last = GAParams.PopulationSize - 1;
            mid = (last - first) / 2;

            //TODO: find better solution for this
            while (idx == -1 && first <= last)
            {
                if (randomFitness < GAParams.FitnessTable[mid])
                {
                    last = mid;
                }
                else if (randomFitness > GAParams.FitnessTable[mid])
                {
                    first = mid;
                }
                mid = (first + last) / 2;
                //between i and i+1
                if ((last - first) == 1)
                    idx = last;
            }
            return idx;
        }

        private double RankPopulation()
        {
            GAParams.TotalFitness = 0.0;

            //Calculate fitness for each genome
            Parallel.ForEach(GAParams.ThisGeneration, (g) =>
            {
                g.Fitness = FitnessFunction(g.Genes());
                GAParams.TotalFitness += g.Fitness;
            });

            GAParams.ThisGeneration.Sort(delegate (Genome x, Genome y) { return Comparer<double>.Default.Compare(x.Fitness, y.Fitness); });

            //sorted in order of fitness
            double fitness = 0.0;
            GAParams.FitnessTable.Clear();

            foreach (Genome t in GAParams.ThisGeneration)
            {
                fitness += t.Fitness;
                GAParams.FitnessTable.Add(t.Fitness);
            }

            return GAParams.FitnessTable[GAParams.FitnessTable.Count - 1];
        }

        private void CreateGenomes()
        {
            for (int i = 0; i < GAParams.PopulationSize; i++)
            {
                Genome g = new Genome(GAParams.GenomeSize);
                GAParams.ThisGeneration.Add(g);
            }
        }

        private void CreateNextGeneration()
        {
            GAParams.NextGeneration.Clear();
            Genome g = null, g2 = null;
            int length = GAParams.PopulationSize;

            if (GAParams.Elitism)
            {
                g = GAParams.ThisGeneration[GAParams.PopulationSize - 1].DeepCopy();
                g.age = GAParams.ThisGeneration[GAParams.PopulationSize - 1].age;
                g2 = GAParams.ThisGeneration[GAParams.PopulationSize - 2].DeepCopy();
                g2.age = GAParams.ThisGeneration[GAParams.PopulationSize - 2].age;

                length -= 2;
            }

            for (int i = 0; i < length; i += 2)
            {
                int pidx1 = SelectOne();
                int pidx2 = SelectOne();
                Genome parent1, parent2, child1, child2;
                parent1 = GAParams.ThisGeneration[pidx1];
                parent2 = GAParams.ThisGeneration[pidx2];

                if (my_rand.NextDouble() < GAParams.CrossoverRate)
                {
                    parent1.Crossover(ref parent2, out child1, out child2);
                }
                else
                {
                    child1 = parent1;
                    child2 = parent2;
                }
                child1.Mutate();
                child2.Mutate();

                GAParams.NextGeneration.Add(child1);
                GAParams.NextGeneration.Add(child2);
            }

            if (GAParams.Elitism && g != null)
            {
                if (g2 != null)
                    GAParams.NextGeneration.Add(g2);
                if (g != null)
                    GAParams.NextGeneration.Add(g);
            }

            //Expand genomes
            if (GAParams.NextGeneration[0].Length != GAParams.GenomeSize)
            {
                Parallel.ForEach(GAParams.NextGeneration, (genome) =>
                {
                    if (genome.Length != GAParams.GenomeSize)
                    {
                        genome.Expand(GAParams.GenomeSize);
                    }
                });
            }

            GAParams.ThisGeneration = new List<Genome>(GAParams.NextGeneration);
        }

        public void Save(string fileName)
        {
            //TODO: Add saving here
        }

        public void Load(string fileName)
        {
            //TODO: Add loading logic here
        }

        public void Resume(GAFunction fitnessFunc, OnGeneration onGenerationFunc)
        {
            FitnessFunction = fitnessFunc;
            OnGenerationFunction = onGenerationFunc;

            Go(true);
        }

        static Random my_rand = new Random((int)DateTime.Now.Ticks);

        static private GAFunction getFitness;

        public GAFunction FitnessFunction
        {
            get
            {
                return getFitness;
            }
            set
            {
                getFitness = value;
            }
        }

        public OnGeneration OnGenerationFunction;

        public void GetBest(out double[] values, out double fitness)
        {
            Genome g = GAParams.ThisGeneration[GAParams.PopulationSize - 1];
            values = new double[g.Length];
            g.GetValues(ref values);
            fitness = g.Fitness;
        }

        public void GetWorst(out double[] values, out double fitness)
        {
            GetNthGenome(0, out values, out fitness);
        }

        public void GetNthGenome(int n, out double[] values, out double fitness)
        {
            if (n < 0 || n > GAParams.PopulationSize - 1)
                throw new ArgumentOutOfRangeException("n too large, or too small");

            Genome g = GAParams.ThisGeneration[n];
            values = new double[g.Length];
            g.GetValues(ref values);
            fitness = g.Fitness;
        }

        public void SetGenomes(int x, double[] stats, double fitness)
        {
            if (x < 0 || x > GAParams.PopulationSize - 1)
            {
                throw new ArgumentOutOfRangeException("x too large, or too small");
            }
            Genome g = GAParams.ThisGeneration[x];
            g.m_genes = stats;
            g.Fitness = fitness;
            GAParams.ThisGeneration[x] = g;
        }
    }

}