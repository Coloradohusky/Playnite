﻿using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Playnite.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Scripting.IronPython
{
    public class IronPythonRuntime : IScriptRuntime
    {
        private static NLog.Logger logger = NLog.LogManager.GetLogger("Python");
        private ScriptEngine engine;
        private ScriptScope scope;

        public IronPythonRuntime()
        {
            engine = Python.CreateEngine();
            scope = engine.CreateScope();
            engine.Execute(string.Format(@"
import clr
import sys
sys.path.append(r""{0}"")
clr.AddReferenceToFile(""PlayniteSDK.dll"")
from Playnite.SDK.Models import *
", Paths.ProgramFolder), scope);

            //scope.ImportModule("clr");
            //scope.ImportModule("sys");
            //engine.Execute($"sys.path.append(r\"{Paths.ProgramFolder}\")", scope);
            //engine.Execute("clr.AddReferenceToFile(\"PlayniteSDK.dll\")", scope);


            //engine.Execute("from Playnite.SDK.Models import *", scope);
            //scope.ImportModule("Playnite.SDK.Models");

            SetVariable("__logger", new Logger("Python"));
        }

        public void Dispose()
        {
            engine.Runtime.Shutdown();
        }

        public object Execute(string script)
        {
            return Execute(script, null);
        }

        public object Execute(string script, Dictionary<string, object> variables)
        {
            if (variables != null)
            {
                foreach (var key in variables.Keys)
                {
                    scope.SetVariable(key, variables[key]);
                }
            }

            var source = engine.CreateScriptSourceFromString(script, SourceCodeKind.AutoDetect);            
            var result = source.Execute<object>(scope);
            return result;
        }

        public void ExecuteFile(string path)
        {
            engine.ExecuteFile(path, scope);
        }

        public object GetVariable(string name)
        {
            try
            {
                return scope.GetVariable<object>(name);
            }
            catch (MissingMemberException)
            {
                return null;
            }
        }

        public void SetVariable(string name, object value)
        {
            scope.SetVariable(name, value);
        }

        public bool GetFunctionExits(string name)
        {
            return engine.Execute<bool>($"'{name}' in globals()", scope);
        }
    }
}
