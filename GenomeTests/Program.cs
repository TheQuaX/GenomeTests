using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genetic;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Show Genome:");
        Genome genome = new Genome(10, true);
        genome.Output();

        Console.ReadLine();

    }
}

