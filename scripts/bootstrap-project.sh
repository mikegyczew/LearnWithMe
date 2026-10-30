#!/usr/bin/env bash
set -Eeuo pipefail

PROJECT_NAME="LearnWithMe"
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT"

echo "=========================================="
echo "  LearnWithMe - Bootstrap projektu"
echo "=========================================="

# 1. Kontrola środowiska
if ! command -v dotnet >/dev/null 2>&1; then
    echo "BLAD: Nie znaleziono .NET SDK."
    exit 1
fi

SDK_VERSION="$(dotnet --version)"
SDK_MAJOR="${SDK_VERSION%%.*}"

echo "Wykryto .NET SDK: $SDK_VERSION"

if (( SDK_MAJOR < 10 )); then
    echo "BLAD: Projekt wymaga .NET SDK 10 lub nowszego."
    exit 1
fi

# Nie nadpisujemy istniejącego projektu
if [[ -f "$PROJECT_NAME.sln" ||
      -f "$PROJECT_NAME.slnx" ||
      -d "src/LearnWithMe.Domain" ]]; then
    echo "BLAD: Struktura projektu już istnieje."
    echo "Nie nadpisuję istniejących plików."
    exit 1
fi

# 2. Instalacja MAUI dla Linux/Android
echo
echo "===== SPRAWDZANIE MAUI ====="

if ! dotnet workload list | grep -q "maui-android"; then
    echo "Instaluję workload maui-android..."
    dotnet workload install maui-android
else
    echo "Workload maui-android jest już zainstalowany."
fi

# 3. Dokumentacja
echo
echo "===== TWORZENIE DOKUMENTACJI ====="

mkdir -p docs

cat > README.md <<'EOF'
# Uczę się z Tobą

**LearnWithMe** is an offline-first educational application for
children and parents, built with C#, .NET MAUI and Clean Architecture.

## Cel projektu

Wspólna nauka dzieci i rodziców poprzez zabawę, odkrywanie,
słuchanie, rysowanie i rozwiązywanie zagadek.

## Główne założenia

- aplikacja przeznaczona przede wszystkim na tablety Android,
- działanie offline,
- lokalna baza SQLite,
- profile dzieci i osobne postępy,
- brak reklam i obowiązkowego konta,
- interfejs w języku polskim,
- kod i nazewnictwo techniczne w języku angielskim.

## Technologie

- C#
- .NET 10
- .NET MAUI
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

## Plan

Szczegółowy plan znajduje się w [docs/PLAN.md](docs/PLAN.md).
Architektura jest opisana w [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).
EOF

cat > docs/PLAN.md <<'EOF'
# LearnWithMe - Plan projektu

## 1. Wizja

„Uczę się z Tobą” to aplikacja edukacyjna dla dzieci rozpoczynających
przygodę ze szkołą. Ma pomagać dziecku poznawać świat razem z rodzicem.

Nie tworzymy kolejnej aplikacji opartej na rywalizacji i rankingach.
Najważniejsze są wspólne odkrywanie, cierpliwość, zabawa i samodzielność.

## 2. Główne zasady produktu

- nauka przez zabawę i interakcję,
- wspólny tryb rodzic + dziecko,
- możliwość samodzielnej nauki dziecka,
- dziecko może również uczyć rodzica,
- każde dziecko ma własny profil i tempo nauki,
- brak karania za błędy,
- nagradzamy ciekawość, próby i postęp,
- aplikacja działa bez internetu,
- brak reklam, subskrypcji i obowiązkowego konta.

## 3. Platforma i technologie

- C# i .NET 10,
- .NET MAUI,
- Android jako pierwsza platforma docelowa,
- tablety jako główne urządzenia,
- MVVM,
- Clean Architecture,
- Entity Framework Core,
- SQLite jako lokalna baza danych,
- xUnit do testów.

## 4. Etap I - MVP: polski alfabet

### Profile dzieci
- tworzenie profilu dziecka,
- imię i opcjonalny awatar,
- osobny postęp każdego dziecka,
- możliwość przełączania profili.

### Nauka liter
- pełny polski alfabet,
- wielkie i małe litery,
- polskie znaki: Ą, Ć, Ę, Ł, Ń, Ó, Ś, Ź, Ż,
- prezentacja litery,
- odsłuchiwanie wymowy,
- przykładowe słowo,
- ilustracja przedstawiająca słowo,
- śledzenie kształtu litery palcem.

### Zabawy i ćwiczenia
- rozpoznawanie wskazanej litery,
- dopasowywanie litery do obrazka,
- łączenie wielkiej i małej litery,
- wybieranie litery rozpoczynającej słowo,
- proste quizy,
- powtarzanie wcześniej poznanych liter.

### Postępy
- zapis poznanych liter,
- historia ćwiczeń,
- osiągnięcia i odznaki,
- przyjazne podsumowanie nauki,
- brak porównywania dzieci między sobą.

### Wspólna nauka
- zadania wykonywane razem z rodzicem,
- pytania, na które odpowiadają na zmianę,
- możliwość zamiany ról,
- dziecko może przygotować zagadkę dla rodzica.

## 5. Etap II - liczby i matematyka

- rozpoznawanie cyfr,
- liczenie przedmiotów,
- porównywanie ilości,
- dodawanie i odejmowanie,
- proste zadania obrazkowe,
- nauka kształtu cyfr.

## 6. Etap III - poznawanie świata

- zwierzęta,
- kolory i kształty,
- przyroda,
- pory roku,
- pogoda,
- ludzkie ciało,
- zawody,
- kosmos,
- otaczający nas świat.

## 7. Etap IV - logika i kreatywność

- układanki,
- sekwencje i wzory,
- proste zagadki logiczne,
- sortowanie i kategoryzowanie,
- rysowanie,
- kolorowanki,
- tworzenie własnych historyjek.

## 8. Etap V - rozwój aplikacji

- dodatkowe poziomy trudności,
- kolejne moduły edukacyjne,
- rozbudowane osiągnięcia,
- kopia zapasowa danych na życzenie użytkownika,
- możliwość przenoszenia profili między urządzeniami.

## 9. Prywatność i bezpieczeństwo

- dane dzieci przechowywane lokalnie,
- brak obowiązkowego logowania,
- brak reklam i śledzenia,
- brak wysyłania danych dziecka do zewnętrznych usług,
- dostęp do funkcji rodzicielskich oddzielony od trybu dziecka.

## 10. Kolejność realizacji

1. Przygotowanie repozytorium i architektury.
2. Konfiguracja aplikacji MAUI i Androida.
3. Podstawowy interfejs i nawigacja.
4. Profile dzieci.
5. Lokalna baza SQLite.
6. Moduł polskiego alfabetu.
7. Audio i ilustracje.
8. Ćwiczenia interaktywne.
9. Rysowanie liter palcem.
10. Zapisywanie postępów i osiągnięć.
11. Tryb wspólnej nauki.
12. Testy na tabletach Android.
13. Budowanie i instalacja APK.

## 11. Kryteria ukończenia MVP

- można utworzyć co najmniej dwa profile,
- każdy profil ma niezależny postęp,
- dostępny jest cały polski alfabet,
- dziecko może poznać, usłyszeć i przećwiczyć literę,
- działa przynajmniej kilka typów ćwiczeń,
- postępy są zapisywane lokalnie,
- aplikacja działa bez połączenia z internetem,
- aplikację można zainstalować na tablecie Android.
EOF

cat > docs/ARCHITECTURE.md <<'EOF'
# Architektura LearnWithMe

## 1. Podejście

Projekt korzysta z Clean Architecture.

Zależności kierujemy do środka:

Presentation -> Application -> Domain
Infrastructure -> Application + Domain

Presentation jest punktem startowym aplikacji i miejscem składania
zależności (Dependency Injection).

## 2. Projekty

### LearnWithMe.Domain
Centralna logika i model domenowy.

Nie zależy od pozostałych projektów.

Przykładowe obszary:
- Entities
- ValueObjects
- Enums
- Events
- Exceptions

### LearnWithMe.Application
Przypadki użycia aplikacji i kontrakty.

Zależy wyłącznie od Domain.

Przykładowe obszary:
- Abstractions
- Common
- DTOs
- Features/Profiles
- Features/Alphabet
- Features/Progress

### LearnWithMe.Infrastructure
Implementacje techniczne.

Odpowiada za:
- SQLite,
- Entity Framework Core,
- repozytoria,
- lokalne przechowywanie danych,
- dostęp do plików i zasobów.

Zależy od Application i Domain.

### LearnWithMe.Presentation
Aplikacja .NET MAUI.

Odpowiada za:
- widoki,
- ViewModele,
- nawigację,
- obsługę dotyku,
- audio,
- integrację z Androidem,
- konfigurację Dependency Injection.

Zależy od Application i Infrastructure.

## 3. Przepływ danych

Widok MAUI
    -> ViewModel
        -> przypadek użycia Application
            -> interfejs z Application
                -> implementacja Infrastructure
                    -> SQLite

## 4. Zasady

- Domain nie zna Entity Framework ani MAUI.
- Application nie zna SQLite ani interfejsu użytkownika.
- Infrastructure implementuje interfejsy z Application.
- Presentation nie zawiera logiki biznesowej.
- ViewModel nie komunikuje się bezpośrednio z DbContext.
- Dane dzieci pozostają lokalne.
- Logika biznesowa jest testowalna bez uruchamiania MAUI.

## 5. Testowanie

- Domain.Tests - testy modelu domenowego.
- Application.Tests - testy przypadków użycia.
- Infrastructure - późniejsze testy integracyjne SQLite.
- Presentation - testy ViewModeli i kluczowych przepływów.
EOF

# 4. Tworzenie rozwiązania
echo
echo "===== TWORZENIE SOLUTION ====="

dotnet new sln -n "$PROJECT_NAME" --format sln

mkdir -p src tests

# 5. Projekty Clean Architecture
echo
echo "===== TWORZENIE PROJEKTOW ====="

dotnet new classlib -n LearnWithMe.Domain \
    -o src/LearnWithMe.Domain --framework net10.0

dotnet new classlib -n LearnWithMe.Application \
    -o src/LearnWithMe.Application --framework net10.0

dotnet new classlib -n LearnWithMe.Infrastructure \
    -o src/LearnWithMe.Infrastructure --framework net10.0

dotnet new maui -n LearnWithMe.Presentation \
    -o src/LearnWithMe.Presentation

# Na Linuxie na początek budujemy wyłącznie Androida.
python3 - <<'PY'
from pathlib import Path
import re

path = Path("src/LearnWithMe.Presentation/LearnWithMe.Presentation.csproj")
content = path.read_text(encoding="utf-8")

content, count = re.subn(
    r"<TargetFrameworks(?:\s+Condition=\"[^\"]+\")?>.*?</TargetFrameworks>",
    "<TargetFrameworks>net10.0-android</TargetFrameworks>",
    content,
    count=1,
    flags=re.DOTALL
)

if count == 0:
    raise SystemExit("Nie udało się ustawić TargetFrameworks w projekcie MAUI.")

path.write_text(content, encoding="utf-8")
PY

# 6. Projekty testowe
echo
echo "===== TWORZENIE TESTOW ====="

dotnet new xunit -n LearnWithMe.Domain.Tests \
    -o tests/LearnWithMe.Domain.Tests --framework net10.0

dotnet new xunit -n LearnWithMe.Application.Tests \
    -o tests/LearnWithMe.Application.Tests --framework net10.0

# 7. Referencje między projektami
echo
echo "===== KONFIGURACJA REFERENCJI ====="

dotnet add src/LearnWithMe.Application/LearnWithMe.Application.csproj \
    reference src/LearnWithMe.Domain/LearnWithMe.Domain.csproj

dotnet add src/LearnWithMe.Infrastructure/LearnWithMe.Infrastructure.csproj \
    reference src/LearnWithMe.Domain/LearnWithMe.Domain.csproj \
              src/LearnWithMe.Application/LearnWithMe.Application.csproj

dotnet add src/LearnWithMe.Presentation/LearnWithMe.Presentation.csproj \
    reference src/LearnWithMe.Application/LearnWithMe.Application.csproj \
              src/LearnWithMe.Infrastructure/LearnWithMe.Infrastructure.csproj

dotnet add tests/LearnWithMe.Domain.Tests/LearnWithMe.Domain.Tests.csproj \
    reference src/LearnWithMe.Domain/LearnWithMe.Domain.csproj

dotnet add tests/LearnWithMe.Application.Tests/LearnWithMe.Application.Tests.csproj \
    reference src/LearnWithMe.Application/LearnWithMe.Application.csproj \
              src/LearnWithMe.Domain/LearnWithMe.Domain.csproj

# 8. SQLite / EF Core
echo
echo "===== DODAWANIE SQLITE ====="

dotnet add src/LearnWithMe.Infrastructure/LearnWithMe.Infrastructure.csproj \
    package Microsoft.EntityFrameworkCore.Sqlite

# 9. Dodanie projektów do solution
echo
echo "===== DODAWANIE PROJEKTOW DO SOLUTION ====="

dotnet sln "$PROJECT_NAME.sln" add \
    src/LearnWithMe.Domain/LearnWithMe.Domain.csproj \
    src/LearnWithMe.Application/LearnWithMe.Application.csproj \
    src/LearnWithMe.Infrastructure/LearnWithMe.Infrastructure.csproj \
    src/LearnWithMe.Presentation/LearnWithMe.Presentation.csproj \
    --solution-folder src

dotnet sln "$PROJECT_NAME.sln" add \
    tests/LearnWithMe.Domain.Tests/LearnWithMe.Domain.Tests.csproj \
    tests/LearnWithMe.Application.Tests/LearnWithMe.Application.Tests.csproj \
    --solution-folder tests

# 10. Foldery wewnętrzne
echo
echo "===== TWORZENIE FOLDEROW ARCHITEKTURY ====="

FOLDERS=(
    "src/LearnWithMe.Domain/Entities"
    "src/LearnWithMe.Domain/ValueObjects"
    "src/LearnWithMe.Domain/Enums"
    "src/LearnWithMe.Domain/Events"
    "src/LearnWithMe.Domain/Exceptions"

    "src/LearnWithMe.Application/Abstractions"
    "src/LearnWithMe.Application/Common"
    "src/LearnWithMe.Application/DTOs"
    "src/LearnWithMe.Application/Features/Profiles"
    "src/LearnWithMe.Application/Features/Alphabet"
    "src/LearnWithMe.Application/Features/Progress"

    "src/LearnWithMe.Infrastructure/Persistence/Configurations"
    "src/LearnWithMe.Infrastructure/Persistence/Converters"
    "src/LearnWithMe.Infrastructure/Repositories"
    "src/LearnWithMe.Infrastructure/Services"

    "src/LearnWithMe.Presentation/Views"
    "src/LearnWithMe.Presentation/ViewModels"
    "src/LearnWithMe.Presentation/Controls"
    "src/LearnWithMe.Presentation/Converters"
    "src/LearnWithMe.Presentation/Resources/Images/Letters"
    "src/LearnWithMe.Presentation/Resources/Raw/Audio"
    "src/LearnWithMe.Presentation/Resources/Raw/Content"

    "tests/LearnWithMe.Domain.Tests/Entities"
    "tests/LearnWithMe.Application.Tests/Features"
)

for folder in "${FOLDERS[@]}"; do
    mkdir -p "$folder"
    touch "$folder/.gitkeep"
done

# 11. Wspólne ustawienia C#
cat > Directory.Build.props <<'EOF'
<Project>
  <PropertyGroup>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <Deterministic>true</Deterministic>
  </PropertyGroup>
</Project>
EOF

# 12. Gitignore
cat > .gitignore <<'EOF'
# .NET
bin/
obj/
*.user
*.suo
*.userosscache
*.sln.docstates

# IDE
.vs/
.idea/

# MAUI / Android
*.apk
*.aab
*.keystore
*.jks

# Local databases
*.db
*.db-shm
*.db-wal

# OS
.DS_Store
Thumbs.db

# Logs
*.log
EOF

# 13. Usunięcie pustych klas wygenerowanych przez classlib
rm -f src/LearnWithMe.Domain/Class1.cs
rm -f src/LearnWithMe.Application/Class1.cs
rm -f src/LearnWithMe.Infrastructure/Class1.cs

# 14. Przywrócenie pakietów i testy
echo
echo "===== RESTORE ====="

dotnet restore "$PROJECT_NAME.sln"

echo
echo "===== BUILD: DOMAIN ====="
dotnet build src/LearnWithMe.Domain/LearnWithMe.Domain.csproj --no-restore

echo
echo "===== BUILD: APPLICATION ====="
dotnet build src/LearnWithMe.Application/LearnWithMe.Application.csproj --no-restore

echo
echo "===== BUILD: INFRASTRUCTURE ====="
dotnet build src/LearnWithMe.Infrastructure/LearnWithMe.Infrastructure.csproj --no-restore

echo
echo "===== TEST: DOMAIN ====="
dotnet test tests/LearnWithMe.Domain.Tests/LearnWithMe.Domain.Tests.csproj --no-restore

echo
echo "===== TEST: APPLICATION ====="
dotnet test tests/LearnWithMe.Application.Tests/LearnWithMe.Application.Tests.csproj --no-restore

# 15. Podsumowanie
echo
echo "=========================================="
echo "  LearnWithMe - struktura utworzona"
echo "=========================================="

echo
echo "Projekty:"
dotnet sln "$PROJECT_NAME.sln" list

echo
echo "Pliki i foldery:"
find docs src tests scripts -maxdepth 3 -print | sort

echo
echo "Status Git:"
git status --short

echo
echo "UWAGA:"
echo "Projekt MAUI jest przygotowany dla Androida."
echo "Konfigurację Android SDK/JDK i pierwszy build APK wykonamy osobno."
echo
echo "Bootstrap zakończony."
