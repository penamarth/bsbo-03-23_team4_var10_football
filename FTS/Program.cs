using System;
using System.Collections.Generic;
using System.Linq;
using StadiumSpace;
using GrandstandSpace;

namespace FTS
{
    internal class Program
    {
        static void Main()
        {
            WriteConsoleText(message: "СИМУЛЯЦИЯ УСПЕШНЫХ ПРЕЦЕДЕНТОВ:", separate: false);

            var stadium = new Stadium();


            // ========================

            WriteConsoleText(message: "1 - СОЗДАНИЕ МАТЧА:", separate: true);

            stadium.Autharization(3);

            stadium.OpenEditingMatch();

            var sectors_rows_seats = new Dictionary<int, Dictionary<int, List<int>>>
            {
                // Сектор 1
                {
                    1, new Dictionary<int, List<int>>
                    {
                        { 1, new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } },  // Ряд 1
                    }
                },
    
                // Сектор 2
                {
                    2, new Dictionary<int, List<int>>
                    {
                        { 1, new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } },  // Ряд 1
                    }
                },
            };

            stadium.CreateMatch(
                Convert.ToDateTime("2025-01-12 11:15:00"),
                "Спартак",
                "ЦСКА",
                sectors_rows_seats
            );

            stadium.CloseEditingMatch();

            // ========================


            // ========================

            WriteConsoleText(message: "2 - ПРОДАЖА БИЛЕТОВ:", separate: true);

            stadium.Autharization(2);

            stadium.CreateSale("single");

            stadium.CreateTicketSingle(
                Convert.ToDateTime("2025-01-12 11:15:00"),
                1, 1, 1
            );

            stadium.CreateTicketSingle(
                Convert.ToDateTime("2025-01-12 11:15:00"),
                1, 1, 5
            );

            stadium.CreateTicketSingle(
                Convert.ToDateTime("2025-01-12 11:15:00"),
                2, 1, 9
            );

            var full_price_single = stadium.MakePrice();
            Console.WriteLine($"Полная цена покупки: {full_price_single}");

            var receipt_single = stadium.MakePayment(full_price_single);
            PrintReceipt(receipt_single);

            stadium.CloseSale();

            // ========================


            // ========================

            WriteConsoleText(message: "3 - ПРОДАЖА АБОНЕМЕНТА:", separate: true);

            stadium.Autharization(2);

            stadium.CreateSale("session");

            stadium.CreateTicketSession(
                Convert.ToDateTime("2025-12-01 00:00:00"),
                Convert.ToDateTime("2025-12-31 23:59:59"),
                2 // не указали при создании матча, что 2 сектор только для абонементов - исправим или оставим так ???
            );

            var full_price_session = stadium.MakePrice();
            Console.WriteLine($"Полная цена покупки: {full_price_session}");

            var receipt_session = stadium.MakePayment(full_price_session);
            PrintReceipt(receipt_session);

            stadium.CloseSale();

            // ========================


            // ========================

            WriteConsoleText(message: "4 - ПРОСМОТР СВОБОДНЫХ МЕСТ:", separate: true);
            
            stadium.Autharization(1);

            var matches = stadium.GetMatches();
            ViewMatches(stadium, matches);

            stadium.Quit();

            // ========================


            // ========================

            WriteConsoleText(message: "СИМУЛЯЦИЯ УСПЕШНЫХ ПРЕЦЕДЕНТОВ ЗАВЕРШЕНА", separate: true);
        }

        static void WriteConsoleText(string message, bool separate)
        {
            if (separate)
                Console.WriteLine("\n===================\n");

            Console.WriteLine($"\n{message}\n");
        }

        static void PrintReceipt(Dictionary<string, object> receipt_data)
        {
            Console.WriteLine("\nЧек о покупке билетов/абонемента:");
            Console.WriteLine("1) Основная информация:");
            Console.WriteLine($"- Дата продажи: {receipt_data["sale_datetime"]}");
            Console.WriteLine($"- Тип продажи: {receipt_data["sale_type"]}");
            Console.WriteLine($"- Общая сумма: {receipt_data["total_price"]}");
            Console.WriteLine($"- Количество билетов: {receipt_data["tickets_count"]}");

            var payment_info = receipt_data["payment_info"] as Dictionary<string, object>;
            Console.WriteLine("2) Платёжная информация:");
            Console.WriteLine($"- ID транзакции: {payment_info["transaction_id"]}");
            Console.WriteLine($"- Дата оплаты: {payment_info["date"]}");
            Console.WriteLine($"- Оплачено: {payment_info["cash_paid"]}");
            Console.WriteLine($"- Сдача: {payment_info["change"]}");
            Console.WriteLine($"- Статус: {(Convert.ToBoolean(payment_info["success"]) ? "УСПЕШНО" : "ОШИБКА")}");

            var tickets = receipt_data["tickets"] as List<Dictionary<string, object>>;
            Console.WriteLine("2) Информация о билетах/абонементе:");
            for (int i = 0; i < tickets.Count; i++)
            {
                var ticket = tickets[i];
                Console.WriteLine($"- Билет №{i + 1}:");
                Console.WriteLine($"  Тип: {ticket["ticket_type"]}");
                Console.WriteLine($"  Цена: {ticket["price"]}");

                if (ticket.ContainsKey("match_datetime"))
                {
                    Console.WriteLine($"  Дата матча: {ticket["match_datetime"]}");
                    Console.WriteLine($"  Сектор: {ticket["sector"]}, Ряд: {ticket["row"]}, Место: {ticket["seat"]}");
                }
                else
                {
                    Console.WriteLine($"  Период действия: {ticket["datetime_start"]} - {ticket["datetime_end"]}");
                    Console.WriteLine($"  Сектор: {ticket["sector_id"]}");
                }
            }

            Console.WriteLine();
        }

        static void ViewMatches(Stadium stadium, List<Match> matches)
        {
            Console.WriteLine("\nЗапланированные матчи:");

            var matches_length = matches.Count;
            for (int i = 0; i < matches_length; i++)
            {
                var match = matches[i];
                (var team_fisrt, var team_second) = match.GetTeams();
                var datetime_start = match.GetDatetimeStart();
                Console.WriteLine($"{i + 1}) \"{team_fisrt}\" против \"{team_second}\", дата проведения: {datetime_start}");
            }

            bool viewing_matches = true;
            while (viewing_matches)
            {
                Console.Write($"\nВведите номер матча для просмотра мест или 0 для выхода из просмотра: ");
                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Ошибка ввода. Пожалуйста, введите число.");
                    continue;
                }

                if (choice == 0)
                {
                    viewing_matches = false;
                    continue;
                }

                if (choice < 1 || choice > matches_length)
                {
                    Console.WriteLine($"Пожалуйста, введите число от 1 до {matches_length}");
                    continue;
                }

                var match_id = choice - 1;
                var grandstands = stadium.SelectMatchSeats(match_id);

                var match = matches[match_id];
                (var team_first, var team_second) = match.GetTeams();
                var datetime_start = match.GetDatetimeStart();

                Console.WriteLine("\n" + new string('-', 50));
                Console.WriteLine($"МАТЧ: \"{team_first}\" против \"{team_second}\"");
                Console.WriteLine($"Дата: {datetime_start:dd.MM.yyyy HH:mm}");
                Console.WriteLine(new string('-', 50));

                Dictionary<int, object> sectors = (Dictionary<int, object>)grandstands["sectors"];
                foreach (var sector in sectors.OrderBy(s => s.Key))
                {
                    int sector_id = sector.Key;

                    Dictionary<string, object> sector_data = (Dictionary<string, object>)sector.Value;
                    Dictionary<int, object> rows = (Dictionary<int, object>)sector_data["rows"];

                    bool is_session_only = Convert.ToBoolean(sector_data["ticket_session_only"]);

                    Console.WriteLine($"\nСектор {sector_id} {(is_session_only ? "(только абонементы)" : "")}");
                    Console.WriteLine(new string('-', 40));

                    foreach (var row in rows.OrderBy(r => r.Key))
                    {
                        int row_id = row.Key;

                        Dictionary<string, object> row_data = (Dictionary<string, object>)row.Value;
                        Dictionary<int, bool> seats = (Dictionary<int, bool>)row_data["seats"];

                        Console.Write($"Ряд {row_id:00}: ");

                        foreach (var seat in seats.OrderBy(s => s.Key))
                        {
                            int seat_id = seat.Key;
                            bool is_booked = seat.Value;

                            if (is_session_only)
                            {
                                // Желтая подсветка для секторов только с абонементами
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Write($"[{seat_id:00}] ");
                                Console.ResetColor();
                            }
                            else if (is_booked)
                            {
                                // Красная подсветка для занятых мест
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write($"[{seat_id:00}] ");
                                Console.ResetColor();
                            }
                            else
                            {
                                // Зеленая подсветка для свободных мест
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write($"[{seat_id:00}] ");
                                Console.ResetColor();
                            }
                        }
                        Console.WriteLine();
                    }
                }

                Console.WriteLine();

                // Легенда
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[XX] ");
                Console.ResetColor();
                Console.Write("- свободно");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(" [XX] ");
                Console.ResetColor();
                Console.Write("- занято");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(" [XX] ");
                Console.ResetColor();
                Console.WriteLine("- только абонемент");
                Console.WriteLine();
            }
        }
    }
}
