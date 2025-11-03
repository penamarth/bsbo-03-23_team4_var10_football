using System;

namespace Tickets
{
    public abstract class Ticket
    {
        protected decimal price;
        public decimal GetPrice()
        {
            return price;
        }
        public void ChangePrice(decimal price)
        {
            this.price = price;
        }
    }

    public class TicketSingle : Ticket
    {
        private DateTime match_datetime_start;
        private int grandstand_sector;
        private int grandstand_row;
        private int grandstand_seat;

        public TicketSingle(DateTime match_datetime_start, int grandstand_sector, int grandstand_row, int grandstand_seat, decimal price)
        {
            this.match_datetime_start = match_datetime_start;
            this.grandstand_sector = grandstand_sector;
            this.grandstand_row = grandstand_row;
            this.grandstand_seat = grandstand_seat;
            this.price = price;
        }

        public DateTime GetMatchDatetimeStart()
        {
            return match_datetime_start;
        }

        public void ChangeMatchDatetimeStart(DateTime match_datetime_start)
        {
            this.match_datetime_start = match_datetime_start;
        }

        public (int sector, int row, int seat) GetSeat()
        {
            return (grandstand_sector, grandstand_row, grandstand_seat);
        }

        public void ChangeSeat(int sector, int row, int seat)
        {
            grandstand_sector = sector;
            grandstand_row = row;
            grandstand_seat = seat;
        }
    }

    public class TicketSession : Ticket
    {
        private int ticket_session_id;
        private int sector_id;
        private DateTime datetime_start;
        private DateTime datetime_end;

        public TicketSession(int ticket_session_id, int sector_id, DateTime datetime_start, DateTime datetime_end, decimal price)
        {
            this.ticket_session_id = ticket_session_id;
            this.sector_id = sector_id;
            this.datetime_start = datetime_start;
            this.datetime_end = datetime_end;
            this.price = price;
        }

        public decimal GetID()
        {
            return ticket_session_id;
        }

        public int GetSectorID()
        {
            return sector_id;
        }

        public void ChangeSectorID(int sector_id)
        {
            this.sector_id = sector_id;
        }

        public (DateTime start, DateTime end) GetDatetime()
        {
            return (datetime_start, datetime_end);
        }

        public void ChangeDatetime(DateTime start, DateTime end)
        {
            datetime_start = start;
            datetime_end = end;
        }
    }
}
