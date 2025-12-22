using PasswordGenerator.Shared;
using PasswordGenerator.MVVM;
using Generator = PasswordGenerator.Shared.PasswordGenerator;

// Setup: Create dependencies
var storage = new JsonPasswordStorage();
var randomSource = RandomSourceFactory.Create(RandomSourceType.CryptoRandom);
var generator = new Generator(randomSource);

// Create ViewModel and View
var viewModel = new PasswordViewModel(generator, storage);
var view = new PasswordView(viewModel);

// Run the view
view.Run();
