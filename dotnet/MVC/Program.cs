using PasswordGenerator.Shared;
using PasswordGenerator.MVC;

// Setup: Create dependencies
var storage = new JsonPasswordStorage();
var randomSource = RandomSourceFactory.Create(RandomSourceType.CryptoRandom);
var generator = new PasswordGenerator(randomSource);

// Create Model and View
var model = new PasswordModel(generator, storage);
var view = new ConsoleView();

// Create Controller and run
var controller = new PasswordController(model, view);
controller.Run();
