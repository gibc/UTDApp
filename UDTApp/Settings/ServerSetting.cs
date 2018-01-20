using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using UDTApp.Encryption;

namespace UDTApp.Settings
{
    public class ServerSetting
    {
        public string serverName
        {
            get;
            set;
        }

        public string userId
        {
            get;
            set;
        }

        private string _pwd = "";
        public string pwd
        {
            get
            {
                return SecureIt.DecryptString(_pwd).ToString();
            }
            set
            {
                var secure = new SecureString();
                foreach (char c in value)
                {
                    secure.AppendChar(c);
                }
                _pwd = SecureIt.EncryptString(secure);
            }
        }
    }
}
