using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreBotsAPI.Prepatch
{
    public static class Utils
    {
        public static void AddEnumValue(ref TypeDefinition type, string name, object value)
        {
            const FieldAttributes defaultEnumFieldAttributes = FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault;
            type.Fields.Add(new FieldDefinition(name, defaultEnumFieldAttributes, type) { Constant = value });
        }
    }
}
