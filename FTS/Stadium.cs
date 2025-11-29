using System;
using System.Collections.Generic;
using System.Linq;
using GrandstandSpace;
using SaleSpace;

namespace StadiumSpace
{
    internal class Stadium
    {
        private byte user_id;
        private List<Match> matches;
        private Sale sale_current;
        private Dictionary<string, IPriceStrategy> price_strategy_list;
        private Log log;

        internal Stadium()
        {
            Console.WriteLine("Вызван конструктор класса Stadium: без параметров");

            matches = new List<Match>();
            log = new Log();
            price_strategy_list = new Dictionary<string, IPriceStrategy>
            {
                ["single"] = new TicketSinglePriceStrategy(),
                ["session"] = new TicketSessionPriceStrategy()
            };
        }

        private string GenerateMatchReport(
            DateTime datetime_start, string team_first, string team_second, Dictionary<int, Dictionary<int, List<int>>> sectors_rows_seats
        )
        {
            Console.WriteLine("Вызван метод класса Stadium - GenerateMatchReport");

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
            Console.WriteLine("Вызван метод класса Stadium - Autharization");

            if (user_id == 0)
                throw new InvalidOperationException(
                    "Невозможно авторизоваться в системе с уровнем доступа 0"
                );

            if (this.user_id != user_id)
            {
                this.user_id = user_id;
                Console.WriteLine($"Вы авторизированы в системе. Ваш уровень доступа: {user_id}");
            }
            else
                Console.WriteLine($"Вы уже авторизированы в системе. Текущий уровень доступа: {user_id}");
        }

        internal void CreateSale(string type = "single")
        {
            Console.WriteLine("Вызван метод класса Stadium - CreateSale");

            if (user_id < 2)
            {
                if (sale_current != null)
                    throw new InvalidOperationException($"Текущая продажа не завершена. Закройте её, после чего повторите попытку создания продажи");

                if (price_strategy_list.ContainsKey(type))
                {
                    sale_current = new Sale(type, price_strategy_list[type]);

                    Console.WriteLine(
                        "Создана новая продажа:\n" +
                        $"- стратегия: {type};\n"
                    );
                }
                else
                    throw new ArgumentException(
                        $"Стратегия типа \"{type}\" не найдена, создание продажи отменено"
                    );
            }
            else
                throw new InvalidOperationException(
                    "Ваш уровень доступа не позволяет исполнить вызов данной процедуры:\n" +
                    "- требуемый уровень: 2 или 3\n" +
                    $"- Ваш уровень: {user_id}"
                );
        }

        internal void CloseSale()
        {
            Console.WriteLine("Вызван метод класса Stadium - CloseSale");

            if (user_id < 2)
            {
                if (sale_current != null)
                {
                    sale_current = null;
                    Console.WriteLine("Продажа успешно завершена");
                }
                else
                    Console.WriteLine("Нет текущей продажи");
            }
            else
                throw new InvalidOperationException(
                    "Ваш уровень доступа не позволяет исполнить вызов данной процедуры:\n" +
                    "- требуемый уровень: 2 или 3\n" +
                    $"- Ваш уровень: {user_id}"
                );
        }

        internal void SetPriceStrategy(string type)
        {
            Console.WriteLine("Вызван метод класса Stadium - SetPriceStrategy");

            if (user_id < 2)
            {
                if (sale_current == null)
                    throw new InvalidOperationException(
                        "Нет текущей продажи для изменения стратегии её ценообразования"
                    );

                if (price_strategy_list.ContainsKey(type))
                {
                    sale_current.SetPriceStrategy(price_strategy_list[type]);

                    Console.WriteLine($"Стратегия ценообразования изменена на: {type}");
                }
                else
                    throw new ArgumentException(
                        $"Стратегия типа \"{type}\" не найдена"
                    );
            }
            else
                throw new InvalidOperationException(
                    "Ваш уровень доступа не позволяет изменить стратегию ценообразования"
                );
        }

        internal decimal MakePrice()
        {
            Console.WriteLine("Вызван метод класса Stadium - MakePrice");

            if (user_id < 2)
            {
                if (sale_current == null)
                    throw new InvalidOperationException("Нет текущей продажи. Создайте её, внести билеты/абонемент для продажи и повторите попытку подсчёта цены продажи");

                decimal full_price = sale_current.MakePrice();
                Console.WriteLine($"Общая стоимость продажи: {full_price}");

                return full_price;
            }
            else
                throw new InvalidOperationException(
                    "Ваш уровень доступа не позволяет исполнить вызов данной процедуры:\n" +
                    "- требуемый уровень: 2 или 3\n" +
                    $"- Ваш уровень: {user_id}"
                );
        }

        internal Dictionary<string, object> MakePayment(decimal cash)
        {
            Console.WriteLine("Вызван метод класса Stadium - MakePayment");

            if (user_id < 2)
            {
                if (sale_current == null)
                    throw new InvalidOperationException("Нет текущей продажи. Создайте её, внести билеты/абонемент для продажи, подсчитайте их цену и повторите попытку оплаты");

                Dictionary<string, object> receipt = sale_current.MakePayment(cash);
                Console.WriteLine("Оплата продажи завершена");

                if (sale_current.type == "single")
                {
                    sale_current.ChangeBooking(this);
                    Console.WriteLine("Данные о бронировании мест в матчах обновлены, данные билетов сохранены в соответствующие матчи");
                }

                sale_current.MakeLogSale(log);
                Console.WriteLine("Данные продажи залогированы");

                return receipt;
            }
            else
                throw new InvalidOperationException(
                    "Ваш уровень доступа не позволяет исполнить вызов данной процедуры:\n" +
                    "- требуемый уровень: 2 или 3\n" +
                    $"- Ваш уровень: {user_id}"
                );
        }

        internal void CreateTicketSingle(DateTime match_datetime_start, int grandstand_sector, int grandstand_row, int grandstand_seat)
        {
            Console.WriteLine("Вызван метод класса Stadium - CreateTicketSingle");

            if (user_id < 2)
            {
                if (sale_current == null)
                    throw new InvalidOperationException("Нет текущей продажи. Создайте её и повторите попытку создания билета");

                sale_current.CreateTicketSingle(match_datetime_start, grandstand_sector, grandstand_row, grandstand_seat);

                Console.WriteLine(
                    $"Создан билет с параметрами: дата матча - {match_datetime_start}, сектор - {grandstand_sector}, ряд - {grandstand_row}, место - {grandstand_seat}"
                );
            }
            else
                throw new InvalidOperationException(
                    "Ваш уровень доступа не позволяет исполнить вызов данной процедуры:\n" +
                    "- требуемый уровень: 2 или 3\n" +
                    $"- Ваш уровень: {user_id}"
                );
        }

        internal void CreateTicketSession(DateTime datetime_start, DateTime datetime_end, int sector_id)
        {
            Console.WriteLine("Вызван метод класса Stadium - CreateTicketSession");

            if (user_id < 2)
            {
                if (sale_current == null)
                    throw new InvalidOperationException("Нет текущей продажи. Создайте её и повторите попытку создания абонемента");

                sale_current.CreateTicketSession(datetime_start, datetime_end, sector_id);

                Console.WriteLine(
                    $"Создан абонемент с параметрами: период действия - ({datetime_start} - {datetime_end}), сектор {sector_id}"
                );
            }
            else
                throw new InvalidOperationException(
                    "Ваш уровень доступа не позволяет исполнить вызов данной процедуры:\n" +
                    "- требуемый уровень: 2 или 3\n" +
                    $"- Ваш уровень: {user_id}"
                );
        }

        internal void CreateMatch(
            DateTime datetime_start, string team_first, string team_second, Dictionary<int, Dictionary<int, List<int>>> sectors_rows_seats
        )
        {
            Console.WriteLine("Вызван метод класса Stadium - CreateMatch");

            if (user_id < 3)
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
            else
                throw new InvalidOperationException(
                    "Ваш уровень доступа не позволяет исполнить вызов данной процедуры:\n" +
                    "- требуемый уровень: 3\n" +
                    $"- Ваш уровень: {user_id}"
                );
        }

        internal Dictionary<string, object> SelectMatchSeats(int matches_id)
        {
            Console.WriteLine("Вызван метод класса Stadium - SelectMatchSeats");

            if (user_id < 1)
                return matches[matches_id].GetSeatsBooking();
            else
                throw new InvalidOperationException(
                    "Ваш уровень доступа не позволяет исполнить вызов данной процедуры:\n" +
                    "- требуемый уровень: 3\n" +
                    $"- Ваш уровень: {user_id}"
                );
        }

        internal Match GetMatch(DateTime datetime_start)
        {
            Console.WriteLine("Вызван метод класса Stadium - GetMatch");

            if (user_id < 1)
                return matches.FirstOrDefault(match => match.GetDatetimeStart() == datetime_start);
            else
                throw new InvalidOperationException(
                    "Ваш уровень доступа не позволяет исполнить вызов данной процедуры:\n" +
                    "- требуемый уровень: 1\n" +
                    $"- Ваш уровень: {user_id}"
                );
        }

        internal List<Match> GetMatches()
        {
            Console.WriteLine("Вызван метод класса Stadium - GetMatches");

            if (user_id < 1)
                return matches;
            else
                throw new InvalidOperationException(
                    "Ваш уровень доступа не позволяет исполнить вызов данной процедуры:\n" +
                    "- требуемый уровень: 1\n" +
                    $"- Ваш уровень: {user_id}"
                );
        }

        internal void OpenEditingMatch()
        {
            Console.WriteLine("Вызван метод класса Stadium - OpenEditingMatch");

            if (user_id < 3)
                Console.WriteLine($"Вы активировали режим редактирования матчей");
            else
                throw new InvalidOperationException(
                    "Ваш уровень доступа не позволяет исполнить вызов данной процедуры:\n" +
                    "- требуемый уровень: 3\n" +
                    $"- Ваш уровень: {user_id}"
                );
        }

        internal void CloseEditingMatch()
        {
            Console.WriteLine("Вызван метод класса Stadium - CloseEditingMatch");

            if (user_id < 3)
                Console.WriteLine($"Вы вышли из режима редактирования матчей");
            else
                throw new InvalidOperationException(
                    "Ваш уровень доступа не позволяет исполнить вызов данной процедуры:\n" +
                    "- требуемый уровень: 3\n" +
                    $"- Ваш уровень: {user_id}"
                );
        }

        internal void Quit()
        {
            Console.WriteLine("Вызван метод класса Stadium - Quit");

            if (user_id < 1)
                Console.WriteLine($"Операция успешно завершена");
            else
                throw new InvalidOperationException(
                    "Ваш уровень доступа не позволяет исполнить вызов данной процедуры:\n" +
                    "- требуемый уровень: 1\n" +
                    $"- Ваш уровень: {user_id}"
                );
        }
    }
}
