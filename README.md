# CSharp Obfuscator
This is a simple obfuscator implemented to resist Control Flow analysis. It is a prototype to demonstrate the concept of control flow obfuscation.

## Features Implemented
### Control Flow Obfuscation
  - If/else statements
  - For-loop statements
  - Sequential statements
 
### Alias Obfuscation
  - variable names
  - method names

## Running the code
The code is being called via the unit test, output will be printed to console. The main method is located in `ObfucatorProject/ObfuscatorProjectAnalyzer.cs` called `AnalyzeMethod()`. Compose decorators in `Decorator.cs` to create your unique code obfucator pipeline.

## Demo

### Input code
```c#
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
}

```

### Output

```c#
    
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
            public int LhyzKuKJr()
            {
var yvC6t=1;var cALblq=1;                int nC9usUpIA= 7;
                int kKapagVjF= 3;
                if(true)
                {                nC9usUpIA = 12;}
                if(true)
                {                kKapagVjF = nC9usUpIA + 1;}
yvC6t = 1;while(yvC6t>0){if(yvC6t==1){if(nC9usUpIA < kKapagVjF){if(true)
{yvC6t=3;}continue;}if(true)
{yvC6t=2;}continue;}if(yvC6t==2){;continue;}if(yvC6t==3){
                    kKapagVjF++;
yvC6t = 0;continue;                }
}int NCdWIlOVtG= 0;if(true)
{cALblq = 1;}while(cALblq>0){if(cALblq==1){if(NCdWIlOVtG < 5){if(true)
{cALblq=2;}continue;}if(true)
{cALblq=3;}continue;}if(cALblq==2){
                    nC9usUpIA++;
cALblq = 1;NCdWIlOVtG++;continue;                }
if(cALblq==3){cALblq = 0;continue;}}                return nC9usUpIA;
            }
        }
    }
```
