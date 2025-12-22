"""MVP pattern entry point."""

from shared import (
    JsonPasswordStorage,
    PasswordGenerator,
    RandomSourceFactory,
    RandomSourceType,
)
from .view import PassivePasswordView
from .presenter import PasswordPresenter

if __name__ == "__main__":
    # Setup: Create dependencies
    storage = JsonPasswordStorage()
    random_source = RandomSourceFactory.create(RandomSourceType.CRYPTO_RANDOM)
    generator = PasswordGenerator(random_source)

    # Create View and Presenter
    view = PassivePasswordView()
    presenter = PasswordPresenter(view, generator, storage)

    # Run the presenter
    presenter.run()
