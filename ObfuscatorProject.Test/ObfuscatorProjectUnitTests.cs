using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = ObfuscatorProject.Test.CSharpAnalyzerVerifier<
    ObfuscatorProject.ObfuscationAnalyzer>;

namespace ObfuscatorProject.Test
{
    [TestClass]
    public class ObfuscatorProjectUnitTest
    {
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class {|#0:TypeName|}
        {   
        }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
            public int stupidTest()
            {
                int x = 7;
                int y = 3;
                x = 12;
                y = x + 1;
                if(x < y){
                    y++;
                }
                for(int i = 0; i < 5; i++){
                    x++;
                }
                return x;
            }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(fixtest);
        }
    }
}
