"""MVC pattern entry point."""

from shared import (
    JsonPasswordStorage,
    PasswordGenerator,
    RandomSourceFactory,
    RandomSourceType,
)
from .model import PasswordModel
from .view import ConsoleView
from .controller import PasswordController

if __name__ == "__main__":
    # Setup: Create dependencies
    storage = JsonPasswordStorage()
    random_source = RandomSourceFactory.create(RandomSourceType.CRYPTO_RANDOM)
    generator = PasswordGenerator(random_source)

    # Create Model and View
    model = PasswordModel(generator, storage)
    view = ConsoleView()

    # Create Controller and run
    controller = PasswordController(model, view)
    controller.run()
