using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Recommender_system.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Recommender_system.Controllers
{
    public class BaseController : Controller
    {
        private ApplicationUserManager _userManager;
        static readonly string PasswordHash = "P@@Sw0rdA()ŠP@21";
        static readonly string SaltKey = "S@LT&KEYOK_NOT";
        static readonly string VIKey = "@1B2c3D4e5F6g7H8f";

        private SRDB db = new SRDB();
        private SREntities db1 = new SREntities();
        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            var user = (GetUser)Session["User"];

            return base.BeginExecuteCore(callback, state);
        }


        public async Task<GetUser> GetUser()
        {
            USER user = new USER();
            GetUser usertotal = new GetUser();
            if (Session["User"] == null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    try
                    {
                        var userfound = await UserManager.FindByEmailAsync(User.Identity.Name);
                        if (userfound == null)
                            userfound = await UserManager.FindByNameAsync(User.Identity.Name);

                        user = db.USERs.Single(q => q.UserID == userfound.Id);

                        usertotal.ID = user.ID;
                        usertotal.UserID = user.UserID;
                        usertotal.Emri = user.Emri;
                        usertotal.Mbiemri = user.Mbiemri;
                        usertotal.Adresa = user.Adresa;
                        Session["User"] = usertotal;
                    }
                    catch
                    {
                        usertotal = null;
                    }
                }
            }
            else
                usertotal = (GetUser)Session["User"];
            return usertotal;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public static string Encrypt(string plainText)
        {
            try
            {
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

                byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
                var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
                var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));

                byte[] cipherTextBytes;

                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        cipherTextBytes = memoryStream.ToArray();
                        cryptoStream.Close();
                    }
                    memoryStream.Close();
                }
                return Convert.ToBase64String(cipherTextBytes);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string Decrypt(string encryptedText)
        {
            try
            {
                byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
                byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
                var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };

                var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
                var memoryStream = new MemoryStream(cipherTextBytes);
                var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                byte[] plainTextBytes = new byte[cipherTextBytes.Length];

                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}