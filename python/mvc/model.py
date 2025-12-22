"""MVC Model for password generator application."""

from shared import (
    PasswordConfig,
    PasswordEntry,
    PasswordGenerator,
    IPasswordStorage,
    IRandomSource,
    RandomSourceType,
    RandomSourceFactory,
    PasswordStrength,
)


class PasswordModel:
    """
    MVC Model encapsulating password generation and storage business logic.
    Pure business logic with no awareness of View or Controller.
    """

    def __init__(self, generator: PasswordGenerator, storage: IPasswordStorage) -> None:
        """
        Initialize the password model.

        Args:
            generator: Password generator instance.
            storage: Password storage instance.

        Raises:
            ValueError: If any parameter is None.
        """
        if generator is None:
            raise ValueError("generator cannot be None")
        if storage is None:
            raise ValueError("storage cannot be None")

        self._generator = generator
        self._storage = storage

    def generate_password(self, config: PasswordConfig) -> str:
        """Generate a password according to the specified configuration."""
        if config is None:
            raise ValueError("config cannot be None")
        return self._generator.generate(config)

    def save_password(self, entry: PasswordEntry) -> None:
        """Save a password entry to storage."""
        if entry is None:
            raise ValueError("entry cannot be None")
        self._storage.save(entry)

    def get_all_passwords(self) -> list[PasswordEntry]:
        """Retrieve all stored password entries."""
        return self._storage.load_all()

    def delete_password(self, key: str) -> bool:
        """Delete a password entry by key."""
        if not key or not key.strip():
            raise ValueError("key cannot be null or empty")
        return self._storage.delete(key)

    def get_password_strength(self, password: str) -> PasswordStrength:
        """Calculate the strength of a password."""
        return PasswordGenerator.calculate_strength(password)

    def get_available_random_sources(self) -> list[tuple[RandomSourceType, str]]:
        """Get all available random sources with their names."""
        sources = RandomSourceFactory.get_available_sources()
        return [(source_type, source.name) for source_type, source in sources]
