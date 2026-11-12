# Scenariusze nagrań głosowych

Dokument opisuje wszystkie teksty odczytywane przez głos w aplikacji oraz
przygotowane pliki audio. Pełny, aktualny spis tekstów wraz z przypisaniem do
plików i pozycją nagrania znajduje się w manifeście:
[`web/public/voice/gosia/manifest.json`](../web/public/voice/gosia/manifest.json).

**Teksty gotowe do nagrania, rozpisane osobno pod każdy plik MP3:**
[`docs/TEKSTY-NAGRAŃ.md`](TEKSTY-NAGRAŃ.md). Pod nagłówkiem `voice-01.mp3`
znajdziesz jego teksty, pod `voice-02.mp3` — kolejne itd.; wypowiedzi są w
kolejności odtwarzania i można je skopiować do innego narzędzia.

## Wersja webowa — gotowe nagrania

Web odtwarza polski głos Piper `pl_PL-gosia-medium` z plików dołączonych do
aplikacji. Audio działa offline; podczas korzystania z aplikacji nie jest
wymagany Piper ani model głosu.

- Katalog: [`web/public/voice/gosia/`](../web/public/voice/gosia/)
- Pliki audio: `voice-01.mp3`–`voice-54.mp3` (54 pliki; nagrania są łączone
  paczkami po maksymalnie 20 wypowiedzi)
- Indeks tekstów i pozycji nagrań: `manifest.json`
- Odtwarzanie: [`web/src/speech.ts`](../web/src/speech.ts)
- Skrypt tworzący katalog wypowiedzi: [`web/scripts/generate_voice_assets.py`](../web/scripts/generate_voice_assets.py)

Manifest zawiera **1069 dokładnych tekstów**. Każdy tekst jest kluczem obiektu,
a przypisany mu wpis wskazuje `file`, `start` i `duration`. Teksty z manifestu
są dokładnymi identyfikatorami używanymi przez aplikację — przy generowaniu
nowego głosu zachowaj ich brzmienie tekstowe, polskie znaki, wielkość liter i
interpunkcję. Wygodny spis lektorski pogrupowany według plików MP3 znajduje się
w [`docs/TEKSTY-NAGRAŃ.md`](TEKSTY-NAGRAŃ.md).

### Zakres tekstów w manifeście

| Obszar | Liczba tekstów | Przykładowy tekst |
|---|---:|---|
| Alfabet: nazwy liter i przykłady słów | 156 | `Litera A`, `A jak auto.` |
| Dzielenie słów na sylaby | 364 | `ananas. Słowo dzielimy na sylaby: a, na, nas.` |
| Liczby: polecenia i odczyt liczby z przykładami | 42 | `Znajdź liczbę 7.` |
| Liczenie obrazków | 1 | `Policz obrazki. Ile ich jest?` |
| Przykłady liczebników i rzeczowników | 63 | `dwa samochody` |
| Dodawanie | 176 | `Ile to 2 + 3?` |
| Odejmowanie | 230 | `Ile to 7 − 4?` |
| Równe grupy (mnożenie) | 15 | `Są 2 równe grupy. W każdej są 3. Ile jest razem?` |
| Dzielenie | 19 | `Podziel 6 po równo do 2 koszyków. Ile będzie w każdym?` |
| Krótkie komunikaty zwrotne | 3 | `Dobra robota!`, `Spróbuj ponownie.` |
| **Razem** | **1069** | |

Wypowiedzi matematyczne obejmują liczby od 0 do 20, działania dodawania
i odejmowania w zakresie obsługiwanym przez ćwiczenia, mnożenie przez równe
grupy oraz dzielenie do 20. Nie wszystkie teksty są pojedynczymi pytaniami:
część odczytuje liczbę razem z jej nazwą i przykładami, np. formami
„samochód”, „sukienka” i „zabawka”.

### Użycie tekstów przez aplikację

- **Litery i alfabet:** odczyt nazwy litery (`Litera A`) oraz karty w formacie
  `A jak auto.`. Przykłady kart pochodzą z katalogu w
  [`web/src/App.tsx`](../web/src/App.tsx).
- **Sylaby:** tekst ma format
  `{słowo}. Słowo dzielimy na sylaby: {sylaba 1}, {sylaba 2}, ...`.
  Słowa i podziały pochodzą z
  [`web/src/data/learningData.ts`](../web/src/data/learningData.ts).
- **Matematyka:** pytania oraz przykłady ilościowe są składane z zakresów
  liczbowych i wzorów w
  [`web/src/components/MathZone.tsx`](../web/src/components/MathZone.tsx).
  Manifest zawiera wszystkie warianty, które ten kod może odtworzyć.
- **Odpowiedzi:** używane są m.in. `Dobra robota!`, `Spróbuj ponownie.` oraz
  `Spróbuj jeszcze raz.`.

### Ponowne wygenerowanie istniejącego zestawu

Wymagane są Python z pakietem `piper-tts`, `ffmpeg` oraz pobrany model Piper
wraz z plikiem konfiguracyjnym. Z katalogu głównego repozytorium:

```sh
web/scripts/generate_voice_assets.sh \
  --model /ścieżka/do/pl_PL-gosia-medium.onnx \
  --config /ścieżka/do/pl_PL-gosia-medium.onnx.json
```

Skrypt odczytuje bieżące teksty z kodu aplikacji i ponownie tworzy pliki MP3
oraz manifest. Instrukcje dotyczące modelu i licencji są w
[`README.md`](../README.md).

### Użycie innego narzędzia do generowania głosu

1. Wczytaj klucze z `manifest.json` jako listę wypowiedzi do wygenerowania.
   To kompletna lista tekstów; nie trzeba ręcznie przepisywać 1069 pozycji.
2. Wygeneruj audio dla każdego klucza, zachowując tekst jako identyfikator.
   Katalog produkcyjny obecnie łączy wypowiedzi w pliki `voice-NN.mp3`, dlatego
   można albo zbudować takie same paczki i zaktualizować w manifeście ich
   pozycje, albo przejść na osobny plik dla każdej wypowiedzi i dostosować
   mapowanie w `web/src/speech.ts`.
3. Jeśli zmieni się nazwa, interpunkcja lub treść wypowiedzi, zaktualizuj
   jednocześnie tekst w kodzie i odpowiadający mu wpis w manifeście. W
   przeciwnym razie odtwarzacz nie znajdzie nagrania.

## Aplikacja MAUI — głos urządzenia

MAUI **nie używa osobnych plików z nagraniami**. Wymowę generuje systemowy
syntezator mowy urządzenia (`TextToSpeech.Default`) z polskim locale, jeśli
jest dostępne. Jeśli chcesz zastąpić syntezator własnymi nagraniami, dodatkowe
53 teksty (wymowy liter i nazwy liczb) są wypisane w sekcji MAUI dokumentu
[`docs/TEKSTY-NAGRAŃ.md`](TEKSTY-NAGRAŃ.md).

- **Alfabet:** odczytywana jest wymowa wybranej litery z `Pronunciation` w
  [`src/LearnWithMe.Application/Features/Alphabet/AlphabetLetter.cs`](../src/LearnWithMe.Application/Features/Alphabet/AlphabetLetter.cs).
- **Matematyka:** przyciski odczytują aktualny tekst pytania oraz przykłady
  liczebników z rzeczownikami, np. „jeden samochód” lub „dwie sukienki”.
  Teksty pytań są tworzone przez serwis matematyczny w
  [`src/LearnWithMe.Application/Features/Math/MathLearningService.cs`](../src/LearnWithMe.Application/Features/Math/MathLearningService.cs),
  a przykłady są przekazywane do syntezatora w
  [`src/LearnWithMe.Presentation/MainPage.xaml.cs`](../src/LearnWithMe.Presentation/MainPage.xaml.cs).

W efekcie pliki z katalogu `web/public/voice/gosia/` dotyczą wersji webowej;
nie są pakietem audio dla MAUI.
