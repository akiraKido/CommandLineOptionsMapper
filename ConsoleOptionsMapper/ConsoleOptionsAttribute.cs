using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ConsoleOptionsMapper
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConsoleOptionsAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class OptionArgumentAttribute : Attribute
    {
        public string ShortName { get; set; }
        public string LongName { get; set; }
        public bool NeedsValue { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OptionAttribute : Attribute
    {
        public string Name { get; set; }
    }

    public static class ConsoleExecuter
    {
        public static void Execute<T>(IEnumerable<string> arguments, T instance = default) where T : new()
        {
            var targetType = typeof(T);

            // check that T is ConsoleOptions
            Attribute.GetCustomAttribute(targetType, typeof(ConsoleOptionsAttribute));

            var target = instance == null || instance.Equals(default) ? new T() : instance;

            SetProperties(targetType, target, arguments.Skip(1));
            ExecuteCommand(targetType, target, arguments.First());
        }

        private static void SetProperties<T>(Type targetType, T target, IEnumerable<string> arguments)
        {
            var argumentInfos = targetType.GetProperties()
                .Select(property => new
                {
                    PropertyInfo = property,
                    AgrumentAttribute = (OptionArgumentAttribute)Attribute.GetCustomAttribute(property, typeof(OptionArgumentAttribute))
                })
                .Where(memberInfo => memberInfo.AgrumentAttribute != null);

            using (var argumentEnumerator = arguments.GetEnumerator())
            {
                var histroy = new HashSet<PropertyInfo>();
                while (argumentEnumerator.MoveNext())
                {
                    // check that its an argument
                    if (argumentEnumerator.Current[0] != '-') throw new Exception("expected - for agrument");
                    var argument = argumentEnumerator.Current.Substring(1); // remove -
                    var selectedArgument = argumentInfos.First(info => info.AgrumentAttribute.ShortName == argument
                                                                       || info.AgrumentAttribute.LongName == argument);
                    var propertyInfo = targetType.GetProperty(selectedArgument.PropertyInfo.Name);

                    // check that set argument is not a duplicate
                    if (histroy.Contains(propertyInfo)) throw new Exception($"duplicate: {argument}");
                    histroy.Add(propertyInfo);

                    if (selectedArgument.AgrumentAttribute.NeedsValue)
                    {
                        // move enumerator once for value
                        argumentEnumerator.MoveNext();
                        var value = argumentEnumerator.Current;

                        propertyInfo.SetValue(target, value);
                    }
                    else
                    {
                        propertyInfo.SetValue(target, true);
                    }
                }
            }
        }

        private static void ExecuteCommand<T>(Type targetType, T target, string command)
        {
            var options = targetType.GetMethods()
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
