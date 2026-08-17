using WangenPizza.Models;

namespace WangenPizza.Helper
{
    public static class OrderTrackingEmailHelper
    {
        /// <summary>Build HTML email for "Out for Delivery" notification</summary>
        public static string BuildOutForDeliveryEmail(Order order)
        {
            var trackingUrl = $"https://pizzawangen.ch/bestellung-verfolgen?id={order.Id}";
            
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #ff6b35, #f7931e); color: white; padding: 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .content {{ padding: 30px; }}
        .status {{ background: #e3f2fd; border-radius: 10px; padding: 20px; text-align: center; margin: 20px 0; }}
        .status-icon {{ font-size: 48px; margin-bottom: 10px; }}
        .status-text {{ font-size: 18px; color: #1976d2; font-weight: bold; }}
        .tracking-btn {{ display: inline-block; background: #ff6b35; color: white; padding: 15px 30px; text-decoration: none; border-radius: 25px; font-weight: bold; margin-top: 15px; }}
        .footer {{ background: #333; color: white; padding: 20px; text-align: center; font-size: 12px; }}
        .order-details {{ background: #f9f9f9; border-radius: 8px; padding: 15px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🛵 Ihre Bestellung ist unterwegs!</h1>
        </div>
        <div class='content'>
            <p>Guten Tag, <strong>{order.Name}</strong></p>
            <p>Wir haben Ihre Bestellung auf den Weg gebracht! 🍕</p>
            
            <div class='status'>
                <div class='status-icon'>🛵</div>
                <div class='status-text'>Unterwegs zu Ihnen</div>
            </div>
            
            <div class='order-details'>
                <strong>Bestellnummer:</strong> #{order.Id}<br>
                <strong>Lieferadresse:</strong> {order.Street}, {order.PostBox} {order.City}
            </div>
            
            <p style='text-align: center;'>
                <a href='{trackingUrl}' class='tracking-btn'>📍 Bestellung verfolgen</a>
            </p>
            
            <p>Ihr Fahrer ist bald bei Ihnen!</p>
            <p>Vielen Dank für Ihre Bestellung!</p>
        </div>
        <div class='footer'>
            <p>Wangen Pizza Kebab</p>
            <p>Zürcherstrasse 3, 8855 Wangen | 055 460 33 66</p>
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>Build HTML email for "Delivered" notification</summary>
        public static string BuildDeliveredEmail(Order order)
        {
            var trackingUrl = $"https://pizzawangen.ch/bestellung-verfolgen?id={order.Id}";
            
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #4caf50, #2e7d32); color: white; padding: 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .content {{ padding: 30px; }}
        .status {{ background: #e8f5e9; border-radius: 10px; padding: 20px; text-align: center; margin: 20px 0; }}
        .status-icon {{ font-size: 48px; margin-bottom: 10px; }}
        .status-text {{ font-size: 18px; color: #2e7d32; font-weight: bold; }}
        .footer {{ background: #333; color: white; padding: 20px; text-align: center; font-size: 12px; }}
        .order-details {{ background: #f9f9f9; border-radius: 8px; padding: 15px; margin: 15px 0; }}
        .rating {{ text-align: center; margin-top: 20px; }}
        .rating p {{ color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Ihre Bestellung wurde geliefert!</h1>
        </div>
        <div class='content'>
            <p>Guten Tag, <strong>{order.Name}</strong></p>
            <p>Ihre Bestellung wurde erfolgreich zugestellt!</p>
            
            <div class='status'>
                <div class='status-icon'>🍕</div>
                <div class='status-text'>Guten Appetit!</div>
            </div>
            
            <div class='order-details'>
                <strong>Bestellnummer:</strong> #{order.Id}<br>
                <strong>Geliefert an:</strong> {order.Street}, {order.PostBox} {order.City}
            </div>
            
            <p>Vielen Dank für Ihre Bestellung bei Wangen Pizza Kebab!</p>
            
            <div class='rating'>
                <p>Wie war Ihre Bestellung?</p>
                <p style='font-size: 24px;'>⭐⭐⭐⭐⭐</p>
            </div>
        </div>
        <div class='footer'>
            <p>Wangen Pizza Kebab</p>
            <p>Zürcherstrasse 3, 8855 Wangen | 055 460 33 66</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
