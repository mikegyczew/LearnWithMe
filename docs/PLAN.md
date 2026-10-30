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
