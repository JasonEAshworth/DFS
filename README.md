# DFS (Valid-DynamicFilterSort)

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/0a3ca6ad209c4cb7a1b8a4825ac2e015)](https://www.codacy.com/gh/JasonEAshworth/DFS/dashboard?utm_source=github.com&utm_medium=referral&utm_content=JasonEAshworth/DFS&utm_campaign=Badge_Grade)

A .NET library for dynamic filtering and sorting of collections and queries, for example
turning runtime criteria (such as API query parameters) into typed, validated queries.

## Projects

- `Valid-DynamicFilterSort/`: core library
- `Valid-DynamicFilterSortII/`: second iteration
- `Valid-DynamicFilterSort.Shared/`: shared types
- `UnitTests/`: test suite

## Build

```bash
./build.sh
```

Built with the .NET SDK; CI via the included `Jenkinsfile`, packaged with NuGet.

## License

[MIT](LICENSE).
