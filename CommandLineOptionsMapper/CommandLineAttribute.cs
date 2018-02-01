using System;

namespace CommandLineOptionsMapper
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandLineAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class OptionArgumentAttribute : Attribute
    {
        public string ShortName { get; set; }
        public string LongName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OptionAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
