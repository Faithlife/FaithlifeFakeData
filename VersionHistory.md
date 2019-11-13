# Version History

## Pending

Describe changes here when they're committed to the `master` branch. Move them to **Released** when the project version number is updated in preparation for publishing an updated NuGet package.

Prefix the description of the change with `[major]`, `[minor]`, or `[patch]` in accordance with [Semantic Versioning](https://semver.org/).

## Released

### 2.0.0

* **Breaking:** Don't automatically create tables for backing fields. (It doesn't work well with (non-)nullable reference types, and reflection can't necessarily set backing fields for read-only properties.)

### 1.0.0

* Official release.

### 0.1.2

* Updated build script.

### 0.1.1

* Automatically create tables.

### 0.1.0

* Initial release.
