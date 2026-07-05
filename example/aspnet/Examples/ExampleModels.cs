namespace NepaliUtilityDemo.AspNet.Examples;

/// <summary>One API call demonstrated: the member name, the exact code that produced it, and its result.</summary>
public sealed record ExampleItem(string Member, string Code, string Result);

/// <summary>A group of related API calls from a single NepaliUtility.Core type.</summary>
public sealed record ExampleSection(string Namespace, string Title, string Description, IReadOnlyList<ExampleItem> Items);
