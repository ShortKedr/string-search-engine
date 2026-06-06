# RenwordDigital String Search Engine

A lightweight, embeddable, general-purpose search index for C# applications.

The main idea is simple:

> Build the index once, search many times.

Instead of scanning every resource with `string.Contains()` on every query, the library precomputes searchable keys and stores them in an inverted index. This makes repeated search queries much faster.

This is not intended to be a universal replacement for `string.Contains()`. It is a specialized data structure for cases where resources are stable, search is frequent, and predictable query performance is valuable.

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

However, when the dataset grows and search happens often, for example in a command palette, asset browser, product list, settings search, file search, or autocomplete UI, scanning every string on every query becomes a bottleneck.

`SearchIndex` solves this by building a lookup structure ahead of time:

```csharp
Dictionary<string, List<Resource>>
```

At search time, a query can be resolved with dictionary lookups instead of a full linear scan.

---

## Usage example

Here's a basic example of using `SearchIndex`:

```csharp
// Create an index
var searchIndex = new SearchIndex();

// Add resources to be searchable
var resources = new Resource[]
{
    new Resource("Cyber Angel"),
    new Resource("Triangle"),
    new Resource("Angel Sanctuary")
};

searchIndex.SetResources(resources);

// Search by query string
var results = searchIndex.GetSearchResult("angel");

// results now contains resources matching "angel"
foreach (var resource in results)
{
    Debug.Log(resource.Name);
}
```

You can also add or remove resources incrementally:

```csharp
var newResource = new Resource("Archangel");
searchIndex.AddResource(newResource);

// Remove a resource
searchIndex.RemoveResource(resources[0]);
```

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

If the searchable text must change, remove the old resource and add a new one:

```csharp
index.RemoveResource(oldResource);
index.AddResource(new Resource("Cyber Angel", "Cyber-Angel angel fighter resistance android"));
```

Important behavior:

- `Resource` is compared by reference unless equality is overridden.
- Two different `Resource` instances with the same name are treated as different resources.
- `Name` and `SearchString` should be non-null.
- Each term in `SearchString` (space-separated) must be at least 3 characters long. Terms shorter than 3 characters will throw `InvalidResourceLengthException`.
- Search data should not be mutated outside the index.

---

## How the index works

The current implementation generates substring keys for every resource and stores them in a dictionary:

```csharp
Dictionary<string, List<Resource>> _searchCache;
```

### Indexing process

1. Each resource's `SearchString` is split by spaces into individual terms.
2. For each term with 3+ characters, all contiguous substrings are generated and stored as lowercase keys.
3. For example, "Cyber" (5 characters) generates these keys:
   - Length 5: `cyber`
   - Length 4: `cybe`, `yber`
   - Length 3: `cyb`, `ybe`, `ber`
4. Terms shorter than 3 characters are stored as-is (but not used to generate substrings).
5. Each key maps to a list of resources containing that substring.

### Search process

1. The search string is normalized (lowercased) and split by spaces.
2. Each search token is looked up directly in the dictionary.
3. Results are deduplicated—each resource appears only once even if it matches multiple tokens.
4. Resources are returned in the order they were found.

**Example:**
```csharp
Query: "and"
Resources: ["TimeAndProgressManager", "TimeHandler", ...]
Match: "and" is found as a substring in keys for "TimeAndProgressManager"
```

---

## Important limitations

The current implementation is intentionally simple and should document its limitations clearly.

### 1. Minimum search length

The substring index operates on a minimum search length of **3 characters**.

Short queries such as:
```text
a
an
```

may not work as expected if they are only partial matches within longer words. However, exact space-separated terms shorter than 3 characters are still indexed and searchable.

### 2. Requires fixed-size terms

Resources must be composed of terms at least 3 characters long (space-separated). If a resource contains a term shorter than 3 characters, an `InvalidResourceLengthException` is thrown during indexing.

### 3. Space-separated term splitting

The index splits both resources and search queries by spaces. Punctuation and special characters attached to words are treated as part of the word.

Example: `"Cyber-Angel"` is indexed as a single term, not split by the hyphen.

### 4. Duplicate results

A resource can appear multiple times in results if it matches multiple query tokens.

Example:
```text
Query: "cyber angel"
Resource: "Cyber Angel"
```

The same resource can be returned once for `cyber` and once for `angel`.

A general-purpose version should deduplicate results by default (this is already implemented internally via `HashSet<Resource>`).

### 5. No ranking by default

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

### 6. Not identical to `string.Contains()`

The index works on normalized tokens and generated keys. It is not guaranteed to behave exactly like `string.Contains()` over the full original string.

For example:
- It matches substrings only within space-separated terms
- Case is normalized (all lowercase)
- Punctuation-sensitive matches depend on how terms are space-separated

### 7. Memory tradeoff

Precomputing substrings increases memory usage.

For a term of length N, approximately `(N-2) * (N+1) / 2` substring keys are generated. Long tokens in large datasets should be carefully considered, as memory usage can grow significantly.
