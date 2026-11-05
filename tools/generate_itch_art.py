"""Export stills in the palette and equations of CasinoVortex.shader."""
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs" / "itch"
OUT.mkdir(parents=True, exist_ok=True)


def saturate(v):
    return np.clip(v, 0, 1)


def smoothstep(a, b, x):
    x = saturate((x - a) / (b - a))
    return x * x * (3 - 2 * x)


def frame(width, height):
    y, x = np.mgrid[:height, :width]
    u = (x + .5) / width
    v = (y + .5) / height
    px = u * 1.7777778
    py = v
    fx = px + .10 * np.sin(5 * py + 1.6 * px)
    fy = py + .13 * np.sin(4 * px - 3.3 * py)
    fx = fx + .045 * np.sin(12 * fy + 6 * fx)
    fy = fy + .045 * np.cos(11 * fx - 5 * fy)
    ribbons = np.sin(9 * fx + 5 * fy + 1.4 * np.sin(3.8 * fy - 2.6 * fx))
    veins = np.sin(17 * fx - 7 * fy)
    field = .46 + .29 * ribbons + .10 * veins + .10 * np.sin(5.4 * fy - 2.4 * fx)
    field = np.floor(saturate(field) * 9 + .5) / 9
    night = np.array([.012, .029, .060])
    teal = np.array([.025, .20, .25])
    blue = np.array([.075, .37, .57])
    coral = np.array([.72, .18, .21])
    gold = np.array([.95, .49, .25])
    c = night + (teal - night) * saturate(field / .34)[..., None]
    c = c + (blue - c) * saturate((field - .34) / .24)[..., None]
    c = c + (coral - c) * saturate((field - .58) / .23)[..., None]
    c = c + (gold - c) * saturate((field - .81) / .19)[..., None]
    mask = 1 - smoothstep(.16, .65, np.sqrt(((u - .34) * 1.2) ** 2 + (v - .49) ** 2))
    c *= (.82 * (1 - mask) + .48 * mask)[..., None]
    c = np.floor(c * 24 + .5) / 24 * .60
    return Image.fromarray((saturate(c) * 255).astype("uint8"), "RGB")


font_path = ROOT / "Assets/Thaleah_PixelFont/Materials/ThaleahFat_TTF.ttf"
font = ImageFont.truetype(str(font_path), 55)
page = frame(320, 180).resize((1920, 1080), Image.Resampling.NEAREST)
page.save(OUT / "DoubleDown_PageBackground_1920x1080.png", optimize=True)

capsule = frame(315, 250)
draw = ImageDraw.Draw(capsule)
# Low contrast shadow keeps the wordmark readable without hiding the shader.
title = "DOUBLE DOWN"
box = draw.textbbox((0, 0), title, font=font)
tw = box[2] - box[0]
tx = (315 - tw) // 2
ty = 86 - box[1]
draw.text((tx + 2, ty + 3), title, font=font, fill=(4, 13, 26), stroke_width=3, stroke_fill=(4, 13, 26))
draw.text((tx, ty), title, font=font, fill=(255, 218, 154), stroke_width=1, stroke_fill=(21, 44, 58))
capsule.resize((630, 500), Image.Resampling.NEAREST).save(OUT / "DoubleDown_Capsule_630x500.png", optimize=True)
