using System;
using System.Collections.Generic;

namespace TicketSpace
{
    internal abstract class Ticket
    {
        private static readonly Random random_object = new Random();
        private static readonly HashSet<int> ticket_id_list = new HashSet<int>();
        protected readonly int ticket_id;
        protected decimal price;

        protected Ticket()
        {
            Console.WriteLine("Вызван конструктор абстрактного класса Ticket");

            ticket_id = GenerateID();
        }

        private static int GenerateID()
        {
            Console.WriteLine("Вызван метод абстрактного класса Ticket - GenerateID");

            int id;

            do
            {
                id = random_object.Next(1, 1000000);
            }
            while (ticket_id_list.Contains(id));

            ticket_id_list.Add(id);

            return id;
        }

        internal decimal GetID()
        {
            Console.WriteLine("Вызван метод абстрактного класса Ticket - GetID");

            return ticket_id;
        }

        internal static void ReleaseID(int ticket_id)
        {
            Console.WriteLine("Вызван метод абстрактного класса Ticket - ReleaseID");

            ticket_id_list.Remove(ticket_id);
        }

        internal decimal GetPrice()
        {
            Console.WriteLine("Вызван метод абстрактного класса Ticket - GetPrice");

            return price;
        }

        internal void ChangePrice(decimal price)
        {
            Console.WriteLine("Вызван метод абстрактного класса Ticket - ChangePrice");

            this.price = price;
        }
    }

    internal class TicketSingle : Ticket
    {
        private DateTime match_datetime_start;
        private int grandstand_sector;
        private int grandstand_row;
        private int grandstand_seat;

        internal TicketSingle(DateTime match_datetime_start, int grandstand_sector, int grandstand_row, int grandstand_seat) : base()
        {
            Console.WriteLine(
                "Вызван конструктор класса TicketSingle: DateTime match_datetime_start, int grandstand_sector, int grandstand_row, int grandstand_seat"
            );

            this.match_datetime_start = match_datetime_start;
            this.grandstand_sector = grandstand_sector;
            this.grandstand_row = grandstand_row;
            this.grandstand_seat = grandstand_seat;
        }

        internal DateTime GetMatchDatetimeStart()
        {
            Console.WriteLine("Вызван метод класса TicketSingle - GetMatchDatetimeStart");

            return match_datetime_start;
        }

        internal void ChangeMatchDatetimeStart(DateTime match_datetime_start)
        {
            Console.WriteLine("Вызван метод класса TicketSingle - ChangeMatchDatetimeStart");

            this.match_datetime_start = match_datetime_start;
        }

        internal (int sector, int row, int seat) GetSeat()
        {
            Console.WriteLine("Вызван метод класса TicketSingle - GetSeat");

            return (grandstand_sector, grandstand_row, grandstand_seat);
        }

        internal void ChangeSeat(int sector, int row, int seat)
        {
            Console.WriteLine("Вызван метод класса TicketSingle - ChangeSeat");

            grandstand_sector = sector;
            grandstand_row = row;
            grandstand_seat = seat;
        }
    }

    internal class TicketSession : Ticket
    {
        private int sector_id;
        private DateTime datetime_start;
        private DateTime datetime_end;

        internal TicketSession(DateTime datetime_start, DateTime datetime_end, int sector_id) : base()
        {
            Console.WriteLine(
                "Вызван конструктор класса TicketSession: DateTime datetime_start, DateTime datetime_end, int sector_id"
            );

            this.datetime_start = datetime_start;
            this.datetime_end = datetime_end;
            this.sector_id = sector_id;
        }

        internal int GetSectorID()
        {
            Console.WriteLine("Вызван метод класса TicketSession - GetSectorID");

            return sector_id;
        }

        internal void ChangeSectorID(int sector_id)
        {
            Console.WriteLine("Вызван метод класса TicketSession - ChangeSectorID");

            this.sector_id = sector_id;
        }

        internal (DateTime start, DateTime end) GetDatetime()
        {
            Console.WriteLine("Вызван метод класса TicketSession - GetDatetime");

            return (datetime_start, datetime_end);
        }

        internal void ChangeDatetime(DateTime start, DateTime end)
        {
            Console.WriteLine("Вызван метод класса TicketSession - ChangeDatetime");

            datetime_start = start;
            datetime_end = end;
        }
    }
}
