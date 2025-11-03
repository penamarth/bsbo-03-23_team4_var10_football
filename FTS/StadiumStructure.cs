using System;
using System.Collections.Generic;
using System.Linq;
using Tickets;
using Sales;

namespace StadiumStructure
{
    internal interface GrandstandParams
    {
        int GetID();
    }

    abstract internal class Grandstand : GrandstandParams
    {
        protected List<GrandstandParams> children = new List<GrandstandParams>();

        protected void CreateChild<T>(int id, Func<int, T> factory, string child_type) where T : GrandstandParams
        {
            if (children.Any(c => c.GetID() == id))
                throw new ArgumentException($"ID = {id}: {child_type} с таким ID уже существует");

            children.Add(factory(id));
        }

        protected void ChangeChild<T>(int id, object child, string child_type) where T : GrandstandParams
        {
            if (child is T child_correct)
            {
                var child_exist = children.FirstOrDefault(c => c.GetID() == id);
                if (child_exist != null)
                {
                    int index = children.IndexOf(child_exist);
                    children[index] = child_correct;
                }
                else
                {
                    throw new ArgumentException($"ID = {id}: {child_type} с таким ID не найден");
                }
            }
            else
            {
                throw new ArgumentException($"Дочерний объект должен быть типа {typeof(T).Name}");
            }
        }

        protected object GetChild<T>(int id, string child_type) where T : GrandstandParams
        {
            foreach (T child in children.OfType<T>())
            {
                if (child.GetID() == id)
                    return child;
            }

            throw new ArgumentException($"ID = {id}: {child_type} с таким ID не найден");
        }

        protected bool GetBooking<T>(List<int> seat_path, string child_type) where T : Grandstand
        {
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

        public abstract void CreateChild(int id);
        public abstract void ChangeChild(int id, object child);
        public abstract Object GetChild(int id);
        public abstract int GetID();
        public abstract bool GetBooking(List<int> seat_path);
        public abstract void ChangeBooking(bool booking, List<int> seat_path);
        public abstract void GetSeatsBooking(); // нигде пока не расписан
    }

    internal class Stadium
    {
        private List<Match> matches;
        public Stadium()
        {
            matches = new List<Match>();
        }

        private ВОЗВРАЩАЕМЫЙ_ТИП??? GenerateMatchReport(DateTime datetime_start, string team_first, string team_second, sectors_rows_seats_id[])
        {

        }

        public void Autharization(byte user_id)
        {

        }

        public void CreateSale(type)
        {

        }

        public void CloseSale()
        {

        }

        public void MakePrice()
        {

        }

        public void MakePayment(cash)
        {

        }

        public void CreateTicket(DateTime match_datetime_start, grandstand_sector, grandstand_row, grandstand_seat)
        {

        }

        public void CreateTicketSession(DateTime datetime_start, DateTime datetime_end, int sector_id)
        {

        }

        public void CreateMatch(DateTime datetime_start, string team_first, string team_second, sectors_rows_seats_id[])
        {

        }

        public void SelectMatchSeats(int matches_id)
        {

        }

        public void OpenEditingMatch()
        {

        }

        public void CloseEditingMatch()
        {

        }

        public void Quit()
        {

        }
    }

    internal class Match : Grandstand
    {
        private DateTime datetime_start;
        private DateTime datetime_end;
        private string team_first;
        private string team_second;
        private List<TicketSingle> tickets;

        public Match(DateTime datetime_start, string team_first, string team_second)
        {
            this.datetime_start = datetime_start;
            this.team_first = team_first;
            this.team_second = team_second;
            tickets = new List<TicketSingle>();
        }

        public override void CreateChild(int id)
        {
            CreateChild<Sector>(id, (sector_id) => new Sector(sector_id, false), "Sector");
        }

        public override void ChangeChild(int id, object child)
        {
            ChangeChild<Sector>(id, child, "Sector");
        }

        public override Object GetChild(int id)
        {
            return GetChild<Sector>(id, "Sector");
        }

        public override int GetID()
        {
            throw new NotImplementedException("Match использует дату своего начала в качестве ID");
        }

        public DateTime GetDatetimeStart()
        {
            return datetime_start;
        }

        public void ChangeDatetime(DateTime datetime_start, DateTime datetime_end)
        {
            this.datetime_start = datetime_start;
            this.datetime_end = datetime_end;
        }

        public void ChangeTeams(string team_first, string team_second)
        {
            this.team_first = team_first;
            this.team_second = team_second;
        }

        public void SaveTicket(TicketSingle ticket_object)
        {
            tickets.Add(ticket_object);
        }

        public override bool GetBooking(List<int> seat_path)
        {
            return GetBooking<Sector>(seat_path, "Sector");
        }

        public override void ChangeBooking(bool booking, List<int> seat_path)
        {
            ChangeBooking<Sector>(booking, seat_path, "Sector");
        }
    }

    internal class Sector : Grandstand
    {
        private int sector_id;
        private bool ticket_session_only;

        public Sector(int sector_id, bool ticket_session_only)
        {
            this.sector_id = sector_id;
            this.ticket_session_only = ticket_session_only;
        }

        public override int GetID()
        {
            return sector_id;
        }

        public override void CreateChild(int id)
        {
            CreateChild<Row>(id, (row_id) => new Row(row_id), "Row");
        }

        public override void ChangeChild(int id, object child)
        {
            ChangeChild<Row>(id, child, "Row");
        }

        public override Object GetChild(int id)
        {
            return GetChild<Row>(id, "Row");
        }

        public override bool GetBooking(List<int> seat_path)
        {
            return GetBooking<Row>(seat_path, "Row");
        }

        public override void ChangeBooking(bool booking, List<int> seat_path)
        {
            ChangeBooking<Row>(booking, seat_path, "Row");
        }
    }

    internal class Row : Grandstand
    {
        private int row_id;

        public Row(int row_id)
        {
            this.row_id = row_id;
        }

        public override int GetID()
        {
            return row_id;
        }

        public override void CreateChild(int id)
        {
            CreateChild<Seat>(id, (seatId) => new Seat(seatId), "Seat");
        }

        public override void ChangeChild(int id, object child)
        {
            ChangeChild<Seat>(id, child, "Seat");
        }

        public override Object GetChild(int id)
        {
            return GetChild<Seat>(id, "Seat");
        }

        public override bool GetBooking(List<int> seat_path)
        {
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

        public override void ChangeBooking(bool booking, List<int> seat_path)
        {
            if (seat_path.Count != 1)
                throw new ArgumentException("Для Row путь должен содержать только ID места");

            int id = seat_path[0];

            foreach (Seat seat in children)
            {
                if (seat.GetID() == id)
                    seat.ChangeBooking(booking);
            }

            throw new ArgumentException($"ID = {id}: место с таким ID не найдено");
        }
    }
    internal class Seat : GrandstandParams
    {
        private int seat_id;
        private bool booking;

        public Seat(int seat_id)
        {
            this.seat_id = seat_id;
            booking = false;
        }

        public int GetID()
        {
            return seat_id;
        }

        public bool GetBooking()
        {
            return booking;
        }

        public void ChangeBooking(bool booking)
        {
            this.booking = booking;
        }
    }
}
