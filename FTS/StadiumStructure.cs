using System;
using System.Collections.Generic;
using System.Linq;
using Sales;
using Tickets;

namespace StadiumStructure
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
            if (children.Any(c => c.GetID() == id))
                throw new ArgumentException($"ID = {id}: {child_type} с таким ID уже существует");

            children.Add(factory(id));
        }

        protected void ChangeChild<T>(int id, object child, string child_type) where T : IGrandstand
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
                    throw new ArgumentException($"ID = {id}: {child_type} с таким ID не найден");
            }
            else
                throw new ArgumentException($"Дочерний объект должен быть типа {typeof(T).Name}");
        }

        protected object GetChild<T>(int id, string child_type) where T : IGrandstand
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

        internal abstract void CreateChild(int id);
        internal abstract void ChangeChild(int id, object child);
        internal abstract Object GetChild(int id);
        internal abstract int GetID();
        int IGrandstand.GetID() // явное обозначение, иначе метод не будет "реализован"
        {
            return GetID();
        }

        internal abstract bool GetBooking(List<int> seat_path);
        internal abstract void ChangeBooking(bool booking, List<int> seat_path);
        internal abstract Dictionary<string, object> GetSeatsBooking();
    }

    internal class Stadium
    {
        private byte user_id;
        private List<Match> matches;
        private Sale sale_current;
        private Log log;
        
        internal Stadium()
        {
            matches = new List<Match>();
            log = new Log();
        }

        private string GenerateMatchReport(
            DateTime datetime_start, string team_first, string team_second, Dictionary<int, Dictionary<int, List<int>>> sectors_rows_seats
        )
        {
            int count_sectors = sectors_rows_seats.Count;
            int count_rows = sectors_rows_seats.Sum(sector => sector.Value.Count);
            int count_seats = sectors_rows_seats.Sum(sector => sector.Value.Sum(row => row.Value.Count));

            return "Отчет по созданному матчу:\n" +
                   $"- Дата проведения: {datetime_start}\n" +
                   $"- Команды: {team_first} vs {team_second}\n" +
                   $"- Количество секторов: {count_sectors}\n" +
                   $"- Количество рядов: {count_rows}\n" +
                   $"- Количество мест: {count_seats}\n";
        }

        internal void Autharization(byte user_id)
        {
            this.user_id = user_id;

            Console.WriteLine($"Вы авторизированы в системе. Ваш уровень доступа: {user_id}");
        }

        internal void CreateSale(string type = "билет")
        {
            if (sale_current !=  null)
                throw new InvalidOperationException($"Текущая продажа не завершена. Закройте её, после чего повторите попытку создания продажи");

            sale_current = new Sale(type);

            Console.WriteLine($"Создана новая продажа. Продаваемый товар: {type}");
        }

        internal void CloseSale()
        {
            if (sale_current != null)
            {
                sale_current = null;
                Console.WriteLine("Продажа успешно завершена");
            }
            else
                Console.WriteLine("Нет текущей продажи");
        }

        internal decimal MakePrice()
        {
            if (sale_current == null)
                throw new InvalidOperationException("Нет текущей продажи. Создайте её, внести билеты/абонемент для продажи и повторите попытку подсчёта цены продажи");

            decimal totalPrice = sale_current.MakePrice();
            Console.WriteLine($"Общая стоимость продажи: {totalPrice}");

            return totalPrice;
        }

        internal void MakePayment(decimal cash) // ВОЗВРАЩЕНИЕ ЧЕКА И БИЛЕТОВ ПОКУПАТЕЛЮ ???
        {
            if (sale_current == null)
                throw new InvalidOperationException("Нет текущей продажи. Создайте её, внести билеты/абонемент для продажи, подсчитайте их цену и повторите попытку оплаты");

            sale_current.MakePayment(cash);
            Console.WriteLine("Оплата продажи завершена");

            if (sale_current.type == "билет")
            {
                sale_current.ChangeBooking(this);
                Console.WriteLine("Данные о бронировании мест в матчах обновлены, данные билетов сохранены в соответствующие матчи");
            }

            sale_current.MakeLogSale(log);
            Console.WriteLine("Данные продажи залогированы");
        }

        internal void CreateTicketSingle(DateTime match_datetime_start, int grandstand_sector, int grandstand_row, int grandstand_seat)
        {
            if (sale_current == null)
                throw new InvalidOperationException("Нет текущей продажи. Создайте её и повторите попытку создания билета");

            sale_current.CreateTicketSingle(match_datetime_start, grandstand_sector, grandstand_row, grandstand_seat);
            
            Console.WriteLine(
                $"Создан билет с параметрами: дата матча - {match_datetime_start}, сектор - {grandstand_sector}, ряд - {grandstand_row}, место - {grandstand_seat}"
            );
        }

        internal void CreateTicketSession(DateTime datetime_start, DateTime datetime_end, int sector_id)
        {
            if (sale_current == null)
                throw new InvalidOperationException("Нет текущей продажи. Создайте её и повторите попытку создания абонемента");

            sale_current.CreateTicketSession(datetime_start, datetime_end, sector_id);

            Console.WriteLine(
                $"Создан абонемент с параметрами: период действия - ({datetime_start} - {datetime_end}), сектор {sector_id}"
            );
        }

        internal void CreateMatch(
            DateTime datetime_start, string team_first, string team_second, Dictionary<int, Dictionary<int, List<int>>> sectors_rows_seats
        )
        {
            Match match = new Match(datetime_start, team_first, team_second);

            foreach (KeyValuePair<int, Dictionary<int, List<int>>> sector_data in sectors_rows_seats)
            {
                int sector_id = sector_data.Key;
                Dictionary<int, List<int>> rows_seats = sector_data.Value;

                match.CreateChild(sector_id);
                Sector sector = match.GetChild(sector_id) as Sector;

                foreach (KeyValuePair<int, List<int>> row_data in rows_seats)
                {
                    int row_id = row_data.Key;
                    List<int> seats_list = row_data.Value;

                    sector.CreateChild(row_id);
                    Row row = sector.GetChild(row_id) as Row;

                    foreach (int seat_id in seats_list)
                        row.CreateChild(seat_id);
                }
            }

            matches.Add(match);

            Console.WriteLine(GenerateMatchReport(datetime_start, team_first, team_second, sectors_rows_seats));
        }

        internal Dictionary<string, object> SelectMatchSeats(int matches_id)
        {
            return matches[matches_id].GetSeatsBooking();
        }

        internal Match GetMatch(DateTime datetime_start)
        {
            return matches.FirstOrDefault(match => match.GetDatetimeStart() == datetime_start);
        }

        internal List<Match> GetMatches()
        {
            return matches;
        }

        internal void OpenEditingMatch()
        {
            Console.WriteLine($"Вы активировали режим редактирования матчей");
        }

        internal void CloseEditingMatch()
        {
            Console.WriteLine($"Вы вышли из режима редактирования матчей");
        }

        internal void Quit()
        {
            Console.WriteLine($"Операция успешно завершена");
        }
    }

    internal class Match : Grandstand
    {
        private DateTime datetime_start;
        private DateTime datetime_end; // НИГДЕ НЕ НАЗНАЧАЕТСЯ - ОСТАВЛЯЕМ В ТАКОМ ВИДЕ ИЛИ НЕТ ???
        private string team_first;
        private string team_second;
        private List<TicketSingle> tickets;

        internal Match(DateTime datetime_start, string team_first, string team_second)
        {
            this.datetime_start = datetime_start;
            this.team_first = team_first;
            this.team_second = team_second;
            tickets = new List<TicketSingle>();
        }

        internal override void CreateChild(int id)
        {
            CreateChild<Sector>(id, (sector_id) => new Sector(sector_id, false), "Sector");
        }

        internal override void ChangeChild(int id, object child)
        {
            ChangeChild<Sector>(id, child, "Sector");
        }

        internal override Object GetChild(int id)
        {
            return GetChild<Sector>(id, "Sector");
        }

        internal override int GetID()
        {
            throw new NotImplementedException("Match использует дату своего начала в качестве ID");
        }

        internal DateTime GetDatetimeStart()
        {
            return datetime_start;
        }

        internal void ChangeDatetime(DateTime datetime_start, DateTime datetime_end)
        {
            this.datetime_start = datetime_start;
            this.datetime_end = datetime_end;
        }

        internal void ChangeTeams(string team_first, string team_second)
        {
            this.team_first = team_first;
            this.team_second = team_second;
        }

        internal void SaveTicket(TicketSingle ticket_object)
        {
            tickets.Add(ticket_object);
        }

        internal override bool GetBooking(List<int> seat_path)
        {
            return GetBooking<Sector>(seat_path, "Sector");
        }

        internal override void ChangeBooking(bool booking, List<int> seat_path)
        {
            ChangeBooking<Sector>(booking, seat_path, "Sector");
        }

        internal override Dictionary<string, object> GetSeatsBooking()
        {
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
            this.sector_id = sector_id;
            this.ticket_session_only = ticket_session_only;
        }

        internal override int GetID()
        {
            return sector_id;
        }

        internal override void CreateChild(int id)
        {
            CreateChild<Row>(id, (row_id) => new Row(row_id), "Row");
        }

        internal override void ChangeChild(int id, object child)
        {
            ChangeChild<Row>(id, child, "Row");
        }

        internal override Object GetChild(int id)
        {
            return GetChild<Row>(id, "Row");
        }

        internal override bool GetBooking(List<int> seat_path)
        {
            return GetBooking<Row>(seat_path, "Row");
        }

        internal override void ChangeBooking(bool booking, List<int> seat_path)
        {
            ChangeBooking<Row>(booking, seat_path, "Row");
        }

        internal override Dictionary<string, object> GetSeatsBooking()
        {
            Dictionary<int, object> rows_seats_values = new Dictionary<int, object>();

            foreach (Row row in children.OfType<Row>())
                rows_seats_values.Add(row.GetID(), row.GetSeatsBooking());

            Dictionary<string, object> seats_id_booking = new Dictionary<string, object>
            {
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
            this.row_id = row_id;
        }

        internal override int GetID()
        {
            return row_id;
        }

        internal override void CreateChild(int id)
        {
            CreateChild<Seat>(id, (seatId) => new Seat(seatId), "Seat");
        }

        internal override void ChangeChild(int id, object child)
        {
            ChangeChild<Seat>(id, child, "Seat");
        }

        internal override Object GetChild(int id)
        {
            return GetChild<Seat>(id, "Seat");
        }

        internal override bool GetBooking(List<int> seat_path)
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

        internal override void ChangeBooking(bool booking, List<int> seat_path)
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

        internal override Dictionary<string, object> GetSeatsBooking()
        {
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
            this.seat_id = seat_id;
            booking = false;
        }

        internal int GetID()
        {
            return seat_id;
        }
        int IGrandstand.GetID() // явное обозначение, иначе метод не будет "реализован"
        {
            return GetID();
        }

        internal bool GetBooking()
        {
            return booking;
        }

        internal void ChangeBooking(bool booking)
        {
            this.booking = booking;
        }
    }
}
