
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Text;

namespace ObfuscatorProject
{
    public interface ObfuscatorComponent
    {
        SyntaxTree Obfuscate(SyntaxTree tree);

    }
}
