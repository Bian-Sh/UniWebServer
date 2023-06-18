using System.Collections.Generic;
using System.Reflection;

namespace zFramework.Web
{
    internal class ControllerMethod
    {
        public IHttpController Controller;
        public List<Parameter> Parameters;
        public MethodInfo MethodInfo;
        public MethodInfo CheckMethod;
    }
}