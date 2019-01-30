using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetNeutronsWpfApp
{
  static public class ClassFilterSig
    {
       static double Dacc = 0;
      static double Dout = 0;
        static public double Filtr(double data, double K)
        {
            //K - RC*дискритизаци АЦП


            double Din = data;

            Dacc = Dacc + Din - Dout;
            Dout = Dacc / K;

            return Dout;
        }
    }
}
