"""
Pre-build script: patch WebServer's body reading to use bulk client.read()
instead of byte-at-a-time Stream::readBytes(). Streaming a ~115 KB image
body through byte-at-a-time reads takes ~7.5 s on the ESP32.

Runs on every build (module body of an extra_scripts pre: script), so the
patch is applied before SCons decides whether to recompile Parsing.cpp.
"""

Import("env")

from pathlib import Path


def apply_patch():
    platform = env.PioPlatform()
    framework_dir = platform.get_package_dir("framework-arduinoespressif32")
    if not framework_dir:
        print("[patch_framework] framework dir not found, skipping")
        return

    path = Path(framework_dir) / "libraries" / "WebServer" / "src" / "Parsing.cpp"
    if not path.exists():
        print("[patch_framework] Parsing.cpp not found, skipping")
        return

    src = path.read_text()

    replacements = [
        # Migration: an earlier patch installed a non-blocking client.read()
        # here which aborts uploads when TCP bursts gap (read() returns 0).
        # Normalize it to the blocking available() + bulk-read form below.
        (
            "int _rawRead = client.read(_currentRaw->buf, HTTP_RAW_BUFLEN);\n"
            "        _currentRaw->currentSize = _rawRead > 0 ? (size_t)_rawRead : 0;",
            "int _avail = 0;\n"
            "        for (int _t = 0; _t < HTTP_MAX_POST_WAIT && (_avail = client.available()) == 0; _t++) {\n"
            "          delay(1);\n"
            "        }\n"
            "        size_t _want = (_avail > 0) ? (_avail > (int)HTTP_RAW_BUFLEN ? (size_t)HTTP_RAW_BUFLEN : (size_t)_avail) : 0;\n"
            "        int _rawRead = _want ? client.read(_currentRaw->buf, _want) : 0;\n"
            "        _currentRaw->currentSize = _rawRead > 0 ? (size_t)_rawRead : 0;",
        ),
        # Raw-body streaming loop (used by POST /image).
        # client.read() alone is non-blocking and returns 0 between TCP
        # bursts, which the loop treats as an aborted upload. So wait for
        # data (available(), up to the POST timeout), then bulk-read.
        (
            "_currentRaw->currentSize = client.readBytes(_currentRaw->buf, HTTP_RAW_BUFLEN);",
            "int _avail = 0;\n"
            "        for (int _t = 0; _t < HTTP_MAX_POST_WAIT && (_avail = client.available()) == 0; _t++) {\n"
            "          delay(1);\n"
            "        }\n"
            "        size_t _want = (_avail > 0) ? (_avail > (int)HTTP_RAW_BUFLEN ? (size_t)HTTP_RAW_BUFLEN : (size_t)_avail) : 0;\n"
            "        int _rawRead = _want ? client.read(_currentRaw->buf, _want) : 0;\n"
            "        _currentRaw->currentSize = _rawRead > 0 ? (size_t)_rawRead : 0;",
        ),
        # Plain body read (readBytesWithTimeout): already waits for
        # available(), so just make the actual copy bulk.
        (
            "    client.readBytes(buf + dataLength, newLength);\n"
            "    dataLength += newLength;",
            "    int _plainRead = client.read((uint8_t *)buf + dataLength, newLength);\n"
            "    dataLength += _plainRead > 0 ? (size_t)_plainRead : 0;",
        ),
    ]

    changed = False
    for old, new in replacements:
        if old in src:
            src = src.replace(old, new)
            changed = True

    if changed:
        path.write_text(src)
        print("[patch_framework] WebServer Parsing.cpp patched to use bulk read()")
    else:
        print("[patch_framework] WebServer Parsing.cpp already patched")


apply_patch()
