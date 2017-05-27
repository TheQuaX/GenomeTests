using System;
using System.Collections;
using System.Collections.Generic;

namespace Genetic
{

    [Serializable]
    public class Genome
    {
        public Genome()
        {
            //TODO: ADD constructor logic here
            Console.WriteLine("Genome created");
        }

        public Genome(int length)
        {
            m_length = length;
            m_genes = new double[length];
            CreateGenes();
        }

        public Genome(int length, bool createGenes)
        {
            m_length = length;
            m_genes = new double[length];
            if (createGenes)
                CreateGenes();
        }

        public Genome(ref double[] genes)
        {
            m_length = genes.Length;
            m_genes = new double[m_length];
            Array.Copy(genes, m_genes, m_length);
        }

        public Genome DeepCopy()
        {
            Genome g = new Genome(m_length, false);
            Array.Copy(m_genes, g.m_genes, m_length);
            return g;
        }

        private void CreateGenes()
        {
            for (int i = 0; i < m_genes.Length; i++)
                m_genes[i] = m_random.NextDouble();
        }

        public void Crossover(ref Genome genome2, out Genome child1, out Genome child2)
        {
            int pos = (int)(m_random.NextDouble() * (double)m_length);
            child1 = new Genome(m_length, false);
            child2 = new Genome(m_length, false);

            for (int i = 0; i < m_length; i++)
            {
                if (i < pos)
                {
                    child1.m_genes[i] = m_genes[i];
                    child2.m_genes[i] = genome2.m_genes[i];
                }
                else
                {
                    child1.m_genes[i] = genome2.m_genes[i];
                    child2.m_genes[i] = m_genes[i];
                }
            }
        }

        public void Mutate()
        {
            //go through each bit
            for (int pos = 0; pos < m_length; pos++)
            {
                //mutate?
                if (m_random.NextDouble() < m_mutationRate)
                {
                    //set mutation type
                    double r = m_random.NextDouble();
                    if (r <= 0.25)
                    {
                        //insert mutation
                        int mutationIndex = pos;

                        //copy of the current bit before we mutate it
                        double shiftBit = m_genes[mutationIndex];

                        //set random bit at mutation index
                        m_genes[mutationIndex] = m_random.NextDouble();

                        //bump bits up or down by 1
                        bool up = m_random.NextDouble() >= 0.5;
                        if (up)
                        {
                            //bump bits up by 1
                            for (int i = mutationIndex + 1; i < m_length; i++)
                            {
                                double nextShiftBit = m_genes[i];

                                m_genes[i] = shiftBit;

                                shiftBit = nextShiftBit;
                            }
                        }
                        else
                        {
                            //bump bits down by 1
                            for (int i = mutationIndex - 1; i >= 0; i--)
                            {
                                double nextShiftBit = m_genes[i];

                                m_genes[i] = shiftBit;

                                shiftBit = nextShiftBit;
                            }
                        }
                    }
                    else if (r <= 0.5)
                    {
                        //delete mutation
                        int mutationIndex = pos;

                        //bump bits up or down by 1
                        bool up = m_random.NextDouble() >= 0.5;
                        if (up)
                        {
                            //bump bits up by 1
                            for (int i = mutationIndex; i > 0; i--)
                            {
                                m_genes[i] = m_genes[i - 1];
                            }

                            //Add a new mutation bit at front of genome to replace the deleted one
                            m_genes[0] = m_random.NextDouble();
                        }
                        else
                        {
                            //bump bits down by 1
                            for (int i = mutationIndex; i < m_length - 1; i++)
                            {
                                m_genes[i] = m_genes[i + 1];
                            }

                            //Add a new mutation bit at end of genome to replace the deleted one.
                            m_genes[m_length - 1] = m_random.NextDouble();
                        }
                    }
                    else if (r <= 0.75)
                    {
                        //Shift/rotation mutation
                        //bump bits up or down by 1
                        bool up = m_random.NextDouble() >= 0.5;
                        if (up)
                        {
                            //bump bits up by 1. 1, 2, 3 => 3, 1, 2
                            double shiftBit = m_genes[0];

                            for (int i = 0; i < m_length; i++)
                            {
                                if (i > 0)
                                {
                                    //make a copy of the current bit
                                    double temp = m_genes[i];

                                    //set the current bit to the previous one
                                    m_genes[i] = shiftBit;

                                    //select the next bit to be copied
                                    shiftBit = temp;
                                }
                                else
                                {
                                    //wrap last bit to front.
                                    m_genes[i] = m_genes[m_length - 1];
                                }
                            }
                        }
                        else
                        {
                            //bump bits down by 1. 1, 2, 3 => 2, 3, 1
                            double shiftBit = m_genes[m_length - 1];

                            for (int i = m_length - 1; i >= 0; i--)
                            {
                                if (i < m_length - 1)
                                {
                                    //copy of the current bit
                                    double temp = m_genes[i];

                                    //set the current bit to the previous one
                                    m_genes[i] = shiftBit;

                                    //select the next bit to be copied
                                    shiftBit = temp;
                                }
                                else
                                {
                                    //wrap first bit to end
                                    m_genes[i] = m_genes[0];
                                }
                            }
                        }
                    }
                    else
                    {
                        //replacement mutation
                        //mutate bits
                        double mutation = m_random.NextDouble();
                        m_genes[pos] = mutation;
                    }
                }
            }
        }

        public void Expand(int size)
        {
            int originalSize = m_genes.Length;
            int difference = size - originalSize;

            //resize genome array
            double[] newGenes = new double[size];

            if (difference > 0)
            {
                if (m_random.NextDouble() < 0.5)
                {
                    //extend at front
                    Array.Copy(m_genes, 0, newGenes, difference, originalSize);

                    for (int i = 0; i < difference; i++)
                    {
                        newGenes[i] = m_random.NextDouble();
                    }
                }
                else
                {
                    //extend at back
                    Array.Copy(m_genes, 0, newGenes, 0, originalSize);

                    for (int i = originalSize; i < size; i++)
                    {
                        newGenes[i] = m_random.NextDouble();
                    }
                }

                m_genes = newGenes;
            }
            else
            {
                Array.Resize(ref m_genes, size);
            }

            m_length = size;
        }

        public double[] Genes()
        {
            return m_genes;
        }

        public void Output()
        {
            foreach (double values in m_genes)
            {
                System.Console.WriteLine($"{values}");
            }
            System.Console.Write("\n");
        }

        public void GetValues(ref double[] values)
        {
            for (int i = 0; i < m_length; i++)
                values[i] = m_genes[i];
        }

        public double[] m_genes;
        public int m_length;
        public double m_fitness;
        public int age;
        static Random m_random = new Random((int)DateTime.Now.Ticks);

        public static double m_mutationRate;

        public double Fitness
        {
            get
            {
                return m_fitness;
            }
            set
            {
                m_fitness = value;
            }
        }

        public static double MutationRate
        {
            get
            {
                return m_mutationRate;
            }
            set
            {
                m_mutationRate = value;
            }
        }

        public int Length
        {
            get
            {
                return m_length;
            }
        }
    }
}