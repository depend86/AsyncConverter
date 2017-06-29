using System;
using System.Collections.Generic;

namespace AsyncConverter
{
    public class CurrentClassInfo
    {
        public string MethodName { get; set; }
        public string ClassName { get; set; }
        public string SourceFilePath { get; set; }
        public IEnumerable<Tuple<string,string,bool>> ConstructorParametersTypes { get; set; }
        public string ProjectName { get; set; }
        public string ClassNamespace { get; set; }
    }
}