using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MSP.Application.Extensions
{
    public static class FirebaseAdminConfiguration
    {
        /// <summary>
        /// Initialize Firebase Admin SDK
        /// </summary>
        public static void InitializeFirebaseAdminSDK(
            this IHostEnvironment environment,
            ILogger logger)
        {
            try
            {
                // Skip if already initialized
                if (FirebaseApp.DefaultInstance != null)
                {
                    logger.LogInformation("[Firebase] Already initialized, skipping...");
                    return;
                }

                var firebaseConfigPath = Path.Combine(
                    environment.ContentRootPath,
                    "FirebaseConfig",
                    "ai-msp-firebase-adminsdk-fbsvc-c42dd34434.json");

                logger.LogInformation("[Firebase] Looking for config at: {Path}", firebaseConfigPath);

                if (File.Exists(firebaseConfigPath))
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(firebaseConfigPath)
                    });

                    logger.LogInformation("[Firebase] Admin SDK initialized successfully");
                }
                else
                {
                    logger.LogWarning("[Firebase] Config file not found at: {Path}", firebaseConfigPath);
                    logger.LogWarning("[Firebase] FCM push notifications will not work!");
                    logger.LogWarning("[Firebase] Download service account key from Firebase Console");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Firebase] Initialization failed: {Message}", ex.Message);
                logger.LogWarning("[Firebase] App will continue without FCM support");
            }
        }

        /// <summary>
        /// Alternative: Initialize from environment variable (for production)
        /// </summary>
        public static void InitializeFirebaseFromEnvironment(
            this IHostEnvironment environment,
            ILogger logger,
            string? firebaseCredentialsJson = null)
        {
            try
            {
                if (FirebaseApp.DefaultInstance != null)
                {
                    logger.LogInformation("[Firebase] Already initialized, skipping...");
                    return;
                }

                // Try from environment variable first
                var credentialsJson = firebaseCredentialsJson
                    ?? Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");

                if (!string.IsNullOrEmpty(credentialsJson))
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromJson(credentialsJson)
                    });

                    logger.LogInformation("[Firebase] Initialized from environment variable");
                    return;
                }

                // Fallback to file
                environment.InitializeFirebaseAdminSDK(logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Firebase] Initialization failed: {Message}", ex.Message);
            }
        }
    }
}
