using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Services.Models;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailVerificationAsync(string email, string username, string verificationToken)
        {
            var subject = "Verify Your Email - VLiving";
            
            var body = GetEmailVerificationTemplate(username, verificationToken);
            
            await SendEmailAsync(email, subject, body);
            
            _logger.LogInformation("Email verification sent to {Email}", email);
        }

        public async Task SendPasswordResetAsync(string email, string username, string resetToken)
        {
            var subject = "Reset Your Password - VLiving";
            
            // Use the resetToken directly as 6-digit code (no URL needed)
            var body = GetPasswordResetTemplate(username, resetToken);
            
            await SendEmailAsync(email, subject, body);
            
            _logger.LogInformation("Password reset email sent to {Email}", email);
        }

        public async Task SendWelcomeEmailAsync(string email, string username)
        {
            var subject = "Welcome to VLiving!";
            var body = GetWelcomeEmailTemplate(username);
            
            await SendEmailAsync(email, subject, body);
            
            _logger.LogInformation("Welcome email sent to {Email}", email);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                
                // Important: Set UseDefaultCredentials to false FIRST
                client.UseDefaultCredentials = false;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Timeout = 30000; // Increased timeout
                
                // Gmail SMTP configuration - Different settings for port 465 vs 587
                if (_emailSettings.SmtpPort == 465)
                {
                    // SSL configuration for port 465 (Gmail's SSL port)
                    client.EnableSsl = true;
                    // For port 465, we need explicit SSL, not STARTTLS
                    client.TargetName = "SMTPSVC/" + _emailSettings.SmtpServer;
                }
                else if (_emailSettings.SmtpPort == 587)
                {
                    // STARTTLS configuration for port 587 (Gmail's TLS port)
                    client.EnableSsl = true; // This enables STARTTLS for port 587
                }
                else
                {
                    // Default configuration for other ports
                    client.EnableSsl = _emailSettings.EnableSsl;
                }
                
                // Set credentials AFTER setting UseDefaultCredentials = false
                client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);

                using var message = new MailMessage();
                message.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;
                message.BodyEncoding = Encoding.UTF8;
                message.Priority = MailPriority.Normal;

                _logger.LogInformation("Attempting to send email to {Email} via {SmtpServer}:{SmtpPort} (SSL: {EnableSsl})", 
                    toEmail, _emailSettings.SmtpServer, _emailSettings.SmtpPort, client.EnableSsl);
                
                await client.SendMailAsync(message);
                
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP Error sending email to {Email}. Status: {Status}, Response: {Response}", 
                    toEmail, smtpEx.StatusCode, smtpEx.Message);
                throw new InvalidOperationException($"SMTP Error: {smtpEx.Message}. Please check email configuration.", smtpEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
            }
        }

        private string GetEmailVerificationTemplate(string username, string verificationCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Verify Your Email</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>Welcome to VLiving!</h1>
    </div>
    
    <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
        <h2 style='color: #333; margin-top: 0;'>Hi {username}! 👋</h2>
        
        <p>Thank you for registering with VLiving! To complete your registration and start using our mobile app, please enter the verification code below in your app.</p>
        
        <div style='text-align: center; margin: 40px 0;'>
            <div style='background: #f8f9fa; border: 2px dashed #667eea; padding: 20px; border-radius: 10px; margin: 20px 0;'>
                <p style='margin: 0; color: #666; font-size: 14px; margin-bottom: 10px;'>Your verification code is:</p>
                <div style='font-size: 36px; font-weight: bold; color: #667eea; letter-spacing: 8px; font-family: Courier, monospace;'>
                    {verificationCode}
                </div>
                <p style='margin: 10px 0 0 0; color: #666; font-size: 12px;'>📱 Tap to copy this code</p>
            </div>
        </div>
        
        <div style='background: #e3f2fd; padding: 15px; border-radius: 8px; margin: 20px 0;'>
            <p style='margin: 0; color: #1976d2; font-size: 14px;'>
                <strong>📲 How to verify:</strong><br>
                1. Open VLiving app on your mobile device<br>
                2. Go to Email Verification screen<br>
                3. Enter the 6-digit code above<br>
                4. Tap ""Verify Email""
            </p>
        </div>
        
        <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
        
        <p style='color: #666; font-size: 14px;'>
            <strong>What's VLiving?</strong><br>
            VLiving is your trusted platform for finding the perfect living space, connecting with roommates, and discovering housing opportunities.
        </p>
        
        <p style='color: #666; font-size: 12px; text-align: center; margin-top: 30px;'>
            This verification code will expire in 24 hours for security reasons.<br>
            If you didn't create this account, please ignore this email.
        </p>
    </div>
</body>
</html>";
        }

        private string GetPasswordResetTemplate(string username, string resetCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Your Password</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>Reset Your Password</h1>
    </div>
    
    <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
        <h2 style='color: #333; margin-top: 0;'>Hi {username}! 🔐</h2>
        
        <p>We received a request to reset your password for your VLiving account. To reset your password, please enter the verification code below in your app.</p>
        
        <div style='text-align: center; margin: 40px 0;'>
            <div style='background: #f8f9fa; border: 2px dashed #ff6b6b; padding: 20px; border-radius: 10px; margin: 20px 0;'>
                <p style='margin: 0; color: #666; font-size: 14px; margin-bottom: 10px;'>Your password reset code is:</p>
                <div style='font-size: 36px; font-weight: bold; color: #ff6b6b; letter-spacing: 8px; font-family: Courier, monospace;'>
                    {resetCode}
                </div>
                <p style='margin: 10px 0 0 0; color: #666; font-size: 12px;'>📱 Tap to copy this code</p>
            </div>
        </div>
        
        <div style='background: #fff3cd; border: 1px solid #ffeeba; color: #856404; padding: 15px; border-radius: 8px; margin: 20px 0;'>
            <p style='margin: 0; color: #856404; font-size: 14px;'>
                <strong>🔄 How to reset password:</strong><br>
                1. Open VLiving app on your mobile device<br>
                2. Go to Reset Password screen<br>
                3. Enter the 6-digit code above<br>
                4. Enter your new password<br>
                5. Tap ""Reset Password""
            </p>
        </div>
        
        <div style='background: #f8d7da; border: 1px solid #f5c6cb; color: #721c24; padding: 15px; border-radius: 5px; margin: 20px 0;'>
            <strong>⚠️ Security Notice:</strong><br>
            • This password reset code will expire in 1 hour<br>
            • If you didn't request this reset, please ignore this email<br>
            • Your password will remain unchanged until you create a new one<br>
            • This code can only be used once
        </div>
        
        <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
        
        <p style='color: #666; font-size: 14px;'>
            <strong>Forgot why you're getting this?</strong><br>
            Someone (hopefully you) requested a password reset for your VLiving account. If this wasn't you, your account is still secure.
        </p>
        
        <p style='color: #666; font-size: 12px; text-align: center; margin-top: 30px;'>
            If you have any concerns about your account security, please contact our support team.
        </p>
    </div>
</body>
</html>";
        }

        private string GetWelcomeEmailTemplate(string username)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Welcome to VLiving!</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #28a745 0%, #20c997 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>Welcome to VLiving! 🏠</h1>
    </div>
    
    <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
        <h2 style='color: #333; margin-top: 0;'>Congratulations {username}! 🎉</h2>
        
        <p>Your email has been successfully verified and your VLiving account is now active! We're excited to have you join our community.</p>
        
        <div style='background: #d4edda; border: 1px solid #c3e6cb; color: #155724; padding: 15px; border-radius: 5px; margin: 20px 0;'>
            <strong>✅ Account Status:</strong> Verified and Active<br>
            <strong>📧 Email:</strong> Confirmed<br>
            <strong>🚀 Ready to:</strong> Start exploring VLiving!
        </div>
        
        <h3 style='color: #28a745; margin-top: 30px;'>What can you do now?</h3>
        <ul style='padding-left: 20px;'>
            <li>🏘️ Browse available properties and rooms</li>
            <li>👥 Find compatible roommates</li>
            <li>📝 Post your own property listings</li>
            <li>💬 Connect with other members</li>
            <li>⭐ Save your favorite listings</li>
        </ul>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{_emailSettings.BaseUrl}' 
               style='background: linear-gradient(135deg, #28a745 0%, #20c997 100%); 
                      color: white; 
                      padding: 15px 30px; 
                      text-decoration: none; 
                      border-radius: 5px; 
                      font-weight: bold; 
                      display: inline-block;'>
                Start Exploring VLiving
            </a>
        </div>
        
        <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
        
        <p style='color: #666; font-size: 14px;'>
            <strong>Need help getting started?</strong><br>
            Check out our user guide or contact our support team. We're here to help you make the most of VLiving!
        </p>
        
        <p style='color: #666; font-size: 12px; text-align: center; margin-top: 30px;'>
            Thanks for choosing VLiving - Your trusted housing companion! 🏡
        </p>
    </div>
</body>
</html>";
        }
    }
}