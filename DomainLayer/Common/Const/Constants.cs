using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Domain.Common.Const;

public static class Constants
{

    public const string DefaultPassword = "123*Pskr*D";
    public const int NumberOfTickets = 20;
    public const int NumberOfParking = 5;
    public const int NumberOfSROTicket = 12;
    public const int OurSuiteConference = 28;
    public const int StageSuiteConference = 28;
    public const int OwnerSuiteConference = 18;

    public const string OurSuiteDisplayName = "Our Suite";
    public const string StageSuiteDisplayName = "Stage Suite";
    public const string OwnersConferenceSpaceDisplayName = "Owners Conference Space";

    public const string OurSuiteConferenceLocation = "5W-11";
    public const string StageSuiteConferenceLocation = "Level 4 & 5";
    public const string OwnerSuiteConferenceLocation = "Level 2";
    public const int OurSuiteConferenceQty = 1;
    public const int StageSuiteConferenceQty = 4;
    public const int OwnerSuiteConferenceQty = 1;

    public const int MinimumPasswordLength = 8;
    public const string EmailFrom = "";
    public const string EmailAdmin = "tikatinnon@gmail.com";

    public const string EmailLogoUrl = "";

    public const string FacebookUrl = "";
    public const string TwitterUrl = "";
    public const string InstagramUrl = "";
    public const string LinkedinUrl = "";
    public const string PhoneNumber = "tel:+xxxxxxxxx";
    public const string DefaultEmailRenderingNamespace = "Infrastructure.Rendering.EmailTemplates";
    public const string BlobStorageContainerName = "event-files";
    public const string BlobVideo = "login-background.mp4";

    public static readonly string[] AllowedImageFileTypes =
    [
        ".jpg", ".jpeg", ".png", ".webp"
    ];

    public static string GetIcon(bool isTrue)
    {
        return isTrue ? "Yes" : "No";
    }

    //public static DateTimeOffset GetCurrentPST()
    //{
    //    TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
    //    return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, pstZone);
    //    return DateTimeOffset.UtcNow;
    //}

    public static string Sha512(string txtPassword)
    {
        using (var sha512Hash = SHA512.Create())
        {
            byte[] bytes = sha512Hash.ComputeHash(Encoding.UTF8.GetBytes(txtPassword));
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }
    }

    public static byte[] HexStringToByteArray(string hex)
    {
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }

    public static async Task<string> DecryptText(string cipherText, string secretKey, string ivKey)
    {
        byte[] key = HexStringToByteArray(secretKey);
        byte[] iv = HexStringToByteArray(ivKey);

        byte[] cipherBytes = Convert.FromBase64String(cipherText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted data from the stream
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

    //public static async Task<string> DecryptText(string cipherText, string secretKey, string ivKey)
    //{
    //    return await Task.Run(() =>
    //    {
    //        byte[] key = HexStringToByteArray(secretKey);
    //        byte[] iv = HexStringToByteArray(ivKey);
    //        byte[] cipherBytes = Convert.FromBase64String(cipherText);

    //        using (Aes aes = Aes.Create())
    //        {
    //            aes.Key = key;
    //            aes.IV = iv;
    //            aes.Mode = CipherMode.CBC;
    //            aes.Padding = PaddingMode.PKCS7;

    //            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
    //            {
    //                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
    //                {
    //                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
    //                    {
    //                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
    //                        {
    //                            // Read the decrypted data asynchronously from the stream
    //                            return srDecrypt.ReadToEnd();
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    });
    //}

    public static DateTimeOffset? ConvertToUtc(DateTimeOffset? dateTimeOffset)
    {
        if (dateTimeOffset.HasValue)
        {
            return dateTimeOffset.Value.ToUniversalTime();
        }
        return null;
    }

    public static DateTimeOffset? ConvertUtcToPst(DateTimeOffset? utcDateTime)
    {
        if (utcDateTime == null) return null;

        TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        DateTimeOffset convertedTime = TimeZoneInfo.ConvertTime((DateTimeOffset)utcDateTime, targetTimeZone);

        return convertedTime;

    }

}
