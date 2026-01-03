"""Pure business logic for password generation."""

from .models import PasswordConfig, PasswordStrength
from .interfaces import IRandomSource

LOWERCASE_CHARS = "abcdefghijklmnopqrstuvwxyz"
UPPERCASE_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
NUMERIC_CHARS = "0123456789"
SPECIAL_CHARS = "!@#$%^&*()-_=+[]{}|;:,.<>?"


class PasswordGenerator:
    """
    Pure business logic for generating passwords.
    This class has no external dependencies and focuses solely on password generation.
    """

    def __init__(self, random_source: IRandomSource) -> None:
        """
        Initialize the password generator.

        Args:
            random_source: Source of randomness for password generation.

        Raises:
            ValueError: When random_source is None.
        """
        if random_source is None:
            raise ValueError("random_source cannot be None")
        self._random_source = random_source

    def generate(self, config: PasswordConfig) -> str:
        """
        Generates a password according to the specified configuration.

        Args:
            config: Password configuration.

        Returns:
            The generated password.

        Raises:
            ValueError: When config is None.
        """
        if config is None:
            raise ValueError("config cannot be None")

        char_pool = self._build_character_pool(config.use_special_chars)
        random_bytes = self._random_source.get_random_bytes(config.length)

        password = ""
        for i in range(config.length):
            index = random_bytes[i] % len(char_pool)
            password += char_pool[index]

        return password

    @staticmethod
    def calculate_strength(password: str) -> PasswordStrength:
        """
        Calculates the strength of a password.

        Args:
            password: The password to evaluate.

        Returns:
            The password strength level.
        """
        if not password:
            return PasswordStrength.WEAK

        has_lower = any(c.islower() for c in password)
        has_upper = any(c.isupper() for c in password)
        has_digit = any(c.isdigit() for c in password)
        has_special = any(c in SPECIAL_CHARS for c in password)

        strength = sum([has_lower, has_upper, has_digit, has_special])

        if len(password) < 8:
            return PasswordStrength.WEAK
        if strength <= 1:
            return PasswordStrength.WEAK

        if strength == 4:
            return PasswordStrength.STRONG

        return PasswordStrength.MEDIUM

    @staticmethod
    def _build_character_pool(use_special_chars: bool) -> str:
        """Build the character pool based on configuration."""
        pool = LOWERCASE_CHARS + UPPERCASE_CHARS + NUMERIC_CHARS
        if use_special_chars:
            pool += SPECIAL_CHARS
        return pool
