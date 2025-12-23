using Xunit;
using PasswordGenerator.Shared;
using Moq;
using PasswordGenerator.MVC;
using Generator = PasswordGenerator.Shared.PasswordGenerator;

namespace PasswordGenerator.Tests;

/// <summary>
/// Unit tests for PasswordController in MVC pattern.
/// Verifies interactions with storage via the model using a mocked IConsoleView.
/// </summary>
public class PasswordControllerTests
{
    private readonly PasswordModel _model;
    private readonly Mock<IConsoleView> _viewMock = new();
    private readonly Mock<IPasswordStorage> _storageMock = new();
    private readonly PasswordController _controller;

    public PasswordControllerTests()
    {
        var randomMock = RandomMockFactory.Create(new byte[] { 0x10, 0x20, 0x30 });
        var generator = new Generator(randomMock.Object);
        _model = new PasswordModel(generator, _storageMock.Object);
        _controller = new PasswordController(_model, _viewMock.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Succeeds()
    {
        Assert.NotNull(_controller);
    }

    [Fact]
    public void Constructor_WithNullModel_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PasswordController(null!, _viewMock.Object)
        );
    }

    [Fact]
    public void ProcessOnce_ViewAll_CallsLoadAllAndDisplaysList()
    {
        _viewMock.Setup(v => v.ShowMainMenu()).Returns('3');
        _storageMock.Setup(s => s.LoadAll()).Returns(Array.Empty<PasswordEntry>());

        _controller.ProcessOnce();

        _storageMock.Verify(s => s.LoadAll(), Times.Once);
        _viewMock.Verify(v => v.ShowPasswordList(It.IsAny<IEnumerable<PasswordEntry>>()), Times.Once);
    }

    [Fact]
    public void ProcessOnce_Delete_CallsDeleteWithProvidedKey()
    {
        _viewMock.Setup(v => v.ShowMainMenu()).Returns('4');
        _viewMock.Setup(v => v.GetPasswordKeyToDelete()).Returns("del-key");
        _storageMock.Setup(s => s.Delete("del-key")).Returns(true);

        _controller.ProcessOnce();

        _storageMock.Verify(s => s.Delete("del-key"), Times.Once);
    }
}
