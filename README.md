# MVC/MVP/MVVM: Canonical Implementations

Canonical implementations of three fundamental architectural patterns (MVC, MVP, MVVM) for a **Strong Password Generator** console application.

> **For detailed metrics and architecture decisions**, see [ARCHITECTURE_GUIDE.md](./ARCHITECTURE_GUIDE.md).

## 🚀 Quick Start

### Prerequisites
- **.NET**: 6.0+ SDK installed
- **Python**: 3.8+ with pytest installed

### Run the Application

**.NET:**
```bash
# Build the solution
cd dotnet && dotnet build

# Run each pattern
cd MVC  && dotnet run   # Run MVC
cd ../MVP  && dotnet run   # Run MVP
cd ../MVVM && dotnet run   # Run MVVM

# Run all tests
cd .. && dotnet test
```

**Python:**
```bash
cd python

# Run each pattern
python -m mvc     # Run MVC
python -m mvp     # Run MVP
python -m mvvm    # Run MVVM

# Run all tests
python -m pytest tests/ -v
```

---

## 📚 Project Overview

The goal is to demonstrate clear **separation of concerns** and how each pattern handles the interaction between business logic (Model), user interface (View), and orchestration (Controller/Presenter/ViewModel).

### Sample Application: Strong Password Generator

A user can:
- Generate passwords with configurable length and character set
- Select from multiple random sources (System Random, Crypto Random, /dev/urandom, Hardware RNG)
- Save passwords locally with descriptions
- View all stored passwords
- Delete stored passwords

## 🏗️ Architecture Patterns

### MVC (Model-View-Controller)
**Variant: Supervising Controller**

```
┌─────────────────────────────────────┐
│         Console View                 │
│  (Display only, no business logic)   │
└────────────────┬────────────────────┘
                 │ Updates
                 ▼
        ┌────────────────┐
        │  Controller    │
        │  (Orchestrator)│
        └────────────────┘
       ↗                    ↖
    Uses                    Uses
      ↙                      ↖
┌─────────────┐        ┌──────────────┐
│ PasswordGen │        │ Storage/Repo │
│  (Model)    │        │   (Model)    │
└─────────────┘        └──────────────┘
```

**Key Characteristics:**
- Controller owns both Model and View
- Controller coordinates all interactions
- View is completely passive (no logic)
- Single-directional updates: Controller → View

**Files:**
- [PasswordModel](./dotnet/MVC/PasswordModel.cs) / [Python](./python/mvc/model.py)
- [ConsoleView](./dotnet/MVC/ConsoleView.cs) / [Python](./python/mvc/view.py)
- [PasswordController](./dotnet/MVC/PasswordController.cs) / [Python](./python/mvc/controller.py)

---

### MVP (Model-View-Presenter)
**Variant: Passive View**

```
┌──────────────────────────────────────┐
│         Passive View                  │
│  (Display only, exposes events)       │
└─────────┬──────────────────────┬─────┘
          │ Events               │ Updates
          ▼                      ▼
   ┌──────────────────────────────┐
   │      Presenter               │
   │ (Mediates V↔M interaction)   │
   └──────────────────────────────┘
          ▲                      ▲
          │ Uses                 │ Uses
          │                      │
  ┌──────────────┐      ┌──────────────┐
  │ PasswordGen  │      │   Storage    │
  │   (Model)    │      │   (Model)    │
  └──────────────┘      └──────────────┘
```

**Key Characteristics:**
- View exposes events/callbacks for user actions
- Presenter subscribes to View events
- Presenter updates View via interface methods
- View has ZERO knowledge of Model
- Bidirectional communication through Presenter

**Files:**
- [IPasswordView](./dotnet/MVP/IPasswordView.cs) / [Python](./python/mvp/view.py)
- [PassivePasswordView](./dotnet/MVP/PassivePasswordView.cs)
- [PasswordPresenter](./dotnet/MVP/PasswordPresenter.cs) / [Python](./python/mvp/presenter.py)

---

### MVVM (Model-View-ViewModel)
**Variant: Observable Properties + Commands**

```
┌────────────────────────────────┐
│          View                   │
│  (Binds to ViewModel properties)│
└────────────┬────────────────────┘
             │ Binding
             ▼
   ┌──────────────────────┐
   │   ViewModel          │
   │ • Observable Props   │
   │ • Commands           │
   └──────────────────────┘
             │ Uses
      ┌──────┴──────┐
      ▼             ▼
 ┌──────────┐  ┌──────────┐
 │  Model   │  │ Storage  │
 └──────────┘  └──────────┘
```

**Key Characteristics:**
- ViewModel exposes observable properties
- View binds to ViewModel properties
- ViewModel implements command pattern
- Automatic synchronization via property change notifications
- Testable without UI framework

**Files:**
- [PasswordViewModel](./dotnet/MVVM/PasswordViewModel.cs) / [Python](./python/mvvm/viewmodel.py)
- [PasswordView](./dotnet/MVVM/PasswordView.cs) / [Python](./python/mvvm/view.py)

---

## 🎮 Using the Application

All three patterns have the same user interface and functionality.

### Main Menu
```
========== PASSWORD GENERATOR (MVC/MVP/MVVM) ==========
1. Generate a new password
2. Save password
3. View all saved passwords
4. Delete a password
5. Exit
```

### Typical Workflow

1. **Generate Password**
   - Enter desired length (minimum 4 characters)
   - Choose if you want special characters
   - Select a random source
   - View the generated password and its strength

2. **Save Password**
   - Provide a name/key to identify it
   - Add optional description
   - Password is saved to `~/.password-generator/passwords.json`

3. **View Passwords**
   - See all saved passwords with descriptions
   - Shows creation timestamps

4. **Delete Password**
   - Remove a password by entering its key

---

## 🔍 Pattern Comparison

### Architecture Overview

| Aspect | MVC | MVP | MVVM |
|--------|-----|-----|------|
| **Complexity** | Low | Medium | Medium |
| **Testability** | Good | Excellent | Excellent |
| **View Logic** | Minimal | None | None |
| **Binding** | Manual | Manual | Automatic |
| **Best For** | Simple apps | Complex UI | UI-heavy apps |

### Code Differences

**MVC:**
```csharp
// Controller directly calls Model and updates View
var password = _model.GeneratePassword(config);
_view.ShowGeneratedPassword(password, strength);
```

**MVP:**
```csharp
// View raises event, Presenter responds
view.OnGenerateRequested += () => {
    var password = _model.GeneratePassword(config);
    _view.DisplayPassword(password);  // Via interface method
};
```

**MVVM:**
```csharp
// View binds to ViewModel properties
public string CurrentPassword { 
    get => _currentPassword; 
    set { SetProperty(ref _currentPassword, value); }
}
// View automatically updates when property changes
```

### Testing Differences

**MVC Testing:**
```csharp
// Test Controller with mock Model and View
var mockModel = new MockModel();
var mockView = new MockView();
var controller = new Controller(mockModel, mockView);
// Verify view.ShowPassword called with correct args
```

**MVP Testing:**
```csharp
// Test Presenter with mock View
var mockView = new MockView();
var presenter = new Presenter(mockView, model, storage);
mockView.OnGenerateRequested?.Invoke();
// Verify view.DisplayPassword called
```

**MVVM Testing:**
```csharp
// Test ViewModel - no View needed
var vm = new PasswordViewModel(generator, storage);
vm.GeneratePasswordCommand.Execute();
Assert.Equal("expected", vm.CurrentPassword);
```

---

## 🧪 Testing Strategy

### Principles
- **Isolation**: Test each layer independently using mocks
- **No File I/O in Unit Tests**: Use in-memory storage
- **Presenter Tests with Mocked Views**: Verify correct method calls
- **Integration Tests**: End-to-end flows with real storage

### Key Test Files
**.NET:**
- `TestHelpers.cs`: Mock implementations (`InMemoryPasswordStorage`, `MockRandomSource`)
- `UnitTests.cs`: Domain model and utility tests
- `PatternTests.cs`: Presenter, Controller, and integration tests

**Python:**
- `test_helpers.py`: Mock implementations and temporary file storage
- `test_shared.py`: Domain model tests with parametrized cases

### Domain Model Tests
- Test `PasswordConfig`, `PasswordEntry`, `PasswordGenerator`
- No View/Controller/Presenter involved
- Fast execution, deterministic results

### Storage Tests
- Test `InMemoryPasswordStorage` and `JsonPasswordStorage`
- Verify CRUD operations (Create, Read, Update, Delete)
- Temporary file cleanup after tests

### Pattern Tests
- Test Presenter with mock View (MVP)
- Test Controller with mock Model (MVC)
- Integration tests with real storage

### Run Tests
```bash
# .NET
cd dotnet && dotnet test

# Python
cd python && python -m pytest tests/ -v

# Specific test (.NET)
cd dotnet && dotnet test --filter PasswordGeneratorTests

# Specific test (Python)
cd python && python -m pytest tests/test_shared.py::TestPasswordGenerator -v
```

## 🧪 Testing Strategy

### Principles
- **Isolation**: Test each layer independently using mocks
- **No File I/O in Unit Tests**: Use in-memory storage
- **Presenter Tests with Mocked Views**: Verify correct method calls
- **Integration Tests**: End-to-end flows with real storage

### Test Files
**.NET:**
- `TestHelpers.cs`: Mock implementations (`InMemoryPasswordStorage`, `MockRandomSource`)
- `UnitTests.cs`: Domain model and utility tests
- `PatternTests.cs`: Presenter, Controller, and integration tests

**Python:**
- `test_helpers.py`: Mock implementations and temporary file storage
- `test_shared.py`: Domain model and utility tests with parametrized cases

### Running Tests

**.NET:**
```bash
cd dotnet
dotnet test
```

**Python:**
```bash
cd python
python -m pytest tests/ -v
```

## 🚀 Running the Applications

### .NET
```bash
cd dotnet/MVC  && dotnet run   # Run MVC
cd dotnet/MVP  && dotnet run   # Run MVP
cd dotnet/MVVM && dotnet run   # Run MVVM
```

### Python
```bash
cd python && python -m mvc    # Run MVC
cd python && python -m mvp    # Run MVP
cd python && python -m mvvm   # Run MVVM
```

## 🎯 Key Learning Outcomes

### Separation of Concerns
Each pattern demonstrates clear boundaries:
- **Model**: Pure business logic, no UI awareness
- **View**: Display-only logic, no business decisions
- **Controller/Presenter/ViewModel**: Orchestration and coordination

### Testability
By separating concerns, each component is independently testable:
- Test models with mock storage
- Test presenters with mock views
- Test views with mock view models

### Flexibility
Each pattern offers different tradeoffs:
| Aspect | MVC | MVP | MVVM |
|--------|-----|-----|------|
| **Complexity** | Low | Medium | Medium |
| **Testability** | Good | Excellent | Excellent |
| **View Logic** | Minimal | None | None |
| **Binding** | Manual | Manual | Automatic |
| **Best For** | Simple apps | Complex UI | UI-heavy apps |

## 📊 Random Sources

The application supports 4 random number sources:

1. **System Random**: Platform default (not cryptographically secure)
2. **Crypto Random**: Cryptographically secure
   - .NET: `System.Security.Cryptography.RandomNumberGenerator`
   - Python: `secrets` module
3. **Dev Urandom**: Unix `/dev/urandom` (Unix-only)
4. **Hardware RNG**: CPU-based (falls back to Crypto Random)

## 💾 Storage

Passwords are stored in JSON format in the user's home directory:
- Default path: `~/.password-generator/passwords.json`
- Custom path: Can be specified when creating storage instance
- Thread-safe: Uses file locking

---

## 🔐 Security Considerations

**Current Implementation:**
- Passwords stored in plain text (JSON file)
- Random sources are cryptographically secure
- File permissions: 644 (readable by user, groups, others)

**For Production:**
1. **Encrypt Storage**: Use symmetric encryption (AES-256)
2. **Access Control**: Restrict file permissions to user only
3. **Master Password**: Add optional master password protection
4. **Key Derivation**: Use PBKDF2 or bcrypt for master password

---

## 🎯 Key Learning Outcomes

### Separation of Concerns
Each pattern demonstrates clear boundaries:
- **Model**: Pure business logic, no UI awareness
- **View**: Display-only logic, no business decisions
- **Controller/Presenter/ViewModel**: Orchestration and coordination

### Testability
By separating concerns, each component is independently testable:
- Test models with mock storage
- Test presenters with mock views
- Test views with mock view models

### Flexibility
Choose a pattern based on your application's needs:
- **Simple app** → MVC
- **Complex UI** → MVP or MVVM
- **Data-binding heavy** → MVVM
- **Maximum testability** → MVP

---

## ❓ FAQ

**Q: Why are there 3 patterns for the same app?**  
A: To demonstrate how different architectural patterns solve the same problem differently. MVC is simplest, MVP/MVVM are more testable.

**Q: Which pattern should I use?**  
A: 
- Simple app → MVC
- Complex UI → MVP or MVVM
- Data-binding heavy → MVVM
- Maximum testability → MVP

**Q: Can I switch patterns mid-project?**  
A: Yes! The shared Model layer is reusable. Only View/Controller/Presenter/ViewModel changes.

**Q: Why no external DI container?**  
A: Demonstrates DI concepts clearly without framework magic. Production apps should use containers.

**Q: How do I add a new feature?**  
A: Add to Model first, then wire it through Controller/Presenter/ViewModel to View.

**Q: Can I use this for a web app?**  
A: The Model layer, yes! Controllers/Presenters/ViewModels would need web-specific implementations.

---

## 🤝 Contributing

To extend this implementation:
1. Add new random source → implement `IRandomSource`
2. Add new storage type → implement `IPasswordStorage`
3. Add new feature → Update Model, then all 3 patterns
4. Write tests → Use mocks for isolated testing

---

## 📚 References

- [MVC Pattern](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93controller)
- [MVP Pattern](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93presenter)
- [MVVM Pattern](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93viewmodel)
- [Separation of Concerns](https://en.wikipedia.org/wiki/Separation_of_concerns)
- [ARCHITECTURE_GUIDE.md](./ARCHITECTURE_GUIDE.md) — Detailed metrics, deliverables breakdown, architecture decisions

---

Enjoy exploring architectural patterns! 🎯
