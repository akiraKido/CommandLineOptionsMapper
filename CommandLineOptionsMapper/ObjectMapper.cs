using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommandLineOptionsMapper
{
    public class ObjectMapper<T>
    {
        private readonly string[] _prefixes = { "-" };
        private readonly Type _targetType = typeof(T);

        public ObjectMapper(params string[] prefixes)
        {
            CheckConsoleOptionsAttribute();
            _prefixes = _prefixes.Concat(prefixes).ToArray();
        }

        public ObjectMapper(IEnumerable<string> prefixes)
        {
            CheckConsoleOptionsAttribute();
            _prefixes = _prefixes.Concat(prefixes).ToArray();
        }

        private void CheckConsoleOptionsAttribute()
        {
            // check that T is ConsoleOptions
            Attribute.GetCustomAttribute(_targetType, typeof(CommandLineAttribute));
        }

        /// <summary>
        /// Maps command line arguments to the given instance.
        /// </summary>
        /// <param name="target">Instance to set the command line arguments</param>
        /// <param name="arguments"></param>
        public void MapTo(T target, IEnumerable<string> arguments)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            (var optionValues, var propertyInfos) = GetOptionValues(arguments);

            foreach (var optionValue in optionValues)
            {
                (string option, object value) = (optionValue.Key, optionValue.Value);
                var propertyInfo = propertyInfos.First(pi => pi.Value.ShortName == option
                                                             || pi.Value.LongName == option);
                propertyInfo.Key.SetValue(target, value);
            }
        }

        /// <summary>
        /// Maps command line arguments to a new object of type T.
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public T Map(IEnumerable<string> arguments)
        {
            var result = Activator.CreateInstance<T>();
            MapTo(result, arguments);
            return result;
        }

        public T ImmutableMap(IEnumerable<string> arguments)
        {
            var optionValues = GetOptionValues(arguments).optionValues;
            var constructors = _targetType.GetConstructors();

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

            var parameters = targetConstructor.Parameters;
            var parameterValues = parameters.Select(parameter => optionValues[parameter.Name]).ToArray();
            return (T)targetConstructor.ConstructorInfo.Invoke(parameterValues);
        }

        private (Dictionary<string, object> optionValues, Dictionary<PropertyInfo, OptionArgumentAttribute> propertyInfos)
            GetOptionValues(IEnumerable<string> arguments)
        {
            var optionValues = new Dictionary<string, object>();
            var propertyInfos = new Dictionary<PropertyInfo, OptionArgumentAttribute>();

            var argumentInfos = _targetType.GetProperties()
                .Select(property => new
                {
                    PropertyInfo = property,
                    AgrumentAttribute = (OptionArgumentAttribute)Attribute.GetCustomAttribute(property, typeof(OptionArgumentAttribute))
                })
                .Where(memberInfo => memberInfo.AgrumentAttribute != null);

            // skip command
            var command = arguments.First();
            if (_prefixes.Any(prefix => command.StartsWith(prefix)) == false) arguments = arguments.Skip(1);

            using (var argumentEnumerator = arguments.GetEnumerator())
            {
                while (argumentEnumerator.MoveNext())
                {
                    // check that its an argument
                    var prefix = _prefixes.Where(p => argumentEnumerator.Current.StartsWith(p))
                        .Aggregate("", (max, current) => max.Length > current.Length ? max : current);
                    var argument = argumentEnumerator.Current.Substring(prefix.Length); // remove -
                    var selectedArgument = argumentInfos.First(info => info.AgrumentAttribute.ShortName == argument
                                                                       || info.AgrumentAttribute.LongName == argument);
                    var propertyInfo = _targetType.GetProperty(selectedArgument.PropertyInfo.Name);

                    // check that set argument is not a duplicate
                    if (propertyInfos.ContainsKey(propertyInfo)) throw new Exception($"duplicate: {argument}");
                    propertyInfos.Add(selectedArgument.PropertyInfo, selectedArgument.AgrumentAttribute);

                    var propertyType = selectedArgument.PropertyInfo.PropertyType;
                    if (propertyType == typeof(string))
                    {
                        // move enumerator once for value
                        argumentEnumerator.MoveNext();
                        var value = argumentEnumerator.Current;

                        //propertyInfo.SetValue(target, value);
                        optionValues[argument] = value;
                    }
                    else if (propertyType == typeof(bool))
                    {
                        //propertyInfo.SetValue(target, true);
                        optionValues[argument] = true;
                    }
                    else
                    {
                        throw new Exception($"bool or string expected, found {propertyType.Name}");
                    }
                }
            }

            return (optionValues, propertyInfos);
        }
    }
}
