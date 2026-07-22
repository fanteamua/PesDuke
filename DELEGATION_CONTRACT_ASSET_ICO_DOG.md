# Delegation Contract — PesDuke Dog Logo .ico

**Agent**: @designer
**Task ID**: ASSET-ICO-DOG
**Scope**: Create .ico file and update WPF icon usage
**Out of Scope**: Backend logic, TTS, YouTube API
**Verification**: Build succeeds, .ico exists, MainWindow.xaml uses icon

---

## Context

PesDuke is a Windows 11 WPF desktop app for streamers that reads YouTube live chat aloud via TTS. The mascot is a dog 🐕. Dark theme with yellow-blue accents (#FFD700, #0057B7, #0D0D0D).

## Contract

### Input
- Project path: `C:\Users\fante\Documents\PesDuke`
- Target .ico path: `C:\Users\fante\Documents\PesDuke\pesduke_logo.ico`
- Colors: Yellow (#FFD700), Blue (#0057B7), Dark (#0D0D0D)
- Current MainWindow.xaml has emoji `🐕` in title bar

### Output
1. Python script `generate_icon.py` that creates `pesduke_logo.ico` with sizes 16x16, 32x32, 48x48, 256x256
2. Updated `MainWindow.xaml` using the .ico file
3. Successful build

### Constraints
- Do NOT modify other files
- Keep the dog icon minimal and recognizable at small sizes
- Use Pillow (PIL) for PNG generation, Python's `struct` for ICO format
- Do NOT add external dependencies — use only standard library + Pillow if available

### Verification
- Run: `set PATH=C:\dotnet;%PATH% && C:\dotnet\dotnet.exe build C:\Users\fante\Documents\PesDuke\PesDuke.csproj`
- .ico file must exist at specified path
- Build must succeed

---

## Prompt for @designer

Create a minimal dog mascot icon for the PesDuke Windows WPF desktop app.

**Step 1: Python script to generate icon**

Create `C:\Users\fante\Documents\PesDuke\generate_icon.py`:

```python
"""Generate PesDuke dog mascot .ico file."""
import struct
import os

# Try Pillow first, fall back to pure Python
try:
    from PIL import Image, ImageDraw
    
    def create_dog_icon(size):
        img = Image.new('RGBA', (size, size), (13, 13, 13, 255))
        draw = ImageDraw.Draw(img)
        
        s = size / 256  # scale factor
        
        # Dog head circle
        draw.ellipse([70*s, 60*s, 186*s, 200*s], fill=(255, 215, 0, 255))
        
        # Ears
        draw.ellipse([50*s, 30*s, 90*s, 100*s], fill=(255, 215, 0, 255))
        draw.ellipse([166*s, 30*s, 206*s, 100*s], fill=(255, 215, 0, 255))
        
        # Inner ears
        draw.ellipse([58*s, 42*s, 82*s, 88*s], fill=(200, 170, 0, 255))
        draw.ellipse([174*s, 42*s, 198*s, 88*s], fill=(200, 170, 0, 255))
        
        # Eyes
        draw.ellipse([90*s, 100*s, 114*s, 130*s], fill=(0, 0, 0, 255))
        draw.ellipse([142*s, 100*s, 166*s, 130*s], fill=(0, 0, 0, 255))
        draw.ellipse([96*s, 106*s, 108*s, 118*s], fill=(255, 255, 255, 255))
        draw.ellipse([148*s, 106*s, 160*s, 118*s], fill=(255, 255, 255, 255))
        
        # Nose
        draw.ellipse([118*s, 130*s, 138*s, 154*s], fill=(0, 87, 183, 255))
        
        # Mouth
        draw.arc([108*s, 140*s, 148*s, 175*s], 0, 180, fill=(0, 0, 0, 255), width=max(1, int(2*s)))
        
        return img
    
    sizes = [16, 32, 48, 256]
    icons = [create_dog_icon(s) for s in sizes]
    
    # Save as ICO
    icons[0].save(
        r'C:\Users\fante\Documents\PesDuke\pesduke_logo.ico',
        format='ICO',
        sizes=[(s, s) for s in sizes],
        append_images=icons[1:]
    )
    print("Icon created with Pillow")

except ImportError:
    # Pure Python ICO writer
    def create_ico():
        size = 32  # Start small for pure Python
        pixels = bytearray(size * size * 4)
        
        cx, cy = size // 2, size // 2
        r = size // 2 - 2
        
        for y in range(size):
            for x in range(size):
                dx, dy = x - cx, y - cy
                dist = (dx*dx + dy*dy) ** 0.5
                
                idx = (y * size + x) * 4
                if dist < r * 0.8:
                    # Yellow dog
                    pixels[idx:idx+4] = bytes([255, 215, 0, 255])
                elif dist < r:
                    # Dark border
                    pixels[idx:idx+4] = bytes([13, 13, 13, 255])
                else:
                    # Transparent
                    pixels[idx:idx+4] = bytes([0, 0, 0, 0])
        
        # ICO format
        data = bytearray()
        
        # PNG for each size (simplified: just use BMP)
        bmp = bytearray()
        bmp += struct.pack('<I', 40)  # Header size
        bmp += struct.pack('<i', size)  # Width
        bmp += struct.pack('<i', size * 2)  # Height (doubled for ICO)
        bmp += struct.pack('<H', 1)  # Planes
        bmp += struct.pack('<H', 32)  # Bits per pixel
        bmp += struct.pack('<I', 0)  # Compression
        bmp += struct.pack('<I', len(pixels))  # Image size
        bmp += struct.pack('<i', 0)  # X pixels per meter
        bmp += struct.pack('<i', 0)  # Y pixels per meter
        bmp += struct.pack('<I', 0)  # Colors used
        bmp += struct.pack('<I', 0)  # Important colors
        
        # Pixel data (bottom-up)
        for y in range(size - 1, -1, -1):
            bmp += pixels[y * size * 4:(y + 1) * size * 4]
        
        # AND mask (1bpp, all zeros = fully opaque where alpha > 0)
        and_mask = bytearray(((size + 31) // 32) * 4 * size)
        
        data += bmp + and_mask
        
        # ICO header
        ico = struct.pack('<HHH', 0, 1, 1)  # Reserved, Type (ICO), Count
        ico += struct.pack('<BBBBHHII', 
            size, size, 0, 0, 1, 32, len(data), 22)
        ico += data
        
        return ico
    
    with open(r'C:\Users\fante\Documents\PesDuke\pesduke_logo.ico', 'wb') as f:
        f.write(create_ico())
    print("Icon created with pure Python (basic)")
```

**Step 2: Run the script**

```bash
python C:\Users\fante\Documents\PesDuke\generate_icon.py
```

**Step 3: Update MainWindow.xaml**

Replace the emoji in the title bar with the icon:

```xml
<!-- Replace Text="🐕" with -->
<Image Source="pesduke_logo.ico" Width="16" Height="16" Margin="0,0,4,0"/>
```

And add to the Window element:
```xml
Icon="pesduke_logo.ico"
```

**Step 4: Build**

```bash
set PATH=C:\dotnet;%PATH% && C:\dotnet\dotnet.exe build C:\Users\fante\Documents\PesDuke\PesDuke.csproj
```
