# RenwordDigital.StringSearchEngine

A lightweight, embeddable, general-purpose search index for C# applications.

The main idea is simple:

> Build the index once, search many times.

Instead of scanning every resource with `string.Contains()` on every query, the library precomputes searchable keys and stores them in an inverted index. This makes repeated search queries much faster for mostly-static datasets.

This is not intended to be a universal replacement for `string.Contains()`. It is a specialized data structure for cases where resources are stable, search is frequent, and predictable query performance matters.

---

## Why this exists

A common search implementation looks like this:

```csharp
foreach (var resource in resources)
{
    if (resource.SearchString.Contains(query))
        results.Add(resource);
}
```

This is simple and works well for small datasets.

However, when the dataset grows and search happens often, for example in a command palette, asset browser, product list, settings search, file search, or autocomplete UI, scanning every string on every input becomes wasteful.

`SearchIndex` solves this by building a lookup structure ahead of time:

```csharp
Dictionary<string, List<Resource>>
```

At search time, a query can be resolved with dictionary lookups instead of a full linear scan.

---

## Core use case

This index is optimized for:

- mostly-static resource collections;
- frequent search queries;
- rare or controlled resource changes;
- incremental additions/removals over time;
- fast lookup after the index has been built.

Good fit:

```text
Create resources -> Build index -> Search many times -> Rarely add/remove resources
```

Bad fit:

```text
Frequently rebuild the whole dataset -> Search once or twice -> Rebuild again
```

If your data changes constantly or is fully replaced often at runtime, this index should not be used. In that case, a direct scan with `string.Contains()` may be simpler and more predictable.

---

## Performance model

A naive search with `string.Contains()` usually behaves like this:

```text
O(resourceCount * averageStringLength)
```

A built search index behaves closer to:

```text
O(queryLength + resultCount)
```

In the right scenario, after the index is built, search can be significantly faster than scanning all resources.

Depending on dataset size, query frequency, and result count, this can be anywhere from a small improvement to a 10x-1000x speedup over repeated `Contains()` checks.

The tradeoff is that the index has a build cost and uses additional memory.

---

## Resource model

A basic resource contains a display name and a searchable string:

```csharp
public class Resource
{
    private string _name;
    private string _searchString;

    public string Name => _name;
    public string SearchString => _searchString;

    public Resource(string name)
    {
        _name = name;
        _searchString = name;
    }

    public Resource(string name, string searchString)
    {
        _name = name;
        _searchString = searchString;
    }
}
```

Resources are treated as immutable search entries.

If the searchable text must change, remove the old resource and add a new one.

```csharp
index.RemoveResource(oldResource);
index.AddResource(new Resource("Cyber Angel", "Cyber-Angel angel fighter resistance android"));
```

Important behavior:

- `Resource` is compared by reference unless equality is overridden.
- Two different `Resource` instances with the same name are treated as different resources.
- `Name` and `SearchString` should be non-null.
- Search data should not be mutated outside the index.

---

## Existing index idea

The current implementation generates substring keys for every resource and stores them in a dictionary:

```csharp
Dictionary<string, List<Resource>> _searchCache;
```

For a resource like:

```text
Cyber Angel
```

the index can store searchable parts such as:

```text
cyber
cybe
cbe...
angel
ange
ngel
...
```

Then a query can be resolved by checking whether the key exists in the dictionary.

```csharp
if (_searchCache.TryGetValue(token, out var resources))
{
    results.AddRange(resources);
}
```

---

## Important limitations

The current implementation is intentionally simple and should document its limitations clearly.

### 1. Minimum search length

The current substring index is based on a minimum search length, usually `3`.

That means short partial queries such as:

```text
a
an
```

may not work as expected when they are only part of a longer word.

### 2. Duplicate results

A resource can appear multiple times if it matches multiple query tokens.

Example:

```text
Query: cyber angel
Resource: Cyber Angel
```

The same resource can be returned once for `cyber` and once for `angel`.

A general-purpose version should deduplicate results by default.

### 3. No ranking by default

The current implementation returns resources in the order they are stored inside index buckets.

It does not know that:

```text
Angel
```

is probably a better result for query:

```text
angel
```

than:

```text
Triangle
```

even though both may contain the same substring.

### 4. Not identical to `string.Contains()`

The index works on normalized tokens and generated keys. It is not guaranteed to behave exactly like `string.Contains()` over the full original string.

For example, cross-word matches and punctuation-sensitive matches depend on tokenizer and normalizer behavior.

### 5. Memory tradeoff

Precomputing substrings increases memory usage.

Long tokens generate many keys, so large datasets should use a carefully selected indexing mode.

---
