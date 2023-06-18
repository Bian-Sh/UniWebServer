using System;
using System.Net;
using System.Threading;
using static zFramework.Web.Loom;
namespace zFramework.Web
{
    public class HttpServer
    {
        private HttpListener httpListener;
        private CancellationTokenSource tokenSource;
        private Router router;

        internal HttpServer()
        {
            httpListener = new HttpListener();
            router = new Router();
        }

        public async void Start(int port)
        {
            var uri = $"http://*:{port}/";
            httpListener.Prefixes.Add(uri);

            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            await ToOtherThread;
            httpListener.Start();
            UnityEngine.Debug.Log($"{nameof(HttpServer)}: Server Started, http://{GetLocalIPAddress()}:{port}");
            while (true)
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    var context = await httpListener.GetContextAsync();
                    await ToMainThread;
                    await router.RouteAsync(context.Request, context.Response);
                }
                catch (Exception e)
                {
                    if (e is OperationCanceledException || e is ObjectDisposedException)
                    {
                        UnityEngine.Debug.Log($"{nameof(HttpServer)}: Server Stoped !");
                        break;
                    }
                }
            }
        }

        public void Stop()
        {
            tokenSource.Cancel();
            httpListener.Stop();
        }

        internal void AddController(IHttpController httpController) => router.AddController(httpController);

        internal void AddControllerMethod(ControllerMethod controllerMethod) => router.AddControllerMethod(controllerMethod);

        // 获取本机ip
        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}