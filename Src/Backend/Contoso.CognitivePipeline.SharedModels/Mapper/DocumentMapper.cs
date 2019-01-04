using Contoso.CognitivePipeline.SharedModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.SharedModels.Mapper
{

    //TODO: To be replaced with AutoMapper (https://github.com/AutoMapper/AutoMapper)
    public class DocumentMapper
    {
        public static T MapDocument<T>(SmartDoc b) where T : SmartDoc, new()
        {
            T clone = new T();

            var members = b.GetType().GetMembers(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < members.Length; i++)
            {
                if (members[i].MemberType == MemberTypes.Property)
                {
                    clone
                        .GetType()
                        .GetProperty(members[i].Name)
                        .SetValue(clone, b.GetType().GetProperty(members[i].Name).GetValue(b, null), null);
                }

            }
            return clone;
        }
    }
}
