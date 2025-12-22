using Generator = PasswordGenerator.Shared.PasswordGenerator;

namespace PasswordGenerator.Tests;

/// <summary>
/// Unit tests for PasswordPresenter in MVP pattern.
/// Demonstrates isolation: testing Presenter with mocked View.
/// </summary>
public class PasswordPresenterTests
{
    private readonly MockPasswordView _mockView = new();
    private readonly MockRandomSource _mockRandom = new(new byte[] { 0x10, 0x20, 0x30 });
    private readonly Generator _generator;
    private readonly InMemoryPasswordStorage _storage = new();
    private readonly PasswordPresenter _presenter;

    public PasswordPresenterTests()
    {
        _generator = new Generator(_mockRandom);
        _presenter = new PasswordPresenter(_mockView, _generator, _storage);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Succeeds()
    {
        // Assert (successful construction means test passes)
        Assert.NotNull(_presenter);
    }

    [Fact]
    public void Constructor_WithNullView_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new PasswordPresenter(null!, _generator, _storage)
        );
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
    }
}

/// <summary>
/// Unit tests for PasswordController in MVC pattern.
/// Demonstrates isolation: testing Controller with real Model and View.
/// </summary>
public class PasswordControllerTests
{
    private readonly PasswordModel _model;
    private readonly ConsoleView _view = new();
    private readonly PasswordController _controller;

    public PasswordControllerTests()
    {
        var mockRandom = new MockRandomSource(new byte[] { 0x10, 0x20, 0x30 });
        var generator = new Generator(mockRandom);
        var storage = new InMemoryPasswordStorage();
        _model = new PasswordModel(generator, storage);
        _controller = new PasswordController(_model, _view);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Succeeds()
    {
        // Assert (successful construction means test passes)
        Assert.NotNull(_controller);
    }

    [Fact]
    public void Constructor_WithNullModel_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new PasswordController(null!, _view)
        );
    }
}

/// <summary>
/// Integration tests demonstrating end-to-end flows.
/// Tests the complete generate → save → list → delete flow.
/// </summary>
public class IntegrationTests
{
    [Fact]
    public void FullFlow_GenerateAndSavePassword_Succeeds()
    {
        // Arrange
        var storage = new InMemoryPasswordStorage();
        var mockRandom = new MockRandomSource(new byte[] { 0x10, 0x20, 0x30, 0x40 });
        var generator = new Generator(mockRandom);

        // Act - Generate
        var config = new PasswordConfig(12, useSpecialChars: true);
        var password = generator.Generate(config);

        // Act - Save
        var entry = new PasswordEntry("mykey", password, "my password");
        storage.Save(entry);

        // Act - Load
        var retrieved = storage.Load("mykey");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(password, retrieved.Password);
        Assert.Equal("mykey", retrieved.Key);
    }

    [Fact]
    public void FullFlow_SaveMultipleAndList_Succeeds()
    {
        // Arrange
        var storage = new InMemoryPasswordStorage();

        // Act - Save multiple
        for (int i = 0; i < 5; i++)
        {
            var entry = new PasswordEntry($"key{i}", $"password{i}", $"description{i}");
            storage.Save(entry);
        }

        // Act - Load all
        var allEntries = storage.LoadAll().ToList();

        // Assert
        Assert.Equal(5, allEntries.Count);
        Assert.All(allEntries, entry => Assert.NotNull(entry));
    }

    [Fact]
    public void FullFlow_SaveAndDelete_Succeeds()
    {
        // Arrange
        var storage = new InMemoryPasswordStorage();
        var entry = new PasswordEntry("deletekey", "password", "to delete");
        storage.Save(entry);

        // Act - Delete
        var deleted = storage.Delete("deletekey");

        // Assert
        Assert.True(deleted);
        Assert.Null(storage.Load("deletekey"));
    }

    [Fact]
    public void FullFlow_GenerateWithDifferentSources_ProducesDifferentPasswords()
    {
        // Arrange
        var randomSource1 = new MockRandomSource(new byte[] { 0x01 });
        var randomSource2 = new MockRandomSource(new byte[] { 0xFF });
        var generator1 = new Generator(randomSource1);
        var generator2 = new Generator(randomSource2);

        var config = new PasswordConfig(20, useSpecialChars: false);

        // Act
        var password1 = generator1.Generate(config);
        var password2 = generator2.Generate(config);

        // Assert
        Assert.NotEqual(password1, password2);
        Assert.Equal(20, password1.Length);
        Assert.Equal(20, password2.Length);
    }
}
