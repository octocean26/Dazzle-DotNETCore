using System.Security.Claims;

namespace DI_Sample
{
    internal class FlagRepositoryForUser : IFlagRepository
    {
        private ClaimsPrincipal user;

        public FlagRepositoryForUser(ClaimsPrincipal user)
        {
            this.user = user;
        }

        public Flag GetFlag(string country)
        {
            return new Flag();
        }
    }
}