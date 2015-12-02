using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubesSisrek
{
    public class Matrix
    {
        public static double[,] SobelMask
        {
            get
            {
                return new double[,]  
                { {-2,-2, 0, }, 
                  {-2, 0, 2, }, 
                  { 0, 2, 2, }, };
            }
        }
    }
}
