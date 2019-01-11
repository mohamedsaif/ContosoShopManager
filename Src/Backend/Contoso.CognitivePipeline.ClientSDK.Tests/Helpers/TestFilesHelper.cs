using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.CognitivePipeline.ClientSDK.Tests.Helpers
{
    public class TestFilesHelper
    {
        private static Assembly assembly = Assembly.GetExecutingAssembly();
        
        public static byte[] GetTestFile(string name)
        {
            using (var stream = assembly.GetManifestResourceStream($"Contoso.CognitivePipeline.ClientSDK.Tests.TestFiles.{name}"))
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
