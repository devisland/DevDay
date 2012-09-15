using System.Security.Principal;

namespace DevDay.Authentication
{
    public class CustomPrincipal : IPrincipal
    {
        public int UserID { get; private set; }
        public string Name { get; private set; }
        public IIdentity Identity { get; private set; }

        public CustomPrincipal(string name, IIdentity genericIdentity, int userId)
        {
            UserID = userId;
            Name = name;
            Identity = genericIdentity;
        }

        public bool IsInRole(string role)
        {
            return true;
        }
    }
}
