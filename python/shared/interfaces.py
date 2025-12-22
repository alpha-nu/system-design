"""Abstract base classes for password generation framework."""

from abc import ABC, abstractmethod
from typing import Iterable

from .models import PasswordEntry, PasswordConfig


class IRandomSource(ABC):
    """Interface for random number generation sources."""

    @property
    @abstractmethod
    def name(self) -> str:
        """Gets the name of this random source."""
        pass

    @property
    @abstractmethod
    def is_available(self) -> bool:
        """Gets a value indicating whether this source is available on the current platform."""
        pass

    @abstractmethod
    def get_random_bytes(self, count: int) -> bytes:
        """
        Generates cryptographically secure random bytes.

        Args:
            count: Number of bytes to generate.

        Returns:
            Bytes array of random values.

        Raises:
            InvalidOperationError: When source is not available.
            ValueError: When count is invalid.
        """
        pass


class IPasswordStorage(ABC):
    """Interface for password persistence storage."""

    @abstractmethod
    def save(self, entry: PasswordEntry) -> None:
        """
        Saves a password entry.

        Args:
            entry: Password entry to save.

        Raises:
            ValueError: When entry is invalid.
            IOError: When save fails.
        """
        pass

    @abstractmethod
    def load(self, key: str) -> PasswordEntry | None:
        """
        Loads a password entry by key.

        Args:
            key: The password entry key.

        Returns:
            The password entry, or None if not found.

        Raises:
            ValueError: When key is invalid.
            IOError: When load fails.
        """
        pass

    @abstractmethod
    def load_all(self) -> Iterable[PasswordEntry]:
        """
        Retrieves all stored password entries.

        Returns:
            Collection of all password entries.

        Raises:
            IOError: When load fails.
        """
        pass

    @abstractmethod
    def delete(self, key: str) -> bool:
        """
        Deletes a password entry by key.

        Args:
            key: The password entry key.

        Returns:
            True if deleted, False if not found.

        Raises:
            ValueError: When key is invalid.
            IOError: When delete fails.
        """
        pass
