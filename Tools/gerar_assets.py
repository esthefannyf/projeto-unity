"""Gera os sprites (PNG 16x16) e os sons (WAV) do jogo.

Uso: python Tools/gerar_assets.py Assets
Só usa a biblioteca padrão do Python.
"""
import math
import os
import random
import struct
import sys
import wave
import zlib

ROOT = sys.argv[1] if len(sys.argv) > 1 else "Assets"
ART = os.path.join(ROOT, "Art")
AUD = os.path.join(ROOT, "Audio")
os.makedirs(ART, exist_ok=True)
os.makedirs(AUD, exist_ok=True)


def rgb(h, a=255):
    h = h.lstrip("#")
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)


class Canvas:
    def __init__(self, w=16, h=16, fill=(0, 0, 0, 0)):
        self.w, self.h = w, h
        self.p = [fill] * (w * h)

    def px(self, x, y, c):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.p[y * self.w + x] = c

    def rect(self, x, y, w, h, c):
        for j in range(y, y + h):
            for i in range(x, x + w):
                self.px(i, j, c)

    def disc(self, cx, cy, r, c):
        for j in range(self.h):
            for i in range(self.w):
                if (i - cx) ** 2 + (j - cy) ** 2 <= r * r:
                    self.px(i, j, c)

    def save(self, name, folder=ART):
        raw = b"".join(
            b"\x00" + bytes(v for q in self.p[y * self.w:(y + 1) * self.w] for v in q)
            for y in range(self.h)
        )

        def chunk(t, d):
            return (struct.pack(">I", len(d)) + t + d
                    + struct.pack(">I", zlib.crc32(t + d) & 0xFFFFFFFF))

        png = b"\x89PNG\r\n\x1a\n"
        png += chunk(b"IHDR", struct.pack(">IIBBBBB", self.w, self.h, 8, 6, 0, 0, 0))
        png += chunk(b"IDAT", zlib.compress(raw, 9)) + chunk(b"IEND", b"")
        with open(os.path.join(folder, name + ".png"), "wb") as f:
            f.write(png)


def grid(rows, pal):
    c = Canvas(max(len(r) for r in rows), len(rows))
    for y, r in enumerate(rows):
        for x, ch in enumerate(r):
            if ch in pal:
                c.px(x, y, pal[ch])
    return c


random.seed(11)

# ---------------------------------------------------------------- terreno
FLOOR_A, FLOOR_B, FLOOR_C = rgb("8a7f6a"), rgb("7d725e"), rgb("6b6150")


def floor_base():
    c = Canvas(fill=FLOOR_A)
    for y in range(16):
        for x in range(16):
            v = random.random()
            if v < 0.18:
                c.px(x, y, FLOOR_B)
            elif v < 0.24:
                c.px(x, y, FLOOR_C)
    for i in range(16):
        c.px(i, 0, FLOOR_C)
        c.px(0, i, FLOOR_C)
    return c


floor_base().save("chao")

wall = Canvas(fill=rgb("3b3a4d"))
for row in range(4):
    y = row * 4
    for x in range(16):
        wall.px(x, y + 3, rgb("24232f"))
    off = 0 if row % 2 == 0 else 4
    for bx in range(off, 16, 8):
        for yy in range(4):
            wall.px(bx % 16, y + yy, rgb("24232f"))
    for x in range(16):
        if random.random() < 0.15:
            wall.px(x, y + 1, rgb("4d4b63"))
        wall.px(x, y, rgb("55536e") if random.random() < 0.6 else rgb("4a4861"))
wall.save("parede")

spikes = floor_base()
for sx in (2, 6, 10, 14 - 1):
    sx = max(1, sx)
    for h in range(7):
        spikes.px(sx, 13 - h, rgb("b9bfc9"))
        if h < 5:
            spikes.px(sx - 1, 13 - h, rgb("7b828f"))
            spikes.px(sx + 1, 13 - h, rgb("9aa1ae"))
    spikes.px(sx, 6, rgb("ffffff"))
spikes.rect(1, 14, 14, 1, rgb("4b4a55"))
spikes.save("espinhos")

door = floor_base()
door.rect(2, 1, 12, 14, rgb("7a4a22"))
for x in range(2, 14):
    door.px(x, 1, rgb("a4682f"))
for yy in (4, 11):
    door.rect(2, yy, 12, 2, rgb("3c3c46"))
for x in (5, 8, 11):
    door.rect(x, 1, 1, 14, rgb("5e3818"))
door.disc(8, 8, 2, rgb("f2c14e"))
door.rect(8, 8, 1, 3, rgb("2b1c0e"))
door.save("porta")

for name, open_ in (("saida_fechada", False), ("saida_aberta", True)):
    e = floor_base()
    e.rect(1, 1, 14, 14, rgb("2a2a38"))
    e.rect(3, 3, 10, 12, rgb("0e0e16"))
    for k in range(5):
        col = rgb("46e0c0") if open_ else rgb("5a2d3a")
        e.rect(4 + k, 4 + k * 2, 8 - k * 1, 2, col if (k % 2 == 0) else (rgb("a7fff0") if open_ else rgb("3d1f29")))
    if open_:
        for _ in range(10):
            e.px(random.randrange(4, 12), random.randrange(3, 10), rgb("ffffff"))
    else:
        e.rect(6, 7, 4, 4, rgb("44444f"))
        e.rect(7, 5, 2, 3, rgb("44444f"))
        e.px(7, 9, rgb("0e0e16"))
        e.px(8, 9, rgb("0e0e16"))
    e.save(name)

# ---------------------------------------------------------------- objetos
player = grid([
    "................",
    ".....hhhhhh.....",
    "....hhhhhhhh....",
    "..hhhhhhhhhhhh..",
    "....ffffffff....",
    "....feffffef....",
    "....ffffffff....",
    ".....ffmmff.....",
    "....gggggggg....",
    "...fggggggggf...",
    "...fggbbbbggf...",
    "....gggggggg....",
    "....pppp.ppp....",
    "....pppp.ppp....",
    "...ssss..sss....",
    "................",
], {"h": rgb("8c5a2b"), "f": rgb("f0c8a0"), "e": rgb("20202a"), "m": rgb("b5524c"),
    "g": rgb("6f9a3a"), "b": rgb("4a2f17"), "p": rgb("3f5a86"), "s": rgb("352414")})
player.save("explorador")

diamond = grid([
    "................",
    "................",
    "....bbbbbbbb....",
    "...bwwlllcccb...",
    "..bwlllcccccdb..",
    ".bwlllcccccdddb.",
    "bbbbbbbbbbbbbbbb",
    ".bllcccccccdddb.",
    "..blccccccddb...",
    "...blcccccdb....",
    "....blccddb.....",
    ".....bcddb......",
    "......bdb.......",
    ".......b........",
    "................",
    "................",
], {"b": rgb("0b4f7a"), "w": rgb("ffffff"), "l": rgb("b6f0ff"), "c": rgb("39c5f0"), "d": rgb("1c8bc2")})
diamond.save("diamante")

key = Canvas()
key.disc(5, 5, 3, rgb("f2c14e"))
key.disc(5, 5, 1, (0, 0, 0, 0))
for i in range(6):
    key.px(7 + i, 7 + i, rgb("f2c14e"))
    key.px(8 + i, 7 + i, rgb("c9962d"))
key.rect(11, 12, 3, 1, rgb("f2c14e"))
key.rect(13, 10, 2, 1, rgb("f2c14e"))
key.px(4, 3, rgb("fff3b0"))
key.save("chave")

clock = Canvas()
clock.disc(8, 9, 6, rgb("2d3f66"))
clock.disc(8, 9, 5, rgb("f4f4f0"))
clock.rect(7, 2, 3, 2, rgb("2d3f66"))
clock.rect(8, 5, 1, 4, rgb("2d3f66"))
clock.rect(8, 9, 3, 1, rgb("c0392b"))
clock.px(8, 9, rgb("2d3f66"))
clock.save("relogio")

boulder = Canvas()
boulder.disc(8, 8, 7, rgb("34343c"))
boulder.disc(8, 8, 6, rgb("77777f"))
boulder.disc(7, 7, 4, rgb("90909a"))
for (x, y) in ((10, 10), (11, 9), (5, 11), (6, 4), (11, 6)):
    boulder.px(x, y, rgb("55555d"))
boulder.px(5, 5, rgb("c4c4cc"))
boulder.px(6, 5, rgb("b2b2ba"))
boulder.save("pedra")

snake = grid([
    "................",
    "................",
    "...gggg.........",
    "..ggddgg........",
    "..gwkgdg........",
    "..ggrgdg........",
    "...ggggg........",
    "....ddggg.......",
    ".....ddggg..ggg.",
    "......ddgggggdg.",
    ".......dddddddg.",
    "........ggggggg.",
    "................",
    "................",
    "................",
    "................",
], {"g": rgb("4fa84a"), "d": rgb("2f6e2d"), "w": rgb("ffffff"), "k": rgb("101010"), "r": rgb("d8434a")})
snake.save("cobra")

mummy = grid([
    "................",
    "....bbbbbbbb....",
    "...bwwbwwwwb....",
    "...bwrwbwrwb....",
    "...bwwwbwwwb....",
    "....bwbwwbw.....",
    "..bbwwwwwwwbb...",
    ".bwwwbwbwwwbwb..",
    ".bwwbwwwwbwwwb..",
    "..bbwwwbwwwwb...",
    "....wwbwwwwb....",
    "....bwwwbwwb....",
    "....wwb.bwww....",
    "....bww..bwb....",
    "...bwwb..bwwb...",
    "................",
], {"b": rgb("b59f7a"), "w": rgb("e6dcc2"), "r": rgb("e03c3c")})
mummy.save("mumia")

heart = grid([
    "................",
    "..rrr....rrr....",
    ".rwrrr..rrrrr...",
    ".rwrrrrrrrrrr...",
    ".rrrrrrrrrrrr...",
    "..rrrrrrrrrr....",
    "...rrrrrrrr.....",
    "....rrrrrr......",
    ".....rrrr.......",
    "......rr........",
    "................",
], {"r": rgb("e04856"), "w": rgb("ffc9d0")})
heart.save("coracao")

# capa 630x500: ladrilhos ampliados
cover = Canvas(630, 500)
tile = floor_base()


def blit_big(cv, spr, ox, oy, s):
    for y in range(spr.h):
        for x in range(spr.w):
            c = spr.p[y * spr.w + x]
            if c[3]:
                cv.rect(ox + x * s, oy + y * s, s, s, c)


cover.rect(0, 0, 630, 500, rgb("2b2a35"))
for ty in range(0, 500, 16 * 5):
    for tx in range(0, 630, 16 * 5):
        blit_big(cover, tile, tx, ty, 5)
for tx in range(0, 630, 80):
    blit_big(cover, wall, tx, 0, 5)
    blit_big(cover, wall, tx, 420, 5)
blit_big(cover, player, 275, 190, 14)
blit_big(cover, diamond, 70, 130, 8)
blit_big(cover, diamond, 460, 260, 8)
blit_big(cover, boulder, 460, 90, 9)
blit_big(cover, mummy, 90, 290, 9)
blit_big(cover, key, 500, 330, 6)
cover.save("capa_itch", os.path.join(ROOT, ".."))

# ---------------------------------------------------------------- audio
SR = 44100


def save_wav(name, samples):
    with wave.open(os.path.join(AUD, name + ".wav"), "wb") as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(SR)
        f.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, s)) * 32000)) for s in samples))


def square(f, t, duty=0.5):
    return 1.0 if (f * t) % 1.0 < duty else -1.0


def tri(f, t):
    return 4 * abs((f * t) % 1.0 - 0.5) - 1


def sine(f, t):
    return math.sin(2 * math.pi * f * t)


def notes(freqs, dur, wav=square, vol=0.3):
    out = []
    n = int(SR * dur)
    for f in freqs:
        for i in range(n):
            out.append((wav(f, i / SR) if f else 0) * vol * (1 - i / n) ** 0.6)
    return out


def sweep(f0, f1, dur, wav=square, vol=0.3):
    n = int(SR * dur)
    return [wav(f0 + (f1 - f0) * i / n, i / SR) * vol * (1 - i / n) for i in range(n)]


def thud(dur=0.12):
    n = int(SR * dur)
    return [(sine(70 - 30 * i / n, i / SR) * 0.7 + random.uniform(-1, 1) * 0.15) * (1 - i / n) ** 2
            for i in range(n)]


save_wav("passo", [sine(180, i / SR) * 0.18 * (1 - i / 1500) for i in range(1500)])
save_wav("empurrar", thud(0.16))
save_wav("diamante", notes([1319, 1760, 2093], 0.06, tri, 0.4))
save_wav("chave", notes([784, 1047], 0.08, square, 0.25))
save_wav("porta", sweep(300, 120, 0.25, square, 0.3) + thud(0.1))
save_wav("relogio", notes([1568, 1568, 2093], 0.05, sine, 0.4))
save_wav("espinhos", sweep(900, 200, 0.18, square, 0.3))
save_wav("dano", [(random.uniform(-1, 1) * 0.5 + square(140 - 90 * i / 9000, i / SR) * 0.3) * (1 - i / 9000)
                  for i in range(9000)])
save_wav("vitoria", notes([523, 659, 784, 1047, 0, 784, 1047], 0.12, square, 0.28))
save_wav("derrota", notes([392, 370, 349, 330, 262], 0.16, tri, 0.45))
save_wav("clique", sweep(1000, 800, 0.05, square, 0.22))
save_wav("desfazer", sweep(500, 900, 0.1, tri, 0.35))

# música: lenta, em lá menor
bpm = 84
step = 60 / bpm / 2
A2, C3, D3, E3, G3, A3, C4, D4, E4, G4, A4 = 110, 130.8, 146.8, 164.8, 196, 220, 261.6, 293.7, 329.6, 392, 440
melody = [A3, 0, C4, 0, E4, 0, D4, C4, A3, 0, G3, 0, A3, 0, 0, 0,
          C4, 0, D4, 0, E4, 0, G4, E4, D4, 0, C4, 0, A3, 0, 0, 0] * 2
bass = [A2] * 8 + [A2] * 4 + [G3 / 2] * 4 + [C3] * 8 + [D3] * 4 + [E3] * 4
bass = (bass * 2)[:len(melody)]
n_step = int(SR * step)
music = []
for k, fm in enumerate(melody):
    fb = bass[k]
    for i in range(n_step):
        t = (k * n_step + i) / SR
        dec = 1 - i / n_step
        s = tri(fb, t) * 0.25
        if fm:
            s += sine(fm, t) * 0.2 * dec ** 0.5 + square(fm * 2, t, 0.2) * 0.03 * dec
        if k % 2 == 0 and i < 1200:
            s += random.uniform(-1, 1) * 0.04 * (1 - i / 1200)
        music.append(s)
save_wav("musica", music)
print("assets gerados")
