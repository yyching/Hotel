namespace Hotel.Models
{
    public class ReceiptTemplate
    {
        public static string GenerateHtml(
            string bookingID,
            DateTime bookingDate,
            DateOnly checkInDate,
            DateOnly checkOutDate,
            string roomNumber,
            List<ServiceItem> services,
            double subtotal,
            double tax,
            double total)
        {
            var receiptHtml = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                .receipt {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ text-align: center; margin-bottom: 30px; }}
                .booking-info {{ margin-bottom: 20px; }}
                .services {{ margin-bottom: 20px; }}
                table {{ width: 100%; border-collapse: collapse; margin-bottom: 20px; }}
                th, td {{ padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }}
                .total {{ margin-top: 20px; text-align: right; }}
                .footer {{ margin-top: 30px; text-align: center; font-size: 14px; }}
            </style>
        </head>
        <body>
            <div class='receipt'>
                <div class='header'>
                    <h1>EASYSTAYS HOTEL</h1>
                    <h2>Booking Receipt</h2>
                    <p>Booking ID: {bookingID}</p>
                    <p>Date: {bookingDate:yyyy-MM-dd}</p>
                </div>

                <div class='booking-info'>
                    <h3>Booking Details</h3>
                    <table>
                        <tr><td>Check-in Date:</td><td>{checkInDate:yyyy-MM-dd}</td></tr>
                        <tr><td>Check-out Date:</td><td>{checkOutDate:yyyy-MM-dd}</td></tr>
                        <tr><td>Room Number:</td><td>{roomNumber}</td></tr>
                    </table>
                </div>";

            if (services.Any(s => s.quantity > 0))
            {
                receiptHtml += @"
                <div class='services'>
                    <h3>Services</h3>
                    <table>
                        <tr><th>Service</th><th>Quantity</th><th>Price</th><th>Total</th></tr>";

                foreach (var service in services.Where(s => s.quantity > 0))
                {
                    var serviceTotal = service.price * service.quantity;
                    receiptHtml += $@"
                        <tr>
                            <td>{service.serviceName}</td>
                            <td>{service.quantity}</td>
                            <td>RM {service.price:F2}</td>
                            <td>RM {serviceTotal:F2}</td>
                        </tr>";
                }

                receiptHtml += "</table></div>";
            }

            receiptHtml += $@"
                <div class='total'>
                    <table>
                        <tr><td>Subtotal:</td><td>RM {subtotal:F2}</td></tr>
                        <tr><td>Tax (10%):</td><td>RM {tax:F2}</td></tr>
                        <tr style='font-weight: bold'><td>Total:</td><td>RM {total:F2}</td></tr>
                    </table>
                </div>

                <div class='footer'>
                    <p>Thank you for choosing EASYSTAYS HOTEL!</p>
                </div>
            </div>
        </body>
        </html>";

            return receiptHtml;
        }
    }
}
