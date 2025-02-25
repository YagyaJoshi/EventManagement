using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.DataAccess.Models
{
    public class BookingDetail
    {
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public long OrganizationId { get; set; }
        public DateTimeOffset? OrderDate { get; set; }
        public long EventId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }

        // OrderDetail properties
        public long OrderDetailId { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }

        // GuestDetails properties
        public long GuestDetailId { get; set; }
        public string PassportFirstName { get; set; }
        public string PassportLastName { get; set; }
        public string PassportNumber { get; set; }
        public DateTimeOffset? DOB { get; set; }
        public string Occupation { get; set; }
        public string Nationality { get; set; }
        public string JobTitle { get; set; }
        public string WorkPlace { get; set; }
        public string DepartureFlightAirport { get; set; }
        public string ArrivalFlightAirport { get; set; }
        public string ArrivalFlightNumber { get; set; }
        public string DepartureFlightNumber { get; set; }
        public DateTimeOffset? ArrivalDateTime { get; set; }
        public DateTimeOffset? DepartureDateTime { get; set; }
        public string ArrivalNotes { get; set; }
        public string DepartureNotes { get; set; }
        public string AccessibilityInfo { get; set; }
        public long? HotelId { get; set; }
        public long? HotelRoomTypeId { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public bool VisaAssistanceRequired { get; set; }
        public int GuestStatus { get; set; }
    }

}
