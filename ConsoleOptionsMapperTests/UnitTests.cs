using System;
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

            var instance = new Foo();

            ConsoleExecuter.Execute(command.Split(" "), instance);

            Assert.Equal("test1", instance.A);
            Assert.Equal("test2", instance.B);
            Assert.True(instance.C);
            Assert.Equal("test1test2", instance.Result);
        }

        [Fact]
        public void DuplicateTest()
        {
            var command = "test -a test1 -a test2";
            Assert.Throws<Exception>(() => ConsoleExecuter.Execute<Foo>(command.Split(" ")));
        }

        [Fact]
        public void ExecutionTest()
        {
            var command = "test -a test";
            ConsoleExecuter.Execute<Foo>( command.Split( " " ) );
        }
    }

    [ConsoleOptions]
    public class Foo
    {
        internal string Result { get; private set; }

        [OptionArgument(ShortName = "a", NeedsValue = true)]
        public string A { get; set; }

        [OptionArgument(ShortName = "b", NeedsValue = true)]
        public string B { get; set; }

        [OptionArgument(ShortName = "c")]
        public bool C { get; set; }

        [Option(Name = "test")]
        public void Test()
        {
            Result = $"{A}{B}";
        }
    }
}
