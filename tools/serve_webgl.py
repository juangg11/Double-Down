"""Serve the WebGL build locally with the gzip headers Unity expects."""
from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

root = Path(__file__).resolve().parents[1] / "Builds/DoubleDown-0.1.1-WebGL"


class Handler(SimpleHTTPRequestHandler):
    def guess_type(self, path):
        if path.endswith(".wasm.gz"):
            return "application/wasm"
        if path.endswith(".js.gz"):
            return "application/javascript"
        if path.endswith(".data.gz"):
            return "application/octet-stream"
        return super().guess_type(path)

    def end_headers(self):
        if self.path.split("?", 1)[0].endswith(".gz"):
            self.send_header("Content-Encoding", "gzip")
        super().end_headers()


if __name__ == "__main__":
    if not (root / "index.html").is_file():
        raise SystemExit("Build WebGL before starting the server")
    print("http://127.0.0.1:8765", flush=True)
    ThreadingHTTPServer(("127.0.0.1", 8765), partial(Handler, directory=str(root))).serve_forever()
