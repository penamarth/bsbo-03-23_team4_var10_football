using System;
using System.Collections.Generic;
using System.Linq;
using StadiumSpace;
using GrandstandSpace;
using TicketSpace;

namespace SaleSpace
{
    internal interface IPriceStrategy
    {
        decimal CalculatePrice(Ticket ticket_object);
    }

    internal class TicketSinglePriceStrategy : IPriceStrategy
    {
        internal TicketSinglePriceStrategy()
        {
            Console.WriteLine("Вызван конструктор класса TicketSinglePriceStrategy без параметров");
        }

        public decimal CalculatePrice(Ticket ticket_object)
        {
            Console.WriteLine("Вызван метод TicketSinglePriceStrategy - CalculatePrice");

            if (ticket_object is TicketSingle ticket_single)
            {
                (int sector_id, int row_id, int seat_id) = ticket_single.GetSeat();

                decimal price_base = 100;
                decimal price_coef_sector = GetTicketPriceCoefSector(sector_id);
                decimal price_coef_row = GetTicketPriceCoefRow(row_id);
                decimal price_coef_seat = GetTicketPriceCoefSeat(seat_id);

                decimal ticket_price = price_base * price_coef_sector * price_coef_row * price_coef_seat;
                return ticket_price;
            }

            throw new ArgumentException("Данная стратегия предназначена только для обычных билетов");
        }

        private decimal GetTicketPriceCoefSector(int sector_id)
        {
            Console.WriteLine("Вызван метод класса Sale - GetTicketPriceCoefSector");

            if ((1 <= sector_id) && (sector_id <= 4))  return 1.5m; // Близкие сектора
            if ((5 <= sector_id) && (sector_id <= 8))  return 1.2m; // Средние сектора
            if ((9 <= sector_id) && (sector_id <= 12)) return 1.2m; // Дальние сектора

            throw new ArgumentException($"Сектор с ID = {sector_id} не существует");
        }

        private decimal GetTicketPriceCoefRow(int row_id)
        {
            Console.WriteLine("Вызван метод класса Sale - GetTicketPriceCoefRow");

            if ((1 <= row_id) && (row_id <= 3)) return 1.5m; // Близкие ряды
            if ((4 <= row_id) && (row_id <= 6)) return 1.2m; // Средние ряды
            if ((7 <= row_id) && (row_id <= 9)) return 1.2m; // Дальние ряды

            throw new ArgumentException($"Ряд с ID = {row_id} не существует");
        }
        private decimal GetTicketPriceCoefSeat(int seat_id)
        {
            Console.WriteLine("Вызван метод класса Sale - GetTicketPriceCoefSeat");

            if ((5 <= seat_id) && (seat_id <= 7))  return 1.2m;  // центральные места
            if ((1 <= seat_id) && (seat_id <= 10)) return 1.0m;  // места по краям

            throw new ArgumentException($"Место с ID = {seat_id} не существует");
        }
    }

    internal class TicketSessionPriceStrategy : IPriceStrategy
    {
        internal TicketSessionPriceStrategy()
        {
            Console.WriteLine("Вызван конструктор класса TicketSessionPriceStrategy без параметров");
        }

        public decimal CalculatePrice(Ticket ticket_object)
        {
            Console.WriteLine("Вызван метод TicketSessionPriceStrategy - CalculatePrice");

            if (ticket_object is TicketSession ticketSession)
            {
                (DateTime datetime_start, DateTime datetime_end) = ticketSession.GetDatetime();

                int days = (datetime_end - datetime_start).Days;
                decimal price_base = 300;

                decimal ticket_price = price_base * days;
                return ticket_price;
            }

            throw new ArgumentException("Данная стратегия предназначена только для абонементов");
        }
    }

    internal class Sale
    {
        internal readonly string type;
        private IPriceStrategy price_strategy_object;
        private DateTime datetime;
        internal decimal FullPrice { get; private set; }
        private List<Ticket> tickets;

        internal Sale(string type, IPriceStrategy price_strategy_object)
        {
            Console.WriteLine("Вызван конструктор класса Sale: string type, IPriceStrategy price_strategy_object");

            this.type = type;
            this.price_strategy_object = price_strategy_object;
            datetime = DateTime.Now;
            tickets = new List<Ticket>();
            FullPrice = 0;
        }

        private Dictionary<string, object> GetReceiptData()
        {
            Dictionary<string, object> receipt = new Dictionary<string, object>
            {
                ["sale_datetime"] = datetime,
                ["sale_type"] = type,
                ["total_price"] = FullPrice,
                ["tickets_count"] = tickets.Count,
                ["tickets"] = tickets.Select(ticket => GetTicketData(ticket)).ToList()
            };

            return receipt;
        }

        private Dictionary<string, object> GetTicketData(Ticket ticket_object)
        {
            Dictionary<string, object> ticket_data = new Dictionary<string, object>
            {
                ["ticket_type"] = ticket_object.GetType().Name,
                ["price"] = ticket_object.GetPrice()
            };

            if (ticket_object is TicketSingle ticket_single)
            {
                (int sector, int row, int seat) = ticket_single.GetSeat();
                ticket_data["match_datetime"] = ticket_single.GetMatchDatetimeStart();
                ticket_data["sector"] = sector;
                ticket_data["row"] = row;
                ticket_data["seat"] = seat;
            }

            else if (ticket_object is TicketSession ticket_session)
            {
                (DateTime datetime_start, DateTime datetime_end) = ticket_session.GetDatetime();
                ticket_data["datetime_start"] = datetime_start;
                ticket_data["datetime_end"] = datetime_end;
                ticket_data["sector_id"] = ticket_session.GetSectorID();
            }

            return ticket_data;
        }

        internal DateTime GetDatetime()
        {
            Console.WriteLine("Вызван метод класса Sale - GetDatetime");

            return datetime;
        }
        internal List<Ticket> GetTickets()
        {
            Console.WriteLine("Вызван метод класса Sale - GetTickets");

            return tickets;
        }

        internal void CreateTicketSingle(DateTime match_datetime_start, int grandstand_sector, int grandstand_row, int grandstand_seat)
        {
            Console.WriteLine("Вызван метод класса Sale - CreateTicketSingle");

            tickets.Add(new TicketSingle(match_datetime_start, grandstand_sector, grandstand_row, grandstand_seat));
        }

        internal void CreateTicketSession(DateTime datetime_start, DateTime datetime_end, int sector_id)
        {
            Console.WriteLine("Вызван метод класса Sale - CreateTicketSession");

            tickets.Add(new TicketSession(datetime_start, datetime_end, sector_id));
        }

        internal void SetPriceStrategy(IPriceStrategy price_strategy_object)
        {
            Console.WriteLine("Вызван метод класса Sale - SetPriceStrategy");

            this.price_strategy_object = price_strategy_object;
        }

        internal decimal MakePrice()
        {
            Console.WriteLine("Вызван метод класса Sale - MakePrice");

            foreach (Ticket ticket in tickets)
            {
                decimal ticket_price = price_strategy_object.CalculatePrice(ticket);
                ticket.ChangePrice(ticket_price);
                FullPrice += ticket_price;
            }

            return FullPrice;
        }

        internal Dictionary<string, object> MakePayment(decimal cash)
        {
            Console.WriteLine("Вызван метод класса Sale - MakePayment");

            if (cash < FullPrice)
                throw new InvalidOperationException(
                    $"Недостаточно средств для оплаты. Цена продажи: {FullPrice}, внесено: {cash}"
                );

            Payment payment = new Payment();
            Dictionary<string, object> payment_result = payment.MakePayment(cash);
            Dictionary<string, object> receipt = GetReceiptData();

            decimal change = cash - FullPrice;
            payment_result.Add("change", change);

            receipt["payment_info"] = payment_result;

            return receipt;
        }

        internal void ChangeBooking(Stadium stadium)
        {
            Console.WriteLine("Вызван метод класса Sale - ChangeBooking");

            foreach (TicketSingle ticket in tickets.Cast<TicketSingle>())
            {
                DateTime ticket_datetime = ticket.GetMatchDatetimeStart();

                Match match = 
                    stadium.GetMatch(ticket_datetime) 
                    ?? 
                    throw new InvalidOperationException(
                        $"Отсутствие матча с датой и временем начала {ticket_datetime}: " +
                        $"обратитесь к организатору для создания записи о данном матче"
                    );
                
                (int, int, int) seat = ticket.GetSeat();

                match.ChangeBooking(true, new List<int> { seat.Item1, seat.Item2, seat.Item3 });
                match.SaveTicket(ticket);
            } 
        }

        internal void MakeLogSale(Log log)
        {
            Console.WriteLine("Вызван метод класса Sale - MakeLogSale");

            log.MakeLogSale(this);
        }
    }

    internal interface IPayment
    {
        Dictionary<string, object> ProcessPayment(decimal cash);
    }

    internal class PaymentMock : IPayment
    {
        public PaymentMock()
        {
            Console.WriteLine("Вызван конструктор класса PaymentMock");
        }

        public Dictionary<string, object> ProcessPayment(decimal cash)
        {
            Console.WriteLine($"Вызван метод класса PaymentMock - ProcessPayment");

            return new Dictionary<string, object>
            {
                ["date"] = DateTime.Now,
                ["success"] = true,
                ["cash_paid"] = cash,
                ["currency"] = "RUB"
            };
        }
    }

    internal class PaymentAdapter : IPayment
    {
        private readonly IPayment mock_object;

        public PaymentAdapter()
        {
            Console.WriteLine("Вызван конструктор класса PaymentAdapter");

            mock_object = new PaymentMock();
        }

        public Dictionary<string, object> ProcessPayment(decimal cash)
        {
            Console.WriteLine("Вызван метод класса PaymentAdapter - ProcessPayment");

            return mock_object.ProcessPayment(cash);
        }
    }

    internal class Payment
    {
        private readonly IPayment adapter_object;

        internal Payment()
        {
            Console.WriteLine("Вызван конструктор класса Payment");

            adapter_object = new PaymentAdapter();
        }

        internal Dictionary<string, object> MakePayment(decimal cash)
        {
            Console.WriteLine("Вызван метод класса Payment - MakePayment");

            Dictionary<string, object> result = adapter_object.ProcessPayment(cash);

            if (!Convert.ToBoolean(result["success"]))
                throw new InvalidOperationException("Оплата не прошла по техническим причинам. Обратитесь к системному администратору");

            Console.WriteLine($"Оплата завершена успешно! ID транзакции: {Convert.ToInt32(result["transaction_id"])}");

            return result;
        }
    }

    internal class Log
    {
        private List<Sale> sales_list;

        internal Log()
        {
            Console.WriteLine("Вызван конструктор класса Log: без параметров");

            sales_list = new List<Sale>();
        }

        private bool IsTicketActive(Ticket ticket_object)
        {
            if (ticket_object is TicketSingle ticket_single)
            {
                return ticket_single.GetMatchDatetimeStart() > DateTime.Now;
            }

            else
            {
                TicketSession ticket_session = (TicketSession)ticket_object;
                (DateTime datetime_start, DateTime datetime_end) = ticket_session.GetDatetime();

                return DateTime.Now >= datetime_start && DateTime.Now <= datetime_end;
            }
        }

        private List<Ticket> GetAllTickets()
        {
            Console.WriteLine("Вызван метод класса Log - GetAllTickets");

            return sales_list.SelectMany(
                       sale => sale.GetTickets()
                   ).Where(
                       ticket => IsTicketActive(ticket)
                   ).ToList();
        }

        internal Ticket GetTicket(int ticket_id)
        {
            Console.WriteLine("Вызван метод класса Log - GetTicket");

            Ticket ticket_exist = GetAllTickets().FirstOrDefault(ticket =>
                                      ticket.GetID() == ticket_id
                                  );

            return ticket_exist
                ??
                throw new ArgumentException(
                    "Билет/абонемент не найден или уже не актуален. Введённые параметры:\n" +
                    $"- ID = {ticket_id}"
                );
        }

        internal TicketSingle GetTicketSingle(DateTime matchDate, int sector_id, int row_id, int seat_id)
        {
            Console.WriteLine("Вызван метод класса Log - GetTicketSingle");

            TicketSingle ticket_exist = GetAllTickets().OfType<TicketSingle>().FirstOrDefault(ticket =>
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
                    "Билет не найден или уже не актуален. Введённые параметры:\n" +
                    $"- начало матча = {matchDate};\n" +
                    $"- сектор = {sector_id};\n" +
                    $"- ряд = {row_id};\n" +
                    $"- место = {seat_id}"
                );
        }

        internal TicketSession GetTicketSession(DateTime datetime_start, DateTime datetime_end)
        {
            Console.WriteLine("Вызван метод класса Log - GetTicketSession: DateTime datetime_start, DateTime datetime_end");

            TicketSession ticket_exist = GetAllTickets().OfType<TicketSession>().FirstOrDefault(ticket => 
                ticket.GetDatetime() == (datetime_start, datetime_end)
            );

            return ticket_exist
                ??
                throw new ArgumentException(
                    "Абонемент не найден или уже не актуален. Введённые параметры:\n" +
                    $"- начало действия = {datetime_start};\n" +
                    $"- конец действия = {datetime_end}"
                );
        }

        internal (int total_sale, int total_ticket_single, int total_ticket_session, decimal total_FullPrice) GetTotalStats()
        {
            Console.WriteLine("Вызван метод класса Log - GetTotalStats");

            int total_sale = sales_list.Count;
            int total_ticket_single = GetAllTickets().OfType<TicketSingle>().Count();
            int total_ticket_session = GetAllTickets().OfType<TicketSession>().Count();
            decimal total_FullPrice = sales_list.Sum(sale => sale.FullPrice);

            return (total_sale, total_ticket_single, total_ticket_session, total_FullPrice);
        }

        internal Sale GetSale(DateTime datetime)
        {
            Console.WriteLine("Вызван метод класса Log - GetSale");

            return (Sale)sales_list.Where(sale => sale.GetDatetime() == datetime);
        }

        internal void MakeLogSale(Sale sale)
        {
            Console.WriteLine("Вызван метод класса Log - MakeLogSale");

            sales_list.Add(sale);
        }
    }
}
