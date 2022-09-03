using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Graph.Common
{

    public enum TaskContextKind
    {
        Agent
    }

    public interface ITask<T> where T : class
    {
        bool Run(TaskContextKind contextKind, string contextName, T contextData);
    }
}
