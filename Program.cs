using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;




class Program
{
    static List<Plane> planes = new List<Plane>();
    static List<Location> locations = new List<Location>();
    static List<Flight> flights = new List<Flight>();
    static List<Reservation> reservations = new List<Reservation>();

    static void Main()
    {
        LoadPlanes();
        LoadLocations();
        LoadFlights();

        Console.WriteLine("Uçuşları görüntülemek ve rezervasyon yapmak için 1'e basın.");
        Console.WriteLine("Çıkış yapmak için herhangi bir tuşa basın.");

        if (Console.ReadLine() == "1")
        {
            DisplayFlights();

            Console.Write("Rezervasyon yapmak istediğiniz uçuşun numarasını girin: ");
            int selectedFlightIndex;
            if (int.TryParse(Console.ReadLine(), out selectedFlightIndex) && selectedFlightIndex >= 1 && selectedFlightIndex <= flights.Count)
            {
                MakeReservation(selectedFlightIndex - 1);
            }
            else
            {
                Console.WriteLine("Geçersiz giriş. Programdan çıkılıyor.");
            }
        }
    }

    static void DisplayFlights()
    {
        Console.WriteLine("Uçuşlar:");
        for (int i = 0; i < flights.Count; i++)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{i + 1}. Uçuş - {flights[i].Location.Country}, {flights[i].Location.City}, {flights[i].Location.Airport} - {flights[i].Time}");
            Console.ResetColor();
            Console.WriteLine($"Uçak Bilgisi: {flights[i].PlaneInfo.Brand} {flights[i].PlaneInfo.Model}, Kapasite: {flights[i].PlaneInfo.Capacity}");
            Console.WriteLine();
        }
    }

    static void MakeReservation(int selectedFlightIndex)
    {
        Flight selectedFlight = flights[selectedFlightIndex];

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Seçilen Uçuş - {selectedFlight.Location.Country}, {selectedFlight.Location.City}, {selectedFlight.Location.Airport} - {selectedFlight.Time}");
        Console.ResetColor();
        Console.WriteLine($"Uçak Bilgisi: {selectedFlight.PlaneInfo.Brand} {selectedFlight.PlaneInfo.Model}, Kapasite: {selectedFlight.PlaneInfo.Capacity}");

        string selectedSeat = ChooseSeat(selectedFlight);

        if (CheckCapacity(selectedFlight, selectedSeat))
        {
            GetUserInfo(out string name, out string surname, out string email, out int age);

            ReservationDetail detail = new ReservationDetail
            {
                FlightCity = selectedFlight.Location.City,
                FlightTime = selectedFlight.Time,
                PlaneBrand = selectedFlight.PlaneInfo.Brand,
                PlaneModel = selectedFlight.PlaneInfo.Model,
                PlaneCapacity = selectedFlight.PlaneInfo.Capacity,
                PassengerName = name,
                PassengerSurname = surname,
                PassengerEmail = email,
                PassengerAge = age,
                SeatNumber = selectedSeat
            };

            reservations.Add(new Reservation
            {
                FlightInfo = selectedFlight,
                Detail = detail
            });

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Rezervasyon başarıyla yapıldı!");
            Console.ResetColor();

            Console.WriteLine($"Uçuş şehri: {selectedFlight.Location.City}");
            Console.WriteLine("Rezervasyon onaylıyor musunuz? (E/H)");
            string confirmation = Console.ReadLine().ToUpper();

            if (confirmation == "E")
            {
                Console.WriteLine("Rezervasyon onaylandı!");
                SaveReservationsToJson();
                DisplayReservationDetails(reservations.Last());
            }
            else
            {
                Console.WriteLine("Rezervasyon onaylanmadı. Programdan çıkılıyor.");
            }
        }
        else
        {
            Console.WriteLine("Üzgünüz, seçilen koltuk dolu veya uçak kapasitesi dolu. Rezervasyon yapılamıyor.");
        }
    }

    static string ChooseSeat(Flight selectedFlight)
    {
        Console.WriteLine("Koltuk Seçimi:");
        var validSeats = GetValidSeats(selectedFlight);
        Console.WriteLine($"Boş Koltuklar: {string.Join(", ", validSeats)}");

        Console.Write("Koltuk seçin: ");
        string selectedSeat = Console.ReadLine().ToUpper();

        while (!validSeats.Contains(selectedSeat))
        {
            Console.WriteLine("Geçersiz koltuk veya koltuk dolu. Lütfen tekrar seçin.");
            Console.Write("Koltuk seçin: ");
            selectedSeat = Console.ReadLine().ToUpper();
        }

        return selectedSeat;
    }

    static List<string> GetValidSeats(Flight selectedFlight)
    {
        List<string> validSeats = new List<string>();
        for (char rowChar = 'A'; rowChar <= 'C'; rowChar++)
        {
            int maxRow = (rowChar == 'A' || rowChar == 'B') ? selectedFlight.PlaneInfo.Capacity / 4 : selectedFlight.PlaneInfo.Capacity / 2;
            for (int rowNumber = 1; rowNumber <= maxRow; rowNumber++)
            {
                string seat = $"{rowChar}{rowNumber:D2}";
                if (!IsSeatTaken(seat, selectedFlight))
                {
                    validSeats.Add(seat);
                }
            }
        }
        return validSeats.Take(10).ToList();
    }

    static bool IsSeatTaken(string seat, Flight selectedFlight)
    {
        return reservations.Any(reservation => reservation.FlightInfo == selectedFlight && reservation.Detail.SeatNumber == seat);
    }

    static void GetUserInfo(out string name, out string surname, out string email, out int age)
    {
        Console.Write("Adınızı girin: ");
        name = Console.ReadLine();

        Console.Write("Soyadınızı girin: ");
        surname = Console.ReadLine();

        Console.Write("E-posta adresinizi girin: ");
        email = Console.ReadLine();

        Console.Write("Yaşınızı girin: ");
        while (!int.TryParse(Console.ReadLine(), out age) || age <= 0)
        {
            Console.WriteLine("Geçersiz yaş. Lütfen tekrar girin.");
        }
    }

    static bool CheckCapacity(Flight selectedFlight, string selectedSeat)
    {
        int reservedSeats = reservations.Count(reservation => reservation.FlightInfo == selectedFlight);
        return reservedSeats < selectedFlight.PlaneInfo.Capacity && !IsSeatTaken(selectedSeat, selectedFlight);
    }

    static void SaveReservationsToJson()
    {
        string json = JsonSerializer.Serialize(reservations);
        File.WriteAllText("reservations.json", json);
    }

    static void DisplayReservationDetails(Reservation reservation)
    {
        ReservationDetail detail = reservation.Detail;

        Console.WriteLine("Rezervasyon Detayları:");
        Console.WriteLine($"Uçuş Bilgisi: {detail.FlightCity}, {detail.FlightTime} - {detail.PlaneBrand} {detail.PlaneModel}, Kapasite: {detail.PlaneCapacity}");
        Console.WriteLine($"Yolcu Bilgisi: {detail.PassengerName} {detail.PassengerSurname}, E-posta: {detail.PassengerEmail}, Yaş: {detail.PassengerAge}, Koltuk: {detail.SeatNumber}\n");
    }

    static void LoadPlanes()
    {
        planes.Add(new Plane { Model = "air", Brand = "airbus", SerialNumber = "WN001", Capacity = 130 });
        planes.Add(new Plane { Model = "chery", Brand = "airbus", SerialNumber = "PN002", Capacity = 124 });
    }

    static void LoadLocations()
    {
        locations.Add(new Location { Country = "Turkey", City = "Istanbul", Airport = "IST", IsActive = true });
        locations.Add(new Location { Country = "Turkey", City = "Ankara", Airport = "ANK", IsActive = true });
        locations.Add(new Location { Country = "USA", City = "Miami", Airport = "Miami", IsActive = true });
    }

    static void LoadFlights()
    {
        flights.Add(new Flight
        {
            Location = new Location { Country = "Turkey", City = "Istanbul", Airport = "IST", IsActive = true },
            Time = new DateTime(2024, 2, 29, 12, 0, 0),
            PlaneInfo = planes[0]
        });
        flights.Add(new Flight
        {
            Location = new Location { Country = "Turkey", City = "Ankara", Airport = "ANK", IsActive = true },
            Time = new DateTime(2024, 2, 25, 12, 0, 0),
            PlaneInfo = planes[0]
        });


        flights.Add(new Flight
        {
            Location = new Location { Country = "USA", City = "Miami", Airport = "Miami", IsActive = true },
            Time = new DateTime(2024, 2, 20, 12, 0, 0),
            PlaneInfo = planes[1]
        });
    }
}

