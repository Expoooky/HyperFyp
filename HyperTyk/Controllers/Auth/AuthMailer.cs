using MailKit.Net.Smtp;
using MimeKit;
using System;

namespace HyperTyk.Controllers.Auth
{
    public class AuthMailer
    {
        public void SendOTP(string toEmail, string username, string OTP)
        {
            try
            {
                // Create a new email message
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Hyper Fyp", "no-reply@hyperfyp.com"));
                message.To.Add(new MailboxAddress(username, toEmail));
                message.Subject = "Your One-Time Password is "+OTP;

                // Capitalize the username
                username = char.ToUpper(username[0]) + username.Substring(1);

                // Create the HTML body
                string htmlBody = $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Bahnschrift; background-color: #f4f4f4; margin: 0; padding: 0; }}
                            .container {{ max-width: 600px; margin: 40px auto; padding: 20px; background-color: #fff; border-radius: 10px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); }}
                            .header {{ font-size: 20px; color: #2c3e50; border-bottom: 1px solid #ddd; padding-bottom: 10px; }}
                            h2 {{ width: 100%; text-align: center; color: #484646; font-size: 50px; font-family: Bahnschrift; margin-top: 55px; }}
                            .content {{ margin-top: 20px; font-size: 16px; color: #34495e; line-height: 1.6; }}
                            .footer {{ margin-top: 30px; font-size: 16px; color: #95a5a6; border-top: 1px solid #ddd; padding-top: 10px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>Dear {username},</div>
                            <div class='content'>
                                You have requested to reset your password. Here is your One-Time Password (OTP):
                                <h2>{OTP}</h2><br>
                                Please use the provided OTP to complete your password reset request. If you did not initiate this request, please disregard this email.
                            </div>
                            <div class='footer'>Best regards,<br>HyperFyp</div>
                        </div>
                    </body>
                    </html>";

                // Add the HTML body to the message
                var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = bodyBuilder.ToMessageBody();

                // Send the email using MailKit's SmtpClient
                using (var client = new SmtpClient())
                {
                    client.Connect("hyperfyp.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                    client.Authenticate("no-reply@hyperfyp.com", "z@r2827lZ");
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions (logging, etc.)
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void SendEmailConfirmation(string toEmail, string username, string link)
        {
            try
            {
                // Create a new email message
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Hyper Fyp", "no-reply@hyperfyp.com"));
                message.To.Add(new MailboxAddress(username, toEmail));
                message.Subject = "Verify your Account @ HyperFyp";

                // Capitalize the username
                username = char.ToUpper(username[0]) + username.Substring(1);

                // Create the HTML body
                string htmlBody = $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Bahnschrift; background-color: #f4f4f4; margin: 0; padding: 0; }}
                            .container {{ max-width: 600px; margin: 40px auto; padding: 20px; background-color: #fff; border-radius: 10px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); }}
                            .header {{ font-size: 20px; color: #2c3e50; border-bottom: 1px solid #ddd; padding-bottom: 10px; }}
                            h2 {{ width: 100%; text-align: center; color: #484646; font-size: 50px; font-family: Bahnschrift; margin-top: 55px; }}
                            .content {{ margin-top: 20px; font-size: 16px; color: #34495e; line-height: 1.6; }}
                            .footer {{ margin-top: 30px; font-size: 16px; color: #95a5a6; border-top: 1px solid #ddd; padding-top: 10px; }}
                            .button-container {{ text-align: center; margin-top: 20px; }}
                            a.button {{ display: inline-block; padding: 15px 22px; font-size: 16px; color: #fff; background-color: #2c3e50; border: none; border-radius: 5px; text-decoration: none; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>Dear {username},</div>
                            <div class='content'>
                                Thank you for registering with HyperFyp! To complete your registration, please verify your email address by clicking the button below:
                            </div>
                            <div class='button-container'>
                                <a href='{link}' target='_blank' class='button'>Verify your Account</a>
                            </div>
                            <div class='content'>
                                <br>If the button above doesn't work, use this link instead: {link}
                            </div>
                            <div class='footer'>Best regards,<br>HyperFyp</div>
                        </div>
                    </body>
                    </html>";

                // Add the HTML body to the message
                var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = bodyBuilder.ToMessageBody();

                // Send the email using MailKit's SmtpClient
                using (var client = new SmtpClient())
                {
                    client.Connect("hyperfyp.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                    client.Authenticate("no-reply@hyperfyp.com", "z@r2827lZ");
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions (logging, etc.)
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
