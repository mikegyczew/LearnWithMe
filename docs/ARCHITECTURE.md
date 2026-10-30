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
