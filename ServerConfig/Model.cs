using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Windows.UI.Xaml;
using Windows.System.Threading;
using System.Windows.Media.Media3D;
using System.Timers;
using System.Text.Json;
using System.Text.Encodings.Web;
using static ServerConfig.PID;
using System.Reflection;

namespace ServerConfig
{
    public class Model
    {
        private double _weightDesired;

        public double Weight;

        public float Step;

        public PID pid;
        TestLerp lerp;

        AutoResetEvent _pidReady = new AutoResetEvent(true);
        ThreadPoolTimer pidTimer;

        public List<object> Modules { get; set; }

        public class Data
        {
            public float Time { get; set; }
            public double Value { get; set; }
        }

        private List<Data> InterPolatedDatas = new List<Data>();

        public Model(float kp,float ki,float kd, int step,List<object> moduls)
        {
            pid = new PID(this, kp, ki, kd);

            _weightDesired = pid.Weight;
            Step = (float)step;

            Modules = moduls;


            lerp = new TestLerp();

            pid.sum = 0;
            pid.sum2 = 0;
        }

        public PID.Module GetModule(ModuleTypes type)
        {
            foreach (var module in Modules)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                PID.Module mod = new PID.Module();

                switch (type)
                {
                    case ModuleTypes.ResModule:
                        mod = JsonSerializer.Deserialize<ResModule>(module.ToString(), options);
                        break;
                    case ModuleTypes.CompZeroModule:
                        mod = JsonSerializer.Deserialize<ResModule>(module.ToString(), options);
                        break;
                    case ModuleTypes.CompAsymModule:
                        mod = JsonSerializer.Deserialize<ResModule>(module.ToString(), options);
                        break;
                }

                return mod;
            }

            return null;
        }

        public Server.Responce GetResultData()
        {
            var data = new Data
            {
                Time = pid.Iterator * Step / 1000f,
                Value = lerp.CosInterpolate(0, pid.Weight, pid.Iterator * Step / 1000f)
            };
            //Debug.WriteLine("interpolated " + pid.Iterator + " : " + data.Time + " / " + data.Value);
            InterPolatedDatas.Add(data);

            var respData = new Server.Responce
            {
                pidRes = pid.GetPidCalc(data, Step / 1000f),
                interRes = data,
            };

            pid.Iterator++;

            return respData; 
        }



        public List<Data> GetInterRes()
        {
            return InterPolatedDatas;
        }

        public List<Data> GetResult()
        {
            return pid.GetListResult(InterPolatedDatas, Step / 1000f);
        }
    }
}
