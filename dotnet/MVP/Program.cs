// Setup: Create dependencies
var storage = new JsonPasswordStorage();
var randomSource = RandomSourceFactory.Create(RandomSourceType.CryptoRandom);
var generator = new PasswordGenerator(randomSource);

// Create View and Presenter
var view = new PassivePasswordView();
var presenter = new PasswordPresenter(view, generator, storage);

// Run the presenter
presenter.Run();
