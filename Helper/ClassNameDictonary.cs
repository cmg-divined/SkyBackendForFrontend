using System;
using System.Collections.Generic;

namespace Coflnet.Sky.Commands.Shared
{
    public class ClassNameDictonary<T> : Dictionary<string, T>
    {
        public void Add<TDerived>() where TDerived : T
        {
            var filter = Activator.CreateInstance<TDerived>();
            this.Add(GetCleardName<TDerived>(), filter);
        }

        public static string GetCleardName<TDerived>() where TDerived : T
        {
            return typeof(TDerived).Name.Replace("Command", "").Replace(typeof(T).Name, "").ToLower();
        }
    }
}