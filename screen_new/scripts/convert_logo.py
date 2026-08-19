"""
Pre-build script: convert data/poweredby.jpg (the boot logo) to a raw
RGB565 image (data/poweredby.r565) for LittleFS. The on-device renderer only
reads raw 240x240 RGB565 little-endian data, not JPEG.

Center-crops to a square, resizes to 240x240, then writes RGB565 LE bytes.
Tries Pillow first, then ImageMagick (PPM round-trip, pure-Python parse);
skips silently if neither is available so the build never breaks.
"""

Import("env")

SIZE = (240, 240)
JPG = "poweredby.jpg"
R565 = "poweredby.r565"


def _rgb565_write(pixels):
    out = bytearray(SIZE[0] * SIZE[1] * 2)
    i = 0
    for r, g, b in pixels:
        rgb565 = ((r >> 3) << 11) | ((g >> 2) << 5) | (b >> 3)
        out[i] = rgb565 & 0xFF
        out[i + 1] = rgb565 >> 8
        i += 2
    return bytes(out)


def _via_pil(src, dst):
    try:
        from PIL import Image
    except ImportError:
        return False

    img = Image.open(src).convert("RGB")
    w, h = img.size
    side = min(w, h)
    img = img.crop(((w - side) // 2, (h - side) // 2, (w + side) // 2, (h + side) // 2))
    img = img.resize(SIZE, Image.LANCZOS)

    pixels = []
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            pixels.append(img.getpixel((x, y)))

    dst.write_bytes(_rgb565_write(pixels))
    return True


def _parse_ppm(path):
    data = open(path, "rb").read()
    if data[:2] != b"P6":
        return None
    # Header: P6\n<width> <height>\n<maxval>\n
    n = 0
    for _ in range(3):
        n = data.find(b"\n", n) + 1
    header = data[: n - 1].decode()
    _, w, h, maxval = header.replace("\n", " ").split()
    w, h, maxval = int(w), int(h), int(maxval)
    if (w, h) != SIZE or maxval != 255:
        return None
    body = data[n:]
    if len(body) < w * h * 3:
        return None
    px = body[: w * h * 3]
    return [(px[i], px[i + 1], px[i + 2]) for i in range(0, len(px), 3)]


def _via_imagemagick(src, dst):
    import os
    import shutil
    import subprocess
    import tempfile

    magick = shutil.which("magick") or shutil.which("convert")
    if not magick:
        return False

    with tempfile.NamedTemporaryFile(suffix=".ppm", delete=False) as tmp:
        tmp_path = tmp.name

    try:
        cmd = [magick, str(src), "-gravity", "center", "-extent", f"{SIZE[0]}x{SIZE[1]}",
               "-resize", f"{SIZE[0]}x{SIZE[1]}!", tmp_path]
        if subprocess.run(cmd, capture_output=True).returncode != 0:
            return False
        pixels = _parse_ppm(tmp_path)
        if pixels is None:
            return False
        dst.write_bytes(_rgb565_write(pixels))
        return True
    finally:
        try:
            os.remove(tmp_path)
        except OSError:
            pass


def convert():
    import os
    from pathlib import Path

    data_dir = Path(env.subst("$PROJECT_DIR")) / "data"
    src = data_dir / JPG
    dst = data_dir / R565
    if not src.exists():
        print("[convert_logo] poweredby.jpg not found, skipping")
        return

    if _via_pil(src, dst):
        print(f"[convert_logo] wrote {dst} ({dst.stat().st_size} bytes) via Pillow")
        return

    if _via_imagemagick(src, dst):
        print(f"[convert_logo] wrote {dst} ({dst.stat().st_size} bytes) via ImageMagick")
        return

    print("[convert_logo] neither Pillow nor ImageMagick available, skipping")


convert()