"""MVVM pattern entry point."""

from shared import (
    JsonPasswordStorage,
    PasswordGenerator,
    RandomSourceFactory,
    RandomSourceType,
)
from .viewmodel import PasswordViewModel
from .view import PasswordView

if __name__ == "__main__":
    # Setup: Create dependencies
    storage = JsonPasswordStorage()
    random_source = RandomSourceFactory.create(RandomSourceType.CRYPTO_RANDOM)
    generator = PasswordGenerator(random_source)

    # Create ViewModel and View
    view_model = PasswordViewModel(generator, storage)
    view = PasswordView(view_model)

    # Run the view
    view.run()
