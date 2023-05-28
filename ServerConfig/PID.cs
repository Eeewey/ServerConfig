using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ServerConfig
{
    public class PID
    {
        private double processVariable = 0;
        public double sum2 = 0;
        public double sum = 0;
        public double sumr1 = 0;
        public double sumr2 = 0;
        public Model model;

        public class ModelConfig
        {
            public float Kp { get; set; }
            public float Ki { get; set; }
            public float Kd { get; set; }
            public int Step { get; set; }
            public List<object> modulesConf { get; set; }
        }
        public enum ModuleTypes
        {
            ResModule,
            CompZeroModule,
            CompAsymModule
        }

        public class Module
        {
            public ModuleTypes Type { get; set; }
            public bool IsActive { set; get; }
        }
        public class ResModule : Module
        {
            public float ResW { set; get; }
        }

        public float Weight;
        public int Iterator;

        public PID(Model mod, double GainProportional, double GainIntegral, double GainDerivative)
        {
            this.GainDerivative = GainDerivative;
            this.GainIntegral = GainIntegral;
            this.GainProportional = GainProportional;

            model = mod;
        }

        public double ControlVariable(float Step)
        {
            double error = SetPoint - ProcessVariableLast;

            sum2 += error * Step;

            double dInput = ProcessVariableLast - ProcessVariableLastOld;
            double derivativeTerm = dInput / Step;

            double res = GainProportional * error + GainIntegral * sum2 + GainDerivative * derivativeTerm;
            var resMod = (model.GetModule(ModuleTypes.ResModule) as ResModule);
            if (resMod.IsActive)
            {
                sumr1 += (res - sumr2 * Math.Sqrt(resMod.ResW)) * Step;
                sumr2 += sumr1 * Step;
                res += 2 * resMod.ResW * sumr1;
            }

            ProcessVariable = res;
            sum += res * Step;

            return sum;
        }

        public Model.Data GetPidCalc(Model.Data data, float step)
        {
            SetPoint = data.Value;

            var resData = new Model.Data
            {
                Time = data.Time,
                Value = ControlVariable(step)
            };

            return resData;
        }

        public List<Model.Data> GetListResult(List<Model.Data> list, float Step)
        {
            var resultList = new List<Model.Data>();

            foreach(var data in list)
            {
                SetPoint = data.Value;

                var resData = new Model.Data
                {
                    Time = data.Time,
                    Value = ControlVariable(Step)
                };

                resultList.Add(resData);
            }

            return resultList;
        }

        public double GainDerivative { get; set; } = 0;

        public double GainIntegral { get; set; } = 0;

        public double GainProportional { get; set; } = 0;

        public double IntegralTerm { get; private set; } = 0;

        public List<Module> modules { get; set; }


        public double ProcessVariable
        {
            get { return processVariable; }
            set
            {
                ProcessVariableLastOld = ProcessVariableLast;
                ProcessVariableLast = processVariable;
                
                processVariable = value;
            }
        }

        public double ProcessVariableLast { get; private set; } = 0;
        public double ProcessVariableLastOld { get; private set; } = 0;


        public double SetPoint { get; set; } = 0;
    }
}
