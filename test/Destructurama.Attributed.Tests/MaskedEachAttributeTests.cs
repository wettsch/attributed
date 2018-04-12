using System.Collections.Generic;
using System.Linq;
using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace Destructurama.Attributed.Tests
{
    public class MaskedEachAttributeTests
    {
        public class CustomizedMaskedLogs
        {
            /// <summary>
            /// [123456789, 987654321] results in ["12***89","98***21"]
            /// </summary>
            [LogMaskedEach(ShowFirst = 2, ShowLast = 2)]
            public IEnumerable<string> ShowFirstAndLastTwoForEachElement { get; set; }

            /// <summary>
            /// [john.smith@email.com, anna.mayers@example.org, terry.kelly@test.net] results in ["joh*****************","ann********************","ter*****************"]
            /// </summary>
            [LogMaskedEach(ShowFirst = 3, PreserveLength = true)]
            public IEnumerable<string> ShowFirstThreeThenDefaultMaskedPreservedLength { get; set; }

            /// <summary>
            /// [] results in []
            /// </summary>
            [LogMaskedEach]
            public IEnumerable<string> SupportsEmptyEnumeration { get; set; }
        }

        [TestFixture]
        public class MaskedAttributeTests
        {
            [Test]
            public void LogMaskedEachAttribute_Shows_First_NChars_And_Last_NChars_Replaces_Value_With_Default_StarMask_ForEachElement()
            {
                // [LogMaskedEach(ShowFirst = 2, ShowLast = 2)]
                // -> "["12***89","98***21"]"

                LogEvent evt = null;

                var log = new LoggerConfiguration()
                    .Destructure.UsingAttributes()
                    .WriteTo.Sink(new DelegatingSink(e => evt = e))
                    .CreateLogger();

                var customized = new CustomizedMaskedLogs
                {
                    ShowFirstAndLastTwoForEachElement = new[] { "123456789", "987654321" }
                };

                log.Information("Here is {@Customized}", customized);

                var sv = (StructureValue)evt.Properties["Customized"];
                var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

                Assert.IsTrue(props.ContainsKey("ShowFirstAndLastTwoForEachElement"));
                Assert.AreEqual("[\"12***89\",\"98***21\"]", props["ShowFirstAndLastTwoForEachElement"].LiteralValue());
            }

            [Test]
            public void LogMaskedEachAttribute_Shows_First_NChars_Replaces_Value_With_Default_StarMask_LenghtPreserved_ForEachElement()
            {
                // [LogMaskedEach(ShowFirst = 3, PreserveLength = true)]
                // -> "["joh*****************","ann********************","ter*****************"]"

                LogEvent evt = null;

                var log = new LoggerConfiguration()
                    .Destructure.UsingAttributes()
                    .WriteTo.Sink(new DelegatingSink(e => evt = e))
                    .CreateLogger();

                var customized = new CustomizedMaskedLogs
                {
                    ShowFirstThreeThenDefaultMaskedPreservedLength = new[] { "john.smith@email.com", "anna.mayers@example.org", "terry.kelly@test.net" }
                };

                log.Information("Here is {@Customized}", customized);

                var sv = (StructureValue)evt.Properties["Customized"];
                var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

                Assert.IsTrue(props.ContainsKey("ShowFirstThreeThenDefaultMaskedPreservedLength"));
                Assert.AreEqual("[\"joh*****************\",\"ann********************\",\"ter*****************\"]", props["ShowFirstThreeThenDefaultMaskedPreservedLength"].LiteralValue());
            }

            [Test]
            public void LogMaskedEachAttribute_Supports_Empty_Enumeration()
            {
                // [LogMaskedEach]
                // -> "[]"

                LogEvent evt = null;

                var log = new LoggerConfiguration()
                    .Destructure.UsingAttributes()
                    .WriteTo.Sink(new DelegatingSink(e => evt = e))
                    .CreateLogger();

                var customized = new CustomizedMaskedLogs
                {
                    SupportsEmptyEnumeration = Enumerable.Empty<string>()
                };

                log.Information("Here is {@Customized}", customized);

                var sv = (StructureValue)evt.Properties["Customized"];
                var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

                Assert.IsTrue(props.ContainsKey("SupportsEmptyEnumeration"));
                Assert.AreEqual("[]", props["SupportsEmptyEnumeration"].LiteralValue());
            }
        }
    }
}
