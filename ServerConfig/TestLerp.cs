using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConfig
{
    public class TestLerp
    {
        private const float PI = 3.145926535987f;

        public double LinInterpolate(double firstNum, double secondNum, double by)
        {
            return firstNum * (1 - by) + secondNum * by;
        }

        public double CosInterpolate(double firstNum, double secondNum, double by)
        {
            var bybuff = (1 - Math.Cos(by * PI)) / 2;

            return firstNum * (1 - bybuff) + secondNum * bybuff;

            //return Math.Sin(2 * PI / 10 * by);
        }
    }
}
