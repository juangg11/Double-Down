"""Create an itch.io HTML Game ZIP with index.html at archive root."""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED

root = Path(__file__).resolve().parents[1]
build = root / "Builds" / "DoubleDown-0.1.1-WebGL"
archive = build.parent / (build.name + ".zip")
if not (build / "index.html").is_file():
    raise SystemExit("WebGL build is missing index.html")
files = sorted(path for path in build.rglob("*") if path.is_file())
files += [root / "docs/itch/CREDITS.txt", root / "docs/itch/UI_LICENSE.txt"]
if len(files) > 1000 or any(path.stat().st_size > 200_000_000 for path in files):
    raise SystemExit("Build exceeds itch.io HTML ZIP limits")
if sum(path.stat().st_size for path in files) > 500_000_000:
    raise SystemExit("Extracted build exceeds itch.io HTML ZIP limit")
with ZipFile(archive, "w", ZIP_DEFLATED, compresslevel=6) as output:
    for path in files:
        name = path.relative_to(build).as_posix() if path.is_relative_to(build) else path.name
        if len(name) > 240:
            raise SystemExit("Archive path exceeds itch.io limit: " + name)
        output.write(path, name)
print(archive)
