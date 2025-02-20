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
