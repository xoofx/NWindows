using System.Text;
using Broslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace NWindows.TerraFX.Interop.CodeGen;

internal class Program
{
    static async Task Main(string[] args)
    {
        var rootFolder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"../../../../.."));
        var srcFolder = Path.Combine(rootFolder, "src");
        var pathToNWindowsProject = Path.Combine(srcFolder, @"NWindows", "NWindows.csproj");
        var destFolder = Path.Combine(srcFolder, @"NWindows", "Interop", "TerraFX");

        if (Directory.Exists(destFolder))
        {
            Directory.Delete(destFolder, true);
        }
        Directory.CreateDirectory(destFolder);

        var baseFolder = Path.GetFullPath(Path.Combine(rootFolder, @"..\TerraFX\terrafx.interop.windows\sources\"));
        var terraFXProjectPath = Path.Combine(baseFolder, @"Interop\Windows\TerraFX.Interop.Windows.csproj");

        if (!File.Exists(terraFXProjectPath))
        {
            throw new FileNotFoundException($"The TerraFX project file {terraFXProjectPath} is missing. Please check that the repository was correctly checked-out");
        }

        var resultForTerraFX = CSharpCompilationCapture.Build(terraFXProjectPath,
            properties: new Dictionary<string, string>()
            {
                {"TargetPlatform", @"net6.0"},
            });

        var terraFxProject =
            resultForTerraFX.Workspace.CurrentSolution.Projects.First(x => x.Name == "TerraFX.Interop.Windows");

        var result = CSharpCompilationCapture.Build(pathToNWindowsProject, properties: new Dictionary<string, string>()
        {
            { "UseTerraFXPackage", "true" },
        });
        
        var workspace = result.Workspace;
        var solution = workspace.CurrentSolution;

        var newTerraFxProject = workspace.AddProject("TerraFX.Interop.Windows", LanguageNames.CSharp);
        newTerraFxProject = newTerraFxProject.WithCompilationOptions(terraFxProject.CompilationOptions!);
        newTerraFxProject = newTerraFxProject.WithParseOptions(terraFxProject.ParseOptions!);
        newTerraFxProject = newTerraFxProject.WithMetadataReferences(terraFxProject.MetadataReferences);
        foreach (var doc in terraFxProject.Documents)
        {
            var filePath = doc.FilePath;
            var newDocument = newTerraFxProject.AddDocument(doc.Name,
                SourceText.From(File.ReadAllText(filePath), Encoding.UTF8), filePath: filePath);
            newTerraFxProject = newDocument.Project;
        }

        solution = newTerraFxProject.Solution;
        workspace.TryApplyChanges(solution);

        var projectNWindows = solution.Projects.First(x => x.Name == "NWindows");
        var referenceToRemove = projectNWindows.MetadataReferences.First(x =>
            x.Display?.Contains("TerraFX.Interop.Windows.dll", StringComparison.OrdinalIgnoreCase) ?? false);

        projectNWindows = projectNWindows.RemoveMetadataReference(referenceToRemove);
        projectNWindows = projectNWindows.AddProjectReference(new ProjectReference(newTerraFxProject.Id));

        //var compilationTerraFx = (await newTerraFxProject.GetCompilationAsync())!;
        //CheckErrors(compilationTerraFx);

        // Compile the project
        var compilation = (await projectNWindows.GetCompilationAsync())!;
        CheckErrors(compilation!);

        var terraFXCompilation = compilation.References.OfType<CompilationReference>().First().Compilation;

        var noDuplicates = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        var listOfSymbolsToVisit = new Stack<ISymbol>();
        var symbolsToKeep = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

        foreach (var compilationSyntaxTree in compilation.SyntaxTrees)
        {
            Console.WriteLine($"Visiting: {compilationSyntaxTree.FilePath}");
            foreach (var symbol in GetAllSymbols(compilation, compilationSyntaxTree.GetRoot()))
            {
                Console.WriteLine($"--> {symbol.Kind} {symbol}");
                ProcessSymbol(symbol);
            }
        }

        Console.WriteLine();

        Console.WriteLine("---------------------------");
        Console.WriteLine("All TerraFX Symbols used");
        Console.WriteLine("---------------------------");
        foreach (var symbol in noDuplicates.OrderBy(x => x.Name))
        {
            Console.WriteLine($"--> {symbol.Kind} {symbol}");
        }

        Console.WriteLine("---------------------------");
        Console.WriteLine("Processing TerraFX symbols transitively");
        Console.WriteLine("---------------------------");

        while (listOfSymbolsToVisit.Count > 0)
        {
            var symbol = listOfSymbolsToVisit.Pop();
            if (symbolsToKeep.Add(symbol))
            {
                Console.WriteLine($"Processing symbol {symbol.Kind} {symbol} ");
                foreach (var nextSymbol in CollectReferencedSymbols(terraFXCompilation, symbol))
                {
                    ProcessSymbol(nextSymbol);
                }
            }
        }

        void ProcessSymbol(ISymbol symbol)
        {
            var visited = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
            ProcessSymbolRecurse(symbol, visited);
        }

        void ProcessSymbolRecurse(ISymbol symbol, HashSet<ISymbol> symbolsVisited)
        {
            if (!symbolsVisited.Add(symbol)) return;
            
            if (symbol is IPropertySymbol || symbol is IMethodSymbol || symbol is IFieldSymbol)
            {
                if (IsTerraFX(symbol))
                {
                    if (noDuplicates.Add(symbol))
                    {
                        Console.WriteLine($"--> Adding {symbol.Kind} {symbol} ");
                        listOfSymbolsToVisit.Push(symbol);
                    }
                }
            }
            else if (symbol is INamedTypeSymbol namedType)
            {
                foreach (var typeArg in namedType.TypeArguments)
                {
                    ProcessSymbolRecurse(typeArg, symbolsVisited);
                }

                foreach (var inter in namedType.Interfaces)
                {
                    if (!IsTerraFX(inter))
                    {
                        ProcessSymbolRecurse(inter, symbolsVisited);
                    }
                }

                // If we have a value type, we will keep all its fields
                if (namedType.IsValueType)
                {
                    foreach (var field in namedType.GetMembers().OfType<IFieldSymbol>())
                    {
                        if (!field.IsConst && !field.IsStatic)
                        {
                            ProcessSymbolRecurse(field, symbolsVisited);
                        }
                    }

                    // Collect explicit implementation
                    foreach (var member in namedType.GetMembers())
                    {
                        if (member is IPropertySymbol prop &&
                            (prop.ExplicitInterfaceImplementations.Any(x => !IsTerraFX(x)) || prop.IsOverride))
                        {
                            ProcessSymbolRecurse(member, symbolsVisited);
                        }
                        else if (member is IMethodSymbol method &&
                                 (method.ExplicitInterfaceImplementations.Any(x => !IsTerraFX(x)) || method.IsOverride))
                        {
                            ProcessSymbolRecurse(member, symbolsVisited);
                        }
                    }

                    // Make sure that we keep implemented interfaces members
                    foreach (var inter in namedType.AllInterfaces)
                    {
                        if (!IsTerraFX(inter))
                        {
                            foreach (var imember in inter.GetMembers())
                            {
                                if (imember is IPropertySymbol || imember is IMethodSymbol)
                                {
                                    var implem = namedType.FindImplementationForInterfaceMember(imember);
                                    if (implem != null)
                                    {
                                        ProcessSymbolRecurse(implem, symbolsVisited);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else //if (symbol is IPointerTypeSymbol pointerType)
            {
                //ProcessSymbol(pointerType.PointedAtType);
                //
            }
        }

        Console.WriteLine();

        Console.WriteLine("---------------------------");
        Console.WriteLine("All TerraFX Transitive Symbols used");
        Console.WriteLine("---------------------------");
        foreach (var symbol in symbolsToKeep.OrderBy(x => x.Name))
        {
            Console.WriteLine($"--> {symbol.Kind} {symbol}");
        }

        var syntaxNodeList = new HashSet<SyntaxNode>();
        foreach (var symbol in symbolsToKeep)
        {
            foreach (var decl in symbol.DeclaringSyntaxReferences)
            {
                KeepSyntaxNode(decl.GetSyntax(), syntaxNodeList);
            }
        }

        Console.WriteLine("---------------------------");
        Console.WriteLine("Generating code");
        Console.WriteLine("---------------------------");

        var removeUnusedNodes = new RemoveUnusedNodes(terraFXCompilation, syntaxNodeList, symbolsToKeep);
        var syntaxTrees = new HashSet<SyntaxTree>(syntaxNodeList.Select(x => x.SyntaxTree));

        var newTrees = new List<SyntaxTree>();
        foreach (var syntaxTree in syntaxTrees)
        {
            var newNode = (CSharpSyntaxNode?) removeUnusedNodes.Visit(syntaxTree.GetRoot());
            if (newNode != null)
            {
                var newSyntaxTree = CSharpSyntaxTree.Create(newNode);
                newTrees.Add(newSyntaxTree);
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine($"{syntaxTree.FilePath}");
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine(newSyntaxTree);

                var newFilePath = Path.Combine(destFolder, syntaxTree.FilePath.Substring(baseFolder.Length));

                var folder = Path.GetDirectoryName(newFilePath);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                File.WriteAllText(newFilePath, newSyntaxTree.ToString());
            }
        }

        static void KeepSyntaxNode(SyntaxNode? node, HashSet<SyntaxNode> nodes)
        {
            while (node != null)
            {
                nodes.Add(node);
                node = node.Parent;
            }
        }

        static void CheckErrors(Compilation compilation)
        {
            var errors = compilation.GetDiagnostics()
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();

            if (errors.Count > 0)
            {
                Console.WriteLine("Compilation errors:");
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }

                Console.WriteLine("Error, Exiting.");
                Environment.Exit(1);
                return;
            }
        }
    }

    public static IEnumerable<ISymbol> GetAllSymbols(Compilation compilation, SyntaxNode root)
    {
        var symbolCollector = new SymbolCollector(compilation.GetSemanticModel(root.SyntaxTree));
        symbolCollector.Visit(root);
        return symbolCollector.Symbols;
    }

    public static IEnumerable<ISymbol> CollectReferencedSymbols(Compilation compilation, ISymbol rootSymbol)
    {
        foreach (var syntaxRef in rootSymbol.DeclaringSyntaxReferences)
        {
            var syntaxToVisit = syntaxRef.GetSyntax();
            if (rootSymbol is IFieldSymbol)
            {
                var originalSyntax = syntaxToVisit;
                while (syntaxToVisit != null && !(syntaxToVisit is FieldDeclarationSyntax) && !(syntaxToVisit is EnumMemberDeclarationSyntax))
                {
                    syntaxToVisit = syntaxToVisit.Parent;
                }

                if (syntaxToVisit == null)
                {
                    throw new InvalidOperationException("Unable to recover field declaration");
                }
            }

            var symbols = GetAllSymbols(compilation, syntaxToVisit);
            foreach (var symbol in symbols)
            {
                yield return symbol;
            }
        }
    }

    private static bool IsTerraFX(ISymbol? symbol) => symbol != null && symbol.Kind != SymbolKind.Namespace &&
                                                      symbol.ContainingAssembly != null &&
                                                      symbol.ContainingAssembly.Identity.Name ==
                                                      "TerraFX.Interop.Windows";
    private static INamedTypeSymbol GetContainingType(ISymbol symbol)
    {
        if (symbol is INamedTypeSymbol namedType)
        {
            return namedType;
        }
        else
        {
            return symbol.ContainingType;
        }
    }


    private class RemoveUnusedNodes : CSharpSyntaxRewriter
    {
        private readonly Compilation _compilation;
        private readonly HashSet<SyntaxNode> _nodesToKeep;
        private readonly HashSet<SyntaxNode> _nodesToKeepRecursively;
        private readonly HashSet<string> _usingToKeep;
        private bool _visitingSpecial;

        public RemoveUnusedNodes(Compilation compilation, HashSet<SyntaxNode> nodesToKeep, HashSet<ISymbol> symbols) : base(false)
        {
            _compilation = compilation;
            _nodesToKeep = nodesToKeep;
            _nodesToKeepRecursively = new HashSet<SyntaxNode>();
            _usingToKeep = new HashSet<string>();
            foreach (var symbol in symbols)
            {
                var containingType = GetContainingType(symbol);
                _usingToKeep.Add(containingType.ContainingNamespace!.ToString()!);
                if (containingType.IsStatic || containingType.TypeKind == TypeKind.Enum)
                {
                    _usingToKeep.Add(containingType.ToString()!);
                }
            }

            foreach (var node in _nodesToKeep.ToList())
            {
                if (node is FileScopedNamespaceDeclarationSyntax fileScoped)
                {
                    _nodesToKeepRecursively.Add(fileScoped.Name);
                }
                else if (node is TypeDeclarationSyntax typeDeclarationSyntax)
                {
                    foreach (var syntax in typeDeclarationSyntax.ConstraintClauses)
                    {
                        _nodesToKeepRecursively.Add(syntax);
                    }

                    var typeParameterList = typeDeclarationSyntax.TypeParameterList;
                    if (typeParameterList != null)
                    {
                        _nodesToKeepRecursively.Add(typeParameterList);
                    }
                    KeepStandardAttributes(compilation, typeDeclarationSyntax.AttributeLists);

                    if (typeDeclarationSyntax.BaseList is { } baseList)
                    {
                        foreach (var item in baseList.Types)
                        {
                            _nodesToKeepRecursively.Add(item);
                        }
                    }

                    if (typeDeclarationSyntax is StructDeclarationSyntax structDeclarationSyntax)
                    {
                        KeepStandardAttributes(compilation, structDeclarationSyntax.AttributeLists);
                    }
                }
                else if (node is OperatorDeclarationSyntax operatorDeclarationSyntax)
                {
                    var kind = operatorDeclarationSyntax.OperatorToken.Kind();
                    if (kind == SyntaxKind.EqualsEqualsToken)
                    {
                        var parentType = (TypeDeclarationSyntax) node.Parent;
                        KeepRecursively(parentType.Members.OfType<OperatorDeclarationSyntax>()
                            .Where(x => x.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken)));
                    }
                    else if (kind == SyntaxKind.LessThanEqualsToken)
                    {
                        var parentType = (TypeDeclarationSyntax)node.Parent;
                        KeepRecursively(parentType.Members.OfType<OperatorDeclarationSyntax>()
                            .Where(x => x.OperatorToken.IsKind(SyntaxKind.GreaterThanEqualsToken)));
                    }
                    else if (kind == SyntaxKind.GreaterThanEqualsToken)
                    {
                        var parentType = (TypeDeclarationSyntax)node.Parent;
                        KeepRecursively(parentType.Members.OfType<OperatorDeclarationSyntax>()
                            .Where(x => x.OperatorToken.IsKind(SyntaxKind.LessThanEqualsToken)));
                    }
                    else if (kind == SyntaxKind.ExclamationEqualsToken)
                    {
                        var parentType = (TypeDeclarationSyntax) node.Parent;
                        KeepRecursively(parentType.Members.OfType<OperatorDeclarationSyntax>()
                            .Where(x => x.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken)));
                    }
                    else if (kind == SyntaxKind.LessThanToken)
                    {
                        var parentType = (TypeDeclarationSyntax)node.Parent;
                        KeepRecursively(parentType.Members.OfType<OperatorDeclarationSyntax>()
                            .Where(x => x.OperatorToken.IsKind(SyntaxKind.GreaterThanToken)));
                    }
                    else if (kind == SyntaxKind.GreaterThanToken)
                    {
                        var parentType = (TypeDeclarationSyntax)node.Parent;
                        KeepRecursively(parentType.Members.OfType<OperatorDeclarationSyntax>()
                            .Where(x => x.OperatorToken.IsKind(SyntaxKind.LessThanToken)));
                    }
                }
            }
        }

        private void KeepStandardAttributes(Compilation compilation, SyntaxList<AttributeListSyntax> attributeLists)
        {
            foreach (var attrList in attributeLists)
            {
                _nodesToKeepRecursively.Add(attrList);
            }
        }

        private void KeepRecursively(IEnumerable<SyntaxNode> nodes)
        {
            foreach (var syntaxNode in nodes)
            {
                _nodesToKeepRecursively.Add(syntaxNode);
            }
        }

        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            SyntaxNode? result = node;

            if (node != null)
            {
                if (node is AttributeSyntax syntax)
                {
                    var semanticModel = _compilation.GetSemanticModel(syntax.SyntaxTree);
                    var symbol = semanticModel.GetSymbolInfo(syntax).Symbol;
                    if (IsTerraFX(symbol))
                    {
                        result = null;
                    }
                }
                else if (_visitingSpecial)
                {
                    result = base.Visit(node);
                }
                else
                {
                    var kind = node.Kind();
                    switch (kind)
                    {
                        case SyntaxKind.FieldDeclaration:
                        case SyntaxKind.MethodDeclaration:
                        case SyntaxKind.ConstructorDeclaration:
                        case SyntaxKind.OperatorDeclaration:
                        case SyntaxKind.ConversionOperatorDeclaration:
                        case SyntaxKind.PropertyDeclaration:
                        case SyntaxKind.IndexerDeclaration:
                        case SyntaxKind.EnumDeclaration:
                            if (!_nodesToKeep.Contains(node) && !_nodesToKeepRecursively.Contains(node))
                            {
                                result = null;
                            }
                            else
                            {
                                _visitingSpecial = true;
                                result = base.Visit(node);
                                _visitingSpecial = false;
                            }

                            break;

                        case SyntaxKind.UsingDirective:
                            if (node is UsingDirectiveSyntax usingDirective)
                            {
                                var usingName = usingDirective.Name.ToString();
                                if (_usingToKeep.Contains(usingName) || usingName.StartsWith("System"))
                                {
                                    result = node;
                                }
                                else
                                {
                                    result = null;
                                }
                            }

                            break;
                        case SyntaxKind.QualifiedName:
                            break;
                        default:
                            var keepRecursively = _nodesToKeepRecursively.Contains(node);
                            if (keepRecursively)
                            {
                                _visitingSpecial = true;
                                result = base.Visit(node);
                                _visitingSpecial = false;
                            }
                            else
                            {
                                if (_nodesToKeep.Contains(node))
                                {
                                    node = base.Visit(node);
                                    result = node;

                                    if (node is TypeDeclarationSyntax typeDeclarationSyntax &&
                                        typeDeclarationSyntax.Members.Count == 0)
                                    {
                                        result = null;
                                    }
                                    else if (node is BaseNamespaceDeclarationSyntax ns && ns.Members.Count == 0)
                                    {
                                        result = null;
                                    }
                                    else if (node is CompilationUnitSyntax cu &&
                                             !cu.Members.OfType<BaseNamespaceDeclarationSyntax>().Any())
                                    {
                                        result = null;
                                    }
                                }
                                else
                                {
                                    result = null;
                                }
                            }

                            break;
                    }
                }
            }

            if (result is MemberDeclarationSyntax memberDeclaration)
            {
                while (true)
                {
                    var attrToRemove = memberDeclaration.AttributeLists.FirstOrDefault(x => x.Attributes.Count == 0);
                    if (attrToRemove == null) break;
                    memberDeclaration = memberDeclaration.WithAttributeLists(memberDeclaration.AttributeLists.Remove(attrToRemove));
                }
                result = memberDeclaration;
            }
            else if (result is ParameterSyntax parameterSyntax)
            {
                while (true)
                {
                    var attrToRemove = parameterSyntax.AttributeLists.FirstOrDefault(x => x.Attributes.Count == 0);
                    if (attrToRemove == null) break;
                    parameterSyntax = parameterSyntax.WithAttributeLists(parameterSyntax.AttributeLists.Remove(attrToRemove));
                }
                result = parameterSyntax;
            }
            else if (result is LocalFunctionStatementSyntax localFunctionStatement)
            {
                while (true)
                {
                    var attrToRemove = localFunctionStatement.AttributeLists.FirstOrDefault(x => x.Attributes.Count == 0);
                    if (attrToRemove == null) break;
                    localFunctionStatement = localFunctionStatement.WithAttributeLists(localFunctionStatement.AttributeLists.Remove(attrToRemove));
                }
                result = localFunctionStatement;
            }

            // TODO: Not sure why we can have some types without a parent that are still contained in a namespace?
            if (result is BaseTypeDeclarationSyntax decl && (decl.Parent is BaseNamespaceDeclarationSyntax || decl.Parent == null))
            {

                var internalToken = SyntaxFactory.Token(SyntaxKind.InternalKeyword);
                var modifierIndex = decl.Modifiers.IndexOf(SyntaxKind.PublicKeyword);
                if (modifierIndex >= 0)
                {

                    internalToken = internalToken.WithTrailingTrivia(SyntaxFactory.Space);
                    var modifiers = decl.Modifiers.RemoveAt(modifierIndex);
                    modifiers = modifiers.Insert(modifierIndex, internalToken);
                    decl = decl.WithModifiers(modifiers);
                    result = decl;
                }
            }

            return result;
        }
    }

    private class SymbolCollector : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        public SymbolCollector(SemanticModel semanticModel) : base(SyntaxWalkerDepth.Node)
        {
            _semanticModel = semanticModel;
            Symbols = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        }
        public HashSet<ISymbol> Symbols { get; }

        public override void Visit(SyntaxNode? node)
        {
            if (node == null) return;

            // Don't collect symbols from comments
            if (node is XmlNodeSyntax) return;
            if (node is CrefSyntax) return;

            if (node is ArgumentSyntax castExpr && node.ToString().Contains("CREATESTRUCTW"))
            {
                var test1 = _semanticModel.GetConversion(node);
            }

            // NOTE: _semanticModel.GetConversion doesn't catch cast to void* that are user-defined implicit
            // so we use operation here that seems to better handle this
            // var conversion = _semanticModel.GetConversion(node);
            var operation = _semanticModel.GetOperation(node);
            if (operation != null)
            {
                CollectConversionsFromOperations(operation);
            }

            var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
            if (IsTerraFX(symbol))
            {
                // Don't pickup any attributes from TerraFX
                if (node is AttributeSyntax)
                {
                    return;
                }

                if (symbol is IParameterSymbol parameterSymbol)
                {
                    if (IsTerraFX(parameterSymbol.Type))
                    {
                        // Only record parameter type
                        symbol = parameterSymbol.Type;
                    }
                    else
                    {
                        return;
                    }
                }

                Symbols.Add(symbol!);
            }

            base.Visit(node);
        }

        private void CollectConversionsFromOperations(IOperation operation)
        {
            if (operation is IConversionOperation convert && convert.OperatorMethod != null && IsTerraFX(convert.OperatorMethod))
            {
                Symbols.Add(convert.OperatorMethod!);
            }

            foreach (var childOp in operation.ChildOperations)
            {
                CollectConversionsFromOperations(childOp);
            }
        }
    }
}