"""Generate PesDuke dog icon as .ico file — pure Python, no dependencies."""
import struct
import math

def create_png(width, height, pixels):
    """Create a minimal PNG from pixel data."""
    def chunk(chunk_type, data):
        c = chunk_type + data
        crc = struct.pack('>I', zlib_crc32(c))
        return struct.pack('>I', len(data)) + c + crc

    import zlib
    def zlib_crc32(data):
        return zlib.crc32(data) & 0xFFFFFFFF

    # RGBA pixels → filtered rows
    raw = b''
    for y in range(height):
        raw += b'\x00'  # filter none
        for x in range(width):
            r, g, b, a = pixels[y * width + x]
            raw += bytes([r, g, b, a])

    sig = b'\x89PNG\r\n\x1a\n'
    ihdr = struct.pack('>IIBBBBB', width, height, 8, 6, 0, 0, 0)
    return sig + chunk(b'IHDR', ihdr) + chunk(b'IDAT', zlib.compress(raw)) + chunk(b'IEND', b'')


def draw_dog(size):
    """Draw a minimal dog face icon."""
    pixels = [(13, 13, 13, 255)] * (size * size)  # dark bg #0D0D0D
    cx, cy = size // 2, size // 2
    r = size * 0.35

    def dist(x1, y1, x2, y2):
        return math.sqrt((x1 - x2) ** 2 + (y1 - y2) ** 2)

    def set_pixel(x, y, color):
        if 0 <= x < size and 0 <= y < size:
            pixels[y * size + x] = color

    def fill_circle(cx, cy, radius, color):
        for y in range(int(cy - radius) - 1, int(cy + radius) + 2):
            for x in range(int(cx - radius) - 1, int(cx + radius) + 2):
                if dist(x, y, cx, cy) <= radius:
                    set_pixel(x, y, color)

    yellow = (255, 215, 0, 255)     # #FFD700
    blue = (0, 87, 183, 255)        # #0057B7
    black = (0, 0, 0, 255)
    white = (255, 255, 255, 255)

    # Head circle
    fill_circle(cx, cy + r * 0.1, r, yellow)

    # Ears (two triangles on top)
    ear_w = r * 0.45
    ear_h = r * 0.6
    for side in [-1, 1]:
        ex = cx + side * r * 0.7
        ey = cy - r * 0.5
        for y in range(int(ey - ear_h), int(ey) + 1):
            for x in range(int(ex - ear_w), int(ex + ear_w) + 1):
                t = (ey - y) / ear_h if ear_h != 0 else 0
                w = ear_w * (1 - t * 0.5)
                if abs(x - ex) <= w:
                    set_pixel(x, y, yellow)

    # Eyes
    eye_r = r * 0.12
    for side in [-1, 1]:
        ex = cx + side * r * 0.4
        ey = cy - r * 0.15
        fill_circle(ex, ey, eye_r, black)
        fill_circle(ex + eye_r * 0.3, ey - eye_r * 0.3, eye_r * 0.3, white)

    # Nose
    nose_r = r * 0.18
    fill_circle(cx, cy + r * 0.2, nose_r, blue)

    return pixels


def save_ico(path, sizes):
    """Save ICO file with multiple sizes."""
    pngs = []
    for s in sizes:
        pixels = draw_dog(s)
        png_data = create_png(s, s, pixels)
        pngs.append((s, png_data))

    # ICO header
    ico = struct.pack('<HHH', 0, 1, len(pngs))

    # Directory entries + image data
    data_offset = 6 + len(pngs) * 16
    for size, png_data in pngs:
        w = size if size < 256 else 0
        h = size if size < 256 else 0
        ico += struct.pack('<BBBBHHII', w, h, 0, 0, 1, 32, len(png_data), data_offset)
        ico += png_data
        data_offset += len(png_data)

    with open(path, 'wb') as f:
        f.write(ico)


if __name__ == '__main__':
    out = r'C:\Users\fante\Documents\PesDuke\pesduke_logo.ico'
    save_ico(out, [16, 32, 48, 256])
    print(f'Created: {out}')
