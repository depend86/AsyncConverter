using System;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace AsyncConverter
{
    public class UnitTestCodeWriter
    {
        public void AppendUnitTestCodeToExistingSourceFile([NotNull] string sourceFilePath, [NotNull] string unitTestCode, [NotNull] string projectName, [NotNull] string classNamespace)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(sourceFilePath));
            if (string.IsNullOrWhiteSpace(unitTestCode))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(unitTestCode));
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(projectName));
            if (string.IsNullOrWhiteSpace(classNamespace))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(classNamespace));

            var fileInfo = new FileInfo(sourceFilePath);
            if (!fileInfo.Exists)
                throw new IOException($"File '{sourceFilePath}' not found.");

            var sourceCodeLines = File.ReadAllLines(sourceFilePath, Encoding.UTF8);
            var n = sourceCodeLines.Length;

            var namespaceClose = false;
            var testClassClose = false;
            while (n > 0)
            {
                if (namespaceClose && testClassClose)
                {
                    var newSourceCode = string.Concat(string.Join(Environment.NewLine, sourceCodeLines.Take(n)), Environment.NewLine, Environment.NewLine, unitTestCode, "\t}", Environment.NewLine, "}");
                    File.WriteAllText(sourceFilePath, newSourceCode, Encoding.UTF8);

                    break;
                }

                n--;

                var line = sourceCodeLines[n].Trim();
                if (line.Equals("}"))
                {
                    if (!namespaceClose)
                        namespaceClose = true;
                    else
                        testClassClose = true;
                }
            }
        }
    }
}