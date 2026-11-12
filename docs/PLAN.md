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
- React i TypeScript dla wersji webowej,
- MVVM,
- Clean Architecture,
- Entity Framework Core,
- SQLite jako lokalna baza danych,
- Docker Compose do uruchamiania wersji webowej,
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

## 5. Zrobiliśmy: strefę cyferek i matematykę (poziom klasy 1)

Strefę matematyczną dodaliśmy do aplikacji MAUI i webowej. Zawiera ona:

- „Waszą podróż przez Cyfry”: rozpoznawanie liczb od 0 do 20, czytanie ich nazw i rysowanie po śladzie;
- czytanie liczb razem z przykładami rzeczowników, m.in. „jeden samochód”, „jedna sukienka” i „jedna zabawka”, z poprawnymi formami liczby mnogiej;
- ćwiczenia z liczenia obrazków;
- dodawanie i odejmowanie w zakresie do 20;
- wprowadzenie do mnożenia przez równe grupy oraz dzielenia przez równe rozdzielanie;
- łagodne komunikaty po odpowiedzi, automatyczne przejście dalej po sukcesie i możliwość ponowienia próby;
- osobny zapis postępów matematycznych dla każdego profilu.

W wersji webowej podłączyliśmy gotowe polskie nagrania Piper Gosia, które
działają offline i obejmują również komunikaty oraz zadania matematyczne.
Wersja MAUI korzysta z głosu dostępnego na urządzeniu. Rysowanie cyfr
wykorzystuje ten sam mechanizm śledzenia kształtu co rysowanie liter.

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

## 10. Co zrobiliśmy i co pozostaje

### Zrobione
- Przygotowaliśmy aplikację MAUI z profilami dzieci i lokalnym zapisem postępów w SQLite.
- Dodaliśmy polski alfabet, ćwiczenia, odsłuchiwanie, rysowanie liter i tryb wspólnej nauki.
- Dodaliśmy do MAUI i aplikacji webowej matematykę klasy 1, podróż przez cyfry,
  czytanie przykładów, rysowanie cyfr oraz zapis postępów dla każdego profilu.
- Dodaliśmy webowe nagrania Piper Gosia działające offline oraz Docker Compose
  do uruchamiania aplikacji webowej.
- Sprawdziliśmy build i lint wersji webowej oraz odpowiedź HTTP 200 uruchomionej
  aplikacji w Dockerze.

### Pozostaje
- Zbudować i sprawdzić aplikację MAUI na Androidzie; wymaga to Android SDK
  oraz urządzenia lub emulatora.
- Kontynuować kolejne moduły edukacyjne opisane w następnych etapach planu.


## 11. Kryteria ukończenia MVP

- można utworzyć co najmniej dwa profile,
- każdy profil ma niezależny postęp,
- dostępny jest cały polski alfabet,
- dziecko może poznać, usłyszeć i przećwiczyć literę,
- działa przynajmniej kilka typów ćwiczeń,
- postępy są zapisywane lokalnie,
- aplikacja działa bez połączenia z internetem,
- aplikację można zainstalować na tablecie Android.
