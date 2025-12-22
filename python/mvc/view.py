"""MVC View for password generator application."""

from shared import PasswordEntry, PasswordStrength, RandomSourceType


class ConsoleView:
    """
    Passive View for MVC pattern.
    Contains NO business logic - only handles display and basic input.
    """

    def show_main_menu(self) -> str:
        """Display the main menu and return the user's choice."""
        print("\n========== PASSWORD GENERATOR (MVC) ==========")
        print("1. Generate a new password")
        print("2. Save password")
        print("3. View all saved passwords")
        print("4. Delete a password")
        print("5. Exit")
        return input("Select an option (1-5): ").strip()

    def show_error(self, message: str) -> None:
        """Display an error message."""
        print(f"\033[91m❌ Error: {message}\033[0m")

    def show_success(self, message: str) -> None:
        """Display a success message."""
        print(f"\033[92m✅ {message}\033[0m")

    def show_info(self, message: str) -> None:
        """Display an info message."""
        print(f"\033[96mℹ️  {message}\033[0m")

    def get_password_length(self) -> int:
        """Get the desired password length from the user."""
        while True:
            try:
                length = int(input("Enter password length (minimum 4): ").strip())
                if length >= 4:
                    return length
                self.show_error("Password length must be at least 4.")
            except ValueError:
                self.show_error("Invalid input. Please enter a valid number.")

    def get_use_special_chars(self) -> bool:
        """Ask if the user wants to include special characters."""
        while True:
            answer = input("Include special characters? (y/n): ").strip().lower()
            if answer == "y":
                return True
            elif answer == "n":
                return False
            else:
                self.show_error("Please enter 'y' or 'n'.")

    def get_random_source_choice(
        self, sources: list[tuple[RandomSourceType, str]]
    ) -> int:
        """Get the random source selection from the user."""
        if not sources:
            self.show_error("No random sources available.")
            return -1

        print("\nAvailable random sources:")
        for i, (_, name) in enumerate(sources):
            print(f"{i + 1}. {name}")

        while True:
            try:
                choice = int(
                    input(f"Select a source (1-{len(sources)}): ").strip()
                )
                if 1 <= choice <= len(sources):
                    return choice - 1
                self.show_error(
                    f"Please enter a number between 1 and {len(sources)}."
                )
            except ValueError:
                self.show_error("Invalid input. Please enter a valid number.")

    def show_generated_password(
        self, password: str, strength: PasswordStrength
    ) -> None:
        """Display the generated password and its strength."""
        print(f"\nGenerated Password: {password}")
        print(f"Strength: {strength.name}")

    def get_password_key(self) -> str:
        """Get the name/key for saving a password."""
        while True:
            key = input("Enter a name/key for this password: ").strip()
            if key:
                return key
            self.show_error("Key cannot be empty.")

    def get_password_description(self) -> str:
        """Get the optional description for a password."""
        return input(
            "Enter an optional description (press Enter to skip): "
        ).strip()

    def show_password_list(self, entries: list[PasswordEntry]) -> None:
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

    def get_password_key_to_delete(self) -> str:
        """Get the key of a password to delete."""
        while True:
            key = input("Enter the key of the password to delete: ").strip()
            if key:
                return key
            self.show_error("Key cannot be empty.")

    def show_goodbye(self) -> None:
        """Display a goodbye message."""
        print("\nGoodbye!")
