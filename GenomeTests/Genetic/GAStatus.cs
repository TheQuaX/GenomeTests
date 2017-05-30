using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genetic
{
    public class GAStatus
    {
		//Best fitness atm
        public double Fitness = 0;
		
		//Fitness when solution was found
        public double MaxFitness = 0;

        //Best program atm
        public string Program = "";

        //Best program output atm
        public string Output = "";

        //Generation count
        public int Iteration = 0;

        //Count status prompt
        public int StatusCount = 0;
        
        //count executives
        public int Ticks = 0;

        //LatestChange
        public DateTime LastChangeDate = DateTime.Now;
    }
}
