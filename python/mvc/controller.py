"""MVC Controller for password generator application."""

from shared import (
    PasswordConfig,
    PasswordEntry,
    PasswordGenerator,
    RandomSourceFactory,
    RandomSourceType,
)
from .model import PasswordModel
from .view import ConsoleView


class PasswordController:
    """
    Supervising MVC Controller.
    Orchestrates interaction between Model and View, handles all application logic flow.
    """

    def __init__(self, model: PasswordModel, view: ConsoleView) -> None:
        """
        Initialize the password controller.

        Args:
            model: The password model.
            view: The console view.

        Raises:
            ValueError: If any parameter is None.
        """
        if model is None:
            raise ValueError("model cannot be None")
        if view is None:
            raise ValueError("view cannot be None")

        self._model = model
        self._view = view
        self._current_password: str | None = None
        self._current_random_source = RandomSourceType.CRYPTO_RANDOM

    def run(self) -> None:
        """Run the main application loop."""
        while True:
            try:
                choice = self._view.show_main_menu()

                match choice:
                    case "1":
                        self._handle_generate_password()
                    case "2":
                        self._handle_save_password()
                    case "3":
                        self._handle_view_passwords()
                    case "4":
                        self._handle_delete_password()
                    case "5":
                        self._view.show_goodbye()
                        return
                    case _:
                        self._view.show_error(
                            "Invalid option. Please select 1-5."
                        )
            except Exception as e:
                self._view.show_error(
                    f"An unexpected error occurred: {str(e)}"
                )

    def _handle_generate_password(self) -> None:
        """Handle password generation flow."""
        try:
            # Get user inputs
            length = self._view.get_password_length()
            use_special_chars = self._view.get_use_special_chars()
            sources = self._model.get_available_random_sources()
            source_index = self._view.get_random_source_choice(sources)

            if source_index < 0:
                return

            self._current_random_source = sources[source_index][0]

            # Generate password via model
            config = PasswordConfig(
                length,
                use_special_chars,
                self._current_random_source,
            )
            random_source = RandomSourceFactory.create(
                self._current_random_source
            )
            generator = PasswordGenerator(random_source)

            self._current_password = generator.generate(config)
            strength = self._model.get_password_strength(self._current_password)

            self._view.show_generated_password(self._current_password, strength)
        except Exception as e:
            self._view.show_error(str(e))

    def _handle_save_password(self) -> None:
        """Handle password save flow."""
        try:
            if not self._current_password:
                self._view.show_error(
                    "No password generated yet. Please generate a password first."
                )
                return

            key = self._view.get_password_key()
            description = self._view.get_password_description()

            # Create and save entry via model
            entry = PasswordEntry(key, self._current_password, description)
            self._model.save_password(entry)

            self._view.show_success(f"Password saved as '{key}'.")
            self._current_password = None
        except Exception as e:
            self._view.show_error(str(e))

    def _handle_view_passwords(self) -> None:
        """Handle viewing all saved passwords."""
        try:
            entries = self._model.get_all_passwords()
            self._view.show_password_list(entries)
        except Exception as e:
            self._view.show_error(str(e))

    def _handle_delete_password(self) -> None:
        """Handle password deletion flow."""
        try:
            key = self._view.get_password_key_to_delete()
            deleted = self._model.delete_password(key)

            if deleted:
                self._view.show_success(f"Password '{key}' deleted.")
            else:
                self._view.show_error(f"Password '{key}' not found.")
        except Exception as e:
            self._view.show_error(str(e))
