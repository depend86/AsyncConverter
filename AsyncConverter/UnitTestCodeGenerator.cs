using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace AsyncConverter
{
    public class UnitTestCodeGenerator
    {
        public string Generate([NotNull] string className, [NotNull] string sourceMethodName, [NotNull] IEnumerable<Tuple<string,string,bool>> constructorDependencies)
        {
            if (constructorDependencies == null)
                throw new ArgumentNullException(nameof(constructorDependencies));
            if (string.IsNullOrWhiteSpace(className))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(className));
            if (string.IsNullOrWhiteSpace(sourceMethodName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(sourceMethodName));

            var sutTypeName = className;
            var sutPropertyName = "sut";

            var testMethodName = sourceMethodName + "Test";

            var testMockParameters = constructorDependencies.Select(dependencyTypeName =>
            {
                var mockPropertyName = "mock" + (dependencyTypeName.Item3 ? dependencyTypeName.Item1.Substring(1) : dependencyTypeName.Item1);
                return "\t\t\t[Frozen] Mock<" + dependencyTypeName.Item1 + "> " + mockPropertyName;
            }).Concat(new[] { "\t\t\t" + sutTypeName + " " + sutPropertyName + ")" });

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("\t\tpublic void " + testMethodName + "(");
            stringBuilder.Append(string.Join("," + Environment.NewLine, testMockParameters));
            stringBuilder.AppendLine(Environment.NewLine + "\t\t{");
            stringBuilder.AppendLine("\t\t\t// arrange");
            stringBuilder.AppendLine("\t\t\t");
            stringBuilder.AppendLine("\t\t\t// act");
            stringBuilder.AppendLine("\t\t\tvar result = sut." + sourceMethodName + "();");
            stringBuilder.AppendLine("\t\t\t");
            stringBuilder.AppendLine("\t\t\t// assert");
            stringBuilder.AppendLine("\t\t\t");
            stringBuilder.AppendLine("\t\t}");
            return stringBuilder.ToString();
        }
    }
}