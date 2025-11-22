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
        internal DateTime GetDatetime()
        {
            return datetime;
        }
        internal List<Ticket> GetTickets()
        {
            return tickets;
        }

        internal void CreateTicketSingle(DateTime match_datetime_start, int grandstand_sector, int grandstand_row, int grandstand_seat)
        {
            tickets.Add(new TicketSingle(match_datetime_start, grandstand_sector, grandstand_row, grandstand_seat));
        }

        internal void CreateTicketSession(DateTime datetime_start, DateTime datetime_end, int sector_id)
        {
            tickets.Add(new TicketSession(datetime_start, datetime_end, sector_id));
        }

        internal decimal MakePrice() // ЦЕНА КАЖДОГО БИЛЕТА ЕЩЁ НЕ УСТАНОВЛЕНА - ВЕРОЯТНО, СДЕЛАТЬ УСТАНОВКУ ЗДЕСЬ ПО УСЛОВНЫМ ПАРАМЕТРАМ (НОМЕР СЕКТОРА И ПРОЧЕЕ)
        {
            decimal price = 0;

            // В СХЕМЕ УКАЗАНО, ЧТО CalcPrice ВЫЗЫВАЕТСЯ КАЖДЫЙ РАЗ В ЦИКЛЕ
            // БУДЕТ ЛИ ЦИКЛ ПОДСЧЁТА ВНУТРИ ЭТОГО МЕТОДА ИЛИ ЦИКЛ БУДЕТ ВЫЗЫВАТЬ ЕГО КАЖДЫЙ РАЗ ???
            full_price = CalcPrice(price);

            return full_price;
        }

        internal void MakePayment(decimal cash) // ВОЗВРАЩЕНИЕ ЧЕКА О ПРОДАЖЕ БИЛЕТОВ ??? - ЛИБО VIEW-КОНТРОЛЛЕР (КЛАСС ТОЛЬКО С ОТКРЫТЫМИ СВОЙСТВАМИ), ЛИБО JSON|МАССИВ
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
            log.MakeLogSale(this);
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
            // В БУДУЩЕМ ПОДКЛЮЧИТЬ ПАТТЕРН АДАПТЕР (YOMANY) - СДЕЛАТЬ ЗАГЛУШКУ ВМЕСТО ВНЕШНЕЙ СИСТЕМЫ
        }
    }

    internal class Log
    {
        private List<Sale> sales_list;

        internal Log()
        {
            sales_list = new List<Sale>();
        }

        private List<TicketSingle> GetAllTicketSingles()
        {
            return sales_list.SelectMany(sale => sale.GetTickets()).OfType<TicketSingle>().ToList();
        }

        private List<TicketSession> GetAllTicketSessions()
        {
            return sales_list.SelectMany(sale => sale.GetTickets()).OfType<TicketSession>().ToList();
        }

        internal TicketSingle GetTicketSingle(DateTime matchDate, int sector_id, int row_id, int seat_id)
        {
            TicketSingle ticket_exist = GetAllTicketSingles().FirstOrDefault(ticket =>
                {
                    (int sector, int row, int seat) = ticket.GetSeat();

                    return ticket.GetMatchDatetimeStart() == matchDate &&
                           sector == sector_id &&
                           row == row_id &&
                           seat == seat_id;
                }
            );

            return ticket_exist 
                ??
                throw new ArgumentException(
                    "Билет не найден. Введённые параметры:\n" +
                    $"- начало матча = {matchDate};\n" +
                    $"- сектор = {sector_id};\n" +
                    $"- ряд = {row_id};\n" +
                    $"- место = {seat_id}"
                );
        }

        internal TicketSession GetTicketSession(int ticket_session_id)
        {
            TicketSession ticket_exist = GetAllTicketSessions().FirstOrDefault(ticket =>
                ticket.GetID() == ticket_session_id
            );

            return ticket_exist
                ??
                throw new ArgumentException(
                    "Абонемент не найден. Введённые параметры:\n" +
                    $"- ID = {ticket_session_id}"
                );
        }

        internal TicketSession GetTicketSession(DateTime datetime_start, DateTime datetime_end)
        {
            TicketSession ticket_exist = GetAllTicketSessions().FirstOrDefault(ticket => 
                ticket.GetDatetime() == (datetime_start, datetime_end)
            );

            return ticket_exist
                ??
                throw new ArgumentException(
                    "Абонемент не найден. Введённые параметры:\n" +
                    $"- начало действия = {datetime_start};\n" +
                    $"- конец действия = {datetime_end}"
                );
        }

        internal (int total_sale, int total_ticket_single, int total_ticket_session, decimal total_full_price) GetTotalStats()
        {
            int total_sale = sales_list.Count;
            int total_ticket_single = GetAllTicketSingles().Count;
            int total_ticket_session = GetAllTicketSessions().Count;
            decimal total_full_price = sales_list.Sum(sale => sale.full_price);

            return (total_sale, total_ticket_single, total_ticket_session, total_full_price);
        }

        internal Sale GetSale(DateTime datetime)
        {
            return (Sale)sales_list.Where(sale => sale.GetDatetime() == datetime);
        }

        internal void MakeLogSale(Sale sale)
        {
            sales_list.Add(sale);
        }
    }
}
