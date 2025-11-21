using System;

namespace Tickets
{
    internal abstract class Ticket
    {
        protected decimal price;
        
        internal decimal GetPrice()
        {
            return price;
        }
        internal void ChangePrice(decimal price)
        {
            this.price = price;
        }
    }

    internal class TicketSingle : Ticket
    {
        private DateTime match_datetime_start;
        private int grandstand_sector;
        private int grandstand_row;
        private int grandstand_seat;

        internal TicketSingle(DateTime match_datetime_start, int grandstand_sector, int grandstand_row, int grandstand_seat)
        {
            this.match_datetime_start = match_datetime_start;
            this.grandstand_sector = grandstand_sector;
            this.grandstand_row = grandstand_row;
            this.grandstand_seat = grandstand_seat;
        }

        internal DateTime GetMatchDatetimeStart()
        {
            return match_datetime_start;
        }

        internal void ChangeMatchDatetimeStart(DateTime match_datetime_start)
        {
            this.match_datetime_start = match_datetime_start;
        }

        internal (int sector, int row, int seat) GetSeat()
        {
            return (grandstand_sector, grandstand_row, grandstand_seat);
        }

        internal void ChangeSeat(int sector, int row, int seat)
        {
            grandstand_sector = sector;
            grandstand_row = row;
            grandstand_seat = seat;
        }
    }

    internal class TicketSession : Ticket // АБОНЕМЕНТ НИКУДА НЕ ПРИВЯЗЫВАЕТСЯ - ДОПОЛНИТЬ КЛАСС ЛОГ, ТАМ МОЖНО ОБРАЩАТЬСЯ
    {
        private int ticket_session_id;    // КАК БУДЕТ ИНИЦИАЛИЗИРОВАТЬСЯ ID И ПРОВЕРЯТЬСЯ ЕГО УНИКАЛЬНОСТЬ ??? - ПОКА НЕ ЗАОСТРЯЕМ  ВНИМАНИЕ
        private int sector_id;            // БУДЕМ ЛИ МЫ ЧТО-ТО ДЕЛАТЬ С СЕКТОРОМ В ПРЕЦЕДЕНТЕ ПРОДАЖИ ???
        private DateTime datetime_start;
        private DateTime datetime_end;

        internal TicketSession(DateTime datetime_start, DateTime datetime_end, int sector_id)
        {
            this.sector_id = sector_id;
            this.datetime_start = datetime_start;
            this.datetime_end = datetime_end;
        }

        internal decimal GetID()
        {
            return ticket_session_id;
        }

        internal int GetSectorID()
        {
            return sector_id;
        }

        internal void ChangeSectorID(int sector_id)
        {
            this.sector_id = sector_id;
        }

        internal (DateTime start, DateTime end) GetDatetime()
        {
            return (datetime_start, datetime_end);
        }

        internal void ChangeDatetime(DateTime start, DateTime end)
        {
            datetime_start = start;
            datetime_end = end;
        }
    }
}
