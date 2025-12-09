using System;
using System.Collections.Generic;
using System.Linq;
using TicketSpace;

namespace GrandstandSpace
{
    internal interface IGrandstand
    {
        int GetID();
    }

    abstract internal class Grandstand : IGrandstand
    {
        protected List<IGrandstand> children = new List<IGrandstand>();

        protected void CreateChild<T>(int id, Func<int, T> factory, string child_type) where T : IGrandstand
        {
            Console.WriteLine("Вызван метод абстрактного класса Grandstand - CreateChild");

            if (children.Any(c => c.GetID() == id))
                throw new ArgumentException($"ID = {id}: {child_type} с таким ID уже существует");

            children.Add(factory(id));
        }

        protected void ChangeChild<T>(int id, object child, string child_type) where T : IGrandstand
        {
            Console.WriteLine("Вызван метод абстрактного класса Grandstand - ChangeChild");

            if (child is T child_correct)
            {
                var child_exist = children.FirstOrDefault(c => c.GetID() == id);
                if (child_exist != null)
                {
                    int index = children.IndexOf(child_exist);
                    children[index] = child_correct;
                }
                else
                    throw new ArgumentException($"ID = {id}: {child_type} с таким ID не найден");
            }
            else
                throw new ArgumentException($"Дочерний объект должен быть типа {typeof(T).Name}");
        }

        protected object GetChild<T>(int id, string child_type) where T : IGrandstand
        {
            Console.WriteLine("Вызван метод абстрактного класса Grandstand - GetChild");

            foreach (T child in children.OfType<T>())
            {
                if (child.GetID() == id)
                    return child;
            }

            throw new ArgumentException($"ID = {id}: {child_type} с таким ID не найден");
        }

        protected bool GetBooking<T>(List<int> seat_path, string child_type) where T : Grandstand
        {
            Console.WriteLine("Вызван метод абстрактного класса Grandstand - GetBooking");

            if (seat_path.Count == 0)
                throw new ArgumentException("Путь не может быть пустым");

            int id = seat_path[0];

            foreach (T child in children.OfType<T>())
            {
                if (child.GetID() == id)
                {
                    seat_path.RemoveAt(0);
                    return child.GetBooking(seat_path);
                }
            }

            throw new ArgumentException($"ID = {id}: {child_type} с таким ID не найден");
        }

        protected void ChangeBooking<T>(bool booking, List<int> seat_path, string child_type) where T : Grandstand
        {
            Console.WriteLine("Вызван метод абстрактного класса Grandstand - ChangeBooking");

            if (seat_path.Count == 0)
                throw new ArgumentException("Путь не может быть пустым");

            int id = seat_path[0];

            foreach (T child in children.OfType<T>())
            {
                if (child.GetID() == id)
                {
                    seat_path.RemoveAt(0);
                    child.ChangeBooking(booking, seat_path);
                    return;
                }
            }

            throw new ArgumentException($"ID = {id}: {child_type} с таким ID не найден");
        }

        internal abstract void CreateChild(int id);
        internal abstract void ChangeChild(int id, object child);
        internal abstract Object GetChild(int id);
        public abstract int GetID();
        internal abstract bool GetBooking(List<int> seat_path);
        internal abstract void ChangeBooking(bool booking, List<int> seat_path);
        internal abstract Dictionary<string, object> GetSeatsBooking();
    }

    internal class Match : Grandstand
    {
        private DateTime datetime_start;
        private DateTime datetime_end;   // НИГДЕ НЕ НАЗНАЧАЕТСЯ - ОСТАВЛЯЕМ В ТАКОМ ВИДЕ ИЛИ НЕТ ???
        private string team_first;
        private string team_second;
        private List<TicketSingle> tickets;

        internal Match(DateTime datetime_start, string team_first, string team_second)
        {
            Console.WriteLine(
                "Вызван конструктор класса Match: DateTime datetime_start, string team_first, string team_second"
            );

            this.datetime_start = datetime_start;
            this.team_first = team_first;
            this.team_second = team_second;
            tickets = new List<TicketSingle>();
        }

        internal override void CreateChild(int id)
        {
            Console.WriteLine("Вызван метод класса Match - CreateChild");

            CreateChild<Sector>(id, (sector_id) => new Sector(sector_id, false), "Sector");
        }

        internal override void ChangeChild(int id, object child)
        {
            Console.WriteLine("Вызван метод класса Match - ChangeChild");

            ChangeChild<Sector>(id, child, "Sector");
        }

        internal override Object GetChild(int id)
        {
            Console.WriteLine("Вызван метод класса Match - GetChild");

            return GetChild<Sector>(id, "Sector");
        }

        public override int GetID()
        {
            Console.WriteLine("Вызван метод класса Match - GetID");

            throw new NotImplementedException("Match использует дату своего начала в качестве ID");
        }

        internal DateTime GetDatetimeStart()
        {
            Console.WriteLine("Вызван метод класса Match - GetDatetimeStart");

            return datetime_start;
        }

        internal void ChangeDatetime(DateTime datetime_start, DateTime datetime_end)
        {
            Console.WriteLine("Вызван метод класса Match - ChangeDatetime");

            this.datetime_start = datetime_start;
            this.datetime_end = datetime_end;
        }

        internal (string team_first, string team_second) GetTeams()
        {
            Console.WriteLine("Вызван метод класса Match - GetTeams");

            return (team_first, team_second);
        }

        internal void ChangeTeams(string team_first, string team_second)
        {
            Console.WriteLine("Вызван метод класса Match - ChangeTeams");

            this.team_first = team_first;
            this.team_second = team_second;
        }

        internal void SaveTicket(TicketSingle ticket_object)
        {
            Console.WriteLine("Вызван метод класса Match - SaveTicket");

            tickets.Add(ticket_object);
        }

        internal override bool GetBooking(List<int> seat_path)
        {
            Console.WriteLine("Вызван метод класса Match - GetBooking");

            return GetBooking<Sector>(seat_path, "Sector");
        }

        internal override void ChangeBooking(bool booking, List<int> seat_path)
        {
            Console.WriteLine("Вызван метод класса Match - ChangeBooking");

            ChangeBooking<Sector>(booking, seat_path, "Sector");
        }

        internal override Dictionary<string, object> GetSeatsBooking()
        {
            Console.WriteLine("Вызван метод класса Match - GetSeatsBooking");

            Dictionary<int, object> sectors_rows_seats_values = new Dictionary<int, object>();

            foreach (Sector sector in children.OfType<Sector>())
                sectors_rows_seats_values.Add(sector.GetID(), sector.GetSeatsBooking());

            Dictionary<string, object> sectors_rows_seats_id_booking = new Dictionary<string, object>
            {
                ["datetime_start"] = datetime_start,
                ["sectors"] = sectors_rows_seats_values
            };

            return sectors_rows_seats_id_booking;
        }
    }

    internal class Sector : Grandstand
    {
        private int sector_id;
        private bool ticket_session_only;

        internal Sector(int sector_id, bool ticket_session_only)
        {
            Console.WriteLine("Вызван конструктор класса Sector: int sector_id, bool ticket_session_only");

            this.sector_id = sector_id;
            this.ticket_session_only = ticket_session_only;
        }

        public override int GetID()
        {
            Console.WriteLine("Вызван метод класса Sector - GetID");

            return sector_id;
        }

        internal bool GetTicketSessionOnlyStatus()
        {
            Console.WriteLine("Вызван метод класса Sector - GetTicketSessionStatus");

            return ticket_session_only;
        }

        internal void ChangeTicketSessionOnlyStatus(bool ticket_session_only)
        {
            Console.WriteLine("Вызван метод класса Sector - ChangeTicketSessionStatus");

            this.ticket_session_only = ticket_session_only;
        }

        internal override void CreateChild(int id)
        {
            Console.WriteLine("Вызван метод класса Sector - CreateChild");

            CreateChild<Row>(id, (row_id) => new Row(row_id), "Row");
        }

        internal override void ChangeChild(int id, object child)
        {
            Console.WriteLine("Вызван метод класса Sector - ChangeChild");

            ChangeChild<Row>(id, child, "Row");
        }

        internal override Object GetChild(int id)
        {
            Console.WriteLine("Вызван метод класса Sector - GetChild");
            
            return GetChild<Row>(id, "Row");
        }

        internal override bool GetBooking(List<int> seat_path)
        {
            Console.WriteLine("Вызван метод класса Sector - GetBooking");

            return GetBooking<Row>(seat_path, "Row");
        }

        internal override void ChangeBooking(bool booking, List<int> seat_path)
        {
            Console.WriteLine("Вызван метод класса Sector - ChangeBooking");

            ChangeBooking<Row>(booking, seat_path, "Row");
        }

        internal override Dictionary<string, object> GetSeatsBooking()
        {
            Console.WriteLine("Вызван метод класса Sector - GetSeatsBooking");

            Dictionary<int, object> rows_seats_values = new Dictionary<int, object>();

            foreach (Row row in children.OfType<Row>())
                rows_seats_values.Add(row.GetID(), row.GetSeatsBooking());

            Dictionary<string, object> seats_id_booking = new Dictionary<string, object>
            {
                ["ticket_session_only"] = ticket_session_only,
                ["rows"] = rows_seats_values
            };

            return seats_id_booking;
        }
    }

    internal class Row : Grandstand
    {
        private int row_id;

        internal Row(int row_id)
        {
            Console.WriteLine("Вызван конструктор класса Row: int row_id");

            this.row_id = row_id;
        }

        public override int GetID()
        {
            Console.WriteLine("Вызван метод класса Row - GetID");

            return row_id;
        }

        internal override void CreateChild(int id)
        {
            Console.WriteLine("Вызван метод класса Row - CreateChild");

            CreateChild<Seat>(id, (seatId) => new Seat(seatId), "Seat");
        }

        internal override void ChangeChild(int id, object child)
        {
            Console.WriteLine("Вызван метод класса Row - ChangeChild");

            ChangeChild<Seat>(id, child, "Seat");
        }

        internal override Object GetChild(int id)
        {
            Console.WriteLine("Вызван метод класса Row - GetChild");

            return GetChild<Seat>(id, "Seat");
        }

        internal override bool GetBooking(List<int> seat_path)
        {
            Console.WriteLine("Вызван метод класса Row - GetBooking");

            if (seat_path.Count != 1)
                throw new ArgumentException("Для Row путь должен содержать только ID места");

            int id = seat_path[0];

            foreach (Seat seat in children)
            {
                if (seat.GetID() == seat_path[0])
                    return seat.GetBooking();
            }

            throw new ArgumentException($"ID = {id}: место с таким ID не найдено");
        }

        internal override void ChangeBooking(bool booking, List<int> seat_path)
        {
            Console.WriteLine("Вызван метод класса Row - ChangeBooking");

            if (seat_path.Count != 1)
                throw new ArgumentException("Для Row путь должен содержать только ID места");

            int id = seat_path[0];

            foreach (Seat seat in children)
            {
                if (seat.GetID() == id)
                {
                    seat.ChangeBooking(booking);
                    return;
                }
            }

            throw new ArgumentException($"ID = {id}: место с таким ID не найдено");
        }

        internal override Dictionary<string, object> GetSeatsBooking()
        {
            Console.WriteLine("Вызван метод класса Row - GetSeatsBooking");

            Dictionary<int, bool> seats_values = new Dictionary<int, bool>();

            foreach (Seat seat in children.OfType<Seat>())
                seats_values.Add(seat.GetID(), seat.GetBooking());

            Dictionary<string, object> seats_id_booking = new Dictionary<string, object>
            {
                ["seats"] = seats_values
            };

            return seats_id_booking;
        }
    }
    internal class Seat : IGrandstand
    {
        private int seat_id;
        private bool booking;

        internal Seat(int seat_id)
        {
            Console.WriteLine("Вызван конструктор класса Seat: int seat_id");

            this.seat_id = seat_id;
            booking = false;
        }

        public int GetID()
        {
            Console.WriteLine("Вызван метод класса Seat - GetID");

            return seat_id;
        }

        internal bool GetBooking()
        {
            Console.WriteLine("Вызван метод класса Seat - GetBooking");

            return booking;
        }

        internal void ChangeBooking(bool booking)
        {
            Console.WriteLine("Вызван метод класса Seat - ChangeBooking");

            this.booking = booking;
        }
    }
}
