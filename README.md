# DFS (Valid-DynamicFilterSort)

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/0a3ca6ad209c4cb7a1b8a4825ac2e015)](https://www.codacy.com/gh/JasonEAshworth/DFS/dashboard?utm_source=github.com&utm_medium=referral&utm_content=JasonEAshworth/DFS&utm_campaign=Badge_Grade)

A .NET library that turns runtime filter and sort criteria (for example, the query string on
an API request) into type-safe, validated queries. Give it a model type and a compact
filter/sort string and it produces an `IQueryable<T>` filter, a sort, or a SQL clause.

## What it does

- Parses criteria like `parId=10,name!=foo,created=2024-06%` into typed parameters
  (operators `=`, `!=`, `<`, `>`, `<=`, `>=`, plus partial matches with `%`).
- Applies them to an `IQueryable<T>` (in-memory or Entity Framework), with pagination.
- Or emits SQL for PostgreSQL, including a **parameterized** form that is safe against SQL
  injection.
- Handles nested/dot-notation properties (`par.alpha.created=...`), JSON/dictionary fields,
  enums and nullables, and partial date/time matching.
- Falls back to in-memory filtering when EF cannot translate a query (configurable).

## Usage

```csharp
var queryable = children.AsQueryable();
DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, "parId=10");
```

Other entry points on `DynamicFilterSort<TModel>`: `ApplySortingToIQueryable`,
`ApplyFilteringAndSortingToPaginationModel`, `GetFilterString` / `GetSortString`, and
`GetPostgreSqlParameterizedFilterString`. The backend is chosen via the `FilterSortType`
enum (`DynamicLinq`, `PostgreSql`, `PostgreSqlParameterized`).

## Build and test

```bash
./build.sh     # builds and runs the xUnit tests in UnitTests/
```

Targets netstandard2.1. NuGet dependencies: System.Linq.Dynamic.Core, Dapper.FluentMap,
Newtonsoft.Json, valid.error.management.

## Projects

- `Valid-DynamicFilterSort/`, `Valid-DynamicFilterSortII/`: the library
- `Valid-DynamicFilterSort.Shared/`: shared pagination types
- `UnitTests/`: xUnit tests (which double as usage examples)

## License

[MIT](LICENSE).
