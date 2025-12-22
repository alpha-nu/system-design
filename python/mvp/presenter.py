"""MVP Presenter for password generator."""

from shared import (
    PasswordConfig,
    PasswordEntry,
    PasswordGenerator,
    RandomSourceFactory,
    RandomSourceType,
)
from .view import IPasswordView


class PasswordPresenter:
    """
    MVP Presenter - mediates between the Passive View and Model (generators/storage).
    Subscribes to View callbacks and updates the View based on responses.
    Has NO direct dependency on concrete View, only on IPasswordView interface.
    """

    def __init__(
        self,
        view: IPasswordView,
        generator: PasswordGenerator,
        storage,
    ) -> None:
        """
        Initialize the presenter.

        Args:
            view: The password view interface.
            generator: Password generator instance.
            storage: Password storage instance.
        """
        if view is None:
            raise ValueError("view cannot be None")
        if generator is None:
            raise ValueError("generator cannot be None")
        if storage is None:
            raise ValueError("storage cannot be None")

        self._view = view
        self._generator = generator
        self._storage = storage
        self._current_password: str | None = None
        self._current_random_source = RandomSourceType.CRYPTO_RANDOM

        # Subscribe to view callbacks
        self._view.on_generate_requested = self._handle_generate_password
        self._view.on_save_requested = self._handle_save_password
        self._view.on_view_all_requested = self._handle_view_passwords
        self._view.on_delete_requested = self._handle_delete_password
        self._view.on_exit_requested = self._handle_exit

    def run(self) -> None:
        """Run the presentation loop."""
        while True:
            try:
                # Update available random sources
                sources = RandomSourceFactory.get_available_sources()
                self._view.set_available_random_sources(sources)

                self._view.show_main_menu()
            except Exception as e:
                self._view.show_error(f"An unexpected error occurred: {str(e)}")

    def _handle_generate_password(self) -> None:
        """Handle password generation."""
        try:
            sources = RandomSourceFactory.get_available_sources()
            if (
                0 <= self._view.selected_random_source_index < len(sources)
            ):
                self._current_random_source = sources[
                    self._view.selected_random_source_index
                ][0]

            config = PasswordConfig(
                self._view.password_length,
                self._view.use_special_chars,
                self._current_random_source,
            )
            random_source = RandomSourceFactory.create(
                self._current_random_source
            )
            generator = PasswordGenerator(random_source)

            self._current_password = generator.generate(config)
            strength = PasswordGenerator.calculate_strength(
                self._current_password
            )

            self._view.display_generated_password(
                self._current_password, strength
            )
        except Exception as e:
            self._view.show_error(str(e))

    def _handle_save_password(self) -> None:
        """Handle password save."""
        try:
            if not self._current_password:
                self._view.show_error(
                    "No password generated yet. Please generate a password first."
                )
                return

            entry = PasswordEntry(
                self._view.password_key,
                self._current_password,
                self._view.password_description,
            )
            self._storage.save(entry)

            self._view.show_success(
                f"Password saved as '{self._view.password_key}'."
            )
            self._current_password = None
        except Exception as e:
            self._view.show_error(str(e))

    def _handle_view_passwords(self) -> None:
        """Handle viewing all passwords."""
        try:
            entries = self._storage.load_all()
            self._view.display_password_list(entries)
        except Exception as e:
            self._view.show_error(str(e))

    def _handle_delete_password(self) -> None:
        """Handle password deletion."""
        try:
            deleted = self._storage.delete(self._view.delete_password_key)
            if deleted:
                self._view.show_success(
                    f"Password '{self._view.delete_password_key}' deleted."
                )
            else:
                self._view.show_error(
                    f"Password '{self._view.delete_password_key}' not found."
                )
        except Exception as e:
            self._view.show_error(str(e))

    def _handle_exit(self) -> None:
        """Handle exit."""
        print("\nGoodbye!")
        exit(0)
