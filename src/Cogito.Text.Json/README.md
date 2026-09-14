# Cogito.Text.Json

Compiles a `JsonElement` into an equality test, so comparing many documents against one template is
fast.

## Why

Comparing `JsonElement` values means walking both documents property by property. When the expected
document is fixed and the incoming ones are many, that walk can be compiled away: build an expression
tree from the template once, and each comparison is straight-line code.

## Install

```shell
dotnet add package Cogito.Text.Json
```

## Use

```csharp
var builder = new JsonElementEqualityExpressionBuilder();
var compare = builder.Build(expected).Compile();

if (compare(incoming))
    Handle(incoming);
```

`JsonElementEqualityExpressionBuilderSettings` controls how the comparison treats things like
property order and number representation.

This is the `System.Text.Json` counterpart to `Cogito.Json`.

## License

MIT.
