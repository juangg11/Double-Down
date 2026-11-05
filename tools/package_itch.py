"""Package the tested Windows build with credits for itch.io."""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED

root = Path(__file__).resolve().parents[1]
build = root / "Builds" / "DoubleDown-0.1.1-Windows-x64"
archive = build.parent / (build.name + ".zip")
if not (build / "DoubleDown.exe").is_file():
    raise SystemExit("Build the Windows player before packaging it.")
with ZipFile(archive, "w", ZIP_DEFLATED, compresslevel=8) as out:
    for path in sorted(build.rglob("*")):
        if path.is_file() and "BurstDebugInformation_DoNotShip" not in path.parts:
            out.write(path, path.relative_to(build).as_posix())
    for name in ("README.txt", "CREDITS.txt", "UI_LICENSE.txt"):
        out.write(root / "docs" / "itch" / name, name)
print(archive)
