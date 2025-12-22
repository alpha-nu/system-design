"""Test helpers and mocks for password generator tests."""

import os
import tempfile
from typing import Optional

from shared import (
    IRandomSource,
    IPasswordStorage,
    PasswordEntry,
    PasswordStrength,
)


class MockRandomSource(IRandomSource):
    """Mock random source for testing with predetermined bytes."""

    def __init__(self, predetermined_bytes: bytes) -> None:
        self._predetermined_bytes = predetermined_bytes
        self._call_count = 0

    @property
    def name(self) -> str:
        return "Mock Random"

    @property
    def is_available(self) -> bool:
        return True

    def get_random_bytes(self, count: int) -> bytes:
        """Return predetermined bytes cycling through the pattern."""
        if count < 0:
            raise ValueError("count must be non-negative")

        self._call_count += 1
        result = bytearray()
        for i in range(count):
            result.append(self._predetermined_bytes[i % len(self._predetermined_bytes)])
        return bytes(result)

    @property
    def call_count(self) -> int:
        return self._call_count


class InMemoryPasswordStorage(IPasswordStorage):
    """In-memory password storage for testing."""

    def __init__(self) -> None:
        self._storage: dict[str, PasswordEntry] = {}

    def save(self, entry: PasswordEntry) -> None:
        """Save a password entry."""
        if entry is None:
            raise ValueError("entry cannot be None")
        self._storage[entry.key] = entry

    def load(self, key: str) -> Optional[PasswordEntry]:
        """Load a password entry by key."""
        if not key or not key.strip():
            raise ValueError("key cannot be null or empty")
        return self._storage.get(key)

    def load_all(self) -> list[PasswordEntry]:
        """Load all password entries."""
        return list(self._storage.values())

    def delete(self, key: str) -> bool:
        """Delete a password entry."""
        if not key or not key.strip():
            raise ValueError("key cannot be null or empty")
        return key in self._storage and bool(self._storage.pop(key))


class TempFileStorage(IPasswordStorage):
    """Temporary file-based storage for testing."""

    def __init__(self) -> None:
        self._temp_dir = tempfile.mkdtemp()
        self._temp_file = os.path.join(self._temp_dir, "test_passwords.json")
        # Import here to avoid circular imports
        from shared import JsonPasswordStorage
        self._storage = JsonPasswordStorage(self._temp_file)

    def save(self, entry: PasswordEntry) -> None:
        """Save a password entry."""
        self._storage.save(entry)

    def load(self, key: str) -> Optional[PasswordEntry]:
        """Load a password entry by key."""
        return self._storage.load(key)

    def load_all(self) -> list[PasswordEntry]:
        """Load all password entries."""
        return self._storage.load_all()

    def delete(self, key: str) -> bool:
        """Delete a password entry."""
        return self._storage.delete(key)

    def cleanup(self) -> None:
        """Clean up temporary files."""
        import shutil
        if os.path.exists(self._temp_dir):
            shutil.rmtree(self._temp_dir)
