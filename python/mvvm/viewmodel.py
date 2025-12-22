"""MVVM ViewModel for password generator."""

from shared import (
    PasswordConfig,
    PasswordEntry,
    PasswordGenerator,
    PasswordStrength,
    RandomSourceFactory,
    RandomSourceType,
)


class PropertyChangedEventArgs:
    """Event arguments for property changes."""

    def __init__(self, property_name: str):
        self.property_name = property_name


class PasswordViewModel:
    """
    MVVM ViewModel for password generation and storage.
    Exposes observable properties and commands for the View to bind to.
    """

    def __init__(
        self,
        generator: PasswordGenerator,
        storage,
    ) -> None:
        """
        Initialize the password view model.

        Args:
            generator: Password generator instance.
            storage: Password storage instance.
        """
        if generator is None:
            raise ValueError("generator cannot be None")
        if storage is None:
            raise ValueError("storage cannot be None")

        self._generator = generator
        self._storage = storage

        # Observable properties
        self._current_password: str = ""
        self._current_password_strength: PasswordStrength = (
            PasswordStrength.WEAK
        )
        self._password_length: int = 12
        self._use_special_chars: bool = True
        self._selected_random_source: RandomSourceType = (
            RandomSourceType.CRYPTO_RANDOM
        )

        # Property change notifications
        self._property_changed_callbacks: list = []

    def subscribe_to_property_changed(self, callback) -> None:
        """Subscribe to property change notifications."""
        self._property_changed_callbacks.append(callback)

    def _notify_property_changed(self, property_name: str) -> None:
        """Notify subscribers that a property has changed."""
        args = PropertyChangedEventArgs(property_name)
        for callback in self._property_changed_callbacks:
            callback(args)

    # Properties
    @property
    def current_password(self) -> str:
        return self._current_password

    @current_password.setter
    def current_password(self, value: str) -> None:
        if self._current_password != value:
            self._current_password = value
            self._notify_property_changed("current_password")

    @property
    def current_password_strength(self) -> PasswordStrength:
        return self._current_password_strength

    @current_password_strength.setter
    def current_password_strength(self, value: PasswordStrength) -> None:
        if self._current_password_strength != value:
            self._current_password_strength = value
            self._notify_property_changed("current_password_strength")

    @property
    def password_length(self) -> int:
        return self._password_length

    @password_length.setter
    def password_length(self, value: int) -> None:
        if self._password_length != value and value >= 4:
            self._password_length = value
            self._notify_property_changed("password_length")

    @property
    def use_special_chars(self) -> bool:
        return self._use_special_chars

    @use_special_chars.setter
    def use_special_chars(self, value: bool) -> None:
        if self._use_special_chars != value:
            self._use_special_chars = value
            self._notify_property_changed("use_special_chars")

    @property
    def selected_random_source(self) -> RandomSourceType:
        return self._selected_random_source

    @selected_random_source.setter
    def selected_random_source(self, value: RandomSourceType) -> None:
        if self._selected_random_source != value:
            self._selected_random_source = value
            self._notify_property_changed("selected_random_source")

    @property
    def available_random_sources(
        self,
    ) -> list[tuple[RandomSourceType, str]]:
        """Get available random sources."""
        return RandomSourceFactory.get_available_sources()

    @property
    def saved_passwords(self) -> list[PasswordEntry]:
        """Get all saved passwords."""
        return self._storage.load_all()

    # Commands
    def generate_password(self) -> None:
        """Command handler for password generation."""
        try:
            config = PasswordConfig(
                self.password_length,
                self.use_special_chars,
                self.selected_random_source,
            )
            random_source = RandomSourceFactory.create(
                self.selected_random_source
            )
            generator = PasswordGenerator(random_source)

            self.current_password = generator.generate(config)
            self.current_password_strength = (
                PasswordGenerator.calculate_strength(self.current_password)
            )
        except Exception as e:
            self.current_password = f"Error: {str(e)}"

    def save_password(self, key: str, description: str = "") -> None:
        """Command handler for saving password."""
        try:
            if not self.current_password:
                raise ValueError("No password to save")

            entry = PasswordEntry(
                key, self.current_password, description
            )
            self._storage.save(entry)

            self.current_password = ""
            self._notify_property_changed("saved_passwords")
        except Exception as e:
            self.current_password = f"Error: {str(e)}"

    def view_all_passwords(self) -> None:
        """Command handler for viewing all passwords."""
        self._notify_property_changed("saved_passwords")

    def delete_password(self, key: str) -> bool:
        """Command handler for deleting a password."""
        try:
            deleted = self._storage.delete(key)
            if deleted:
                self._notify_property_changed("saved_passwords")
            return deleted
        except Exception as e:
            self.current_password = f"Error: {str(e)}"
            return False
