"""MVP View Interface and Passive View implementation."""

from abc import ABC, abstractmethod
from shared import PasswordEntry, PasswordStrength, RandomSourceType


class IPasswordView(ABC):
    """Interface defining the Passive View contract."""

    @abstractmethod
    def show_main_menu(self) -> None:
        """Displays the main menu."""
        pass

    @abstractmethod
    def show_error(self, message: str) -> None:
        """Displays an error message."""
        pass

    @abstractmethod
    def show_success(self, message: str) -> None:
        """Displays a success message."""
        pass

    @abstractmethod
    def show_info(self, message: str) -> None:
        """Displays an info message."""
        pass

    @abstractmethod
    def set_available_random_sources(
        self, sources: list[tuple[RandomSourceType, str]]
    ) -> None:
        """Sets available random sources."""
        pass

    @abstractmethod
    def display_generated_password(
        self, password: str, strength: PasswordStrength
    ) -> None:
        """Displays the generated password and its strength."""
        pass

    @abstractmethod
    def display_password_list(self, entries: list[PasswordEntry]) -> None:
        """Displays all saved passwords."""
        pass

    @property
    @abstractmethod
    def password_length(self) -> int:
        """Gets the desired password length."""
        pass

    @property
    @abstractmethod
    def use_special_chars(self) -> bool:
        """Gets whether to use special characters."""
        pass

    @property
    @abstractmethod
    def selected_random_source_index(self) -> int:
        """Gets the selected random source index."""
        pass

    @property
    @abstractmethod
    def password_key(self) -> str:
        """Gets the password key."""
        pass

    @property
    @abstractmethod
    def password_description(self) -> str:
        """Gets the password description."""
        pass

    @property
    @abstractmethod
    def delete_password_key(self) -> str:
        """Gets the key of password to delete."""
        pass


class PassivePasswordView(IPasswordView):
    """
    Passive View implementation for MVP pattern.
    Contains NO business logic - only handles display and input/output.
    Exposes callback methods for Presenter to subscribe to.
    """

    def __init__(self) -> None:
        self._available_random_sources: list[tuple[RandomSourceType, str]] = []
        self._password_length: int = 0
        self._use_special_chars: bool = False
        self._selected_random_source_index: int = 0
        self._password_key: str = ""
        self._password_description: str = ""
        self._delete_password_key: str = ""

        # Callbacks
        self.on_generate_requested = None
        self.on_save_requested = None
        self.on_view_all_requested = None
        self.on_delete_requested = None
        self.on_exit_requested = None

    def show_main_menu(self) -> None:
        """Display the main menu and handle basic flow."""
        print("\n========== PASSWORD GENERATOR (MVP) ==========")
        print("1. Generate a new password")
        print("2. Save password")
        print("3. View all saved passwords")
        print("4. Delete a password")
        print("5. Exit")
        choice = input("Select an option (1-5): ").strip()

        match choice:
            case "1":
                self._handle_generate_flow()
            case "2":
                self._handle_save_flow()
            case "3":
                if self.on_view_all_requested:
                    self.on_view_all_requested()
            case "4":
                self._handle_delete_flow()
            case "5":
                if self.on_exit_requested:
                    self.on_exit_requested()
            case _:
                self.show_error("Invalid option. Please select 1-5.")

    def show_error(self, message: str) -> None:
        """Display an error message."""
        print(f"\033[91m❌ Error: {message}\033[0m")

    def show_success(self, message: str) -> None:
        """Display a success message."""
        print(f"\033[92m✅ {message}\033[0m")

    def show_info(self, message: str) -> None:
        """Display an info message."""
        print(f"\033[96mℹ️  {message}\033[0m")

    def set_available_random_sources(
        self, sources: list[tuple[RandomSourceType, str]]
    ) -> None:
        """Set available random sources."""
        self._available_random_sources = sources

    def display_generated_password(
        self, password: str, strength: PasswordStrength
    ) -> None:
        """Display the generated password."""
        print(f"\nGenerated Password: {password}")
        print(f"Strength: {strength.name}")

    def display_password_list(self, entries: list[PasswordEntry]) -> None:
        """Display all saved passwords."""
        if not entries:
            self.show_info("No saved passwords.")
            return

        print("\n========== SAVED PASSWORDS ==========")
        for entry in entries:
            print(f"\nKey: {entry.key}")
            print(f"Password: {entry.password}")
            description = entry.description if entry.description else "(none)"
            print(f"Description: {description}")
            print(f"Created: {entry.created_at.strftime('%Y-%m-%d %H:%M:%S')}")

    @property
    def password_length(self) -> int:
        return self._password_length

    @property
    def use_special_chars(self) -> bool:
        return self._use_special_chars

    @property
    def selected_random_source_index(self) -> int:
        return self._selected_random_source_index

    @property
    def password_key(self) -> str:
        return self._password_key

    @property
    def password_description(self) -> str:
        return self._password_description

    @property
    def delete_password_key(self) -> str:
        return self._delete_password_key

    def _handle_generate_flow(self) -> None:
        """Handle the generate password flow."""
        self._password_length = self._get_password_length()
        self._use_special_chars = self._get_use_special_chars()
        self._selected_random_source_index = self._get_random_source_choice()

        if self.on_generate_requested:
            self.on_generate_requested()

    def _handle_save_flow(self) -> None:
        """Handle the save password flow."""
        self._password_key = self._get_password_key()
        self._password_description = self._get_password_description()

        if self.on_save_requested:
            self.on_save_requested()

    def _handle_delete_flow(self) -> None:
        """Handle the delete password flow."""
        self._delete_password_key = self._get_password_key_to_delete()

        if self.on_delete_requested:
            self.on_delete_requested()

    def _get_password_length(self) -> int:
        """Get password length from user."""
        while True:
            try:
                length = int(input("Enter password length (minimum 4): ").strip())
                if length >= 4:
                    return length
                self.show_error("Password length must be at least 4.")
            except ValueError:
                self.show_error("Invalid input. Please enter a valid number.")

    def _get_use_special_chars(self) -> bool:
        """Ask if user wants special characters."""
        while True:
            answer = input("Include special characters? (y/n): ").strip().lower()
            if answer == "y":
                return True
            elif answer == "n":
                return False
            else:
                self.show_error("Please enter 'y' or 'n'.")

    def _get_random_source_choice(self) -> int:
        """Get random source selection from user."""
        if not self._available_random_sources:
            self.show_error("No random sources available.")
            return -1

        print("\nAvailable random sources:")
        for i, (_, name) in enumerate(self._available_random_sources):
            print(f"{i + 1}. {name}")

        while True:
            try:
                choice = int(
                    input(
                        f"Select a source (1-{len(self._available_random_sources)}): "
                    ).strip()
                )
                if 1 <= choice <= len(self._available_random_sources):
                    return choice - 1
                self.show_error(
                    f"Please enter a number between 1 and {len(self._available_random_sources)}."
                )
            except ValueError:
                self.show_error("Invalid input. Please enter a valid number.")

    def _get_password_key(self) -> str:
        """Get password key from user."""
        while True:
            key = input("Enter a name/key for this password: ").strip()
            if key:
                return key
            self.show_error("Key cannot be empty.")

    def _get_password_description(self) -> str:
        """Get password description from user."""
        return input(
            "Enter an optional description (press Enter to skip): "
        ).strip()

    def _get_password_key_to_delete(self) -> str:
        """Get password key to delete from user."""
        while True:
            key = input("Enter the key of the password to delete: ").strip()
            if key:
                return key
            self.show_error("Key cannot be empty.")
