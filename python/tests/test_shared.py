"""Unit tests for shared domain models and utilities."""

import pytest
from datetime import datetime

from shared import (
    PasswordConfig,
    PasswordEntry,
    PasswordStrength,
    RandomSourceType,
    PasswordGenerator,
    RandomSourceFactory,
    JsonPasswordStorage,
)
from .test_helpers import (
    MockRandomSource,
    InMemoryPasswordStorage,
    TempFileStorage,
)


class TestPasswordConfig:
    """Tests for PasswordConfig domain model."""

    def test_valid_construction(self):
        """Test creating a valid PasswordConfig."""
        config = PasswordConfig(
            length=12,
            use_special_chars=True,
            random_source_type=RandomSourceType.CRYPTO_RANDOM,
        )

        assert config.length == 12
        assert config.use_special_chars is True
        assert config.random_source_type == RandomSourceType.CRYPTO_RANDOM

    def test_invalid_length_raises_error(self):
        """Test that length < 4 raises ValueError."""
        with pytest.raises(ValueError):
            PasswordConfig(length=3, use_special_chars=True)

    def test_default_random_source_type(self):
        """Test that random source type defaults to CRYPTO_RANDOM."""
        config = PasswordConfig(length=12, use_special_chars=False)
        assert config.random_source_type == RandomSourceType.CRYPTO_RANDOM


class TestPasswordEntry:
    """Tests for PasswordEntry domain model."""

    def test_valid_construction(self):
        """Test creating a valid PasswordEntry."""
        entry = PasswordEntry(
            key="testkey", password="testpass", description="test"
        )

        assert entry.key == "testkey"
        assert entry.password == "testpass"
        assert entry.description == "test"

    def test_invalid_key_raises_error(self):
        """Test that empty key raises ValueError."""
        with pytest.raises(ValueError):
            PasswordEntry(key="", password="password")

    def test_invalid_password_raises_error(self):
        """Test that empty password raises ValueError."""
        with pytest.raises(ValueError):
            PasswordEntry(key="key", password="")

    def test_default_created_at(self):
        """Test that created_at is set to current time."""
        before = datetime.utcnow()
        entry = PasswordEntry(key="key", password="pass")
        after = datetime.utcnow()

        assert before <= entry.created_at <= after

    def test_empty_description_is_allowed(self):
        """Test that empty description is allowed."""
        entry = PasswordEntry(key="key", password="pass", description="")
        assert entry.description == ""


class TestPasswordGenerator:
    """Tests for PasswordGenerator class."""

    def test_generate_returns_correct_length(self):
        """Test that generated password has correct length."""
        mock_random = MockRandomSource(bytes([0x10, 0x20, 0x30]))
        generator = PasswordGenerator(mock_random)
        config = PasswordConfig(length=10, use_special_chars=False)

        password = generator.generate(config)

        assert len(password) == 10

    def test_generate_without_special_chars(self):
        """Test that password without special chars is alphanumeric only."""
        mock_random = MockRandomSource(bytes(range(256)))
        generator = PasswordGenerator(mock_random)
        config = PasswordConfig(length=50, use_special_chars=False)

        password = generator.generate(config)

        assert all(c.isalnum() for c in password)

    def test_generate_with_special_chars(self):
        """Test that password can contain special chars."""
        mock_random = MockRandomSource(bytes([255]))
        generator = PasswordGenerator(mock_random)
        config = PasswordConfig(length=20, use_special_chars=True)

        password = generator.generate(config)

        assert len(password) == 20

    def test_generate_with_null_config_raises_error(self):
        """Test that None config raises ValueError."""
        mock_random = MockRandomSource(bytes([0x01]))
        generator = PasswordGenerator(mock_random)

        with pytest.raises(ValueError):
            generator.generate(None)

    def test_constructor_with_null_random_raises_error(self):
        """Test that None random source raises ValueError."""
        with pytest.raises(ValueError):
            PasswordGenerator(None)

    @pytest.mark.parametrize(
        "password,expected_strength",
        [
            ("abcdefgh", PasswordStrength.WEAK),
            ("abcDefgh", PasswordStrength.MEDIUM),
            ("abcDef12", PasswordStrength.MEDIUM),
            ("abcDef12!", PasswordStrength.STRONG),
            ("AbCdEf12!@#", PasswordStrength.STRONG),
        ],
    )
    def test_calculate_strength(self, password, expected_strength):
        """Test password strength calculation."""
        strength = PasswordGenerator.calculate_strength(password)
        assert strength == expected_strength

    def test_calculate_strength_empty_password(self):
        """Test that empty password is weak."""
        strength = PasswordGenerator.calculate_strength("")
        assert strength == PasswordStrength.WEAK

    def test_calculate_strength_short_password(self):
        """Test that short password is weak."""
        strength = PasswordGenerator.calculate_strength("aB1!")
        assert strength == PasswordStrength.WEAK


class TestRandomSourceFactory:
    """Tests for RandomSourceFactory."""

    def test_create_crypto_random(self):
        """Test creating CryptoRandom source."""
        source = RandomSourceFactory.create(RandomSourceType.CRYPTO_RANDOM)
        assert source is not None
        assert source.is_available

    def test_get_available_sources_not_empty(self):
        """Test that at least one source is available."""
        sources = RandomSourceFactory.get_available_sources()
        assert len(sources) > 0

    def test_all_available_sources_are_available(self):
        """Test that all returned sources are actually available."""
        sources = RandomSourceFactory.get_available_sources()
        assert all(source.is_available for _, source in sources)


class TestInMemoryPasswordStorage:
    """Tests for InMemoryPasswordStorage."""

    def test_save_and_load(self):
        """Test saving and loading a password entry."""
        storage = InMemoryPasswordStorage()
        entry = PasswordEntry("testkey", "testpass", "test description")

        storage.save(entry)
        loaded = storage.load("testkey")

        assert loaded is not None
        assert loaded.key == "testkey"
        assert loaded.password == "testpass"

    def test_load_nonexistent_returns_none(self):
        """Test that loading nonexistent entry returns None."""
        storage = InMemoryPasswordStorage()
        result = storage.load("nonexistent")
        assert result is None

    def test_load_all(self):
        """Test loading all entries."""
        storage = InMemoryPasswordStorage()
        storage.save(PasswordEntry("key1", "pass1"))
        storage.save(PasswordEntry("key2", "pass2"))

        all_entries = storage.load_all()

        assert len(all_entries) == 2

    def test_delete_existing(self):
        """Test deleting an existing entry."""
        storage = InMemoryPasswordStorage()
        entry = PasswordEntry("key", "pass")
        storage.save(entry)

        deleted = storage.delete("key")

        assert deleted is True
        assert storage.load("key") is None

    def test_delete_nonexistent(self):
        """Test deleting nonexistent entry returns False."""
        storage = InMemoryPasswordStorage()
        deleted = storage.delete("nonexistent")
        assert deleted is False

    def test_save_duplicate_key_updates(self):
        """Test that saving with duplicate key updates the entry."""
        storage = InMemoryPasswordStorage()
        storage.save(PasswordEntry("key", "pass1"))
        storage.save(PasswordEntry("key", "pass2"))

        loaded = storage.load("key")

        assert loaded.password == "pass2"
        assert len(storage.load_all()) == 1


class TestJsonPasswordStorage:
    """Tests for JsonPasswordStorage with file-based storage."""

    def test_save_and_load_from_file(self):
        """Test saving and loading from actual file."""
        storage = TempFileStorage()
        try:
            entry = PasswordEntry("key", "password", "description")
            storage.save(entry)

            loaded = storage.load("key")

            assert loaded is not None
            assert loaded.key == "key"
            assert loaded.password == "password"
        finally:
            storage.cleanup()

    def test_persistence_across_instances(self):
        """Test that data persists across storage instances."""
        temp_storage = TempFileStorage()
        try:
            # Save with first instance
            entry = PasswordEntry("persistent", "data")
            temp_storage.save(entry)
            temp_file = temp_storage._temp_file

            # Load with new instance
            from shared import JsonPasswordStorage
            new_storage = JsonPasswordStorage(temp_file)
            loaded = new_storage.load("persistent")

            assert loaded is not None
            assert loaded.password == "data"
        finally:
            temp_storage.cleanup()
