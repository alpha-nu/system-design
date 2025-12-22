"""Random number source implementations."""

import os
import secrets
import platform
from typing import List, Tuple

from .interfaces import IRandomSource
from .models import RandomSourceType


class SystemRandomSource(IRandomSource):
    """
    System default random number generator.
    Uses platform-dependent random source (may not be cryptographically secure).
    """

    @property
    def name(self) -> str:
        return "System Random"

    @property
    def is_available(self) -> bool:
        return True

    def get_random_bytes(self, count: int) -> bytes:
        """Get random bytes using os.urandom (standard library)."""
        if count < 0:
            raise ValueError("count must be non-negative")
        return os.urandom(count)


class CryptoRandomSource(IRandomSource):
    """
    Cryptographically secure random number generator using Python's secrets module.
    """

    @property
    def name(self) -> str:
        return "Crypto Random (Python secrets)"

    @property
    def is_available(self) -> bool:
        return True

    def get_random_bytes(self, count: int) -> bytes:
        """Get cryptographically secure random bytes."""
        if count < 0:
            raise ValueError("count must be non-negative")
        return secrets.token_bytes(count)


class DevUrandomSource(IRandomSource):
    """
    Unix /dev/urandom random source.
    Only available on Unix/Linux/macOS systems. Reads directly from /dev/urandom device.
    """

    @property
    def name(self) -> str:
        return "Unix /dev/urandom"

    @property
    def is_available(self) -> bool:
        return platform.system() in ("Linux", "Darwin")  # Linux or macOS

    def get_random_bytes(self, count: int) -> bytes:
        """Read random bytes from /dev/urandom."""
        if not self.is_available:
            raise RuntimeError("This random source is only available on Unix/Linux systems")
        if count < 0:
            raise ValueError("count must be non-negative")

        try:
            with open("/dev/urandom", "rb") as f:
                return f.read(count)
        except Exception as e:
            raise RuntimeError(f"Failed to read from /dev/urandom: {e}") from e


class HardwareRngSource(IRandomSource):
    """
    Hardware-based random number generator.
    Falls back to CryptoRandom if hardware RNG is not available.
    """

    def __init__(self) -> None:
        self._fallback = CryptoRandomSource()

    @property
    def name(self) -> str:
        return "Hardware RNG"

    @property
    def is_available(self) -> bool:
        return True  # Fallback ensures it's always available

    def get_random_bytes(self, count: int) -> bytes:
        """Get random bytes using hardware RNG or fallback."""
        if count < 0:
            raise ValueError("count must be non-negative")
        # For now, use secrets which may leverage hardware RNG if available
        return self._fallback.get_random_bytes(count)


class RandomSourceFactory:
    """Factory for creating random source instances by type."""

    _SOURCES = {
        RandomSourceType.SYSTEM_RANDOM: SystemRandomSource,
        RandomSourceType.CRYPTO_RANDOM: CryptoRandomSource,
        RandomSourceType.DEV_URANDOM: DevUrandomSource,
        RandomSourceType.HARDWARE_RNG: HardwareRngSource,
    }

    @classmethod
    def create(cls, source_type: RandomSourceType) -> IRandomSource:
        """
        Creates a random source instance based on the specified type.

        Args:
            source_type: The desired random source type.

        Returns:
            An instance of the requested random source.

        Raises:
            ValueError: If source type is unknown.
            RuntimeError: If source is not available on current platform.
        """
        source_class = cls._SOURCES.get(source_type)
        if source_class is None:
            raise ValueError(f"Unknown random source type: {source_type}")

        source = source_class()
        if not source.is_available:
            raise RuntimeError(f"Random source '{source.name}' is not available on this platform")

        return source

    @classmethod
    def get_available_sources(cls) -> List[Tuple[RandomSourceType, IRandomSource]]:
        """
        Gets all available random sources on the current platform.

        Returns:
            List of tuples (RandomSourceType, IRandomSource) for available sources.
        """
        available = []
        for source_type in RandomSourceType:
            try:
                source = cls.create(source_type)
                available.append((source_type, source))
            except RuntimeError:
                # Skip unavailable sources
                pass
        return available
