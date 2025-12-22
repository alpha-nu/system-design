"""MVVM View for password generator."""

from shared import PasswordSourceType, PasswordStrength
from .viewmodel import PasswordViewModel


class PasswordView:
    """
    MVVM View for console-based password generator.
    In MVVM, the View binds to the ViewModel's properties and commands.
    For console apps, we simulate binding by manually updating from ViewModel.
    """

    def __init__(self, view_model: PasswordViewModel) -> None:
        """
        Initialize the view.

        Args:
            view_model: The password view model.
        """
        if view_model is None:
            raise ValueError("view_model cannot be None")

        self._view_model = view_model

        # Subscribe to ViewModel property changes
        self._view_model.subscribe_to_property_changed(
            self._on_view_model_property_changed
        )

    def run(self) -> None:
        """Run the main view loop."""
        while True:
            try:
                choice = self._show_main_menu()

                match choice:
                    case "1":
                        self._handle_generate_flow()
                    case "2":
                        self._handle_save_flow()
                    case "3":
                        self._view_model.view_all_passwords()
                        self._display_all_passwords()
                    case "4":
                        self._handle_delete_flow()
                    case "5":
                        self._show_goodbye()
                        return
                    case _:
                        self._show_error("Invalid option. Please select 1-5.")
            except Exception as e:
                self._show_error(
                    f"An unexpected error occurred: {str(e)}"
                )

    def _show_main_menu(self) -> str:
        """Display the main menu."""
        print("\n========== PASSWORD GENERATOR (MVVM) ==========")
        print("1. Generate a new password")
        print("2. Save password")
        print("3. View all saved passwords")
        print("4. Delete a password")
        print("5. Exit")
        return input("Select an option (1-5): ").strip()

    def _handle_generate_flow(self) -> None:
        """Handle password generation flow."""
        # Get user inputs and update ViewModel properties
        try:
            length = int(
                input("Enter password length (minimum 4): ").strip()
            )
            if length < 4:
                self._show_error("Password length must be at least 4.")
                return
            self._view_model.password_length = length
        except ValueError:
            self._show_error("Invalid input.")
            return

        while True:
            answer = (
                input("Include special characters? (y/n): ").strip().lower()
            )
            if answer == "y":
                self._view_model.use_special_chars = True
                break
            elif answer == "n":
                self._view_model.use_special_chars = False
                break

        sources = self._view_model.available_random_sources
        print("\nAvailable random sources:")
        for i, (_, name) in enumerate(sources):
            print(f"{i + 1}. {name}")

        while True:
            try:
                choice = int(
                    input(f"Select a source (1-{len(sources)}): ").strip()
                )
                if 1 <= choice <= len(sources):
                    self._view_model.selected_random_source = sources[
                        choice - 1
                    ][0]
                    break
                self._show_error(
                    f"Please enter a number between 1 and {len(sources)}."
                )
            except ValueError:
                self._show_error("Invalid input. Please enter a valid number.")

        # Execute the command
        self._view_model.generate_password()

        # Display result
        self._display_current_password()

    def _handle_save_flow(self) -> None:
        """Handle password save flow."""
        if not self._view_model.current_password:
            self._show_error("No password generated yet.")
            return

        key = input("Enter a name/key for this password: ").strip()
        description = input(
            "Enter an optional description (press Enter to skip): "
        ).strip()

        self._view_model.save_password(key, description)
        self._display_all_passwords()

    def _handle_delete_flow(self) -> None:
        """Handle password deletion flow."""
        key = input("Enter the key of the password to delete: ").strip()

        if key:
            deleted = self._view_model.delete_password(key)
            if deleted:
                self._show_success(f"Password '{key}' deleted.")
            else:
                self._show_error(f"Password '{key}' not found.")

    def _display_current_password(self) -> None:
        """Display the currently generated password."""
        print(f"\nGenerated Password: {self._view_model.current_password}")
        print(
            f"Strength: {self._view_model.current_password_strength.name}"
        )

    def _display_all_passwords(self) -> None:
        """Display all saved passwords."""
        entries = self._view_model.saved_passwords
        if not entries:
            self._show_info("No saved passwords.")
            return

        print("\n========== SAVED PASSWORDS ==========")
        for entry in entries:
            print(f"\nKey: {entry.key}")
            print(f"Password: {entry.password}")
            description = (
                entry.description if entry.description else "(none)"
            )
            print(f"Description: {description}")
            print(
                f"Created: {entry.created_at.strftime('%Y-%m-%d %H:%M:%S')}"
            )

    def _show_error(self, message: str) -> None:
        """Display an error message."""
        print(f"\033[91m❌ Error: {message}\033[0m")

    def _show_success(self, message: str) -> None:
        """Display a success message."""
        print(f"\033[92m✅ {message}\033[0m")

    def _show_info(self, message: str) -> None:
        """Display an info message."""
        print(f"\033[96mℹ️  {message}\033[0m")

    def _show_goodbye(self) -> None:
        """Display a goodbye message."""
        print("\nGoodbye!")

    def _on_view_model_property_changed(self, args) -> None:
        """Handle ViewModel property changes."""
        # In a real WPF/XAML application, this binding would be automatic
        # For console app, we just acknowledge the change
        # The View would update its display based on property changes
        pass
