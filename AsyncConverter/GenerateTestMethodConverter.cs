using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.JavaScript.Resolve;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;

namespace AsyncConverter
{
    [ContextAction(Group = "C#", Name = "GenerateTestMethod", Description = "Generate test method.")]
    public class GenerateTestMethodConverter : ContextActionBase
    {
        private ICSharpContextActionDataProvider Provider { get; }

        public GenerateTestMethodConverter(ICSharpContextActionDataProvider provider)
        {
            Provider = provider;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var method = GetMethodFromCarretPosition();

            var currentClassInfo = GetCurrentClassInfo(method);
            if (currentClassInfo == null)
                return null;

            /*
            var solutionItems = solution.GetAllProjects()
                .SelectMany(x => x.GetAllProjectFiles())
                .ToList();

            ToLog(string.Join(Environment.NewLine,
                solutionItems
                .Where(item => IsTestsFile(item, currentClassInfo))
                .Select(x => x.Location.FullPath)));
            */

            var testFile = GetTestFilePath(currentClassInfo);
            if (!string.IsNullOrWhiteSpace(testFile) && File.Exists(testFile))
            {
                var unitTestCodeGenerator = new UnitTestCodeGenerator();
                var unitTestCode = unitTestCodeGenerator.Generate(currentClassInfo.ClassName, currentClassInfo.MethodName, currentClassInfo.ConstructorParametersTypes);

                var unitTestCodeWriter = new UnitTestCodeWriter();
                unitTestCodeWriter.AppendUnitTestCodeToExistingSourceFile(testFile, unitTestCode, currentClassInfo.ProjectName, currentClassInfo.ClassNamespace);
            }

            return null;
        }

        private bool IsTestsFile(IProjectItem projectItem, CurrentClassInfo currentClassInfo)
        {
            var testFile = GetTestFilePath(currentClassInfo);
            if (string.IsNullOrWhiteSpace(testFile))
                return false;

            return projectItem.Kind == ProjectItemKind.PHYSICAL_FILE && File.Exists(testFile) && projectItem.Location.FullPath.Equals(testFile, StringComparison.InvariantCultureIgnoreCase);
        }

        private string GetTestFilePath(CurrentClassInfo currentClassInfo)
        {
            var projectDir = GetProjectFolder(currentClassInfo);
            if (string.IsNullOrWhiteSpace(projectDir))
                return null;

            var projectDirDi = new DirectoryInfo(projectDir);
            if (!projectDirDi.Exists)
                return null;

            var testProjectName = $"{currentClassInfo.ProjectName}.Tests";
            var testFileNs = string.Join(Path.DirectorySeparatorChar.ToString(), currentClassInfo.ClassNamespace.Split(new[] { '.' }, StringSplitOptions.None));

            var fnWithoutExt = Path.GetFileNameWithoutExtension(currentClassInfo.SourceFilePath);
            var testFileName = $"{fnWithoutExt}Tests.cs";
            
            var testFile = Path.Combine(projectDirDi.Parent.FullName, testProjectName, testFileNs, testFileName);

            return testFile;
        }

        private string GetProjectFolder(CurrentClassInfo currentClassInfo)
        {
            var sourceFilePathFi = new FileInfo(currentClassInfo.SourceFilePath);
            var directory = sourceFilePathFi.Directory;
            var ni = currentClassInfo.ClassNamespace.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Reverse();

            for (var i = 0; i < ni.Count; ++i)
            {
                if (directory.Name.Equals(ni[i], StringComparison.InvariantCultureIgnoreCase))
                    directory = directory.Parent;
                else
                    return null;
            }

            return directory.FullName;
        }

        private CurrentClassInfo GetCurrentClassInfo(IMethodDeclaration method)
        {
            var methodDeclaredElement = method?.DeclaredElement;
            if (methodDeclaredElement == null)
                return null;

            var @class = methodDeclaredElement.GetContainingType();
            if (@class == null)
                return null;

            var constructor = @class.Constructors.FirstOrDefault();
            if (constructor == null)
                return null;

            var sourceFile = @class.GetSingleOrDefaultSourceFile();
            if (sourceFile == null)
                return null;


            return new CurrentClassInfo
                   {
                       MethodName = methodDeclaredElement.ShortName,
                       ClassName = @class.ShortName,
                       SourceFilePath = sourceFile.GetLocation().FullPath,
                       ProjectName = sourceFile.GetContainingProject().Name,
                       ClassNamespace = @class.GetContainingNamespace().ShortName,
                       ConstructorParametersTypes = constructor.Parameters.Select(x => new Tuple<string, string, bool>(x.Type.GetPresentableName(CSharpLanguage.Instance), x.ShortName, x.Type.IsInterfaceType()))
                   };
        }

        private void ToLog(string text)
        {
            File.AppendAllText(@"c:\temp\tt.txt", text + Environment.NewLine);
        }

        public override string Text { get; } = "Generate test method.";
        public override bool IsAvailable(IUserDataHolder cache)
        {
            var method = GetMethodFromCarretPosition();

            var currentClassInfo = GetCurrentClassInfo(method);
            if (currentClassInfo == null)
                return false;

            var testFile = GetTestFilePath(currentClassInfo);
            return !string.IsNullOrWhiteSpace(testFile) && File.Exists(testFile);
        }

        [CanBeNull]
        private IMethodDeclaration GetMethodFromCarretPosition()
        {
            var identifier = Provider.TokenAfterCaret as ICSharpIdentifier;
            identifier = identifier ?? Provider.TokenBeforeCaret as ICSharpIdentifier;
            return identifier?.Parent as IMethodDeclaration;
        }
    }
}