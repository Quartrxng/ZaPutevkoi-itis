using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.FrameWork
{
    internal class ControllersInfo
    {
        private static ControllersInfo _instance;
        public static ControllersInfo Instance()
        {
            if (_instance == null)
                _instance = new ControllersInfo();
            return _instance;
        }

        public Dictionary<Type, MethodInfo[]> controllers = new Dictionary<Type, MethodInfo[]>();

        private ControllersInfo() 
        { 
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                controllers.Add(type, type.GetMethods());
            }
        }
    }
}
