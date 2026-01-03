---
applyTo: '**/*.py'
---

## General
- Avoid using semantics and constructs specifically designed for backward compatibility

## Writing Tests
### Unit Tests
- should not contain any code that has a side effect
- should favor Mocking over PAtching or MonkeyPatching
- should map to source code files 1:1, that is one test file per code file
- should be organized into Test Classes, each class describes a scenario or a feature/module. A file can contain multiple tests classes.

## Running Scripts and tests
- use the venv path for python binaries whenever running any script or test. i.e the path to Python command is: .venv/bin/python

