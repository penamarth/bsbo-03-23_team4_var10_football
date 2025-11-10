using System;
using System.Collections.Generic;
using System.Linq;
using StadiumStructure;
using Tickets;

namespace Sales
{
    internal class Sale
    {
        internal readonly string type;
        private DateTime datetime;
        internal decimal full_price { get; private set; } // ПОЛЕ ПРЕВРАТИЛОСЬ В СВОЙСТВО - ОСТАВЛЯЕМ ИЛИ МЕНЯЕМ НА НОРМАЛЬНОЕ ПОЛЕ ???
        private List<Ticket> tickets;

        internal Sale(string type)
        {
            this.type = type;
            datetime = DateTime.Now;
            tickets = new List<Ticket>();
            full_price = 0;
        }

        private decimal CalcPrice(decimal price)
        {
            // ЛИБО ОБЩИЙ ПОДСЧЁТ УСТАНОВЛЕННОЙ ЦЕНЫ (текущий код), ЛИБО НАЗНАЧЕНИЕ ЦЕНЫ В ЗАВИСИМОСТИ ОТ ТИПА БИЛЕТА/СЕКТОРА/РЯДА
            return tickets.Sum(ticket => ticket.GetPrice());
        }

        internal void CreateTicketSingle(DateTime match_datetime_start, int grandstand_sector, int grandstand_row, int grandstand_seat)
        {
            tickets.Add(new TicketSingle(match_datetime_start, grandstand_sector, grandstand_row, grandstand_seat));
        }

        internal void CreateTicketSession(DateTime datetime_start, DateTime datetime_end, int sector_id)
        {
            tickets.Add(new TicketSession(datetime_start, datetime_end, sector_id));
        }

        internal decimal MakePrice() // ЦЕНА КАЖДОГО БИЛЕТА ЕЩЁ НЕ УСТАНОВЛЕНА 
        {
            decimal price = 0;

            // В СХЕМЕ УКАЗАНО, ЧТО CalcPrice ВЫЗЫВАЕТСЯ КАЖДЫЙ РАЗ В ЦИКЛЕ
            // БУДЕТ ЛИ ЦИКЛ ПОДСЧЁТА ВНУТРИ ЭТОГО МЕТОДА ИЛИ ЦИКЛ БУДЕТ ВЫЗЫВАТЬ ЕГО КАЖДЫЙ РАЗ ???
            full_price = CalcPrice(price);

            return full_price;
        }

        internal void MakePayment(decimal cash) // ВОЗВРАЩЕНИЕ ЧЕКА О ПРОДАЖЕ БИЛЕТОВ ???
        {
            if (cash < full_price)
                throw new InvalidOperationException($"Недостаточно средств для оплаты. Цена продажи: {full_price}, внесено: {cash}");

            Payment payment = new Payment();
            payment.MakePayment(cash);
        }

        internal void ChangeBooking(Stadium stadium)
        {
            foreach (TicketSingle ticket in tickets.Cast<TicketSingle>())
            {
                DateTime ticket_datetime = ticket.GetMatchDatetimeStart();

                Match match = 
                    stadium.GetMatch(ticket_datetime) 
                    ?? 
                    throw new InvalidOperationException(
                        $"Отсутствие матча с датой и временем начала {ticket_datetime}: обратитесь к организатору для создания записи о данном матче"
                    );
                
                (int, int, int) seat = ticket.GetSeat();

                match.ChangeBooking(true, new List<int> { seat.Item1, seat.Item2, seat.Item3 });
                match.SaveTicket(ticket);
            } 
        }

        internal void MakeLogSale(Log log)
        {
            log.MakeLogSale(datetime, full_price, tickets);
        }
    }

    internal class Payment
    {
        internal Payment()
        {
            // КАКИЕ ПОЛЯ У ОПЛАТЫ ???
        }

        internal void MakePayment(decimal cash) // ВОЗВРАЩЕНИЕ ЧЕКА О ПРОДАЖЕ БИЛЕТОВ ???
        {
            // КАК БУДЕТ ОСУЩЕСТВЛЯТЬСЯ ОПЛАТА ???
        }
    }

    internal class Log
    {
        private List<(DateTime, decimal, List<Ticket>)> sale_operations;

        internal Log()
        {
            sale_operations = new List<(DateTime, decimal, List<Ticket>)>();
        }

        internal void MakeLogSale(DateTime datetime, decimal full_price, List<Ticket> tickets)
        { 
            sale_operations.Add((datetime, full_price, tickets));
        }
    }
}
