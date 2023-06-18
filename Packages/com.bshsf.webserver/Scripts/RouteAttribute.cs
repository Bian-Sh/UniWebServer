using System;

namespace zFramework.Web
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RouteAttribute : Attribute
    {
        public string Path;

        public RouteAttribute(string path)
        {
            Path = path;
        }
    }
}