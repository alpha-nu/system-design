import builtins
import json
import platform
from typing import Iterator

import pytest

from mvc.controller import PasswordController
from mvc.model import PasswordModel
from mvc.view import ConsoleView
from shared import (
    JsonPasswordStorage,
    PasswordEntry,
    PasswordGenerator,
    RandomSourceFactory,
    RandomSourceType,
)
from tests.test_helpers import MockRandomSource


def _input_sequence(responses: list[str]) -> Iterator[str]:
    yield from responses


def test_mvc_cli_flow(tmp_path, monkeypatch):
    responses = [
        "1",  # generate
        "8",  # length
        "y",  # include special chars
        "1",  # random source choice
        "2",  # save
        "e2e-key",
        "e2e-description",
        "3",  # view
        "4",  # delete
        "e2e-key",
        "5",  # exit
    ]
    iterator = _input_sequence(responses)

    def fake_input(prompt: str = "") -> str:
        try:
            return next(iterator)
        except StopIteration as exc:
            raise RuntimeError("Out of scripted inputs") from exc

    monkeypatch.setattr(builtins, "input", fake_input)

    storage_path = tmp_path / "passwords.json"
    storage = JsonPasswordStorage(str(storage_path))

    deterministic_source = MockRandomSource(b"\x03")
    monkeypatch.setattr(
        RandomSourceFactory,
        "get_available_sources",
        lambda: [(RandomSourceType.SYSTEM_RANDOM, deterministic_source)],
    )
    monkeypatch.setattr(
        RandomSourceFactory,
        "create",
        lambda _: deterministic_source,
    )

    generator = PasswordGenerator(deterministic_source)
    model = PasswordModel(generator, storage)
    view = ConsoleView()
    controller = PasswordController(model, view)

    controller.run()

    assert storage_path.parent.exists()
    assert not storage_path.exists()


def test_json_storage_edge_cases(tmp_path):
    storage_file = tmp_path / "json_storage.json"
    storage = JsonPasswordStorage(str(storage_file))

    with pytest.raises(ValueError):
        storage.save(None)  # type: ignore[arg-type]

    with pytest.raises(ValueError):
        storage.load("")

    with pytest.raises(ValueError):
        storage.delete("   ")

    assert storage.delete("missing") is False

    storage_file.write_text("not json")
    with pytest.raises(IOError):
        storage.load("key")

    storage_file.write_text(json.dumps([{"bad": "entry"}]))
    assert storage.load_all() == []

    entry = PasswordEntry("key", "pass")
    storage.save(entry)
    assert storage.load("key") is not None
    assert storage.delete("key") is True
    assert not storage_file.exists()


def test_random_sources_edge_coverage(monkeypatch):
    monkeypatch.setattr(platform, "system", lambda: "Linux")

    for random_type in RandomSourceType:
        source = RandomSourceFactory.create(random_type)
        assert source.get_random_bytes(1)
        with pytest.raises(ValueError):
            source.get_random_bytes(-1)

    class UnknownType:
        pass

    with pytest.raises(ValueError):
        RandomSourceFactory.create(UnknownType)  # type: ignore[arg-type]
