"""Domain models for the password generator application."""

from dataclasses import dataclass
from datetime import datetime, timezone
from enum import Enum, auto


class PasswordStrength(Enum):
    """Enumeration representing the strength level of a generated password."""
    WEAK = auto()  # Lowercase letters only or fewer than 8 characters
    MEDIUM = auto()  # Mixed case and numbers, 8-15 characters
    STRONG = auto()  # Mixed case, numbers, and special characters, 16+ characters


class RandomSourceType(Enum):
    """Enumeration of available random number sources for password generation."""
    SYSTEM_RANDOM = auto()  # Platform-dependent system random
    CRYPTO_RANDOM = auto()  # Cryptographic secure random (Python secrets)
    DEV_URANDOM = auto()  # Unix /dev/urandom (Unix-only)
    HARDWARE_RNG = auto()  # Hardware-based RNG (if available)


@dataclass(frozen=True)
class PasswordConfig:
    """Immutable configuration for password generation."""
    length: int
    use_special_chars: bool
    random_source_type: RandomSourceType = RandomSourceType.CRYPTO_RANDOM

    def __post_init__(self) -> None:
        """Validate configuration after initialization."""
        if self.length < 4:
            raise ValueError("Password length must be at least 4 characters")


@dataclass(frozen=True)
class PasswordEntry:
    """Represents a stored password entry with metadata."""
    key: str
    password: str
    description: str = ""
    created_at: datetime = None

    def __post_init__(self) -> None:
        """Validate entry after initialization."""
        if not self.key or not self.key.strip():
            raise ValueError("Key cannot be null or empty")
        if not self.password or not self.password.strip():
            raise ValueError("Password cannot be null or empty")
        
        # Set default created_at if not provided
        if self.created_at is None:
            object.__setattr__(self, 'created_at', datetime.now(timezone.utc))
