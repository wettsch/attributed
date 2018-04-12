using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog.Core;
using Serilog.Events;

namespace Destructurama.Attributed
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LogMaskedEachAttribute : LogMaskedAttribute
    {
        internal object FormatMaskedValues(object propValue)
        {
            string[] values = null;

            if (propValue is IEnumerable<string>)
            {
                values = propValue as string[];
            }

            if (values == null)
            {
                return propValue;
            }

            var sb = new StringBuilder();

            foreach (string value in values)
            {
                sb.AppendFormat("\"{0}\",", FormatMaskedValue(value));
            }

            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            
            return $"[{sb}]";
        }

        protected internal override bool TryCreateLogEventProperty(string name, object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventProperty property)
        {
            property = new LogEventProperty(name, new ScalarValue(FormatMaskedValues(value)));
            return true;
        }
    }
}
