using Xunit;
using PasswordGenerator.Shared;
using Moq;
using PasswordGenerator.MVP;
using Generator = PasswordGenerator.Shared.PasswordGenerator;

namespace PasswordGenerator.Tests;

/// <summary>
/// Unit tests for PasswordPresenter in MVP pattern.
/// Demonstrates isolation: testing Presenter with mocked View.
/// </summary>
#pragma warning disable CS0067 // suppress unused event warnings in mock view
public class PasswordPresenterTests
{
    private readonly MockPasswordView _mockView = new();
    private readonly Mock<IRandomSource> _randomMock = RandomMockFactory.Create(Enumerable.Repeat((byte)0x10, 12).ToArray());
    private readonly Generator _generator;
    private readonly Mock<IPasswordStorage> _storageMock = new();
    private readonly PasswordPresenter _presenter;

    public PasswordPresenterTests()
    {
        _generator = new Generator(_randomMock.Object);
        _presenter = new PasswordPresenter(_mockView, _generator, _storageMock.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Succeeds()
    {
        Assert.NotNull(_presenter);
    }

    [Fact]
    public void Constructor_WithNullView_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PasswordPresenter(null!, _generator, _storageMock.Object)
        );
    }

    [Fact]
    public void Save_AfterGenerate_CallsStorageSaveOnce()
    {
        _mockView.PasswordKey = "testkey";
        _mockView.PasswordDescription = "desc";

        _mockView.RaiseGenerateRequested();
        _mockView.RaiseSaveRequested();

        _storageMock.Verify(s => s.Save(It.Is<PasswordEntry>(e =>
            e.Key == "testkey" && !string.IsNullOrEmpty(e.Password) && e.Description == "desc"
        )), Times.Once);
    }

    [Fact]
    public void ViewAll_CallsStorageLoadAllOnce()
    {
        _storageMock.Setup(s => s.LoadAll()).Returns(Array.Empty<PasswordEntry>());

        _mockView.RaiseViewAllRequested();

        _storageMock.Verify(s => s.LoadAll(), Times.Once);
    }

    [Fact]
    public void Delete_CallsStorageDeleteWithViewKey()
    {
        _mockView.DeletePasswordKey = "to-delete";
        _storageMock.Setup(s => s.Delete("to-delete")).Returns(true);

        _mockView.RaiseDeleteRequested();

        _storageMock.Verify(s => s.Delete("to-delete"), Times.Once);
    }

    private sealed class MockPasswordView : IPasswordView
    {
        public event EventHandler? OnGenerateRequested;
        public event EventHandler? OnSaveRequested;
        public event EventHandler? OnViewAllRequested;
        public event EventHandler? OnDeleteRequested;
        public event EventHandler? OnExitRequested;

        public int PasswordLength { get; set; } = 12;
        public bool UseSpecialChars { get; set; } = true;
        public int SelectedRandomSourceIndex { get; set; } = 0;
        public string PasswordKey { get; set; } = "testkey";
        public string PasswordDescription { get; set; } = "test";
        public string DeletePasswordKey { get; set; } = "";

        private IEnumerable<(RandomSourceType, string)> _sources = Enumerable.Empty<(RandomSourceType, string)>();

        public void ShowMainMenu() { }
        public void ShowError(string message) { }
        public void ShowSuccess(string message) { }
        public void ShowInfo(string message) { }
        public void SetAvailableRandomSources(IEnumerable<(RandomSourceType Type, string Name)> sources)
        {
            _sources = sources.ToList();
        }
        public void DisplayGeneratedPassword(string password, PasswordStrength strength) { }
        public void DisplayPasswordList(IEnumerable<PasswordEntry> entries) { }

        // Test helpers to raise events
        public void RaiseGenerateRequested() => OnGenerateRequested?.Invoke(this, EventArgs.Empty);
        public void RaiseSaveRequested() => OnSaveRequested?.Invoke(this, EventArgs.Empty);
        public void RaiseViewAllRequested() => OnViewAllRequested?.Invoke(this, EventArgs.Empty);
        public void RaiseDeleteRequested() => OnDeleteRequested?.Invoke(this, EventArgs.Empty);
        public void RaiseExitRequested() => OnExitRequested?.Invoke(this, EventArgs.Empty);
    }
}
#pragma warning restore CS0067
