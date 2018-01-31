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

            SetProperties(targetType, ref target, arguments.Skip(1));
            ExecuteCommand(targetType, target, arguments.First());
        }

        private static void SetProperties<T>(Type targetType, ref T target, IEnumerable<string> arguments)
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
                var propertyInfos = new Dictionary<PropertyInfo, OptionArgumentAttribute>();
                var optionValues = new Dictionary<string, object>();
                while (argumentEnumerator.MoveNext())
                {
                    // check that its an argument
                    if (argumentEnumerator.Current[0] != '-') throw new Exception("expected - for agrument");
                    var argument = argumentEnumerator.Current.Substring(1); // remove -
                    var selectedArgument = argumentInfos.First(info => info.AgrumentAttribute.ShortName == argument
                                                                       || info.AgrumentAttribute.LongName == argument);
                    var propertyInfo = targetType.GetProperty(selectedArgument.PropertyInfo.Name);

                    // check that set argument is not a duplicate
                    if (propertyInfos.ContainsKey(propertyInfo)) throw new Exception($"duplicate: {argument}");
                    propertyInfos.Add(selectedArgument.PropertyInfo, selectedArgument.AgrumentAttribute);

                    if (selectedArgument.AgrumentAttribute.NeedsValue)
                    {
                        // move enumerator once for value
                        argumentEnumerator.MoveNext();
                        var value = argumentEnumerator.Current;

                        //propertyInfo.SetValue(target, value);
                        optionValues[argument] = value;
                    }
                    else
                    {
                        //propertyInfo.SetValue(target, true);
                        optionValues[argument] = true;
                    }
                }

                var constructors = targetType.GetConstructors();
                var targetConstructor = constructors
                    .Select(constructor => new
                    {
                        ConstructorInfo = constructor,
                        Parameters = constructor.GetParameters()
                    })
                    .FirstOrDefault(constructorInfo =>
                   {
                       var constructorParameters = constructorInfo.Parameters;
                       if (constructorParameters.Length != optionValues.Count) return false;

                       var parameterNames = constructorParameters.Select(p => p.Name);
                       return optionValues.Keys.All(option => parameterNames.Contains(option));
                   });
                if (targetConstructor == null)
                {
                    foreach (var optionValue in optionValues)
                    {
                        (string option, object value) = (optionValue.Key, optionValue.Value);
                        var propertyInfo = propertyInfos.First(pi => pi.Value.ShortName == option
                                                                     || pi.Value.LongName == option);
                        propertyInfo.Key.SetValue(target, value);
                    }
                }
                else
                {
                    var parameters = targetConstructor.Parameters;
                    var parameterValues = parameters.Select(parameter => optionValues[parameter.Name]).ToArray();
                    target = (T)targetConstructor.ConstructorInfo.Invoke(parameterValues);
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
