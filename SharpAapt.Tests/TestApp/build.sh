#!/bin/sh
# Rebuilds ../Fixtures/test.apk from this directory with aapt2.
# Usage: ./build.sh [path-to-android-sdk]
set -eu

SDK="${1:-${ANDROID_HOME:-${ANDROID_SDK_ROOT:-}}}"
if [ -z "$SDK" ]; then
  echo "Set ANDROID_HOME or pass the SDK path." >&2
  exit 1
fi

AAPT2="$(ls -d "$SDK"/build-tools/*/aapt2 | sort -V | tail -1)"
ANDROID_JAR="$SDK/platforms/android-34/android.jar"

cd "$(dirname "$0")"
WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

"$AAPT2" compile --dir res -o "$WORK/res.zip"
APK="$(cd ../Fixtures && pwd)/test.apk"
"$AAPT2" link -o "$APK" -I "$ANDROID_JAR" --manifest AndroidManifest.xml "$WORK/res.zip"

# Placeholder native libraries, so badging reports native-code for two ABIs.
for ABI in arm64-v8a armeabi-v7a; do
  mkdir -p "$WORK/native/lib/$ABI"
  printf 'stub' > "$WORK/native/lib/$ABI/libstub.so"
done
(cd "$WORK/native" && zip -qrD "$APK" lib)

echo "Wrote $APK"
