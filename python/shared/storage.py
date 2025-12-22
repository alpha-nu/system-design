"""JSON file-based password storage implementation."""

import json
import os
from pathlib import Path
from typing import Optional
from datetime import datetime

from .models import PasswordEntry
from .interfaces import IPasswordStorage


class JsonPasswordStorage(IPasswordStorage):
    """
    JSON file-based password storage implementation.
    Stores password entries in a JSON file in the user's home directory.
    """

    def __init__(self, storage_path: Optional[str] = None) -> None:
        """
        Initialize the JSON password storage.

        Args:
            storage_path: Optional custom storage path.
                         Defaults to ~/.password-generator/passwords.json
        """
        if storage_path is None:
            storage_path = os.path.join(
                Path.home(),
                ".password-generator",
                "passwords.json"
            )

        self._storage_path = storage_path

        # Ensure directory exists
        directory = os.path.dirname(self._storage_path)
        if directory:
            Path(directory).mkdir(parents=True, exist_ok=True)

    def save(self, entry: PasswordEntry) -> None:
        """Save a password entry."""
        if entry is None:
            raise ValueError("entry cannot be None")

        try:
            entries = list(self._load_all_internal())
            # Remove existing entry with same key
            entries = [e for e in entries if e.key != entry.key]
            # Add new entry
            entries.append(entry)

            # Serialize to JSON
            data = [self._entry_to_dict(e) for e in entries]
            with open(self._storage_path, 'w') as f:
                json.dump(data, f, indent=2, default=str)
        except Exception as e:
            raise IOError(f"Failed to save password to {self._storage_path}") from e

    def load(self, key: str) -> Optional[PasswordEntry]:
        """Load a password entry by key."""
        if not key or not key.strip():
            raise ValueError("key cannot be null or empty")

        for entry in self._load_all_internal():
            if entry.key == key:
                return entry
        return None

    def load_all(self) -> list[PasswordEntry]:
        """Retrieve all stored password entries."""
        return list(self._load_all_internal())

    def delete(self, key: str) -> bool:
        """Delete a password entry by key."""
        if not key or not key.strip():
            raise ValueError("key cannot be null or empty")

        try:
            entries = list(self._load_all_internal())
            initial_count = len(entries)
            entries = [e for e in entries if e.key != key]

            if len(entries) == initial_count:
                return False  # Nothing was deleted

            if not entries:
                # Delete the file if empty
                if os.path.exists(self._storage_path):
                    os.remove(self._storage_path)
            else:
                data = [self._entry_to_dict(e) for e in entries]
                with open(self._storage_path, 'w') as f:
                    json.dump(data, f, indent=2, default=str)

            return True
        except Exception as e:
            raise IOError(f"Failed to delete password from {self._storage_path}") from e

    def _load_all_internal(self) -> list[PasswordEntry]:
        """Load all entries from the storage file."""
        if not os.path.exists(self._storage_path):
            return []

        try:
            with open(self._storage_path, 'r') as f:
                content = f.read()
                if not content:
                    return []

                data = json.loads(content)
                if not isinstance(data, list):
                    return []

                entries = []
                for item in data:
                    try:
                        entry = self._dict_to_entry(item)
                        entries.append(entry)
                    except (ValueError, KeyError):
                        # Skip invalid entries
                        continue

                return entries
        except Exception as e:
            raise IOError(f"Failed to load passwords from {self._storage_path}") from e

    @staticmethod
    def _entry_to_dict(entry: PasswordEntry) -> dict:
        """Convert a PasswordEntry to a dictionary for JSON serialization."""
        return {
            "key": entry.key,
            "password": entry.password,
            "description": entry.description,
            "created_at": entry.created_at.isoformat() if entry.created_at else None,
        }

    @staticmethod
    def _dict_to_entry(data: dict) -> PasswordEntry:
        """Convert a dictionary from JSON to a PasswordEntry."""
        created_at = None
        if data.get("created_at"):
            created_at = datetime.fromisoformat(data["created_at"])

        return PasswordEntry(
            key=data["key"],
            password=data["password"],
            description=data.get("description", ""),
            created_at=created_at
        )
