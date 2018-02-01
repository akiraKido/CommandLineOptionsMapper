using System;
using System.Linq;
using ConsoleOptionsMapper;
using Xunit;

namespace ConsoleOptionsMapperTests
{
    public class UnitTests
    {
        [Fact]
        public void GeneralTest()
        {
            var command = "test -a test1 -b test2 -c";

            var mapper = new ObjectMapper<Foo>();
            var instance = mapper.Map(command.Split(" "));

            Assert.Equal("test1", instance.A);
            Assert.Equal("test2", instance.B);
            Assert.True(instance.C);
        }

        [Fact]
        public void DuplicateTest()
        {
            var command = "test -a test1 -a test2";

            var mapper = new ObjectMapper<Foo>();
            Assert.Throws<Exception>(() => mapper.Map(command.Split(" ")));
        }

        [Fact]
        public void InvalidArgumentsTest()
        {
            var command = "test test".Split( " " );
            Assert.ThrowsAny<Exception>( () => new ObjectMapper<Foo>().Map( command ) );
        }

        [Fact]
        public void ImmutableConstructorTest()
        {
            var command = "test -a test1 -b test2";

            var mapper = new ObjectMapper<Bar>();
            var instance = mapper.ImmutableMap(command.Split(" "));

            Assert.Equal("test1", instance.A);
            Assert.Equal("test2", instance.B);
        }

        [Fact]
        public void ExecutionTest()
        {
            var command = "test -a test".Split(" ");

            var mapper = new ObjectMapper<Foo>();
            var target = mapper.Map(command);

            var runner = new CommandLineRunner<Foo>();
            runner.Run(target, command.First());

            Assert.Equal("test", target.Result);
        }

        [Fact]
        public void SimpleExecutionTest()
        {
            var command = "test -a test1 -b test2".Split(" ");
            new CommandLineRunner<Foo>().Run(command);
            new CommandLineRunner<Bar>().Run(command);
        }

    }

    [CommandLine]
    public class Foo
    {
        internal string Result { get; private set; }

        [OptionArgument(ShortName = "a")]
        public string A { get; set; }

        [OptionArgument(ShortName = "b")]
        public string B { get; set; }

        [OptionArgument(ShortName = "c")]
        public bool C { get; set; }

        [Option(Name = "test")]
        public void Test()
        {
            Result = $"{A}{B}";
        }
    }

    [CommandLine]
    public class Bar
    {
        internal string Result { get; private set; }

        [OptionArgument(ShortName = "a")]
        public string A { get; }
        [OptionArgument(ShortName = "b")]
        public string B { get; }

        public Bar(string a, string b)
        {
            A = a;
            B = b;
        }

        [Option(Name = "test")]
        public void Test()
        {
            Result = $"{A}{B}";
        }
    }
}
