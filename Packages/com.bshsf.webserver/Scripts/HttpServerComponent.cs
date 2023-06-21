using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace zFramework.Web
{
    public class HttpServerComponent : MonoBehaviour
    {
        [SerializeField]
        private int port = 8080;

        [SerializeField, Header("同类型控制器只需一个")]
        private List<GameObject> controllers = new List<GameObject>();

        [SerializeField]
        private List<StaticRouteSetting> staticRouteSettings = new List<StaticRouteSetting>();

        private HttpServer httpServer;

        private void Awake()
        {
            httpServer = new HttpServer();

            // 使用 Linq 获取 controllers 中所有 IHttpController并根据 Type 去重（同一个Type只能出现一次）
            var httpControllers = controllers
                .SelectMany(go => go.GetComponents<IHttpController>())
                .GroupBy(controller => controller.GetType())
                .Select(group => group.First());

            foreach (var httpController in httpControllers)
            {
                httpServer.AddController(httpController);
            }

            // Add static route
            foreach (var staticRouteSetting in staticRouteSettings)
            {
                var staticPageController = new StaticPageController(staticRouteSetting);
                httpServer.AddControllerMethod(staticPageController.GetControllerMethod());
            }
        }
        private void OnEnable() => httpServer?.Start(port);
        private void OnDisable() => httpServer?.Stop();
    }
}
