using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using static ServerConfig.Model;

namespace ServerConfig
{
    public class Server
    {
        public class Responce
        {
            public Data pidRes { get; set; }
            public Data interRes { get; set; }
        }

        private Model _model { get; set; }
        public TcpListener? tcpListener;
        public async Task Init()
        {
            tcpListener = new TcpListener(IPAddress.Any, 8888);

            try
            {
                tcpListener.Start();
                Debug.WriteLine("Сервер запущен");

                while (true)
                {
                    var tcpClient = await tcpListener.AcceptTcpClientAsync();

                    Task.Run(async () => await ProcessClientAsync(tcpClient));
                }
            }
            finally
            {
                tcpListener.Stop();
            }
        }

        async Task ProcessClientAsync(TcpClient tcpClient)
        {
            var stream = tcpClient.GetStream();
            

            var response = new List<byte>();
            int bytesRead = 10;
            while (true)
            {
                while ((bytesRead = stream.ReadByte()) != '\n')
                {
                    response.Add((byte)bytesRead);
                }
                var respStr = Encoding.UTF8.GetString(response.ToArray());

                if (respStr == "END") break;

                var textResult = "";

                if (respStr.EndsWith("START"))
                {
                    _model = null;

                    PID.ModelConfig conf = JsonSerializer.Deserialize<PID.ModelConfig>(respStr.Remove(respStr.Length - 5));

                    _model = new Model(conf.Kp, conf.Ki, conf.Kd, conf.Step, conf.modulesConf);
                    Debug.WriteLine(conf.Kp + "/" + conf.Ki + "/" + conf.Kd);

                    textResult = "STARTED";
                }
                
                if(respStr.EndsWith("WEIGHT"))
                {
                    float weight;

                    bool Wsuccess = float.TryParse(respStr.Remove(respStr.Length - 6), out weight);

                    _model.pid.Weight = weight;

                    var calc = _model.GetResultData();

                    var result = new Responce
                    {
                        pidRes = calc.pidRes,
                        interRes = calc.interRes,
                    };

                    textResult = JsonSerializer.Serialize(result);
                }


                Debug.WriteLine(textResult);
                await stream.WriteAsync(Encoding.UTF8.GetBytes(textResult + '\n'));

                response.Clear();
            }

            tcpClient.Close();
        }
    }
}
