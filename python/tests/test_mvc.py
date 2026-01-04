import builtins
import runpy
import pytest

from mvc.controller import PasswordController
from mvc.model import PasswordModel
from mvc.view import ConsoleView
from shared import (
    JsonPasswordStorage,
    PasswordConfig,
    PasswordEntry,
    PasswordGenerator,
    RandomSourceFactory,
    RandomSourceType,
)
from shared import random_sources as random_sources_module
from shared import storage as storage_module
from shared.interfaces import IPasswordStorage, IRandomSource
from shared.models import PasswordStrength
from tests.test_helpers import InMemoryPasswordStorage, MockRandomSource

_ORIGINAL_RANDOM_GET_SOURCES = RandomSourceFactory.get_available_sources
_ORIGINAL_RANDOM_CREATE = RandomSourceFactory.create


class StubConsoleView:
    def __init__(self) -> None:
        self.length = 12
        self.use_special_chars = True
        self.random_choice = 0
        self.password_key = "key"
        self.password_description = "desc"
        self.delete_key = "key"
        self.generated: list[tuple[str, PasswordStrength]] = []
        self.errors: list[str] = []
        self.successes: list[str] = []
        self.last_entries: list[PasswordEntry] | None = None
        self.info_messages: list[str] = []

    def get_password_length(self) -> int:
        return self.length

    def get_use_special_chars(self) -> bool:
        return self.use_special_chars

    def get_random_source_choice(self, sources):
        return self.random_choice

    def show_generated_password(
        self, password: str, strength: PasswordStrength
    ) -> None:
        self.generated.append((password, strength))

    def show_error(self, message: str) -> None:
        self.errors.append(message)

    def show_success(self, message: str) -> None:
        self.successes.append(message)

    def get_password_key(self) -> str:
        return self.password_key

    def get_password_description(self) -> str:
        return self.password_description

    def show_password_list(self, entries: list[PasswordEntry]) -> None:
        self.last_entries = entries

    def get_password_key_to_delete(self) -> str:
        return self.delete_key

    def show_info(self, message: str) -> None:
        self.info_messages.append(message)


@pytest.fixture(autouse=True)
def deterministic_random(monkeypatch):
    available = [(RandomSourceType.SYSTEM_RANDOM, MockRandomSource(b"\x01"))]

    def fake_available():
        return available

    def fake_create(_: RandomSourceType):
        return MockRandomSource(b"\x02")

    monkeypatch.setattr(RandomSourceFactory, "get_available_sources", fake_available)
    monkeypatch.setattr(RandomSourceFactory, "create", fake_create)


def controller_setup():
    storage = InMemoryPasswordStorage()
    generator = PasswordGenerator(MockRandomSource(b"\x03"))
    model = PasswordModel(generator, storage)
    view = StubConsoleView()
    controller = PasswordController(model, view)
    return controller, view, storage


def stub_input(monkeypatch, responses: list[str]) -> None:
    iterator = iter(responses)

    def fake_input(prompt: str = "") -> str:
        return next(iterator)

    monkeypatch.setattr("builtins.input", fake_input)


class TestPasswordController:
    def test_generate_password_success(self):
        controller, view, _ = controller_setup()
        controller._handle_generate_password()

        assert view.generated
        assert controller._current_password is not None
        assert isinstance(view.generated[0][1], PasswordStrength)
        assert not view.errors

    def test_generate_password_aborts_on_negative_source(self):
        controller, view, _ = controller_setup()
        view.random_choice = -1

        controller._handle_generate_password()

        assert not view.generated
        assert controller._current_password is None

    def test_generate_password_handles_errors(self):
        controller, view, _ = controller_setup()

        def broken_length() -> int:
            raise ValueError("boom")

        view.get_password_length = broken_length
        controller._handle_generate_password()

        assert view.errors and "boom" in view.errors[-1]
        assert controller._current_password is None

    def test_save_password_requires_current_value(self):
        controller, view, _ = controller_setup()
        controller._handle_save_password()

        assert view.errors[-1].startswith("No password generated yet")

    def test_save_password_success(self):
        controller, view, storage = controller_setup()
        controller._current_password = "secret"
        controller._handle_save_password()

        assert storage.load("key") is not None
        assert controller._current_password is None
        assert "Password saved as 'key'." in view.successes[-1]

    def test_view_passwords_displays_entries(self):
        controller, view, storage = controller_setup()
        storage.save(PasswordEntry("display", "pass"))

        controller._handle_view_passwords()

        assert view.last_entries
        assert view.last_entries[0].key == "display"

    def test_delete_password_success(self):
        controller, view, storage = controller_setup()
        storage.save(PasswordEntry("del", "pass"))
        view.delete_key = "del"

        controller._handle_delete_password()

        assert storage.load("del") is None
        assert "Password 'del' deleted." in view.successes[-1]

    def test_delete_password_not_found(self):
        controller, view, _ = controller_setup()
        view.delete_key = "missing"

        controller._handle_delete_password()

        assert "not found" in view.errors[-1]


class TestPasswordModel:
    def test_generate_password_requires_config(self):
        storage = InMemoryPasswordStorage()
        generator = PasswordGenerator(MockRandomSource(b"\x04"))
        model = PasswordModel(generator, storage)

        with pytest.raises(ValueError):
            model.generate_password(None)

    def test_save_password_requires_entry(self):
        storage = InMemoryPasswordStorage()
        generator = PasswordGenerator(MockRandomSource(b"\x05"))
        model = PasswordModel(generator, storage)

        with pytest.raises(ValueError):
            model.save_password(None)

    def test_delete_password_validates_key(self):
        storage = InMemoryPasswordStorage()
        generator = PasswordGenerator(MockRandomSource(b"\x06"))
        model = PasswordModel(generator, storage)

        with pytest.raises(ValueError):
            model.delete_password("")

    def test_get_all_and_delete_of_model(self):
        storage = InMemoryPasswordStorage()
        generator = PasswordGenerator(MockRandomSource(b"\x07"))
        model = PasswordModel(generator, storage)
        entry = PasswordEntry("key", "pass")

        model.save_password(entry)
        assert model.get_all_passwords()
        assert model.delete_password("key")
        assert not model.get_all_passwords()

    def test_get_available_random_sources(self, monkeypatch):
        class FakeSource:
            name = "fake"

            @property
            def is_available(self) -> bool:
                return True

            def get_random_bytes(self, count: int) -> bytes:
                return b"\x00" * count

        def fake_sources():
            return [(RandomSourceType.SYSTEM_RANDOM, FakeSource())]

        monkeypatch.setattr(RandomSourceFactory, "get_available_sources", fake_sources)

        storage = InMemoryPasswordStorage()
        generator = PasswordGenerator(MockRandomSource(b"\x08"))
        model = PasswordModel(generator, storage)

        available = model.get_available_random_sources()
        assert available == [(RandomSourceType.SYSTEM_RANDOM, "fake")]


class TestConsoleView:
    def test_get_password_length_retries(self, monkeypatch):
        view = ConsoleView()
        errors: list[str] = []
        view.show_error = lambda message: errors.append(message)
        stub_input(monkeypatch, ["bad", "2", "5"])

        assert view.get_password_length() == 5
        assert errors

    def test_get_use_special_chars_retries(self, monkeypatch):
        view = ConsoleView()
        stub_input(monkeypatch, ["maybe", "N"])

        assert view.get_use_special_chars() is False

    def test_get_random_source_choice_invalid_then_valid(self, monkeypatch):
        view = ConsoleView()
        errors: list[str] = []
        view.show_error = lambda message: errors.append(message)
        sources = [
            (RandomSourceType.SYSTEM_RANDOM, "System"),
            (RandomSourceType.CRYPTO_RANDOM, "Crypto"),
        ]
        stub_input(monkeypatch, ["bad", "0", "2"])

        assert view.get_random_source_choice(sources) == 1
        assert errors

    def test_get_random_source_choice_no_sources(self):
        view = ConsoleView()
        messages: list[str] = []
        view.show_error = lambda message: messages.append(message)

        assert view.get_random_source_choice([]) == -1
        assert messages

    def test_get_password_key_retries(self, monkeypatch):
        view = ConsoleView()
        inputs = ["", "   ", "name"]
        stub_input(monkeypatch, inputs)

        assert view.get_password_key() == "name"

    def test_get_password_key_to_delete_retries(self, monkeypatch):
        view = ConsoleView()
        stub_input(monkeypatch, ["", "target"])

        assert view.get_password_key_to_delete() == "target"

    def test_get_password_description_returns_value(self, monkeypatch):
        view = ConsoleView()
        stub_input(monkeypatch, ["desc"])

        assert view.get_password_description() == "desc"

    def test_show_password_list_empty(self):
        view = ConsoleView()
        infos: list[str] = []
        view.show_info = lambda message: infos.append(message)

        view.show_password_list([])
        assert infos and "No saved passwords." in infos[0]

    def test_show_password_list_entries(self, capsys):
        view = ConsoleView()
        entry = PasswordEntry("k", "value")

        view.show_password_list([entry])
        captured = capsys.readouterr()
        assert "Key: k" in captured.out
        assert "Password: value" in captured.out


class ControllerModelStub:
    def __init__(self):
        self.raise_on_save = False
        self.raise_on_view = False
        self.raise_on_delete = False

    def get_available_random_sources(self):
        return [(RandomSourceType.CRYPTO_RANDOM, "Crypto")]

    def get_password_strength(self, password: str) -> PasswordStrength:
        return PasswordStrength.MEDIUM

    def save_password(self, entry: PasswordEntry) -> None:
        if self.raise_on_save:
            raise RuntimeError("save failure")

    def get_all_passwords(self) -> list[PasswordEntry]:
        if self.raise_on_view:
            raise RuntimeError("view failure")
        return []

    def delete_password(self, key: str) -> bool:
        if self.raise_on_delete:
            raise RuntimeError("delete failure")
        return False


class ControllerViewStub:
    def __init__(self, main_choices: list[str] | None = None) -> None:
        self._choices = main_choices[:] if main_choices else []
        self.errors: list[str] = []
        self.success: list[str] = []
        self.goodbye_shown = False

    def show_main_menu(self) -> str:
        if self._choices:
            return self._choices.pop(0)
        return "5"

    def show_error(self, message: str) -> None:
        self.errors.append(message)

    def show_success(self, message: str) -> None:
        self.success.append(message)

    def show_goodbye(self) -> None:
        self.goodbye_shown = True

    def get_password_length(self) -> int:
        return 8

    def get_use_special_chars(self) -> bool:
        return False

    def get_random_source_choice(self, sources: list[tuple[RandomSourceType, str]]) -> int:
        return 0

    def show_generated_password(self, password: str, _strength: PasswordStrength) -> None:
        pass

    def get_password_key(self) -> str:
        return "key"

    def get_password_description(self) -> str:
        return "desc"

    def show_password_list(self, entries: list[PasswordEntry]) -> None:
        pass

    def get_password_key_to_delete(self) -> str:
        return "key"


class CoverageRandomSource(IRandomSource):
    @property
    def name(self) -> str:
        super().name
        return "coverage"

    @property
    def is_available(self) -> bool:
        super().is_available
        return True

    def get_random_bytes(self, count: int) -> bytes:
        super().get_random_bytes(count)
        return bytes([0] * count)


class CoverageStorage(IPasswordStorage):
    def save(self, entry: PasswordEntry) -> None:
        super().save(entry)

    def load(self, key: str) -> PasswordEntry | None:
        super().load(key)
        return None

    def load_all(self):
        super().load_all()
        return []

    def delete(self, key: str) -> bool:
        super().delete(key)
        return False


def test_mvc_main_module_runs_quickly(monkeypatch, tmp_path):
    monkeypatch.setattr(
        storage_module.Path,
        "home",
        classmethod(lambda cls: tmp_path),
    )

    inputs = ["5"]

    def fake_input(prompt: str = "") -> str:
        if inputs:
            return inputs.pop(0)
        return "5"

    monkeypatch.setattr("builtins.input", fake_input)

    runpy.run_module("mvc", run_name="__main__")


def test_password_controller_validates_dependencies():
    model = ControllerModelStub()
    view = ControllerViewStub()
    with pytest.raises(ValueError):
        PasswordController(None, view)
    with pytest.raises(ValueError):
        PasswordController(model, None)


def test_password_controller_handles_invalid_choice():
    model = ControllerModelStub()
    view = ControllerViewStub(["invalid", "5"])
    controller = PasswordController(model, view)
    controller.run()
    assert any("Invalid option" in msg for msg in view.errors)
    assert view.goodbye_shown


def test_password_controller_handles_main_menu_exception():
    class ExceptionMainMenuView(ControllerViewStub):
        def show_main_menu(self) -> str:
            if not getattr(self, "_raised", False):
                self._raised = True
                raise RuntimeError("boom")
            return super().show_main_menu()

    model = ControllerModelStub()
    view = ExceptionMainMenuView()
    controller = PasswordController(model, view)
    controller.run()
    assert any("An unexpected error" in msg for msg in view.errors)


def test_password_controller_handler_errors():
    model = ControllerModelStub()
    view = ControllerViewStub()
    controller = PasswordController(model, view)
    controller._current_password = "secret"

    model.raise_on_save = True
    controller._handle_save_password()
    assert any("save failure" in msg for msg in view.errors)

    model.raise_on_view = True
    controller._handle_view_passwords()
    assert any("view failure" in msg for msg in view.errors)

    model.raise_on_delete = True
    controller._handle_delete_password()
    assert any("delete failure" in msg for msg in view.errors)


def test_password_model_constructor_validates_dependencies():
    generator = PasswordGenerator(MockRandomSource(bytes([0x01])))
    storage = InMemoryPasswordStorage()
    with pytest.raises(ValueError):
        PasswordModel(None, storage)
    with pytest.raises(ValueError):
        PasswordModel(generator, None)


def test_password_model_validation_errors():
    generator = PasswordGenerator(MockRandomSource(bytes([0x01])))
    storage = InMemoryPasswordStorage()
    model = PasswordModel(generator, storage)

    with pytest.raises(ValueError):
        model.generate_password(None)
    with pytest.raises(ValueError):
        model.save_password(None)
    with pytest.raises(ValueError):
        model.delete_password("")

    valid_config = PasswordConfig(length=4, use_special_chars=False)
    assert isinstance(model.generate_password(valid_config), str)


def test_console_view_show_info_runs():
    ConsoleView().show_info("info")


def test_interface_pass_methods_execute():
    source = CoverageRandomSource()
    assert source.name == "coverage"
    assert source.is_available
    assert source.get_random_bytes(1) == b"\x00"

    storage_impl = CoverageStorage()
    storage_impl.save(PasswordEntry("key", "pass"))
    storage_impl.load("key")
    storage_impl.load_all()
    storage_impl.delete("key")


def test_random_sources_cover_branches(monkeypatch):
    system = random_sources_module.SystemRandomSource()
    assert system.name == "System Random"
    assert system.is_available
    assert isinstance(system.get_random_bytes(1), bytes)

    crypto = random_sources_module.CryptoRandomSource()
    assert crypto.name.startswith("Crypto")
    assert crypto.is_available
    assert len(crypto.get_random_bytes(2)) == 2

    hardware = random_sources_module.HardwareRngSource()
    assert hardware.name == "Hardware RNG"
    assert hardware.is_available

    dev = random_sources_module.DevUrandomSource()
    assert dev.name.startswith("Unix")
    assert isinstance(dev.get_random_bytes(1), bytes)

    original_system = random_sources_module.platform.system
    original_open = builtins.open

    monkeypatch.setattr(random_sources_module.platform, "system", lambda: "Windows")
    with pytest.raises(RuntimeError):
        random_sources_module.DevUrandomSource().get_random_bytes(1)

    monkeypatch.setattr(random_sources_module.platform, "system", lambda: "Darwin")
    def raising_open(*args, **kwargs):
        raise RuntimeError("open failed")

    monkeypatch.setattr("builtins.open", raising_open)
    with pytest.raises(RuntimeError):
        random_sources_module.DevUrandomSource().get_random_bytes(1)
    monkeypatch.setattr("builtins.open", original_open)

    monkeypatch.setattr(random_sources_module.platform, "system", original_system)

    with pytest.raises(ValueError):
        random_sources_module.HardwareRngSource().get_random_bytes(-1)


def test_random_source_factory_handles_unavailable(monkeypatch):
    monkeypatch.setattr(
        RandomSourceFactory,
        "get_available_sources",
        _ORIGINAL_RANDOM_GET_SOURCES,
    )
    monkeypatch.setattr(
        RandomSourceFactory,
        "create",
        _ORIGINAL_RANDOM_CREATE,
    )
    monkeypatch.setattr(
        random_sources_module.SystemRandomSource,
        "is_available",
        property(lambda self: False),
    )
    with pytest.raises(RuntimeError):
        RandomSourceFactory.create(RandomSourceType.SYSTEM_RANDOM)
    assert RandomSourceFactory.get_available_sources()


def test_json_storage_default_path(monkeypatch, tmp_path):
    monkeypatch.setattr(
        storage_module.Path,
        "home",
        classmethod(lambda cls: tmp_path),
    )
    storage = JsonPasswordStorage()
    entry = PasswordEntry("key", "value")
    storage.save(entry)
    assert storage.load("key") is not None
    assert str(tmp_path) in storage._storage_path


def test_json_storage_load_all_handles_empty_and_invalid(tmp_path):
    path = tmp_path / "vault" / "store.json"
    path.parent.mkdir(parents=True)
    path.write_text("")
    storage = JsonPasswordStorage(str(path))
    assert storage.load_all() == []

    path.write_text("{}")
    assert storage.load_all() == []
    assert storage.load("missing") is None


def test_json_storage_delete_branches(tmp_path):
    path = tmp_path / "vault" / "store.json"
    storage = JsonPasswordStorage(str(path))
    storage.save(PasswordEntry("first", "pass1"))
    storage.save(PasswordEntry("second", "pass2"))

    assert storage.delete("first") is True
    assert storage.load("second") is not None

    assert storage.delete("second") is True
    assert not path.exists()


def test_json_storage_error_branches(monkeypatch, tmp_path):
    path = tmp_path / "vault" / "store.json"
    storage = JsonPasswordStorage(str(path))
    original_internal = JsonPasswordStorage._load_all_internal

    def raising_internal(self):
        raise RuntimeError("boom")

    monkeypatch.setattr(JsonPasswordStorage, "_load_all_internal", raising_internal)
    with pytest.raises(IOError):
        storage.save(PasswordEntry("x", "y"))
    with pytest.raises(IOError):
        storage.delete("x")

    monkeypatch.setattr(JsonPasswordStorage, "_load_all_internal", original_internal)

    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("[]")

    def raising_open(*args, **kwargs):
        raise IOError("boom")

    original_open = builtins.open
    monkeypatch.setattr("builtins.open", raising_open)
    with pytest.raises(IOError):
        storage.load_all()

    monkeypatch.setattr("builtins.open", original_open)

    with pytest.raises(ValueError):
        storage.load("")
