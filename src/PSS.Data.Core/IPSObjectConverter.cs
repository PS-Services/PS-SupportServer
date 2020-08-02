using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PSS.Data.Core
{
    public static class PSObjectConverterExtensions
    {
        public static PSObject ToPSObject<TSource>(this TSource source)
        {
            var test = new PSObject();
            var result = PSObject.AsPSObject(source);

            if (result?.BaseObject is PowerShell ps)
            {
                ps.Streams.Debug.Add(new DebugRecord($"Created {result}"));
            }

#pragma warning disable CS8603 // Possible null reference return.
            return result;
#pragma warning restore CS8603 // Possible null reference return.
        }

        public static TOut FromPSObject<TOut>(this PSObject source)
        {
            var type = typeof(TOut);
            var result = Activator.CreateInstance<TOut>();

            foreach (var pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (source.Properties[pi.Name].IsGettable)
                {
                    pi.SetValue(result, source.Properties[pi.Name].Value);
                }
            }

            return result;
        }

        public static object FromPSObject(this PSObject source, Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var objArray = new object[properties.Length];

            var objIdx = 0;

            foreach (var info in properties)
            {
                objArray[objIdx++] = source.Properties[info.Name].Value;
            }

            var result = Activator.CreateInstance(type, objArray);

#pragma warning disable CS8603 // Possible null reference return.
            return result;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
