namespace ECommerce.Shared.Constants
{
    public static class Messages
    {
        public static class Common
        {
            public const string RecordFound = "Kayıt bulundu";
            public const string RecordNotFound = "Kayıt bulunamadı";
            public const string RecordCreated = "Kayıt oluşturuldu";
            public const string RecordUpdated = "Kayıt güncellendi";
            public const string RecordDeleted = "Kayıt silindi";
        }

        public static class Auth
        {
            public const string UserNotFound = "Kullanıcı bulunamadı";
            public const string PasswordError = "Şifre hatalı";
            public const string SuccessfulLogin = "Giriş başarılı";
            public const string UserAlreadyExists = "Kullanıcı zaten mevcut";
            public const string UserRegistered = "Kullanıcı kaydedildi";
            public const string AccessTokenCreated = "Token oluşturuldu";
        }
    }
} 