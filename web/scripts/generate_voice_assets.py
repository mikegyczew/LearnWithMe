import argparse
import json
import re
import site
import subprocess
import sys
import tempfile
import wave
from pathlib import Path

for package_path in site.getsitepackages():
    if package_path not in sys.path:
        sys.path.append(package_path)

from piper import PiperVoice, SynthesisConfig


ROOT = Path(__file__).resolve().parents[2]
APP_SOURCE = ROOT / "web/src/App.tsx"
DATA_SOURCE = ROOT / "web/src/data/learningData.ts"
OUTPUT_DIRECTORY = ROOT / "web/public/voice/gosia"

NUMBER_WORDS = ["zero", "jeden", "dwa", "trzy", "cztery", "pięć", "sześć", "siedem", "osiem", "dziewięć", "dziesięć"]
NUMBER_WORDS.extend(["jedenaście", "dwanaście", "trzynaście", "czternaście", "piętnaście", "szesnaście", "siedemnaście", "osiemnaście", "dziewiętnaście", "dwadzieścia"])

def read_phrases() -> list[str]:
    app_source = APP_SOURCE.read_text(encoding="utf-8")
    data_source = DATA_SOURCE.read_text(encoding="utf-8")
    letter_block = re.search(r"export const letters: Letter\[\] = \[(.*?)\n\]", data_source, re.S)
    if letter_block is None:
        raise RuntimeError("Could not find the letter catalog.")

    letters = re.findall(
        r"uppercase:\s*'([^']+)'.*?word:\s*'([^']+)'",
        letter_block.group(1),
    )
    words_block = re.search(r"const genericWords = \{(.*?)\n  \}", app_source, re.S)
    if words_block is None:
        raise RuntimeError("Could not find the letter reading words.")
    fixes_block = re.search(
        r"const syllableWordFixes: Record<string, string\[]> = \{(.*?)\n\}",
        data_source,
        re.S,
    )
    if fixes_block is None:
        raise RuntimeError("Could not find normalized syllable data.")

    reading_words = {
        letter: re.findall(r"'([^']+)'", words)
        for letter, words in re.findall(r"([A-ZĄĆĘŁŃÓŚŹŻ]+): \[([^\]]*)\]", words_block.group(1))
    }
    syllable_fixes = {
        word: re.findall(r"'([^']+)'", parts)
        for word, parts in re.findall(r"(\w+): \[([^\]]+)\]", fixes_block.group(1))
    }

    phrases = set()
    for uppercase, word in letters:
        phrases.add(f"Litera {uppercase}")
        phrases.add(f"{uppercase} jak {word}.")
        examples = reading_words.get(uppercase, [word] * 5)
        phrases.update(f"{uppercase} jak {example}." for example in examples)
    phrases.add("Spróbuj jeszcze raz.")

    for line in data_source.splitlines():
        if "active: true" not in line or "word:" not in line or "syllables:" not in line:
            continue
        word = re.search(r"word:\s*'([^']+)'", line)
        syllables = re.search(r"syllables:\s*\[([^\]]*)\]", line)
        if word is None or syllables is None:
            raise RuntimeError(f"Could not parse syllable entry: {line}")
        parts = syllable_fixes.get(
            word.group(1).lower(),
            re.findall(r"'([^']+)'", syllables.group(1)),
        )
        if not parts:
            raise RuntimeError(f"Syllable list is empty: {line}")
        phrases.add(f"{word.group(1)}. Słowo dzielimy na sylaby: {', '.join(parts)}.")

    examples = [
        ("jeden samochód", "dwa", "samochody", "samochodów"),
        ("jedna sukienka", "dwie", "sukienki", "sukienek"),
        ("jedna zabawka", "dwie", "zabawki", "zabawek"),
    ]

    def number_word(value: int) -> str:
        return NUMBER_WORDS[value]

    def count_phrase(value: int, example: tuple[str, str, str, str]) -> str:
        if value == 1:
            return example[0]
        last_two = value % 100
        last = value % 10
        form = example[2] if 2 <= last <= 4 and not 12 <= last_two <= 14 else example[3]
        number = example[1] if value == 2 else number_word(value)
        return f"{number} {form}"

    phrases.update(("Dobra robota!", "Spróbuj ponownie."))
    phrases.add("Policz obrazki. Ile ich jest?")
    for value in range(21):
        prompt = f"Znajdź liczbę {value}."
        examples_for_number = [count_phrase(value, example) for example in examples]
        phrases.add(prompt)
        phrases.add(f"{prompt} {number_word(value)}. {'. '.join(examples_for_number)}")
        for example in examples_for_number:
            phrases.add(example)

    for first in range(11):
        for second in range(21 - first):
            phrases.add(f"Ile to {first} + {second}?")
    for first in range(1, 21):
        for second in range(first + 1):
            phrases.add(f"Ile to {first} − {second}?")
    for groups in range(2, 6):
        for per_group in range(2, min(5, 20 // groups) + 1):
            phrases.add(f"Są {groups} równe grupy. W każdej są {per_group}. Ile jest razem?")
        for per_group in range(1, min(5, 20 // groups) + 1):
            total = groups * per_group
            phrases.add(f"Podziel {total} po równo do {groups} koszyków. Ile będzie w każdym?")

    return sorted(phrases)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--model", type=Path, required=True)
    parser.add_argument("--config", type=Path, required=True)
    args = parser.parse_args()

    OUTPUT_DIRECTORY.mkdir(parents=True, exist_ok=True)
    phrases = read_phrases()
    voice = PiperVoice.load(args.model, config_path=args.config)
    synthesis = SynthesisConfig(length_scale=1.04, noise_scale=0.667, noise_w_scale=0.8)
    manifest = {}
    silence = bytes(round(voice.config.sample_rate * 0.22) * 2)

    phrases_per_file = 20
    with tempfile.TemporaryDirectory(dir=OUTPUT_DIRECTORY) as temporary_directory:
        staging_directory = Path(temporary_directory)
        for group_start in range(0, len(phrases), phrases_per_file):
            group = phrases[group_start:group_start + phrases_per_file]
            audio_name = f"voice-{group_start // phrases_per_file + 1:02}.mp3"
            wav_path = staging_directory / "voice.wav"
            audio_path = staging_directory / audio_name
            offset = 0

            with wave.open(str(wav_path), "wb") as audio_file:
                audio_file.setnchannels(1)
                audio_file.setsampwidth(2)
                audio_file.setframerate(voice.config.sample_rate)

                for phrase in group:
                    start = offset / voice.config.sample_rate
                    frames_written = 0
                    for chunk in voice.synthesize(phrase, synthesis):
                        audio_file.writeframes(chunk.audio_int16_bytes)
                        frames_written += len(chunk.audio_int16_bytes) // 2
                    duration = frames_written / voice.config.sample_rate
                    manifest[phrase] = {
                        "file": audio_name,
                        "start": start,
                        "duration": duration,
                    }
                    audio_file.writeframes(silence)
                    offset += frames_written + len(silence) // 2
                    print(f"Generated {len(manifest)}/{len(phrases)}: {phrase}")

            subprocess.run(
                [
                    "ffmpeg",
                    "-hide_banner",
                    "-loglevel",
                    "error",
                    "-y",
                    "-i",
                    str(wav_path),
                    "-codec:a",
                    "libmp3lame",
                    "-b:a",
                    "48k",
                    "-ar",
                    str(voice.config.sample_rate),
                    "-ac",
                    "1",
                    str(audio_path),
                ],
                check=True,
            )

        staged_manifest_path = staging_directory / "manifest.json"
        staged_manifest_path.write_text(
            json.dumps(manifest, ensure_ascii=False, separators=(",", ":")),
            encoding="utf-8",
        )

        audio_names = {clip["file"] for clip in manifest.values()}
        for audio_name in audio_names:
            (staging_directory / audio_name).replace(OUTPUT_DIRECTORY / audio_name)
        for old_audio in OUTPUT_DIRECTORY.glob("voice-*.mp3"):
            if old_audio.name not in audio_names:
                old_audio.unlink()
        old_sprite = OUTPUT_DIRECTORY / "voice.mp3"
        if old_sprite.exists():
            old_sprite.unlink()
        manifest_path = OUTPUT_DIRECTORY / "manifest.json"
        staged_manifest_path.replace(manifest_path)

    print(f"Wrote {len(phrases)} voice clips across {len(audio_names)} audio files")
    print(f"Wrote voice manifest to {manifest_path}")


if __name__ == "__main__":
    main()
