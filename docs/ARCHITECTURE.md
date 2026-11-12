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

## 5. Pierwszy wycinek: profile dzieci

- `ChildProfile` jest encją domenową z walidacją imienia i opcjonalnego awatara.
- `IChildProfileService` obsługuje przypadki użycia tworzenia i odczytu profili.
- Infrastructure przechowuje profile w SQLite w `learnwithme.db3` w katalogu danych aplikacji.
- Baza jest tworzona lokalnie przy pierwszym odczycie lub zapisie; aplikacja nie wymaga połączenia z internetem.
- `AlphabetLetter.All` zawiera 32 litery polskiego alfabetu z przykładowym słowem i ilustracją.
- `IAlphabetLearningService` tworzy cztery typy ćwiczeń i zapisuje oznaczone jako poznane litery oraz każdą próbę.
- `LetterProgress` i `PracticeRecord` są przechowywane per profil; historia i odznaki nie porównują dzieci.
- Ekran pozwala dodawać i przełączać profile, poznać i odsłuchać literę, rysować palcem, ćwiczyć quizy i zamieniać role.
- `IMathLearningService` tworzy zadania z rozpoznawania liczb, liczenia, dodawania i odejmowania w zakresie 20 oraz wprowadza mnożenie przez równe grupy i dzielenie przez równe rozdzielanie.
- `MathTopicProgress` przechowuje próby per profil i temat w SQLite; wersja webowa zapisuje odpowiadające im dane w pamięci lokalnej przeglądarki.
- Podróż przez cyfry pokazuje nazwy liczb i przykłady z rzeczownikami, a rysowanie cyfr korzysta ze wspólnego mechanizmu śledzenia kształtu używanego dla liter.
- Poprawne odpowiedzi i ponowne próby otrzymują przyjazny komunikat; po poprawnej odpowiedzi aplikacja automatycznie przechodzi dalej.
- Web odtwarza polskie nagrania Piper `pl_PL-gosia-medium` dołączone jako pliki audio, więc matematyka działa z lektorem offline bez głosu zainstalowanego w przeglądarce. MAUI korzysta z głosu tekst-na-mowę dostępnego na urządzeniu.

## 6. Testowanie

- Domain.Tests - testy modelu domenowego.
- Application.Tests - testy przypadków użycia.
- Infrastructure.Tests - testy integracyjne repozytoriów SQLite.
- Presentation - testy ViewModeli i kluczowych przepływów.
