"""Gera os efeitos sonoros e a música do jogo (WAV).

Uso: python Tools/gerar_sons.py Assets
Só usa a biblioteca padrão do Python.
"""
import math
import os
import random
import struct
import sys
import wave

ROOT = sys.argv[1] if len(sys.argv) > 1 else "Assets"
AUD = os.path.join(ROOT, "Audio")
os.makedirs(AUD, exist_ok=True)
random.seed(11)

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


# música de aventura: 132 bpm, em dó maior
BPM = 132
STEP = 60 / BPM / 2
C3, G2, A2, F2 = 130.81, 98.0, 110.0, 87.31
C4, D4, E4, G4, A4 = 261.63, 293.66, 329.63, 392.0, 440.0
C5, D5, E5, G5 = 523.25, 587.33, 659.25, 783.99

melody_a = [E4, G4, A4, G4, E4, G4, C5, 0, D5, C5, A4, G4, E4, D4, E4, 0]
melody_b = [G4, A4, C5, A4, G4, E4, G4, 0, A4, C5, D5, C5, A4, G4, A4, 0]
melody_c = [C5, 0, E5, D5, C5, A4, G4, 0, A4, G4, E4, G4, A4, C5, D5, 0]
melody_d = [E5, D5, C5, D5, E5, G5, E5, 0, D5, C5, A4, C5, D5, C5, 0, 0]
melody = melody_a + melody_b + melody_c + melody_d

bars = [C3, G2, A2, F2, C3, G2, F2, G2]


def bass_note(step):
    root = bars[(step // 8) % len(bars)]
    return root if step % 4 < 2 else root * 2


n_step = int(SR * STEP)
music = []
for k, fm in enumerate(melody):
    fb = bass_note(k)
    for i in range(n_step):
        t = (k * n_step + i) / SR
        dec = 1 - i / n_step
        s = tri(fb, t) * 0.26
        if fm:
            s += square(fm, t, 0.25) * 0.13 * dec ** 0.5
        if k % 4 == 0 and i < 2600:
            s += sine(115 - 60 * i / 2600, t) * 0.4 * (1 - i / 2600)
        if k % 2 == 1 and i < 1000:
            s += random.uniform(-1, 1) * 0.07 * (1 - i / 1000)
        music.append(s)
save_wav("musica", music)
print("sons gerados")
