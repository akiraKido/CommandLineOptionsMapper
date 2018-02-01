using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleOptionsMapper
{
    public class CommandLineRunner<T>
    {
        private readonly IEnumerable<string> _prefixes;
        private readonly Type _targetType = typeof(T);

        public CommandLineRunner(params string[] prefixes)
        {
            _prefixes = prefixes;
        }

        public void Run(IEnumerable<string> arguments)
        {
            T target;
            var mapper = new ObjectMapper<T>(_prefixes);
            try
            {
                target = mapper.Map(arguments);
            }
            catch
            {
                target = mapper.ImmutableMap(arguments);
            }

            Run(target, arguments.First());
        }

        public void Run(T target, string command)
        {
            var options = _targetType.GetMethods()
                .Select(methodInfo => new
                {
                    MethodInfo = methodInfo,
                    OptionAttribute = (OptionAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(OptionAttribute))
                })
                .Where(memberInfo => memberInfo.OptionAttribute != null);

            var selectedMethod = options.First(memberInfo => memberInfo.OptionAttribute.Name == command);
            selectedMethod.MethodInfo.Invoke(target, null);
        }
    }
}
