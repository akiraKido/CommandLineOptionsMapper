# Console Options Mapper

This project will map options into properties.

Or just use [Microsoft.Extensions.Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?tabs=basicconfiguration#commandline-configuration-provider).

## Quick start

```csharp
    [ConsoleOptions]
    public class Foo
    {
        [OptionArgument(ShortName = "a", NeedsValue = true)]
        public string A { get; set; }

        [OptionArgument(ShortName = "b", NeedsValue = true)]
        public string B { get; set; }

        [OptionArgument(ShortName = "c")]
        public bool C { get; set; }

        [Option(Name = "test")]
        public void Test()
        {
			// Do something using A, B, and C
        }
    }

	public static class Program
	{
		public static void Main(string[] args)
		{
			ConsoleExecuter.Execute<Foo>(args);
		}
	}
```

```
> someProgram test -a someValue -b someOtherValue -c
```

This will invoke `Test()` with `"someValue"` set in property `A`, `"someOtherValue"` set in property `B`, and true set in property `C`.

You may also use Immutable Models.

```csharp
    [ConsoleOptions]
    public class Bar
    {
        [OptionArgument(ShortName = "a", NeedsValue = true)]
        public string A { get; }
        [OptionArgument(ShortName = "b", NeedsValue = true)]
        public string B { get; }

        public Bar(string a, string b)
        {
            A = a;
            B = b;
        }

        [Option(Name = "test")]
        public void Test()
        {
			// Do something
        }
    }
```

Make sure that the constructor's parameter names and the short names / long names match.

## Details

### ConsoleOptions

- Add this attribute to the class you want to use as the master.
	- If this attribute does not exist, the `ConsoleExecuter` will throw an exception.

### OptionArgument

- Add this attribute to the properties you want to use as arguments.
	- Use `string` for arguments with `NeedsAttribute`
	- Use `bool` for argument without `NeedsAttribute`
	- Set either `ShortName` or `LongName`. These currently do not have any restrictions.
	- Currently, `OptionArguments` **MUST be a `public` `property` with both getter and a setter**.

### Option

- Add this attribute to methods you want to set as options.
	- Currenlty, these methods cannot have any arguments of their own.
