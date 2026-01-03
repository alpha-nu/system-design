---
description: A sample template to use for Agent Tasks
---

# Feature: Export saved passwords to JSON (console UIs)

Outcome
- User can export saved entries to a chosen file path via MVC, MVP, and MVVM console flows.
- Non-goals: encryption, format changes, cloud sync.

Repo grounding
- Shared layer: reuse IPasswordStorage.load_all(); do not change shared interfaces.
- Wire from each pattern entrypoint and view.

Inputs/Outputs
- Input: file path (prompted).
- Output: JSON array of PasswordEntry dicts; success/error messages.

Constraints
- No writes to home directory unless user selects it.
- Works on macOS/Linux; no external deps.

Wiring guidance
- MVC: add “6. Export” to mvc/view.py menu; controller calls model.get_all_passwords() and writes file.
- MVP: add menu option + presenter handler; use storage.load_all().
- MVVM: add command on ViewModel; View prompts for path and triggers export.

Files to touch
- mvc/view.py, mvc/controller.py
- mvp/view.py, mvp/presenter.py
- mvvm/view.py, mvvm/viewmodel.py
- tests/test_shared.py (add export test using TempFileStorage)

Acceptance criteria
- New menu option present in all three UIs.
- File written with correct JSON content when entries exist; empty list when none.
- Unit tests pass: .venv/bin/python -m pytest -q

Run & test
- MVC: python3 ./mvc/__main__.py
- MVP: python3 ./mvp/__main__.py
- MVVM: python3 ./mvvm/__main__.py