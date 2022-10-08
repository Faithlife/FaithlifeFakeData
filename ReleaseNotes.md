# Release Notes

## 2.1.2

* Add .NET 6 target.

## 2.1.1

* Fix bug when locking database context.

## 2.1.0

* Update `Faithlife.Reflection` and `System.ComponentModel.Annotations`.

## 2.0.0

* **Breaking:** Don't automatically create tables for backing fields. (It doesn't work well with (non-)nullable reference types, and reflection can't necessarily set backing fields for read-only properties.)

## 1.0.0

* Official release.

## 0.1.2

* Updated build script.

## 0.1.1

* Automatically create tables.

## 0.1.0

* Initial release.
