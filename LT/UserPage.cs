using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Web;

namespace LT
{
	public class UserPage<TAccountId> : BasePage where TAccountId : new()
	{
		public static TAccountId AccountID
		{
			get
			{
                if (HttpContext.Current == null || HttpContext.Current.Session["AccountID"] == null)
                    return new TAccountId();
                return (TAccountId)HttpContext.Current.Session["AccountID"];
			}
			set { HttpContext.Current.Session["AccountID"] = value; }
		}

		public static string AccountName
		{
			get { return (string)HttpContext.Current.Session["AccountName"]; }
			set { HttpContext.Current.Session["AccountName"] = value; }
		}

		public static bool IsLoggedIn
		{
			get
            {
                if (HttpContext.Current == null || HttpContext.Current.Session == null)
                    return false;
                return HttpContext.Current.Session["AccountID"] != null; 
            }
		}

		public virtual bool Admin
		{
			get { return IsLoggedIn && Convert.ToInt32(AccountID) == 1; }
		}

        /*
            CREATE TABLE `AccountLogin` (
                `AccountID` int( 11 ) NOT NULL default '0',
                `DateTime` datetime NOT NULL default '0000-00-00 00:00:00',
                `Browser` varchar( 255 ) NOT NULL default '',
                `IPAddress` varchar( 255 ) NOT NULL default '',
                PRIMARY KEY ( `AccountID` , `DateTime` )
            ) ENGINE = MYISAM DEFAULT CHARSET = latin1;
         */
		public virtual string Login(string loginName, string password, string fieldName)
		{
			Hashtable account =
				EvaluateRow
				(
				"select * from Account where {0} = '{1}' and (Password = '{2}' or Password = '{3}')",
				fieldName,
				AddSlashes(loginName),
				AddSlashes(password),
                AddSlashes(CalculateHash(password))
				);
			if (account == null)
				return Resources.TextStrings.InvalidLogin;

			AccountID = (TAccountId)account["ID"];
			AccountName = (string)account[fieldName];

            try
            {
                Execute
                    (
                    "insert ignore into AccountLogin (AccountID, DateTime, IPAddress) values ('{0}', Now(), '{1}')",
                    AccountID,
                    AddSlashes(Request.UserHostAddress)
                    );
            }
            catch (MySql.Data.MySqlClient.MySqlException)
            {
                // throw away duplicate insert error
            }

			return null;
		}

		public virtual void Logout()
		{
			Session.Remove("AccountID");
		}
	
		public void Authenticate()
		{
			if (!IsLoggedIn)
				Response.Redirect("/", true);
		}

		public string IsValidEmail(string emailAddress)
		{
			if (!IsValidEmailAddress(emailAddress))
				return Resources.TextStrings.InvalidEmail;
			if (Evaluate("select ID from Account where Email = '{0}'", AddSlashes(emailAddress)) != null)
				return Resources.TextStrings.DuplicateEmail;
			return null;
		}

		public string ModifyEmail(string emailAddress)
		{
			string temp = IsValidEmail(emailAddress);
			if (temp != null)
				return temp;
			Execute("update account set email = '{0}' where id = {1}", AddSlashes(emailAddress), AccountID);
			return Resources.TextStrings.EmailChangeSuccess;
		}

		public static string IsValidPassword(string password, string passwordMatch)
		{
			if (password.Length < 6)
				return Resources.TextStrings.PasswordLength;
			if (password != passwordMatch)
				return Resources.TextStrings.PasswordMismatch;
			return null;
		}

		public string ModifyPassword(string oldPassword, string newPassword, string newPasswordVerify)
		{
			Hashtable account = EvaluateRow("select password from Account where ID = {0}", AccountID);

            if (oldPassword != (string)account["password"] && CalculateHash(oldPassword) != (string)account["password"])
				return Resources.TextStrings.IncorrectPassword;

			string isValid = IsValidPassword(newPassword, newPasswordVerify);
			if (isValid != null)
				return isValid;

            Execute("update account set password = '{0}' where id = {1}", AddSlashes(CalculateHash(newPassword)), AccountID);
			return Resources.TextStrings.PasswordSuccess;
		}

		public void ChangePasswordForm()
		{
			if (IsLoggedIn)
			{
				string result = String.Empty;
				if (IsSet("OldPassword"))
					result = ModifyPassword(GetString("OldPassword"), GetString("NewPassword"), GetString("VerifyPassword"));

				// change password form
				Response.Write("<form method=\"post\">");
				if (result.Length > 0)
					Response.Write(String.Format("<span class=Error>{0}</span>", result));
				Response.Write("<table>");
                Response.Write("<tr><td style=\"text-align: right\" width=120>{0}</td><td><input type=\"password\" name=\"OldPassword\" size=\"20\" class=\"inputBox\"></td></tr>", Resources.TextStrings.CurrentPassword);
                Response.Write("<tr><td style=\"text-align: right\" width=120>{0}</td><td><input type=\"password\" name=\"NewPassword\" size=\"20\" class=\"inputBox\"></td></tr>", Resources.TextStrings.NewPassword);
                Response.Write("<tr><td style=\"text-align: right\" width=120>{0}</td><td><input type=\"password\" name=\"VerifyPassword\" size=\"20\" class=\"inputBox\"></td></tr>", Resources.TextStrings.VerifyPassword);
                Response.Write("<tr><td style=\"text-align: right\" width=120></td><td><input type=\"submit\" value=\"{0}\" class=\"submitBox\"></td></tr>", Resources.TextStrings.ChangePassword);
				Response.Write("</table>");
				Response.Write("</form>");
			}
		}

		public void ChangeEmailForm()
		{
			if (IsLoggedIn)
			{
				string result = String.Empty;
				if (IsSet("EmailAddress"))
					result = ModifyEmail(GetString("EmailAddress"));

				Hashtable account = EvaluateRow("select email from Account where ID = {0}", AccountID);

				// email address form
				Response.Write("<form method=\"post\">");
				if (result.Length > 0)
					Response.Write(String.Format("<span class=Error>{0}</span>", result));
				Response.Write("<table>");
                Response.Write("<tr><td style=\"text-align: right\" width=120>{0}</td><td><input name='EmailAddress' value='{1}' size='20' class='inputBox'></td></tr>", Resources.TextStrings.EmailAddress, account["email"]);
                Response.Write("<tr><td style=\"text-align: right\" width=120></td><td><input type=\"submit\" value=\"{0}\" class=\"submitBox\"></td></tr>", Resources.TextStrings.UpdateEmail);
				Response.Write("</table>");
				Response.Write("</form>");
			}
		}

		// http://dotnet.org.za/deon/articles/2998.aspx
		private static string EncryptString(string inputText, string password)
		{
			// We are now going to create an instance of the 
			// Rihndael class.  
			RijndaelManaged rijndaelCipher = new RijndaelManaged();

			// First we need to turn the input strings into a byte array.
			byte[] plainText = System.Text.Encoding.Unicode.GetBytes(inputText);

			// We are using salt to make it harder to guess our key
			// using a dictionary attack.
			byte[] salt = Encoding.ASCII.GetBytes(password.Length.ToString());

			// The (Secret Key) will be generated from the specified 
			// password and salt.
			PasswordDeriveBytes secretKey = new PasswordDeriveBytes(password, salt);

			// Create a encryptor from the existing SecretKey bytes.
			// We use 32 bytes for the secret key 
			// (the default Rijndael key length is 256 bit = 32 bytes) and
			// then 16 bytes for the IV (initialization vector),
			// (the default Rijndael IV length is 128 bit = 16 bytes)
			ICryptoTransform encryptor = rijndaelCipher.CreateEncryptor(secretKey.GetBytes(32), secretKey.GetBytes(16));

			// Create a MemoryStream that is going to hold the encrypted bytes 
			MemoryStream memoryStream = new MemoryStream();

			// Create a CryptoStream through which we are going to be processing our data. 
			// CryptoStreamMode.Write means that we are going to be writing data 
			// to the stream and the output will be written in the MemoryStream
			// we have provided. (always use write mode for encryption)
			CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

			// Start the encryption process.
			cryptoStream.Write(plainText, 0, plainText.Length);

			// Finish encrypting.
			cryptoStream.FlushFinalBlock();

			// Convert our encrypted data from a memoryStream into a byte array.
			byte[] cipherBytes = memoryStream.ToArray();

			// Close both streams.
			memoryStream.Close();
			cryptoStream.Close();

			// Convert encrypted data into a base64-encoded string.
			// A common mistake would be to use an Encoding class for that. 
			// It does not work, because not all byte values can be
			// represented by characters. We are going to be using Base64 encoding
			// That is designed exactly for what we are trying to do. 
			string encryptedData = Convert.ToBase64String(cipherBytes);

			// Return encrypted string.
			return encryptedData;
		}

		private static string DecryptString(string inputText, string password)
		{
			RijndaelManaged RijndaelCipher = new RijndaelManaged();

			byte[] EncryptedData = Convert.FromBase64String(inputText);
			byte[] Salt = Encoding.ASCII.GetBytes(password.Length.ToString());

			PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(password, Salt);

			// Create a decryptor from the existing SecretKey bytes.
			ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));

			MemoryStream memoryStream = new MemoryStream(EncryptedData);

			// Create a CryptoStream. (always use Read mode for decryption).
			CryptoStream cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read);

			// Since at this point we don't know what the size of decrypted data
			// will be, allocate the buffer long enough to hold EncryptedData;
			// DecryptedData is never longer than EncryptedData.
			byte[] PlainText = new byte[EncryptedData.Length];

			// Start decrypting.
			int DecryptedCount = cryptoStream.Read(PlainText, 0, PlainText.Length);

			memoryStream.Close();
			cryptoStream.Close();

			// Convert decrypted data into a string. 
			string DecryptedData = Encoding.Unicode.GetString(PlainText, 0, DecryptedCount);

			// Return decrypted string.   
			return DecryptedData;
		}

		public static string EncryptPassword(string password)
		{
			return EncryptString(password, password);
		}

        static SHA512 hasher = System.Security.Cryptography.SHA512.Create();

        public static string CalculateHash(string input)
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes("s&~D$L{a8_" + input);
            byte[] hash = hasher.ComputeHash(inputBytes);
            return Convert.ToBase64String(hash);
        }

        // generate random password
        static char[] pwdCharArray = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        static Random Random = new Random();

        public static string GeneratePassword(int length)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < length; i++)
                result.Append(pwdCharArray[Random.Next(pwdCharArray.Length)]);

            return result.ToString();
        }
	}
}