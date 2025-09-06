# Email Configuration Guide for TvShop

## Error Details
The application was experiencing an SMTP authentication error:
```
SmtpException: The SMTP server requires a secure connection or the client was not authenticated. The server response was: 5.7.0 Authentication Required.
```

## Quick Solution for Development

For development purposes, you can now bypass email sending completely using the `UseDevModeBypass` option. This has been enabled by default in your `appsettings.Development.json`:

```json
"EmailSettings": {
  ...
  "UseDevModeBypass": "true"
}
```

With this option enabled:
- No actual emails will be sent
- Email content will be logged to the console instead
- User registration will work without any SMTP configuration

## Production Email Configuration

For production or when you want to test actual email delivery, you need to:

1. Set `"UseDevModeBypass": "false"` or remove it from the settings
2. Configure valid SMTP credentials

### Using Gmail SMTP Server

1. **Update the Email Settings**:
   Open `appsettings.Development.json` (or `appsettings.json` for production) and update the following section with your actual Gmail credentials:

   ```json
   "EmailSettings": {
     "SmtpServer": "smtp.gmail.com",
     "SmtpPort": 587,
     "SmtpUsername": "your-actual-email@gmail.com",
     "SmtpPassword": "your-gmail-app-password",
     "FromName": "TvShop Support",
     "FromAddress": "your-actual-email@gmail.com",
     "UseDevModeBypass": "false"
   }
   ```

2. **Create an App Password for Gmail**:
   If you have 2-Factor Authentication enabled on your Google account (recommended), you'll need to use an App Password instead of your regular account password:

   a. Go to your Google Account settings: https://myaccount.google.com/
   b. Navigate to Security → App passwords (you may need to sign in again)
   c. Select "Mail" as the app and "Other" as the device (you can name it "TvShop")
   d. Copy the generated 16-character password
   e. Paste this password in the `SmtpPassword` field in your settings file

3. **Make sure both email addresses match**:
   - Set `SmtpUsername` to your full Gmail address
   - Set `FromAddress` to the same Gmail address as your `SmtpUsername`

### Using Another Email Provider

If you prefer to use another email provider:

1. Update the SMTP server settings:
   ```json
   "SmtpServer": "your-providers-smtp-server",
   "SmtpPort": your-providers-smtp-port,
   ```

2. Provide your email provider's username and password:
   ```json
   "SmtpUsername": "your-email@provider.com",
   "SmtpPassword": "your-email-password",
   ```

## Important Security Notes

1. **Never commit your actual password or app password to source control**

2. **For production deployments**, consider using user secrets or environment variables:
   ```csharp
   // In Program.cs or Startup.cs
   builder.Configuration.AddUserSecrets<Program>();
   // or
   builder.Configuration.AddEnvironmentVariables();
   ```

3. **Common Email Issues**:
   - Ensure 'Less secure app access' is enabled for Gmail (or use App Passwords with 2FA)
   - Check firewall settings aren't blocking SMTP ports
   - Verify your email provider hasn't rate-limited your account

## Changes Made to Fix the Issue

1. Added development bypass mode (enabled by default)
2. Improved error handling in EmailService.cs
3. Added proper validation of email settings
4. Added comprehensive logging for email operations
5. Created this guide for future reference

## Testing Your Configuration

1. To test with the bypass mode: simply register a new user and check the console logs
2. To test actual email delivery: set `UseDevModeBypass` to `false`, configure your SMTP settings, and register a new user
