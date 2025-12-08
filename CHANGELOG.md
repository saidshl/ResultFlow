# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- `Result.Try()` and `Result.TryAsync()` for exception-safe operations
- `Result.Combine()` for combining multiple results
- `Result.Success()` and `Result.Failure()` aliases for `Ok()` and `Failed()`
- Comprehensive unit test suite (146 tests)
- `.editorconfig` for consistent code style
- `Directory.Build.props` for shared build configuration
- Code coverage reporting in CI

### Fixed
- Fixed `ResultExtentions.cs` typo (renamed to `ResultExtensions.cs`)
- Fixed test project to multi-target .NET 8, 9, and 10
- Fixed FluentValidation package README (now uses package-specific README)
- Fixed GitHub URL inconsistencies in documentation

## [2.0.2] - 2024-01-15

### Added
- Initial release with core Result pattern implementation
- `Result<T>` and `VoidResult` types
- Functional operations: `Map`, `Bind`, `Filter`, `Tap`, `TapError`, `Then`
- Pattern matching with `Match`
- Async extensions for `Task<Result<T>>`
- Built-in error types: `NotFoundError`, `BadRequestError`, `ValidationError`, `ConflictError`, `UnauthorizedError`, `ForbiddenError`, `InternalServerError`, `TooManyRequestsError`
- `ErrorBuilder` for custom error construction
- ASP.NET Core integration (`ResultFlow.AspNetCore`)
- FluentValidation integration (`ResultFlow.FluentValidation`)

### Changed
- N/A

### Deprecated
- N/A

### Removed
- N/A

### Fixed
- N/A

### Security
- N/A

## [1.0.0] - 2024-01-01

### Added
- Initial beta release

[Unreleased]: https://github.com/saidshl/ResultFlow/compare/v2.0.2...HEAD
[2.0.2]: https://github.com/saidshl/ResultFlow/compare/v1.0.0...v2.0.2
[1.0.0]: https://github.com/saidshl/ResultFlow/releases/tag/v1.0.0
