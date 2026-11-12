Voice: pl_PL-gosia-medium, Piper
Voice files are generated from the current learning vocabulary in small MP3
segments with:
https://github.com/OHF-Voice/piper1-gpl

Voice model: https://huggingface.co/rhasspy/piper-voices/tree/main/pl/pl_PL/gosia/medium
Model card dataset license: CC0
Model card: https://huggingface.co/rhasspy/piper-voices/resolve/main/pl/pl_PL/gosia/medium/MODEL_CARD

Regenerate after adding vocabulary:
bash web/scripts/generate_voice_assets.sh --model /path/to/pl_PL-gosia-medium.onnx --config /path/to/pl_PL-gosia-medium.onnx.json

The generator requires Piper TTS and ffmpeg. It regenerates the phrase catalog
from the current app vocabulary and creates compressed MP3 segments.
