using System;
using System.Collections.Generic;
using Tickets;

namespace Sales
{
    public class Sale
    {
        private DateTime datetime;
        public decimal full_price { get; private set; }
        private List<Ticket> tickets;

        public Sale()
        {
            tickets = new List<Ticket>();
            full_price = 0;
        }

        private decimal CalcPrice(decimal price)
        {

        }

        public TicketSingle CreateTicket(DateTime match_datetime_start, int grandstand_sector, int grandstand_row, int grandstand_seat)
        {
            
        }

        public TicketSession CreateTicketSession(DateTime datetime_start, DateTime datetime_end, int sector_id)
        {
            
        }

        public decimal MakePrice()
        {
            //full_price = tickets.Sum(ticket => ticket.GetPrice());
            //return full_price;
        }

        public Payment MakePayment(decimal cash)
        {
            
        }
    }

    public class Payment
    {
        public Payment()
        {
            
        }

        private void MakePayment(decimal cash)
        {

        }
    }

    public class Log
    {
        private List<(DateTime, decimal, List<Ticket>)> sale_operations;

        public Log()
        {
            sale_operations = new List<(DateTime, decimal, List<Ticket>)>();
        }

        public void MakeLogSale(DateTime datetime, decimal full_price, List<Ticket> tickets)
        { 
            sale_operations.Add((datetime, full_price, tickets));
        }
    }
}
