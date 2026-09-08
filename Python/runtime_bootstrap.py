"""
Runtime setup for FormatForge Python entry points.

The bundled Python runtime is launched from the native converter, so each
entry script should be usable without relying on the user's shell settings.
"""

from pathlib import Path
import os
import sys


def configure():
    """Apply process-local settings needed by the bundled runtime."""
    _configure_stdio()
    _configure_environment()


def _configure_stdio():
    for stream in (sys.stdout, sys.stderr):
        reconfigure = getattr(stream, "reconfigure", None)
        if callable(reconfigure):
            try:
                reconfigure(encoding="utf-8", errors="replace")
            except Exception:
                pass


def _configure_environment():
    os.environ.setdefault("PYTHONUTF8", "1")
    os.environ.setdefault("PYTHONIOENCODING", "utf-8")

    if os.environ.get("PLAYWRIGHT_BROWSERS_PATH"):
        return

    runtime_dir = Path(sys.executable).resolve().parent
    candidates = (
        runtime_dir / "ms-playwright",
        runtime_dir.parent / "ms-playwright",
    )

    for candidate in candidates:
        if candidate.exists():
            os.environ["PLAYWRIGHT_BROWSERS_PATH"] = str(candidate)
            break