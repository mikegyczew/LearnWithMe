#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "$0")" && pwd)"
project_root="$(cd "$script_dir/../.." && pwd)"
python="${PYTHON:-$project_root/.venv/bin/python}"

cd "$project_root"
"$python" -c 'import runpy, sys; script = sys.argv[1]; sys.argv = sys.argv[1:]; runpy.run_path(script, run_name="__main__")' \
  "$script_dir/generate_voice_assets.py" "$@"
