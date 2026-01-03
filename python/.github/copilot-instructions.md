# AI Assistant Guidelines for this repository

Purpose: concise notes to help an AI coding agent be productive in this repo.

Big picture
- This repo demonstrates three UI architecture patterns (examples only): MVC, MVP, and MVVM. See the top-level folders: `mvc/`, `mvp/`, and `mvvm/`.
- Core domain and reusable logic live in `shared/` (models, random sources, password generator, file storage).
- Each pattern wires the same business logic differently: controllers/presenters/viewmodels orchestrate user interaction while the shared layer implements generation and persistence.

Key files to inspect (quick links)
- MVC: [mvc/controller.py](../mvc/controller.py), [mvc/model.py](../mvc/model.py), [mvc/view.py](../mvc/view.py), [mvc/__main__.py](../mvc/__main__.py)
- MVP: [mvp/presenter.py](../mvp/presenter.py), [mvp/view.py](../mvp/view.py), [mvp/__main__.py](../mvp/__main__.py)
- MVVM: [mvvm/viewmodel.py](../mvvm/viewmodel.py), [mvvm/view.py](../mvvm/view.py), [mvvm/__main__.py](../mvvm/__main__.py)
- Shared domain & infra: [shared/models.py](../shared/models.py), [shared/password_generator.py](../shared/password_generator.py), [shared/random_sources.py](../shared/random_sources.py), [shared/storage.py](../shared/storage.py), [shared/interfaces.py](../shared/interfaces.py)
- Tests & helpers: [tests/test_shared.py](../tests/test_shared.py), [tests/test_helpers.py](../tests/test_helpers.py)

Concrete patterns and conventions observed
- Dependency injection is explicit: constructors receive `PasswordGenerator`, `IPasswordStorage`, or `IPasswordView` implementations. Favor creating instances in `__main__.py` entrypoints.
- Views are passive (console-based): they perform I/O only and expose callbacks/properties for presenters/viewmodels.
- The shared layer is the single source of truth for domain models and interfaces; try not to duplicate types or change public interfaces in `shared/` without updating callers across `mvc/`, `mvp/`, and `mvvm/`.
- Random sources: use `RandomSourceFactory.create(RandomSourceType.X)` to obtain a platform-appropriate `IRandomSource`.
- Storage: `JsonPasswordStorage` writes to `~/.password-generator/passwords.json` by default; tests use `TempFileStorage`/`InMemoryPasswordStorage` to avoid global side effects.

Developer workflows (how to run & test)
- Run an example UI (console):
```bash
python3 ./mvc/__main__.py
python3 ./mvp/__main__.py
python3 ./mvvm/__main__.py
```
- Run unit tests from the repo root through the workspace venv:
```bash
.venv/bin/python -m pytest -q
```

Integration points & gotchas
- Platform-specific behavior: `DevUrandomSource` reads `/dev/urandom` and is only available on Unix-like systems (Linux/macOS). Use `RandomSourceFactory.get_available_sources()` to detect available options.
- File writes: `JsonPasswordStorage` persists to the current user's home directory. Tests provide temp and in-memory storages — prefer those in CI to avoid touching user files.
- No external dependencies: the project uses Python standard library features (dataclasses, typing, secrets). Keep compatibility with the repo's Python version when adding third-party packages.

How the AI should make changes
- Preserve public interfaces in `shared/` (models, interfaces). Refactor with cross-file updates if you change a model or interface.
- Prefer adding tests in `tests/` (use `MockRandomSource`, `InMemoryPasswordStorage`, `TempFileStorage` helpers in `tests/test_helpers.py`).
- For behavior changes, update the corresponding `__main__.py` wiring or add new examples rather than mutating example flows silently.

Notes for reviewers
- If you modify storage defaults, update `tests/test_helpers.py` accordingly.
- For changes touching platform-specific code (random sources), include tests that run on CI-friendly fallbacks.

If any section is unclear or you'd like the guidelines adjusted (more examples, stricter rules), tell me which part to expand.
