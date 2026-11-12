# Uczę się z Tobą

**LearnWithMe** is an offline-first educational application for
children and parents. The project keeps the .NET MAUI Android app and also
includes a React web app that opens in a browser.

## Cel projektu

Wspólna nauka dzieci i rodziców poprzez zabawę, odkrywanie,
słuchanie, rysowanie i rozwiązywanie zagadek.

## Główne założenia

- wersja MAUI przeznaczona przede wszystkim na tablety Android,
- lokalny zapis SQLite w wersji MAUI oraz pamięć przeglądarki w wersji webowej,
- profile dzieci i osobne postępy,
- brak reklam i obowiązkowego konta,
- interfejs w języku polskim,
- kod i nazewnictwo techniczne w języku angielskim.

## Technologie

- C#
- .NET 10
- .NET MAUI
- React and TypeScript web app
- Clean Architecture
- MVVM
- Entity Framework Core
- SQLite
- xUnit

## Struktura

- `src/` - kod aplikacji
- `tests/` - testy automatyczne
- `docs/` - dokumentacja projektu
- `scripts/` - skrypty pomocnicze
- `web/` - aplikacja webowa React

## Plan

Szczegółowy plan znajduje się w [docs/PLAN.md](docs/PLAN.md).
Architektura jest opisana w [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Działający MVP

Pierwszy wycinek obejmuje profile dzieci, polski alfabet (32 litery),
przykładowe słowa i ilustracje, wymowę, rysowanie liter palcem,
cztery typy ćwiczeń, tryb zamiany ról oraz lokalny zapis postępów i historii
w SQLite w aplikacji MAUI. Wersja Reactowa przechowuje profile i postępy w
przeglądarce i nie synchronizuje ich z wersją MAUI.

Obie wersje zawierają też strefę matematyczną: rozpoznawanie liczb do 20,
liczenie przedmiotów, dodawanie i odejmowanie do 20 oraz wprowadzenie do
mnożenia przez równe grupy i dzielenia przez równe rozdzielanie. Postępy są
zapisywane osobno dla każdego profilu.

### Testy

```sh
dotnet test tests/LearnWithMe.Domain.Tests/LearnWithMe.Domain.Tests.csproj
dotnet test tests/LearnWithMe.Application.Tests/LearnWithMe.Application.Tests.csproj
dotnet test tests/LearnWithMe.Infrastructure.Tests/LearnWithMe.Infrastructure.Tests.csproj
```

### Android

Zainstaluj .NET 10 MAUI Android workload oraz Android SDK (platform i build-tools
API 36), następnie podłącz urządzenie Android z włączonym debugowaniem USB.

```sh
dotnet build src/LearnWithMe.Presentation/LearnWithMe.Presentation.csproj -f net10.0-android
dotnet build src/LearnWithMe.Presentation/LearnWithMe.Presentation.csproj -f net10.0-android -c Release
```

APK do instalacji ręcznej jest generowany w katalogu `bin/Release/net10.0-android/`.

### Aplikacja webowa

Do uruchomienia wersji przeglądarkowej potrzebny jest Node.js 20.19+ lub 22.12+.

```sh
cd web
npm install
npm run dev
```

Wersja produkcyjna:

```sh
npm run build
```

Pliki do hostowania statycznego znajdują się w `web/dist/`. Profile i postępy
wersji webowej są przechowywane lokalnie w pamięci przeglądarki; nie są
synchronizowane z aplikacją MAUI ani pomiędzy urządzeniami.

Polskie nagrania wersji webowej są dołączone do aplikacji i działają offline.
Głos matematyczny i lektorski to nagrania wygenerowane Piperem
`pl_PL-gosia-medium`; nie jest wymagane instalowanie ani uruchamianie modelu
Piper podczas korzystania z aplikacji. Kolekcja głosów Piper jest na licencji
MIT, a karta modelu Gosia podaje licencję CC0 dla danych głosowych. Aplikacja
MAUI korzysta z głosu zainstalowanego na urządzeniu.
[Kolekcja głosów Piper](https://huggingface.co/rhasspy/piper-voices) ·
[Karta modelu Gosia](https://huggingface.co/rhasspy/piper-voices/blob/v1.0.0/pl/pl_PL/gosia/medium/MODEL_CARD).

#### Ponowne generowanie nagrań

Nagrania są gotowe w repozytorium i nie trzeba ich generować, aby uruchomić
aplikację. Do ich odtworzenia potrzebne są Python z pakietem `piper-tts`,
`ffmpeg` oraz pobrany model Piper wraz z plikiem konfiguracyjnym `.onnx.json`.
Model nie jest przechowywany w repozytorium. Z katalogu głównego uruchom:

```sh
web/scripts/generate_voice_assets.sh \
  --model /ścieżka/do/pl_PL-gosia-medium.onnx \
  --config /ścieżka/do/pl_PL-gosia-medium.onnx.json
```

Skrypt odświeża nagrania alfabetu, sylab i zadań matematycznych w
`web/public/voice/gosia/`.

Uruchomienie wersji webowej w Dockerze z katalogu głównego repozytorium:

```sh
docker compose up --build
```

Aplikacja będzie dostępna pod adresem http://localhost:4173. Zatrzymanie:

```sh
docker compose down
```
