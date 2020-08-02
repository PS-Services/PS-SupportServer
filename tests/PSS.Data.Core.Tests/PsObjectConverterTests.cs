using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using PSS.Data.Core;
using PSS.Data.Core.Tests;
using Xunit;
using Xunit.Abstractions;

namespace MyApp.Tests
{
    public class PsObjectConverterTests
    {
        private readonly ITestOutputHelper _log;

        private readonly PowerShell ps = null;

        public PsObjectConverterTests(ITestOutputHelper log)
        {
            ps = PowerShell.Create(InitialSessionState.CreateDefault2());

            ps.Streams.Information.DataAdded += (sender, args) =>
            {
                _log.WriteLine(string.Join(Environment.NewLine, args));
            };

            ps.Streams.Warning.DataAdded += (sender, args) =>
            {
                _log.WriteLine("WARN: " + string.Join(Environment.NewLine, args));
            };

            ps.Streams.Error.DataAdded += (sender, args) =>
            {
                _log.WriteLine("ERROR: " + string.Join(Environment.NewLine, args));
            };

            ps.Streams.Verbose.DataAdded += (sender, args) =>
            {
                _log.WriteLine("Verbose: " + string.Join(Environment.NewLine, args));
            };

            ps.Streams.Debug.DataAdded += (sender, args) =>
            {
                _log.WriteLine("DBG: " + string.Join(Environment.NewLine, args));
            };

            ps.Streams.Progress.DataAdded += (sender, args) =>
            {
                _log.WriteLine("Progress: " + string.Join(Environment.NewLine, args));
            };

            _log = log;
            _log.WriteLine(ps.Runspace.Name);
        }

        [Theory]
        [ClassData(typeof(PsObjectConverterTestsDataSet1))]
        public void PsObjectConverterTests1(object data)
        {
            var result = data.ToPSObject();
            
            Assert.NotNull(result);

            var twist = result.FromPSObject(data.GetType());

            Assert.NotNull(twist);

            foreach (var propertyInfo in data.GetType().GetProperties(BindingFlags.Instance
                                                            | BindingFlags.Public))
            {
                Assert.NotNull(propertyInfo);
                Assert.NotNull(propertyInfo.Name);
                Assert.NotNull(result.Properties[propertyInfo.Name].Value);

                var value = propertyInfo.GetValue(data);

                Assert.NotNull(value);

                Assert.Equal(value, result.Properties[propertyInfo.Name].Value);
                
                var twistedValue = propertyInfo.GetValue(twist);

                Assert.NotNull(twistedValue);

                Assert.Equal(value, twistedValue);

                _log.WriteLine($"Original Property {propertyInfo.Name} is {value}");
                _log.WriteLine($"Twisted Property {propertyInfo.Name} is {twistedValue}");
            }

        }
    }
}


/*
 * (C) 2019 Your Legal Entity's Name
 */
